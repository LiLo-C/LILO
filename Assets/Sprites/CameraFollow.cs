using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    [SerializeField] private Transform target;
    [SerializeField] private float smoothTime = 0.2f;

    private Vector3 offset;
    private Vector3 currentVelocity = Vector3.zero;

    private void Start()
    {
        if (target != null)
        {
            // Stores the distance set up in the Scene view
            offset = transform.position - target.position;
        }
    }

    private void LateUpdate()
    {
        if (target == null) return;

        Vector3 targetPosition = target.position + offset;
        transform.position = Vector3.SmoothDamp(transform.position, targetPosition, ref currentVelocity, smoothTime);
    }
}