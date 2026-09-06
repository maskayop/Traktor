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

        RCCP_CarController carController;

        void Start()
        {
            tractorInput.Init(tractorGearbox, tractorEngine);
            tractorGearbox.Init(tractorInput);
            tractorEngine.Init(tractorInput, tractorGearbox);

            carController = tractorInput.RCCP_Vehicle;
        }

        void Update()
        {
            if (!carController)
                return;

            speed = carController.speed;
        }
    }
}
