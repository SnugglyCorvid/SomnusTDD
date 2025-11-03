using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;
using TMPro;

public class PlayerInteractor : MonoBehaviour
{
    public Camera playerCamera;
    public float interactDistance = 3f;
    public LayerMask interactableLayer;
    public float holdThreshold = 0f;

    private PlayerAction playerActions;
    private IInteractable currentTarget;
    private IInteractable heldTarget;
    private Transform currentTransform;

    public GameObject interactPrompt;
    public TextMeshProUGUI interactText;
    public Vector2 mousePosition;

    private bool isHolding = false;
    private bool isPressed = false;
    private float holdTimer = 0f;

    void Awake()
    {
        if (playerCamera == null)
            playerCamera = GetComponent<Camera>();

        playerActions = new PlayerAction();

        playerActions.Player.Interact.performed += ctx => OnInteractStarted(ctx);
        playerActions.Player.Interact.canceled += ctx => OnInteractCanceled(ctx);
    }

    void OnEnable()
    {
        playerActions.Player.Enable();
    }

    void OnDisable()
    {
        playerActions.Player.Disable();
    }

    void Update()
    {
        mousePosition = Mouse.current.position.ReadValue();

        Ray ray = new Ray(playerCamera.transform.position, playerCamera.transform.forward);

        if (Physics.Raycast(ray, out RaycastHit hit, interactDistance, interactableLayer))
        {
            var interactable = hit.collider.GetComponentInParent<IInteractable>();
            if (interactable != null)
            {
                if (currentTarget != interactable)
                {
                    currentTarget?.OnUnFocus();
                    currentTarget = interactable;
                    currentTarget.OnFocus();
                    currentTransform = hit.collider.transform;
                }

                if (isHolding && heldTarget == null &&(currentTarget.interactionType == InteractionType.HoldOnly || currentTarget.interactionType == InteractionType.Both))
                {
                    heldTarget = currentTarget;
                }
            }
        }
        else
        {
            if (currentTarget != null)
            {
                currentTarget.OnUnFocus();
                currentTarget = null;
                currentTransform = null;
            }
        }

        if (isPressed && !isHolding && holdThreshold > 0f)
        {
            holdTimer += Time.deltaTime;
            if (holdTimer >= holdThreshold)
            {
                isHolding = true;
                Debug.Log("Hold detected (Mouse 1 held)");
            }
        }

        if (isHolding && heldTarget != null)
        {
            heldTarget.OnHold(playerCamera.transform.position + playerCamera.transform.forward * interactDistance, playerCamera.transform.forward, mousePosition);
        }
    }

    private void OnInteractStarted(InputAction.CallbackContext context)
    {
        if (currentTarget == null) return;

        Debug.Log("Interact started (Mouse 1 down)");
        isPressed = true;
        holdTimer = 0f;
        isHolding = false;

        if (currentTarget.interactionType == InteractionType.HoldOnly || currentTarget.interactionType == InteractionType.Both)
        {
            isHolding = true;
            Debug.Log("Hold detected (Mouse 1 held) [instant]");
        }
    }

    private void OnInteractCanceled(InputAction.CallbackContext context)
    {
        if (!isHolding)
        {
            if (currentTarget != null &&
                (currentTarget.interactionType == InteractionType.ClickOnly || currentTarget.interactionType == InteractionType.Both))
            {
                Debug.Log("Click detected (Mouse 1 quick press)");
                currentTarget.OnInteract();
            }
        }
        else
        {
            if (heldTarget != null &&
                (heldTarget.interactionType == InteractionType.HoldOnly || heldTarget.interactionType == InteractionType.Both))
            {
                Debug.Log("Release detected (Mouse 1 released after hold)");
                heldTarget.OnRelease();
            }
        }
        heldTarget = null;
        ResetHold();
    }

    private void ResetHold()
    {
        isPressed = false;
        isHolding = false;
        holdTimer = 0f;
    }

    void OnDrawGizmos()
    {
        if (playerCamera == null) return;
        Gizmos.color = Color.cyan;
        Gizmos.DrawLine(playerCamera.transform.position, playerCamera.transform.position + playerCamera.transform.forward * interactDistance);
    }
}
