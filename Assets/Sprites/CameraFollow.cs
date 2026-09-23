using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    [SerializeField] private Transform target;
    [SerializeField] private float smoothTime = 0.2f;

    private Vector3 currentVelocity = Vector3.zero;
    private Vector3 offset;

    private void Start()
    {
        if (target == null)
        {
            GameObject player = GameObject.Find("PlayerCharacter");
            target = player != null ? player.transform : null;
        }

        if (target == null)
        {
            Debug.LogWarning("[CameraFollow] PlayerCharacter not found.", this);
            return;
        }

        // The Transform and Camera Inspector define the framing. Only follow movement.
        offset = transform.position - target.position;
    }

    private void LateUpdate()
    {
        if (target == null) return;

        Vector3 targetPosition = target.position + offset;

        transform.position = Vector3.SmoothDamp(
            transform.position,
            targetPosition,
            ref currentVelocity,
            smoothTime
        );
    }
}
