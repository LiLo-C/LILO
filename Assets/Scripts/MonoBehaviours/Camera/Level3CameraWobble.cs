using UnityEngine;
using UnityEngine.InputSystem;

namespace Lilo.MonoBehaviours.Camera
{
    /// <summary>Adds inverse phone tilt and a slow idle wobble to OfficeLevel3's camera rotation.</summary>
    [DisallowMultipleComponent]
    [DefaultExecutionOrder(500)]
    [AddComponentMenu("LILO/Camera/Level 3 Camera Wobble")]
    public sealed class Level3CameraWobble : MonoBehaviour
    {
        [Header("Phone tilt")]
        [SerializeField] private bool inverseTilt = true;
        [SerializeField, Range(0f, 1f)] private float tiltSensitivity = 0.8f;
        [SerializeField, Range(0f, 15f)] private float maximumTiltDegrees = 9f;
        [SerializeField, Range(0f, 5f)] private float tiltDeadZoneDegrees = 0.6f;

        [Header("Continuous wobble")]
        [SerializeField, Range(0f, 20f)] private float rollWobbleDegrees = 1.5f;
        [SerializeField, Range(0f, 20f)] private float pitchWobbleDegrees = 1.5f;
        [SerializeField, Range(0f, 1f)] private float wobbleIntensity = 0.5f;
        [SerializeField, Min(0.01f)] private float wobbleFrequencyHz = 1.2f;
        [SerializeField, Min(0f)] private float rotationSmoothing = 8f;

        private Quaternion _baseRotation;
        private Quaternion _neutralAttitude;
        private AttitudeSensor _sensor;
        private ScreenOrientation _screenOrientation;
        private float _startTime;
        private bool _hasNeutralAttitude;
        private bool _enabledSensor;

        private void OnEnable()
        {
            _baseRotation = transform.localRotation;
            _screenOrientation = Screen.orientation;
            _startTime = Time.time;
            _hasNeutralAttitude = false;
            ConnectSensor();
        }

        private void LateUpdate()
        {
            ConnectSensor();
            if (_screenOrientation != Screen.orientation)
            {
                _screenOrientation = Screen.orientation;
                Recenter();
            }

            float inverseRoll = ReadInverseTilt();
            float elapsed = Time.time - _startTime;
            float phase = elapsed * (2f * Mathf.PI * wobbleFrequencyHz);
            float fadeIn = Mathf.Clamp01(elapsed);
            float roll = rollWobbleDegrees * wobbleIntensity * fadeIn *
                (0.65f * Mathf.Sin(phase) + 0.35f * Mathf.Sin(phase * 1.71f + 1f));
            float pitch = pitchWobbleDegrees * wobbleIntensity * fadeIn *
                (0.7f * Mathf.Sin(phase * 0.79f + 1.2f) + 0.3f * Mathf.Sin(phase * 1.33f));

            Quaternion target = _baseRotation * Quaternion.Euler(pitch, 0f, inverseRoll + roll);
            float blend = 1f - Mathf.Exp(-rotationSmoothing * Time.deltaTime);
            transform.localRotation = Quaternion.Slerp(transform.localRotation, target, blend);
        }

        [ContextMenu("Recenter Phone Tilt")]
        public void Recenter()
        {
            _hasNeutralAttitude = false;
        }

        private void ConnectSensor()
        {
            AttitudeSensor available = AttitudeSensor.current ?? InputSystem.GetDevice<AttitudeSensor>();
            if (available == _sensor)
                return;

            ReleaseSensor();
            _sensor = available;
            _hasNeutralAttitude = false;
            if (_sensor != null && !_sensor.enabled)
            {
                InputSystem.EnableDevice(_sensor);
                _enabledSensor = true;
            }
        }

        private float ReadInverseTilt()
        {
            if (_sensor == null || !_sensor.enabled || _sensor.lastUpdateTime <= 0)
                return 0f;

            Quaternion attitude = _sensor.attitude.ReadValue();
            if (attitude.x * attitude.x + attitude.y * attitude.y +
                attitude.z * attitude.z + attitude.w * attitude.w < 0.5f)
                return 0f;

            if (!_hasNeutralAttitude)
            {
                _neutralAttitude = attitude;
                _hasNeutralAttitude = true;
                return 0f;
            }

            Quaternion relative = Quaternion.Inverse(_neutralAttitude) * attitude;
            float phoneRoll = Mathf.DeltaAngle(0f, relative.eulerAngles.z);
            float outsideDeadZone = Mathf.Sign(phoneRoll) *
                Mathf.Max(0f, Mathf.Abs(phoneRoll) - tiltDeadZoneDegrees);
            float cameraRoll = Mathf.Clamp(outsideDeadZone * tiltSensitivity,
                -maximumTiltDegrees, maximumTiltDegrees);
            // Unity's attitude sensor roll sign is opposite the camera's visible screen-tilt sign.
            // Flip it here so a rightward phone tilt produces the requested leftward camera tilt.
            return inverseTilt ? cameraRoll : -cameraRoll;
        }

        private void OnDisable()
        {
            transform.localRotation = _baseRotation;
            ReleaseSensor();
        }

        private void ReleaseSensor()
        {
            if (_sensor != null && _enabledSensor && _sensor.added)
                InputSystem.DisableDevice(_sensor);
            _sensor = null;
            _enabledSensor = false;
        }
    }
}
