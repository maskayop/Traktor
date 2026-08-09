using UnityEngine;

namespace Tractor
{
    public class TractorMain : MonoBehaviour
    {
        public float speed;

        RCCP_CarController carController;

        void Update()
        {
            if (!carController)
                return;

            speed = carController.speed;
        }

        public void Init(TractorInput tractorInput)
        {
            if (!tractorInput)
                return;

            carController = tractorInput.RCCP_Vehicle;
        }
    }
}
