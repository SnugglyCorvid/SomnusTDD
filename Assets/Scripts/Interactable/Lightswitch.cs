using UnityEngine;

public class LightSwitch : MonoBehaviour, IInteractable
{
    public InteractionType interactionType => InteractionType.ClickOnly;

    public Light targetLight;
    private bool startOn = true;

    void Start()
    {
        if (targetLight != null)
        {
            targetLight.enabled = startOn;
        }
    }

    public void OnInteract()
    {
        Debug.Log("Toggling lightswitch");
        if (targetLight != null)
        {
            targetLight.enabled = !targetLight.enabled;
        }
    }

    public void OnFocus()
    {
        Debug.Log("Looking at lightswitch");
        var interactor = FindFirstObjectByType<PlayerInteractor>();
        if (interactor != null)
        {
            interactor.interactPrompt.SetActive(true);
            interactor.interactText.text = "A light switch";
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
        return;
    }

    public void OnRelease()
    {
        return;
    }
}
