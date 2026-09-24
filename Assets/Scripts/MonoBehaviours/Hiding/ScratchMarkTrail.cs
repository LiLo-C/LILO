using System.Collections.Generic;
using Lilo.MonoBehaviours.Player;
using StarterAssets;
using UnityEngine;

namespace Lilo.MonoBehaviours.Hiding
{
    /// <summary>Short lived hidden scratch marks used by monster tracking while sprinting.</summary>
    public sealed class ScratchMarkTrail : MonoBehaviour
    {
        private const float Lifetime = 10f;
        private const float StepDistance = 0.75f;
        private const float VisibleToMonsterRange = 8f;

        private sealed class Mark
        {
            public Vector3 Position;
            public float Created;
        }

        private static ScratchMarkTrail _active;
        private readonly List<Mark> _marks = new List<Mark>();
        private StarterAssetsInputs _input;
        private PlayerMovementController _movement;
        private Vector3 _lastPosition;
        private Vector3 _lastMarkPosition;
        private bool _hasLastMark;

        private void Awake()
        {
            _active = this;
            _input = GetComponent<StarterAssetsInputs>();
            _movement = GetComponent<PlayerMovementController>();
            _lastPosition = transform.position;
        }

        private void Update()
        {
            for (int i = _marks.Count - 1; i >= 0; i--)
            {
                Mark mark = _marks[i];
                float age = Time.time - mark.Created;
                if (age >= Lifetime)
                {
                    _marks.RemoveAt(i);
                }
            }

            Vector3 position = transform.position;
            Vector3 motion = position - _lastPosition;
            motion.y = 0f;
            _lastPosition = position;
            bool sprinting = (_input != null && _input.sprint)
                || (_movement != null && _movement.IsSprinting);
            if (HidingController.IsPlayerMovementLocked || !sprinting || motion.sqrMagnitude < 0.0001f)
            {
                _hasLastMark = false;
                return;
            }

            if (_hasLastMark && Vector3.Distance(position, _lastMarkPosition) < StepDistance)
                return;

            _lastMarkPosition = position;
            _hasLastMark = true;
            AddMark(position);
        }

        private void AddMark(Vector3 position)
        {
            float groundY = position.y - 0.6f;
            if (Physics.Raycast(position + Vector3.up * 0.1f, Vector3.down,
                    out RaycastHit ground, 2f, Physics.DefaultRaycastLayers,
                    QueryTriggerInteraction.Ignore))
                groundY = ground.point.y;
            var mark = new Mark
            {
                Position = new Vector3(position.x, groundY + 0.04f, position.z),
                Created = Time.time,
            };
            _marks.Add(mark);
        }

        public static bool TryGetLatestNear(Vector3 monsterPosition, out Vector3 markPosition)
        {
            markPosition = default;
            if (_active == null)
                return false;

            for (int i = _active._marks.Count - 1; i >= 0; i--)
            {
                Mark mark = _active._marks[i];
                if (Time.time - mark.Created >= Lifetime)
                    continue;
                Vector3 delta = mark.Position - monsterPosition;
                if (new Vector2(delta.x, delta.z).magnitude > VisibleToMonsterRange)
                    continue;
                if (Physics.Linecast(monsterPosition + Vector3.up * 1.3f,
                        mark.Position + Vector3.up * 0.5f,
                        Physics.DefaultRaycastLayers, QueryTriggerInteraction.Ignore))
                    continue;
                markPosition = mark.Position;
                return true;
            }
            return false;
        }

        private void OnDestroy()
        {
            if (_active == this)
                _active = null;
        }
    }
}
