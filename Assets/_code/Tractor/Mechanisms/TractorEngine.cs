using UnityEngine;

namespace Tractor
{
    public class TractorEngine : MonoBehaviour, ICheckable
    {
        [SerializeField] float neutralAccelerationRate = 1.0f;
        [SerializeField] float driveAccelerationRate = 0.5f;

        [Header("RPM")]
        public float RPM;
        [SerializeField] int engineShutdownRPM = 500;

        bool mass = false;
        public bool Mass { get { return mass; } set { mass = value; } }

        bool starter = false;
        public bool Starter { get { return starter; } set { starter = value; } }

        bool ignition = false;
        public bool Ignition { get { return ignition; } set { ignition = value; } }

        bool gearboxCoupling = false;

        // Ключ в положении "I" — приборы запитаны, идёт проверка ламп
        public bool IsKeyOn => mass && starter;

        // Двигатель реально работает (RCCP подтвердил запуск)
        public bool IsRunning => engine != null && engine.engineRunning;

        RCCP_Engine engine;
        TractorGearbox tractorGearbox;

        public bool HasVariable(string conditionName)
        {
            switch (conditionName)
            {
                case "mass":
                case "starter":
                case "ignition":
                case "RPM":
                case "IsRunning":
                case "IsKeyOn":
                    return true;
                default:
                    return false;
            }
        }

        public float GetVariableValue(string conditionName)
        {
            switch (conditionName)
            {
                case "mass": return mass ? 1f : 0f;
                case "starter": return starter ? 1f : 0f;
                case "ignition": return ignition ? 1f : 0f;
                case "RPM": return RPM;
                case "IsRunning": return IsRunning ? 1f : 0f;
                case "IsKeyOn": return IsKeyOn ? 1f : 0f;
                default:
                    Debug.LogWarning($"Неизвестное условие: {conditionName}");
                    return 0f;
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

            RPM = engine.engineRPM;

            CheckForGearboxCoupling();
            CheckForWorkingRPM();
        }

        public void Init(TractorInput tractorInput, TractorGearbox INtractorGearbox)
        {
            if (!tractorInput)
                return;

            engine = tractorInput.RCCP_Vehicle.Engine;
            tractorGearbox = INtractorGearbox;

            ResetEngine();
        }

        public void ResetEngine()
        {
            MassTurnOff();
            StarterTurnOff();
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

        public void StarterTurnOn()
        {
            if (!mass || !starter)
                return;

            ignition = true;
            starter = true;

            engine.StartEngine();
        }

        public void StarterTurnOff()
        {
            ignition = false;
            starter = false;

            engine.StopEngine();
        }

        void CheckForGearboxCoupling()
        {
            if (tractorGearbox.isNeutralGear || tractorGearbox.currentClutch == 1)
                gearboxCoupling = false;
            else
                gearboxCoupling = true;

            if (gearboxCoupling)
                engine.engineAccelerationRate = driveAccelerationRate;
            else
                engine.engineAccelerationRate = neutralAccelerationRate;
        }

        void CheckForWorkingRPM()
        {
            if (!gearboxCoupling)
                return;

            if (RPM < engineShutdownRPM)
                if (engine.engineStarting == false || engine.engineRunning == true)
                    engine.StopEngine();
        }
    }
}
