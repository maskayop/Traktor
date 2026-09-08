using UnityEngine;

namespace Tractor
{
    public class TractorController : MonoBehaviour, ICheckable
    {
        public enum TractorBehavior { Main, Gearbox, Engine }

        [Header("Механизмы")]
        public TractorInput tractorInput;
        public TractorGearbox tractorGearbox;
        public TractorEngine tractorEngine;
        public RoadDetector roadDetector;

        [Header("Инфо")]
        public float speed;
        public float RPM;
        public bool isHandbrake = false;

        RCCP_CarController carController;

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
                default:
                    Debug.LogWarning($"Неизвестное условие: {conditionName}");
                    return 0;
            }
        }

        void Start()
        {
            tractorInput.Init(this, tractorGearbox, tractorEngine);
            tractorGearbox.Init(tractorInput);
            tractorEngine.Init(tractorInput, tractorGearbox);

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
                default:
                    return null;
            }
        }

        public void SetHandbrake(bool state)
        {
            isHandbrake = state;
        }
    }
}
