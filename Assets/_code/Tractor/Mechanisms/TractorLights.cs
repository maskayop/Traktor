using UnityEngine;

namespace Tractor
{
    public class TractorLights : MonoBehaviour, ICheckable
    {
        public float GetVariableValue(string conditionName)
        {
            switch (conditionName)
            {
                default:
                    Debug.LogWarning($"Неизвестное условие: {conditionName}");
                    return 0;
            }
        }

        public bool leftTurnIsOn = false;
        public bool rightTurnIsOn = false;
        public bool alarmIsOn = false;
        public bool turnLightsRelayCanWork = false;

        TractorEngine tractorEngine;

        void Update()
        {
            if (!tractorEngine.Mass)
            {
                turnLightsRelayCanWork = false;
                return;
            }
            else
                turnLightsRelayCanWork = true;
        }

        public void Init(TractorInput INtractorInput, TractorEngine INtractorEngine)
        {
            if (!INtractorInput || !INtractorEngine)
                return;

            tractorEngine = INtractorEngine;
        }

        public void TurnOnLeftTurnLight()
        {
            leftTurnIsOn = true;
            rightTurnIsOn = false;
        }

        public void TurnOnRightTurnLight()
        {
            leftTurnIsOn = false;
            rightTurnIsOn = true;
        }

        public void TurnOffTurnLight()
        {
            leftTurnIsOn = false;
            rightTurnIsOn = false;
        }
    }
}
