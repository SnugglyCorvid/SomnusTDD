using UnityEngine;

public class Door : MonoBehaviour, IInteractable
{
    public InteractionType interactionType => InteractionType.HoldOnly;

    public float sensitivity = 0.75f;

    private MouseLook mouseLook;

    private bool isHeld = false;
    private Vector2 lastMousePosition;
    private Rigidbody rb;
    private HingeJoint hinge;
    private float holdSide = 1f;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        hinge = GetComponent<HingeJoint>();
        mouseLook = FindFirstObjectByType<MouseLook>();
    }

    public void OnFocus()
    {
        var interactor = FindFirstObjectByType<PlayerInteractor>();
        if (interactor != null)
        {
            interactor.interactPrompt.SetActive(true);
            interactor.interactText.text = "A door";
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

    public void OnHold(Vector3 holdPoint, Vector3 playerForward, Vector2 mousePosition)
    {
        if (!isHeld)
        {
            isHeld = true;
            lastMousePosition = mousePosition;
            holdSide = Mathf.Sign(Vector3.Dot(playerForward, transform.forward)); // Calculate once to determine the side of the door we're on
            if (mouseLook != null)
                mouseLook.enabled = false;
        }
        else
        {
            Vector2 mouseDelta = mousePosition - lastMousePosition;
            lastMousePosition = mousePosition;

            float torqueAmount = mouseDelta.y * sensitivity * holdSide;
            rb.AddTorque(transform.up * torqueAmount, ForceMode.Force);
        }
    }

    public void OnRelease()
    {
        isHeld = false;
        rb.angularVelocity = Vector3.zero;
        if (mouseLook != null)
            mouseLook.enabled = true;
    }

    public void OnInteract()
    {
        return;
    }
}
