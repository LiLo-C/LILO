using Lilo.MonoBehaviours;
using UnityEngine;
using UnityEngine.UI;

namespace Lilo.MonoBehaviours.Flashlight
{
    /// <summary>Visible playtest telemetry for battery/light tuning; remove before release.</summary>
    public sealed class LightDebugHud : MonoBehaviour
    {
        [SerializeField] private Text label;
        private LightingRig _rig;

        private void Awake()
        {
            if (label == null)
                label = GetComponent<Text>();
            GameObject player = GameObject.Find("PlayerCharacter");
            if (player != null)
                _rig = player.GetComponent<LightingRig>();
        }

        private void Update()
        {
            if (label == null || _rig == null)
                return;

            float percent = _rig.ChargeFraction * 100f;
            label.text = $"BATTERY {percent:0}%\nLIGHT {_rig.CurrentState}\nINTENSITY {_rig.CurrentIntensity:0.00}\nRADIUS {_rig.DisplayedRadius:0.0}";
        }
    }
}
