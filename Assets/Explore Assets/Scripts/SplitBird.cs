using UnityEngine;

public class SplitBird : Bird
{
    [Tooltip("A plain Bird prefab. Do NOT use a SplitBird here.")]
    [SerializeField] private GameObject splitPrefab;

    [SerializeField] private int splitCount = 3;
    [SerializeField] private float spreadAngle = 15f;

    protected override void Ability()
    {
        if (splitPrefab == null) { return; }

        Vector3 velocity = body.linearVelocity;

        for (int i = 0; i < splitCount; i++)
        {
            // With 3 birds at 15 degrees this gives -15, 0, +15.
            float angle = (i - (splitCount - 1) * 0.5f) * spreadAngle;

            Vector3 spread = Quaternion.AngleAxis(angle, Vector3.up) * velocity;

            GameObject copy = Instantiate(splitPrefab, transform.position, transform.rotation);
            copy.GetComponent<Rigidbody>().linearVelocity = spread;
        }

        Destroy(gameObject);
    }
}