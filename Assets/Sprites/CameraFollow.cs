using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    [SerializeField] private Transform target;
    [SerializeField] private float smoothTime = 0.2f;

    private Vector3 currentVelocity = Vector3.zero;
    private Vector3 offset;
    private Vector3 _followPosition;
    private Lilo.UI.ChaseScreenEffects _chaseScreenEffects;

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
        _followPosition = transform.position;
        offset = transform.position - target.position;
        _chaseScreenEffects = FindAnyObjectByType<Lilo.UI.ChaseScreenEffects>();
    }

    private void LateUpdate()
    {
        if (target == null) return;

        Vector3 targetPosition = target.position + offset;

        _followPosition = Vector3.SmoothDamp(
            _followPosition,
            targetPosition,
            ref currentVelocity,
            smoothTime
        );
        if (_chaseScreenEffects == null)
            _chaseScreenEffects = FindAnyObjectByType<Lilo.UI.ChaseScreenEffects>();
        transform.position = _followPosition
            + (_chaseScreenEffects != null ? _chaseScreenEffects.CameraPositionOffset : Vector3.zero);
    }
}
