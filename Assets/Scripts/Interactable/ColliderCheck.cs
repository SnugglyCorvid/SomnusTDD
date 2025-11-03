using UnityEngine;

[RequireComponent(typeof(Collider))]
public class DoorCollisionIgnore : MonoBehaviour
{
    public Collider[] collidersToIgnore;

    void Start()
    {
        Collider doorCollider = GetComponent<Collider>();
        foreach (var col in collidersToIgnore)
        {
            if (col == null) continue;
            Physics.IgnoreCollision(doorCollider, col, true);
        }
    }
}