using UnityEngine;

namespace Tractor
{
    public class TractorEngine : MonoBehaviour, ICheckable
    {
        [SerializeField] float neutralAccelerationRate = 1.0f;
        [SerializeField] float driveAccelerationRate = 0.5f;

        bool mass = false;
        public bool Mass { get { return mass; } set { mass = value; } }

        bool starter = false;
        public bool Starter { get { return starter; } set { starter = value; } }

        bool ignition = false;
        public bool Ignition { get { return ignition; } set { ignition = value; } }

        RCCP_Engine engine;
        TractorGearbox tractorGearbox;

        public float GetVariableValue(string conditionName)
        {
            switch (conditionName)
            {
                case "mass":
                    if (mass)
                        return 1;
                    else
                        return 0;
                case "starter":
                    if (starter)
                        return 1;
                    else
                        return 0;
                case "ignition":
                    if (ignition)
                        return 1;
                    else
                        return 0;
                default:
                    Debug.LogWarning($"Неизвестное условие: {conditionName}");
                    return 0;
            }
        }

        void Update()
        {
            if (engine == null)
                return;

            if (engine.engineRunning || engine.engineStarting)
            {
                if (mass == false || starter == false)
                {
                    engine.StopEngine();
                    return;
                }
            }

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

        public void MassTurnOn()
        {
            mass = true;
        }

        public void MassTurnOff()
        {
            mass = false;
            engine.StopEngine();
        }

        public void StarterOn()
        {
            if (!mass || !starter)
                return;

            ignition = true;
            starter = true;
            engine.StartEngine();
        }

        public void StarterOff()
        {
            ignition = false;
            starter = false;
            engine.StopEngine();
        }
    }
}
