using System.Collections.Generic;
using Lilo.MonoBehaviours.Player;
using StarterAssets;
using UnityEngine;

namespace Lilo.MonoBehaviours.Hiding
{
    /// <summary>Short lived red floor marks left only while sprinting.</summary>
    public sealed class ScratchMarkTrail : MonoBehaviour
    {
        private const float Lifetime = 10f;
        private const float StepDistance = 0.75f;
        private const float VisibleToMonsterRange = 8f;

        private sealed class Mark
        {
            public Vector3 Position;
            public float Created;
            public LineRenderer Line;
        }

        private static ScratchMarkTrail _active;
        private readonly List<Mark> _marks = new List<Mark>();
        private StarterAssetsInputs _input;
        private PlayerMovementController _movement;
        private Material _material;
        private Vector3 _lastPosition;
        private Vector3 _lastMarkPosition;
        private bool _hasLastMark;

        private void Awake()
        {
            _active = this;
            _input = GetComponent<StarterAssetsInputs>();
            _movement = GetComponent<PlayerMovementController>();
            _lastPosition = transform.position;
            Shader shader = Shader.Find("Sprites/Default");
            if (shader != null)
                _material = new Material(shader) { name = "SprintScratchMarkRuntime" };
        }

        private void Update()
        {
            for (int i = _marks.Count - 1; i >= 0; i--)
            {
                Mark mark = _marks[i];
                float age = Time.time - mark.Created;
                if (age >= Lifetime)
                {
                    if (mark.Line != null) Destroy(mark.Line.gameObject);
                    _marks.RemoveAt(i);
                }
                else if (mark.Line != null)
                {
                    Color color = new Color(1f, 0.08f, 0.06f, 0.8f * (1f - age / Lifetime));
                    mark.Line.startColor = color;
                    mark.Line.endColor = color;
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
            AddMark(position, motion.normalized);
        }

        private void AddMark(Vector3 position, Vector3 direction)
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
            if (_material != null)
            {
                var go = new GameObject("SprintScratchMark");
                mark.Line = go.AddComponent<LineRenderer>();
                mark.Line.sharedMaterial = _material;
                mark.Line.useWorldSpace = true;
                mark.Line.positionCount = 2;
                mark.Line.widthMultiplier = 0.075f;
                mark.Line.numCapVertices = 2;
                Vector3 across = Vector3.Cross(Vector3.up, direction) * 0.17f;
                mark.Line.SetPosition(0, mark.Position - direction * 0.18f - across);
                mark.Line.SetPosition(1, mark.Position + direction * 0.18f + across);
            }
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
            foreach (Mark mark in _marks)
                if (mark.Line != null) Destroy(mark.Line.gameObject);
            if (_material != null)
                Destroy(_material);
        }
    }
}
