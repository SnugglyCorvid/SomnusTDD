using UnityEngine;

public enum InteractionType
{
    ClickOnly,
    HoldOnly,
    Both,
    None
}

public interface IInteractable
{
    InteractionType interactionType { get; }
    void OnInteract();
    void OnFocus();
    void OnUnFocus();
    void OnHold(Vector3 holdPoint, Vector3 playerForward, Vector2 mousePosition);
    void OnRelease();
}
