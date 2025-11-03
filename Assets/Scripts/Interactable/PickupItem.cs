using NUnit.Framework.Interfaces;
using UnityEngine;

public class PickupItem : MonoBehaviour, IInteractable
{
    public InventoryItem itemData;
    public string itemArticle = "A";
    private bool pickedUp = false;

    public Material outlineMaterial;
    private Material originalMaterial;
    private Renderer itemRenderer;

    public InteractionType interactionType => InteractionType.ClickOnly;

    void Start()
    {
        itemRenderer = GetComponent<Renderer>();
        if (itemRenderer != null)
            originalMaterial = itemRenderer.material;
    }

    public void OnInteract()
    {
        if (pickedUp) return;
        pickedUp = true;

        var player = FindFirstObjectByType<Player>();
        var inventory = player.GetComponent<Inventory>();
        if (inventory != null && itemData != null)
        {
            if (inventory.AddItem(itemData))
            {
                Destroy(gameObject);
                Debug.Log($"{itemArticle} {itemData.itemName} picked up.");
            }
            else
            {
                Debug.Log("Cannot pick up item: Inventory is full.");
                pickedUp = false;
            }
        }
    }

    public void OnFocus()
    {
        var interactor = FindFirstObjectByType<PlayerInteractor>();
        if (interactor != null)
        {
            interactor.interactPrompt.SetActive(true);
            interactor.interactText.text = $"{itemArticle} {itemData.itemName}";
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

    public void OnHold(Vector3 holdPoint, Vector3 playerForward, Vector2 mousePosition) { }
    public void OnRelease() { }
}
