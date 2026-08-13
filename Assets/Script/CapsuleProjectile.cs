using UnityEngine;

/// <summary>
/// Gere la disparition d'une capsule tiree par CapsuleWeapon.
/// </summary>
[RequireComponent(typeof(Rigidbody), typeof(Collider))]
public class CapsuleProjectile : MonoBehaviour
{
    private float lifetime = 5f;

    public void SetLifetime(float seconds)
    {
        lifetime = Mathf.Max(0.1f, seconds);
    }

    private void Start()
    {
        Destroy(gameObject, lifetime);
    }

    private void OnCollisionEnter(Collision collision)
    {
        Destroy(gameObject);
    }
}
