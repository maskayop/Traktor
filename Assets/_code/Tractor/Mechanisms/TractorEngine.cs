using UnityEngine;

namespace Tractor
{
    public class TractorEngine : MonoBehaviour
    {
        [SerializeField] float neutralAccelerationRate = 1.0f;
        [SerializeField] float driveAccelerationRate = 0.5f;

        RCCP_Engine engine;
        TractorGearbox tractorGearbox;

        void Update()
        {
            if (engine == null)
                return;

            if (tractorGearbox.currentGear == 0)
                engine.engineAccelerationRate = neutralAccelerationRate;
            else
                engine.engineAccelerationRate = driveAccelerationRate;
        }

        public void Init(TractorInput tractorInput, TractorGearbox INtractorGearbox)
        {
            if (!tractorInput)
                return;

            engine = tractorInput.RCCP_Vehicle.Engine;
            tractorGearbox = INtractorGearbox;
        }
    }
}
