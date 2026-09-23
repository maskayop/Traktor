using UnityEngine;

namespace Tractor
{
    public class TractorController : MonoBehaviour, ICheckable
    {
        public enum TractorBehavior { Main, Gearbox, Engine, Lights }

        [Header("Механизмы")]
        public TractorInput tractorInput;
        public TractorGearbox tractorGearbox;
        public TractorEngine tractorEngine;
        public TractorLights tractorLights;
        public TractorAudio tractorAudio;
        public RoadDetector roadDetector;

        [Header("Инфо")]
        public float speed;
        public float RPM;
        public bool isHandbrake = false;

        RCCP_CarController carController;
        public RCCP_CarController CarController { get { return carController; } }

        Transform destinationTransform;
        public Transform DestinationTransform { get { return destinationTransform; } set { destinationTransform = value; } }

        public float GetVariableValue(string conditionName)
        {
            switch (conditionName)
            {
                case "speed":
                    return speed;
                case "RPM":
                    return RPM;
                case "isHandbrake":
                    if (isHandbrake)
                        return 1;
                    else
                        return 0;
                case "handbrake":
                    if (isHandbrake)
                        return 1;
                    else
                        return 0;
                case "destinationDistance":
                    return Vector3.Distance(transform.position, destinationTransform.position);
                default:
                    Debug.LogWarning($"Неизвестное условие: {conditionName}");
                    return 0;
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
        }

        void Update()
        {
            if (!carController)
                return;

            speed = carController.speed;
            RPM = carController.engineRPM;

            if (isHandbrake)
            {
                carController.handbrakeInput_P = 1f;
                carController.handbrakeInput_V = 1f;
            }
            else
            {
                carController.handbrakeInput_P = 0f;
                carController.handbrakeInput_V = 0f;
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
                default:
                    return null;
            }
        }

        public void SetHandbrake(bool state)
        {
            isHandbrake = state;
        }

        public void ResetTractor()
        {
            if (!tractorEngine || !tractorGearbox)
                return;

            tractorEngine.ResetEngine();
            tractorGearbox.ResetGearbox();
        }
    }
}
