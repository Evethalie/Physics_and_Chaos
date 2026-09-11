using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Interactables;
using UnityEngine.XR.Interaction.Toolkit.Interactors;

[RequireComponent(typeof(Rigidbody))]
[RequireComponent(typeof(XRGrabInteractable))]
public class SlingshotBand : MonoBehaviour
{
    [Header("Setup")]
    [Tooltip("The BirdSocket child. Drop a bird in here to load it.")]
    [SerializeField] private XRSocketInteractor birdSocket;

    [Tooltip("Line Renderer that draws the predicted arc while you pull.")]
    [SerializeField] private LineRenderer trajectoryLine;

    [Tooltip("Its blue Z arrow is treated as downrange.")]
    [SerializeField] private Transform aimDirection;

    [Header("Tuning")]
    [SerializeField] private float snapBackSpeed = 15f;
    [SerializeField] private float maxStretch = 0.4f;
    [SerializeField] private float launchPower = 15f;

    [Header("Trajectory")]
    [SerializeField] private int trajectoryPoints = 30;
    [SerializeField] private float trajectoryTimeStep = 0.05f;

    private XRGrabInteractable grab;
    private Vector3 restPosition;
    private bool isHeld;
    private Collider launchedBirdCollider;

    private void Awake()
    {
        restPosition = transform.position;

        Rigidbody body = GetComponent<Rigidbody>();
        body.isKinematic = true;

        grab = GetComponent<XRGrabInteractable>();
        grab.trackRotation = false;
        grab.throwOnDetach = false;

        grab.selectEntered.AddListener(OnGrabbed);
        grab.selectExited.AddListener(OnReleased);

        if (trajectoryLine != null)
        {
            trajectoryLine.enabled = false;
        }
    }

    private void OnGrabbed(SelectEnterEventArgs args)
    {
        isHeld = true;
    }

    private void OnReleased(SelectExitEventArgs args)
    {
        isHeld = false;

        Vector3 launchVelocity = -GetPullOffset() * launchPower;

        if (trajectoryLine != null)
        {
            trajectoryLine.enabled = false;
        }

        LaunchBird(launchVelocity);
    }

    private void LateUpdate()
    {
        if (isHeld)
        {
            Vector3 offset = GetPullOffset();
            transform.position = restPosition + offset;

            DrawTrajectory(-offset * launchPower);
        }
        else
        {
            transform.position = Vector3.Lerp(transform.position,
                                              restPosition,
                                              Time.deltaTime * snapBackSpeed);
        }
    }

    // One place that works out how far the band has been pulled, so the
    // preview and the real shot can never disagree.
    private Vector3 GetPullOffset()
    {
        Vector3 offset = transform.position - restPosition;

        if (aimDirection != null)
        {
            float forwardAmount = Vector3.Dot(offset, aimDirection.forward);

            if (forwardAmount > 0f)
            {
                offset -= aimDirection.forward * forwardAmount;
            }
        }

        return Vector3.ClampMagnitude(offset, maxStretch);
    }

    private void LaunchBird(Vector3 velocity)
    {
        if (birdSocket == null || birdSocket.hasSelection == false)
        {
            Debug.Log("Released with no bird loaded.");
            return;
        }

        IXRSelectInteractable bird = birdSocket.interactablesSelected[0];

        Rigidbody birdBody = bird.transform.GetComponent<Rigidbody>();
        if (birdBody == null) { return; }

        // Turn the socket off BEFORE dropping the bird, or it grabs it
        // straight back while it's still inside the trigger volume.
        birdSocket.socketActive = false;
        birdSocket.interactionManager.SelectExit(birdSocket, bird);

        birdBody.isKinematic = false;
        birdBody.linearVelocity = velocity;

        Bird birdScript = bird.transform.GetComponent<Bird>();
        if (birdScript != null)
        {
            birdScript.Launched();
        }

        if (GameManager.Instance != null)
        {
            GameManager.Instance.OnBirdLaunched();
        }

        launchedBirdCollider = bird.transform.GetComponent<Collider>();
        if (launchedBirdCollider != null)
        {
            launchedBirdCollider.enabled = false;
        }

        Invoke(nameof(EnableBirdCollider), 0.3f);
        Invoke(nameof(ReactivateSocket), 0.5f);
    }

    private void EnableBirdCollider()
    {
        if (launchedBirdCollider != null)
        {
            launchedBirdCollider.enabled = true;
        }
    }

    private void ReactivateSocket()
    {
        if (birdSocket != null)
        {
            birdSocket.socketActive = true;
        }
    }

    private void DrawTrajectory(Vector3 startVelocity)
    {
        if (trajectoryLine == null) { return; }

        Vector3 startPosition = transform.position;

        if (birdSocket != null)
        {
            startPosition = birdSocket.transform.position;
        }

        trajectoryLine.enabled = true;
        trajectoryLine.positionCount = trajectoryPoints;

        for (int i = 0; i < trajectoryPoints; i++)
        {
            float t = i * trajectoryTimeStep;

            Vector3 point = startPosition
                          + startVelocity * t
                          + 0.5f * Physics.gravity * t * t;

            trajectoryLine.SetPosition(i, point);
        }
    }
}