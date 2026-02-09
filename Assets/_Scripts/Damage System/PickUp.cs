using System;
using UnityEngine;

public class PickUp : MonoBehaviour
{
    [SerializeField] protected float rotationSpeed;

    public Action<PickUp> OnPickedUp;

    protected virtual void OnTriggerEnter(Collider other)
    {
        Absorption(other);
    }

    protected virtual void Absorption(Collider other)
    {
        OnPickedUp?.Invoke(this);
    }

    protected void Update()
    {
        transform.Rotate(0, rotationSpeed, 0);
    }
}
