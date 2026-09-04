using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Interactables;

[RequireComponent(typeof(XRGrabInteractable))]
public class SlingshotPouch : MonoBehaviour
{
    [SerializeField] private Transform anchor;
    [SerializeField] private float snapBackSpeed = 15f;

    private void LateUpdate()
    {
        transform.position = Vector3.Lerp(transform.position,
                                          anchor.position,
                                          Time.deltaTime * snapBackSpeed);
    }
}
