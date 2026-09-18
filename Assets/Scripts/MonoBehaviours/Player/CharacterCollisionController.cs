using System.Collections.Generic;
using Lilo.Config;
using Lilo.Systems.Collision;
using UnityEngine;

namespace Lilo.MonoBehaviours.Player
{
    /// <summary>Applies pure collision resolution after PlayerMovementController's tentative step.</summary>
    [DefaultExecutionOrder(100)]
    public sealed class CharacterCollisionController : MonoBehaviour
    {
        [SerializeField] private GameConfig config;
        [SerializeField] private List<Obstacle> obstacles = new List<Obstacle>();

        private void LateUpdate()
        {
            if (config == null)
                return;

            Vector3 current = transform.position;
            CollisionResult result = CollisionResolver.ResolveMany(
                new Vector2(current.x, current.z), config.playerRadius, obstacles);

            if (!result.DidResolve)
                return;

            transform.position = new Vector3(result.CorrectedPosition.x, current.y, result.CorrectedPosition.y);
        }
    }
}
