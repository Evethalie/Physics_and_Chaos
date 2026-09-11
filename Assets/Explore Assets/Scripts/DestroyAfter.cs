using UnityEngine;

public class DestroyAfter : MonoBehaviour
{
    [SerializeField] private float seconds = 15f;

    private void Start()
    {
        Destroy(gameObject, seconds);
    }
}