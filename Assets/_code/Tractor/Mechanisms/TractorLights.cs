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

        [Header("Параметры материала")]
        [SerializeField] string turnLightPropertyName;
        [SerializeField] float turnLightMultiplier = 1.0f;
        [SerializeField] float turnLightsBlinkingSpeed = 1.0f;

        [Header("Поворотники")]
        [SerializeField] MeshRenderer lightSideFrontLeft;
        [SerializeField] MeshRenderer lightSideFrontRight;
        [SerializeField] MeshRenderer lightSideBackLeft;
        [SerializeField] MeshRenderer lightSideBackRight;

        [Header("Info")]
        public bool turnLightsRelayCanWork = false;
        public bool leftTurnIsOn = false;
        public bool rightTurnIsOn = false;
        public bool alarmIsOn = false;

        TractorEngine tractorEngine;

        public float turnLightsRelayValue = 0;
        float turnLightsRelayTime = 0;

        void Update()
        {
            if (!tractorEngine.Mass)
            {
                turnLightsRelayCanWork = false;
                return;
            }
            else
                turnLightsRelayCanWork = true;

            UpdateTurnLightsRelay();
        }

        public void Init(TractorInput INtractorInput, TractorEngine INtractorEngine)
        {
            if (!INtractorInput || !INtractorEngine)
                return;

            tractorEngine = INtractorEngine;
        }

        void UpdateTurnLightsRelay()
        {
            if (!turnLightsRelayCanWork)
                return;

            if (!leftTurnIsOn && !rightTurnIsOn && !alarmIsOn)
            {
                UpdateTurnLightsMaterials();
                return;
            }

            turnLightsRelayTime += Time.deltaTime;

            if (turnLightsRelayTime >= float.MaxValue)
                turnLightsRelayTime = 0;

            turnLightsRelayValue = Mathf.Sin(turnLightsRelayTime * turnLightsBlinkingSpeed);

            if (turnLightsRelayValue > 0)
                turnLightsRelayValue = 1;
            else
                turnLightsRelayValue = 0;

            UpdateTurnLightsMaterials();
        }

        void UpdateTurnLightsMaterials()
        {
            if (leftTurnIsOn)
            {
                SetMaterialFloatValue(lightSideFrontLeft.material, turnLightPropertyName, turnLightsRelayValue * turnLightMultiplier);
                SetMaterialFloatValue(lightSideBackLeft.material, turnLightPropertyName, turnLightsRelayValue * turnLightMultiplier);
                SetMaterialFloatValue(lightSideFrontRight.material, turnLightPropertyName, 0);
                SetMaterialFloatValue(lightSideBackRight.material, turnLightPropertyName, 0);
            }
            else if (rightTurnIsOn)
            {
                SetMaterialFloatValue(lightSideFrontLeft.material, turnLightPropertyName, 0);
                SetMaterialFloatValue(lightSideBackLeft.material, turnLightPropertyName, 0);
                SetMaterialFloatValue(lightSideFrontRight.material, turnLightPropertyName, turnLightsRelayValue * turnLightMultiplier);
                SetMaterialFloatValue(lightSideBackRight.material, turnLightPropertyName, turnLightsRelayValue * turnLightMultiplier);
            }
            else if (alarmIsOn)
            {
                SetMaterialFloatValue(lightSideFrontLeft.material, turnLightPropertyName, turnLightsRelayValue * turnLightMultiplier);
                SetMaterialFloatValue(lightSideBackLeft.material, turnLightPropertyName, turnLightsRelayValue * turnLightMultiplier);
                SetMaterialFloatValue(lightSideFrontRight.material, turnLightPropertyName, turnLightsRelayValue * turnLightMultiplier);
                SetMaterialFloatValue(lightSideBackRight.material, turnLightPropertyName, turnLightsRelayValue * turnLightMultiplier);
            }
            else
            {
                SetMaterialFloatValue(lightSideFrontLeft.material, turnLightPropertyName, 0);
                SetMaterialFloatValue(lightSideBackLeft.material, turnLightPropertyName, 0);
                SetMaterialFloatValue(lightSideFrontRight.material, turnLightPropertyName, 0);
                SetMaterialFloatValue(lightSideBackRight.material, turnLightPropertyName, 0);
            }
        }

        void SetMaterialFloatValue(Material m, string p, float v)
        {
            m.SetFloat(p, v);
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

        public void ActivateAlarmLights(bool state)
        {
            alarmIsOn = state;
        }
    }
}
