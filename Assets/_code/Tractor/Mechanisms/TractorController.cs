using UnityEngine;

namespace Tractor
{
    public class TractorController : MonoBehaviour, ICheckable
    {
        public enum TractorBehavior { Main, Gearbox, Engine, Lights, Audio, RoadDetector, Trailer }

        [Header("Механизмы")]
        public TractorInput tractorInput;
        public TractorGearbox tractorGearbox;
        public TractorEngine tractorEngine;
        public TractorLights tractorLights;
        public TractorAudio tractorAudio;
        public RoadDetector roadDetector;
        public TractorCameraController tractorCameraController;
        public TractorTrailer tractorTrailer;

        [Header("Инфо")]
        public float speed;
        public float RPM;
        public bool isHandbrake = false;
        public float throttle;
        public float brake;
        public float clutch;

        [Header("Ручник")]
        public WheelCollider[] handbrakeWheels;   // задние колёса
        public float handbrakeTorque = 50000f;    // сила ручника в Нм

        RCCP_CarController carController;
        public RCCP_CarController CarController { get { return carController; } }

        Transform destinationTransform;
        public Transform DestinationTransform { get { return destinationTransform; } set { destinationTransform = value; } }

        GameController gameController;

        public bool HasVariable(string conditionName)
        {
            switch (conditionName)
            {
                case "speed":
                case "speedAbs":
                case "RPM":
                case "isHandbrake":
                case "handbrake":
                case "destinationDistance":
                    return true;
                default:
                    return false;
            }
        }

        public float GetVariableValue(string conditionName)
        {
            switch (conditionName)
            {
                case "speed":
                    return speed;
                case "speedAbs":
                    return Mathf.Abs(speed);
                case "RPM":
                    return RPM;
                case "isHandbrake":
                case "handbrake":
                    return isHandbrake ? 1f : 0f;
                case "destinationDistance":
                    return destinationTransform == null ? 0f : Vector3.Distance(transform.position, destinationTransform.position);
                default:
                    Debug.LogWarning($"Неизвестное условие: {conditionName}");
                    return 0f;
            }
        }

        void Start()
        {
            tractorInput.Init(this, tractorGearbox, tractorEngine, tractorLights, tractorAudio);
            tractorGearbox.Init(tractorInput);
            tractorEngine.Init(tractorInput, tractorGearbox);
            tractorLights.Init(tractorInput, tractorEngine);
            tractorAudio.Init(tractorInput, tractorEngine);

            carController = tractorInput.RCCP_Vehicle;
            tractorTrailer.Init(carController);

            gameController = GameController.Instance;
            gameController.SetTractorController(this);
        }

        void Update()
        {
            if (!carController)
                return;

            speed = carController.speed;
            RPM = carController.engineRPM;
        }

        void FixedUpdate()
        {
            if (handbrakeWheels == null) return;

            float torque = isHandbrake ? handbrakeTorque : 0f;

            foreach (var wheel in handbrakeWheels)
            {
                if (wheel != null)
                    wheel.brakeTorque = torque;
            }
        }

        public MonoBehaviour GetMonoBehaviour(TractorBehavior tractorBehavior)
        {
            switch (tractorBehavior)
            {
                case TractorBehavior.Main:
                    return this;
                case TractorBehavior.Gearbox:
                    return tractorGearbox;
                case TractorBehavior.Engine:
                    return tractorEngine;
                case TractorBehavior.Lights:
                    return tractorLights;
                case TractorBehavior.Audio:
                    return tractorAudio;
                case TractorBehavior.RoadDetector:
                    return roadDetector;
                case TractorBehavior.Trailer:
                    return tractorTrailer;
                default:
                    return null;
            }
        }

        public void ResetTractor()
        {
            if (!tractorEngine || !tractorGearbox)
                return;

            tractorEngine.ResetEngine();
            tractorGearbox.ResetGearbox();
        }

        public void PlaceTractor(Transform t)
        {
            RCCP.Transport(carController, t.position, t.rotation);
        }

        public void SetHandbrake(bool state)
        {
            isHandbrake = state;
        }

        public bool HasTrailer()
        {
            if (!tractorTrailer)
                return false;

            return tractorTrailer.CurrentTrailer != null;
        }

        public void DetachTrailer()
        {
            tractorTrailer?.DetachTrailer();
        }

        public void ForceDetachTrailer()
        {
            tractorTrailer?.ForceDetachTrailer();
        }
    }
}
