using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.InputSystem;
using TMPro;
using UnityEngine.UI;

public class Player : MonoBehaviour
{
    public float moveSpeed = 3.5f;
    public float sprintSpeed = 6.0f;
    public float jumpForce = 1.2f;
    public float gravity = -12.0f;
    public float airControl = 0.2f;
    public float airDrag = 2.0f;
    public float deceleration = 18.0f;
    public float velocityThreshold = 0.05f;
    public float lightLevel;
    
    public float sanity = 1f;
    public enum sanityLevel { State_Normal, State_Uneasy, State_Disturb, State_Panic, State_Insane }
    private sanityLevel currentSanityLevel;
    private sanityLevel previousSanityLevel;
    public float sanityDecreaseRate = 0.002f;
    public float lightThreshold = 0.3f;
    public float darknessDelay = 2.0f;
    public float maxSanityDrainRate = 0.01f;
    public float sanityRestDureation = 3f;
    private float sanityRestTimer = 0f;
    public float increaseTime = 5f;
    private float darknessTimer = 0f;
    private bool inDarkness = false;


    private CharacterController characterController;
    public LightProbes probes;
    private Vector3 velocity;
    private Vector3 currentDirection;
    public bool isGrounded;
    public bool isSprinting;

    private PlayerAction playerActions;
    private Vector2 moveInput;
    private bool jumpInput;

    private void Awake()
    {
        playerActions = new PlayerAction();
        playerActions.Player.Move.performed += ctx => moveInput = ctx.ReadValue<Vector2>();
        playerActions.Player.Move.canceled += ctx => moveInput = Vector2.zero;
        playerActions.Player.Jump.performed += ctx =>
        {
            if (isGrounded)
                jumpInput = true;
        };
        playerActions.Player.Sprint.performed += ctx => isSprinting = true;
        playerActions.Player.Sprint.canceled += ctx => isSprinting = false;
    }

    void OnEnable()
    {
        playerActions.Player.Enable();
    }

    void OnDisable()
    {
        playerActions.Player.Disable();
    }

    void Start()
    {
        characterController = GetComponent<CharacterController>();
    }

    float ReadLightProbes()
    {
        Vector3 position = transform.position;
        SphericalHarmonicsL2 shData;
        LightProbes.GetInterpolatedProbe(position, null, out shData);

        Vector3[] directions = new Vector3[] { Vector3.up, Vector3.forward, Vector3.right, Vector3.left, Vector3.back, Vector3.down };
        Color[] results = new Color[directions.Length];
        shData.Evaluate(directions, results);

        float avgBrightness = 0f;
        for (int i = 0; i < results.Length; i++)
            avgBrightness += results[i].grayscale;
        avgBrightness /= results.Length;
        return avgBrightness;
    }

    float ReadLightDirect()
    {
        float totalIntensity = 0f;
        int sampleCount = 0;
        float headHeight = 1.5f;
        float sideOffset = 0.3f;

        Vector3[] samplePoints = new Vector3[]
{
        transform.position,
        transform.position + Vector3.up * headHeight,
        transform.position + transform.right * sideOffset,
        transform.position - transform.right * sideOffset
};

        foreach (var light in FindObjectsByType<Light>(FindObjectsSortMode.None))
        {
            if (light.enabled)
            {
                foreach (var origin in samplePoints)
                {
                    Vector3 toLight = light.transform.position - origin;
                    float distance = toLight.magnitude;
                    if (!Physics.Raycast(origin, toLight.normalized, distance))
                    {
                        float intensity = light.intensity / (distance * distance);
                        totalIntensity += intensity;
                        sampleCount++;
                    }
                }
            }
        }

        if (sampleCount > 0)
            return Mathf.Clamp01(totalIntensity / sampleCount);
        else
            return 0f;
    }

    void OnControllerColliderHit(ControllerColliderHit hit)
    {
        Rigidbody rb = hit.collider.attachedRigidbody;

        if (rb != null && !rb.isKinematic)
        {
            Vector3 pushDir = new Vector3(hit.moveDirection.x, 0, hit.moveDirection.z);

            rb.AddForce(pushDir * 0.15f, ForceMode.Impulse);
        }
    }

    void Update()
    {
        float probeLevel = ReadLightProbes();
        float directLevel = ReadLightDirect();

        float combined = (probeLevel + directLevel) * 3f;
        lightLevel = Mathf.Clamp01(combined);
        if (currentSanityLevel != previousSanityLevel)
        {
            sanityRestTimer = sanityRestDureation;
            previousSanityLevel = currentSanityLevel;
        }

        // Handle rest timer
        if (sanityRestTimer > 0f)
        {
            sanityRestTimer -= Time.deltaTime;
        }
        else
        {
            if (lightLevel < lightThreshold)
            {
                if (!inDarkness)
                {
                    inDarkness = true;
                    darknessTimer = 0f;
                }
                else
                {
                    darknessTimer += Time.deltaTime;
                    if (darknessTimer > darknessDelay)
                    {
                        float ramp = Mathf.Clamp01((darknessTimer - darknessDelay) / increaseTime);
                        float drainRate = Mathf.Lerp(sanityDecreaseRate, maxSanityDrainRate, ramp);
                        sanity -= drainRate * Time.deltaTime;
                        sanity = Mathf.Clamp01(sanity);
                    }
                }
            }
            else
            {
                inDarkness = false;
                darknessTimer = 0f;
                sanity += (sanityDecreaseRate * 0.5f) * Time.deltaTime;
                sanity = Mathf.Clamp01(sanity);
            }
        }

        if (sanity >= 0.8f)
            currentSanityLevel = sanityLevel.State_Normal;
        else if (sanity > 0.6f)
            currentSanityLevel = sanityLevel.State_Uneasy;
        else if (sanity > 0.4f)
            currentSanityLevel = sanityLevel.State_Disturb;
        else if (sanity > 0.2f)
            currentSanityLevel = sanityLevel.State_Panic;
        else
            currentSanityLevel = sanityLevel.State_Insane;

        isGrounded = characterController.isGrounded;
        if (isGrounded && velocity.y < 0)
        {
            velocity.y = -2f;
        }

        Transform cam = GetComponentInChildren<Camera>().transform;
        Vector3 camForward = cam.forward;
        camForward.y = 0f;
        camForward.Normalize();

        Vector3 camRight = cam.right;
        camRight.y = 0f;
        camRight.Normalize();

        if (camForward.sqrMagnitude < 0.01f)
            camForward = transform.forward;

        Vector3 move = (camRight * moveInput.x + camForward * moveInput.y).normalized;
        float currentSpeed = isSprinting ? sprintSpeed : moveSpeed;

        if (isGrounded)
        {
            characterController.Move(move * currentSpeed * Time.deltaTime);
            currentDirection = move * currentSpeed;
        }
        else
        {
            currentDirection = Vector3.Lerp(currentDirection, move * currentSpeed, airControl * Time.deltaTime);
            currentDirection -= currentDirection * airDrag * Time.deltaTime; // Apply air drag
            characterController.Move(currentDirection * Time.deltaTime);
        }

        if (jumpInput && isGrounded)
        {
            velocity.y = Mathf.Sqrt(jumpForce * -2f * gravity);
            jumpInput = false;
        }

        velocity.y += gravity * Time.deltaTime;
        characterController.Move(velocity * Time.deltaTime);
        if ((characterController.collisionFlags & CollisionFlags.Above) != 0 && velocity.y > 0)
        {
            velocity.y = 0f;
        }
    }
}
