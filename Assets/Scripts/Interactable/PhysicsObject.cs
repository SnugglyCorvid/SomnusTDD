using UnityEngine;

public class PhysicsObject : MonoBehaviour, IInteractable
{
    public InteractionType interactionType => InteractionType.HoldOnly;

    public float holdDistance = 2f;
    public float moveSpeed = 7.5f;
    public float rotateSpeed = 2f;
    public Camera playerCamera;

    public string itemArticle = "An";
    public string itemName = "UNDEFINED PHYSICSOBJECT NAME! REPORT THIS AS A BUG!";
    public float maxHoldDistance = 0.125f;

    private Rigidbody rb;
    private bool isHeld = false;
    private Transform holdPoint;

    private void DropObject()
    {
        isHeld = false;
        rb.useGravity = true;
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
            isHeld = true;
        }
        else
        {
            DropObject();
        }
    }

    public void OnHold(Vector3 holdPosition, Vector3 playerForward, Vector2 mousePosition)
    {
        if (!isHeld)
        {
            holdPoint = playerCamera.transform;
            rb.useGravity = false;
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
            Vector3 targetPos = holdPoint.position + holdPoint.forward * holdDistance;
            Vector3 moveDir = targetPos - transform.position;

            float effectiveSpeed = moveSpeed / Mathf.Max(rb.mass, 1f);

            // Drop if too far from player
            float currentDistance = Vector3.Distance(transform.position, holdPoint.position);
            if (currentDistance > maxHoldDistance)
            {
                DropObject();
                return;
            }

            rb.linearVelocity = moveDir * effectiveSpeed;
            rb.rotation = Quaternion.Euler(0f, rb.rotation.eulerAngles.y, 0f);
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
