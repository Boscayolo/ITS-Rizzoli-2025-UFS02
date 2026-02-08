using UnityEngine;

public class PickUp : MonoBehaviour
{
    [SerializeField] protected float rotationSpeed;
    protected virtual void OnTriggerEnter(Collider other)
    {
        Absorption(other);
    }

    protected virtual void Absorption(Collider other)
    {

    }

    protected void Update()
    {
        transform.Rotate(0, rotationSpeed, 0);
    }
}
