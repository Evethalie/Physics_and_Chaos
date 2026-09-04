using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Interactables;

[RequireComponent(typeof(XRGrabInteractable))]
public class SlingshotPouch : MonoBehaviour
{
    [SerializeField] private Transform anchor;
    [SerializeField] private float snapBackSpeed = 15f;
    private XRGrabInteractable grab;
    private bool isHeld;
    [SerializeField] private float maxStretch = 0.45f;
    [SerializeField] private Transform ammoSpawn;
    [SerializeField] private GameObject ammoPrefab;
    [SerializeField] private float launchPower = 15f;
    [SerializeField] private float reloadDelay = 1.5f;

    private GameObject loadedAmmo;

    [SerializeField] private Transform postLeft;
    [SerializeField] private Transform postRight;
    [SerializeField] private LineRenderer bandLeft;
    [SerializeField] private LineRenderer bandRight;


    private void Awake()
    {
        grab = GetComponent<XRGrabInteractable>();
        grab.selectEntered.AddListener(OnGrabbed);
        grab.selectExited.AddListener(OnReleased);
    }

    private void Start()
    {
        Reload();
    }

    private void Reload()
    {
        loadedAmmo = Instantiate(ammoPrefab, ammoSpawn.position, Quaternion.identity);

        Rigidbody body = loadedAmmo.GetComponent<Rigidbody>();
        body.isKinematic = true;
        body.useGravity = false;
    }


    private void OnGrabbed(SelectEnterEventArgs args)
    {
        isHeld = true;
    }

    private void OnReleased(SelectExitEventArgs args)
    {
        {
            isHeld = false;

            if (loadedAmmo == null) { return; }

            Vector3 pull = anchor.position - transform.position;

            Rigidbody body = loadedAmmo.GetComponent<Rigidbody>();
            body.isKinematic = false;
            body.useGravity = true;
            body.linearVelocity = pull * launchPower;

            loadedAmmo = null;
            Invoke(nameof(Reload), reloadDelay);
        }

    }


    private void LateUpdate()
    {
        if (isHeld)
        {
            Vector3 offset = transform.position - anchor.position;

            if (offset.magnitude > maxStretch)
            {
                transform.position = anchor.position
                                   + offset.normalized * maxStretch;
            }
        }
        else
        {
            transform.position = Vector3.Lerp(transform.position,
                                              anchor.position,
                                              Time.deltaTime * snapBackSpeed);
        }
        if (loadedAmmo != null)
        {
            loadedAmmo.transform.position = transform.position;
        }
        DrawBands();

    }

    private void DrawBands()
    {
        bandLeft.SetPosition(0, postLeft.position);
        bandLeft.SetPosition(1, transform.position);

        bandRight.SetPosition(0, postRight.position);
        bandRight.SetPosition(1, transform.position);
    }


}
