using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit.Interactables;


[RequireComponent(typeof(Rigidbody))]
public class ImpactEffect : MonoBehaviour
{
    [Header("When to react")]
    [Tooltip("Impact speed needed before anything happens.")]
    [SerializeField] private float minImpactSpeed = 2f;

    [Tooltip("Seconds before this object can make another effect.")]
    [SerializeField] private float cooldown = 0.25f;

    [Header("What happens")]
    [SerializeField] private GameObject effectPrefab;
    [SerializeField] private AudioClip impactClip;
    [SerializeField] private float effectLifetime = 3f;

    [Tooltip("Tick this for blocks that should break apart. Leave off for birds.")]
    [SerializeField] private bool destroyOnImpact;

    private float nextEffectTime;



    private XRGrabInteractable grab;

    private void Awake()
    {
        // Blocks don't have one of these, which is fine - grab stays null.
        grab = GetComponent<XRGrabInteractable>();
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (grab != null && grab.isSelected) { return; }
        if (Time.time < nextEffectTime) { return; }
        if (collision.gameObject.CompareTag("Slingshot")) return;
        float impactSpeed = collision.relativeVelocity.magnitude;
        if (impactSpeed < minImpactSpeed) { return; }

        nextEffectTime = Time.time + cooldown;

        Vector3 point = collision.GetContact(0).point;

        if (effectPrefab != null)
        {
            GameObject effect = Instantiate(effectPrefab, point, Quaternion.identity);
            Destroy(effect, effectLifetime);
        }

        if (impactClip != null)
        {
            AudioSource.PlayClipAtPoint(impactClip, point);
        }

        if (destroyOnImpact)
        {
            if (GameManager.Instance != null)
            {
                GameManager.Instance.OnBlockDestroyed();
            }

            Destroy(gameObject);
        }
    }
}