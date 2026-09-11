using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class Bird : MonoBehaviour
{
    // The bird currently in flight. Static, so anything can find it.
    public static Bird ActiveBird;

    protected Rigidbody body;

    private bool abilityUsed;

    protected virtual void Awake()
    {
        body = GetComponent<Rigidbody>();
    }

    // The slingshot calls this the moment the bird is fired.
    public void Launched()
    {
        ActiveBird = this;
        abilityUsed = false;
    }

    public void UseAbility()
    {
        if (abilityUsed) { return; }
        abilityUsed = true;

        Ability();
    }

    // The plain bird does nothing, which is right for Red.
    protected virtual void Ability()
    {
    }
}