using UnityEngine;

namespace Tractor
{
    public class TractorMain : MonoBehaviour
    {
        [Header("Механизмы")]
        public TractorInput tractorInput;
        public TractorGearbox tractorGearbox;
        public TractorEngine tractorEngine;


        [Header("Инфо")]
        public float speed;
        public float RPM;
        public bool isHandbrake = false;

        RCCP_CarController carController;

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

        public void SetHandbrake(bool state)
        {
            isHandbrake = state;
        }
    }
}
