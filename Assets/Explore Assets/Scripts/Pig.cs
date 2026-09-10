using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class Pig : MonoBehaviour
{
    [Header("Death Thresholds")]
    [Tooltip("Impact speed from a bird collision needed to kill this pig.")]
    public float birdHitThreshold = 3f;

    [Tooltip("Impact speed from ANY collision for hitting the ground after falling off a building) needed to kill this pig.")]
    public float fallDamageThreshold = 6f;

    public GameObject deathEffectPrefab;

    bool isDead;

    void OnCollisionEnter(Collision collision)
    {
        if (isDead) return;

        float impactSpeed = collision.relativeVelocity.magnitude;
        bool hitByBird = collision.gameObject.CompareTag("Bird");

        if (hitByBird && impactSpeed >= birdHitThreshold)
        {
            Debug.Log($"[Pig] {name} killed by bird hit at {impactSpeed:F2}m/s");
            Die();
        }
        else if (impactSpeed >= fallDamageThreshold)
        {
            Debug.Log($"[Pig] {name} killed by fall/impact at {impactSpeed:F2}m/s");
            Die();
        }
    }

    void Die()
    {
        if (isDead) return;
        isDead = true;

        if (deathEffectPrefab != null)
            Instantiate(deathEffectPrefab, transform.position, Quaternion.identity);

        if (GameManager.Instance != null)
            GameManager.Instance.OnPigDied();

        Destroy(gameObject);
    }
}