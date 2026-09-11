using UnityEngine;
using UnityEngine.InputSystem;

public class BirdAbilityInput : MonoBehaviour
{
    [Tooltip("Drag in an Activate action from XRI Default Input Actions.")]
    [SerializeField] private InputActionReference activateAction;

    private void OnEnable()
    {
        if (activateAction == null) { return; }

        activateAction.action.performed += OnPressed;
        activateAction.action.Enable();
    }

    private void OnDisable()
    {
        if (activateAction == null) { return; }

        activateAction.action.performed -= OnPressed;
    }

    private void OnPressed(InputAction.CallbackContext context)
    {
        if (Bird.ActiveBird != null)
        {
            Bird.ActiveBird.UseAbility();
        }
    }
}