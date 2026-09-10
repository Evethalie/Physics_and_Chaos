using System.Collections;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Interactables;
using UnityEngine.XR.Interaction.Toolkit.Interactors;

// Attach to SlingshotRubberBand.
// Requires: an XR Socket Interactor placed where the bird sits in the
// pouch (assign below), and a LineRenderer for the trajectory preview.
[RequireComponent(typeof(Rigidbody))]
[RequireComponent(typeof(XRGrabInteractable))]
public class SlingshotBandGrab : MonoBehaviour
{
    [Header("Bird Socket")]
    [Tooltip("XR Socket Interactor positioned in the pouch. Player drops a bird here to load it.")]
    public XRSocketInteractor birdSocket;

    [Header("Tuning")]
    public float maxStretchDistance = 0.4f;
    public float launchForceMultiplier = 15f;

    [Header("Trajectory Preview")]
    [Tooltip("LineRenderer used to draw the predicted arc while pulling.")]
    public LineRenderer trajectoryLine;
    public int trajectoryPoints = 30;
    public float trajectoryTimeStep = 0.05f;

    XRGrabInteractable grabInteractable;
    Rigidbody rb;
    Vector3 restPosition;
    Vector3 currentClampedOffset; // updated once per LateUpdate, reused at release
    bool isGrabbed;

    void Awake()
    {
        restPosition = transform.position;

        rb = GetComponent<Rigidbody>();
        rb.isKinematic = true;

        grabInteractable = GetComponent<XRGrabInteractable>();
        grabInteractable.throwOnDetach = false;
        grabInteractable.trackPosition = true;
        grabInteractable.trackRotation = false;

        grabInteractable.selectEntered.AddListener(OnGrab);
        grabInteractable.selectExited.AddListener(OnRelease);

        if (trajectoryLine != null)
            trajectoryLine.enabled = false;

        if (birdSocket == null)
            Debug.LogWarning("[SlingshotBandGrab] Bird Socket not assigned in the inspector, birds can never be launched.");
        else
        {
            birdSocket.selectEntered.AddListener(OnBirdSocketed);
            birdSocket.selectExited.AddListener(OnBirdUnsocketed);
        }

        Debug.Log($"[SlingshotBandGrab] Ready on '{name}', rest position = {restPosition}");
        Debug.Log($"[SlingshotBandGrab] Physics.gravity = {Physics.gravity} (should be roughly (0, -9.81, 0))");
    }

    void OnBirdSocketed(SelectEnterEventArgs args)
    {
        Debug.Log($"[SlingshotBandGrab] Bird loaded: {args.interactableObject.transform.name}");
    }

    void OnBirdUnsocketed(SelectExitEventArgs args)
    {
        Debug.Log($"[SlingshotBandGrab] Bird left socket: {args.interactableObject.transform.name}");
    }

    void OnGrab(SelectEnterEventArgs args)
    {
        // Always allow the pull/snap-back. Whether a bird actually launches
        // is decided later, in LaunchBird - gating this flag caused the band
        // to get dragged by the hand but never reset on release.
        isGrabbed = true;

        bool hasBird = birdSocket != null && birdSocket.hasSelection;
        Debug.Log($"[SlingshotBandGrab] GRABBED by {args.interactorObject}, bird loaded = {hasBird}");
    }

    void LateUpdate()
    {
        if (!isGrabbed) return;

        Vector3 offset = transform.position - restPosition;
        offset = Vector3.ClampMagnitude(offset, maxStretchDistance);
        transform.position = restPosition + offset;
        currentClampedOffset = offset;

        Vector3 launchVelocity = ComputeLaunchVelocity();

        // Draw from wherever the bird actually sits (the socket), not the
        // band's own pivot - they can be offset, which was throwing the
        // preview off from the real flight path.
        Vector3 trajectoryStart = birdSocket != null ? birdSocket.transform.position : transform.position;
        DrawTrajectory(trajectoryStart, launchVelocity);
    }

    void OnRelease(SelectExitEventArgs args)
    {
        isGrabbed = false;

        Vector3 launchVelocity = ComputeLaunchVelocity();

        if (trajectoryLine != null)
            trajectoryLine.enabled = false;

        Debug.Log($"[SlingshotBandGrab] RELEASED, launchVelocity={launchVelocity.magnitude:F2}m/s");

        LaunchBird(launchVelocity);

        transform.position = restPosition;
    }

    Vector3 ComputeLaunchVelocity()
    {
        // Use the cached, already-clamped offset from the last LateUpdate
        // instead of re-reading transform.position here. OnRelease fires
        // from Unity's Update phase, which runs BEFORE this frame's
        // LateUpdate - so a fresh read here could catch a raw, un-clamped,
        // mid-jerk position and produce a launch velocity far above the
        // configured max (confirmed from logs: releases up to 16.6m/s
        // despite a 6m/s theoretical cap from maxStretchDistance x
        // launchForceMultiplier). Reusing the cached value fixes both the
        // magnitude overshoot and the inconsistent direction.
        return -currentClampedOffset * launchForceMultiplier;
    }

    void LaunchBird(Vector3 velocity)
    {
        if (birdSocket == null || !birdSocket.hasSelection)
        {
            Debug.Log("[SlingshotBandGrab] Released with no bird loaded, nothing to launch.");
            return;
        }

        var bird = birdSocket.interactablesSelected[0] as XRGrabInteractable;
        if (bird == null)
        {
            Debug.LogWarning("[SlingshotBandGrab] Socketed interactable isn't an XRGrabInteractable, can't launch it.");
            return;
        }

        Debug.Log($"[SlingshotBandGrab] Launching {bird.name} at {velocity.magnitude:F2}m/s");

        Rigidbody birdRb = bird.GetComponent<Rigidbody>();
        if (birdRb == null)
        {
            Debug.LogWarning($"[SlingshotBandGrab] {bird.name} has no Rigidbody, can't apply launch velocity.");
            return;
        }

        // Deactivate the socket BEFORE letting go, so it can't immediately
        // re-catch the bird while it's still sitting inside its trigger
        // volume (this was causing the bird to look stuck in the pouch).
        birdSocket.socketActive = false;

        // Force the socket to drop the bird.
        birdSocket.interactionManager.SelectExit(birdSocket, (IXRSelectInteractable)bird);

        // Apply the launch velocity immediately (Velocity Tracking movement
        // type doesn't fight this the way Kinematic did) so the real flight
        // starts from the exact instant the trajectory preview predicted,
        // instead of free-falling for a frame first.
        birdRb.isKinematic = false;
        birdRb.linearVelocity = velocity;

        // The bird launches from right next to the band's own grab collider
        // and the fork geometry. Briefly disable its collider so it can't
        // physically clip either on the way out and bounce/deflect.
        Collider birdCollider = bird.GetComponent<Collider>();
        if (birdCollider != null)
            birdCollider.enabled = false;

        StartCoroutine(PostLaunchCleanup(birdCollider));
    }

    IEnumerator PostLaunchCleanup(Collider birdCollider)
    {
        // Re-enable the bird's collider once it's had a moment to clear the
        // pouch/fork geometry.
        yield return new WaitForSeconds(0.3f);
        if (birdCollider != null)
            birdCollider.enabled = true;

        // Give the socket a bit longer before it's allowed to grab anything
        // else, so it doesn't immediately re-catch the just-launched bird.
        yield return new WaitForSeconds(0.2f);
        if (birdSocket != null)
            birdSocket.socketActive = true;
    }

    void DrawTrajectory(Vector3 startPos, Vector3 startVelocity)
    {
        if (trajectoryLine == null) return;

        trajectoryLine.enabled = true;
        trajectoryLine.positionCount = trajectoryPoints;

        Vector3 gravity = Physics.gravity;
        for (int i = 0; i < trajectoryPoints; i++)
        {
            float t = i * trajectoryTimeStep;
            Vector3 point = startPos + startVelocity * t + 0.5f * gravity * t * t;
            trajectoryLine.SetPosition(i, point);
        }
    }
}