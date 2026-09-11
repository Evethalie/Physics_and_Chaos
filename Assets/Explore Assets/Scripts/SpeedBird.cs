using UnityEngine;

public class SpeedBird : Bird
{
    [SerializeField] private float dashSpeed = 30f;
    [SerializeField] private GameObject dashEffect;

    protected override void Ability()
    {
        // Keep the direction we're already flying, replace the speed.
        body.linearVelocity = body.linearVelocity.normalized * dashSpeed;

        if (dashEffect != null)
        {
            Instantiate(dashEffect, transform.position, Quaternion.identity);
        }
    }
}