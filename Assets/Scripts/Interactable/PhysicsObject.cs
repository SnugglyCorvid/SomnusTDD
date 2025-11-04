using UnityEngine;

public class PhysicsObject : MonoBehaviour, IInteractable
{
    public InteractionType interactionType => InteractionType.HoldOnly;

    public float holdDistance = 0.5f;
    public float moveSpeed = 7.5f;
    public float rotateSpeed = 2f;
    public Camera playerCamera;

    public string itemArticle = "An";
    public string itemName = "UNDEFINED PHYSICSOBJECT NAME! REPORT THIS AS A BUG!";

    public float maxHoldDistance = 0.125f;

    private Rigidbody rb;
    private bool isHeld = false;
    public bool IsHeld => isHeld;
    private Transform holdPoint;
    private Vector3 localGrabOffset;
    private Vector3 desiredGrabPointWorld;

    public void SetGrabbedOffset(Vector3 worldPoint)
    {
        localGrabOffset = transform.InverseTransformPoint(worldPoint);
    }

    private void DropObject()
    {
        isHeld = false;
        rb.useGravity = true;
        rb.isKinematic = false;
        rb.linearVelocity = Vector3.zero;
    }

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        rb.interpolation = RigidbodyInterpolation.Interpolate;
        playerCamera = Camera.main;
    }

    public void OnInteract()
    {
        if (!isHeld)
        {
            holdPoint = playerCamera.transform;
            rb.useGravity = false;
            rb.isKinematic = false;
            isHeld = true;
        }
        else
        {
            DropObject();
        }
    }

    public void OnHold(Vector3 holdPoint, Vector3 playerForward, Vector2 mousePosition)
    {
        desiredGrabPointWorld = holdPoint;
        if (!isHeld)
        {
            rb.useGravity = false;
            rb.isKinematic = false;
            isHeld = true;
        }
    }

    public void OnRelease()
    {
        if (isHeld)
        {
            DropObject();
        }
    }

    void FixedUpdate()
    {
        if (isHeld)
        {
            Vector3 playerPos = playerCamera.transform.position;
            Vector3 desiredGrabPoint = playerPos + playerCamera.transform.forward * holdDistance;
            Vector3 currentGrabPointWorld = transform.TransformPoint(localGrabOffset);

            float grabDistance = Vector3.Distance(playerPos, currentGrabPointWorld);
            if (grabDistance > holdDistance * 2.0f)
            {
                DropObject();
                return;
            }

            float springStrength = 800f / Mathf.Max(rb.mass, 1f);
            float springDamping = 80f;
            Vector3 force = (desiredGrabPoint - currentGrabPointWorld) * springStrength - rb.linearVelocity * springDamping;
            rb.AddForce(force, ForceMode.Force);
        }
    }

    public void OnFocus()
    {
        Debug.Log("Looking at physics object");
        var interactor = FindFirstObjectByType<PlayerInteractor>();
        if (interactor != null)
        {
            interactor.interactPrompt.SetActive(true);
            interactor.interactText.text = $"{itemArticle} {itemName}";
        }
    }

    public void OnUnFocus()
    {
        var interactor = FindFirstObjectByType<PlayerInteractor>();
        if (interactor != null)
        {
            interactor.interactPrompt.SetActive(false);
        }
    }
}
