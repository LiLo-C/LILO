using System;
using System.Linq;
using UnityEngine;

namespace Lilo.Config
{
    /// <summary>Fails loudly on a missing/invalid GameConfig — never invents a silent default.</summary>
    public static class GameConfigLoader
    {
        public static GameConfig LoadAndValidate(GameConfig config)
        {
            if (config == null)
                throw new InvalidOperationException("GameConfig reference is null — cannot start without a valid config asset.");

            var failures = config.Validate();
            if (failures.Count > 0)
            {
                string message = "GameConfig failed validation:\n" + string.Join("\n", failures.Select(f => f.ToString()));
                throw new InvalidOperationException(message);
            }

            return config;
        }
    }
}
