using UnityEngine;

namespace Tractor
{
    public class TractorLights : MonoBehaviour, ICheckable
    {
        public float GetVariableValue(string conditionName)
        {
            switch (conditionName)
            {
                case "leftTurnIsOn":
                    if (leftTurnIsOn)
                        return 1;
                    else
                        return 0;
                case "leftTurn":
                    if (leftTurnIsOn)
                        return 1;
                    else
                        return 0;
                case "rightTurnIsOn":
                    if (rightTurnIsOn)
                        return 1;
                    else
                        return 0;
                case "rightTurn":
                    if (rightTurnIsOn)
                        return 1;
                    else
                        return 0;
                case "alarmIsOn":
                    if (alarmIsOn)
                        return 1;
                    else
                        return 0;
                case "alarm":
                    if (alarmIsOn)
                        return 1;
                    else
                        return 0;
                default:
                    Debug.LogWarning($"Неизвестное условие: {conditionName}");
                    return 0;
            }
        }

        [Header("Параметры материалов")]
        [SerializeField] string turnLightPropertyName;
        [SerializeField] float turnLightMultiplier = 1.0f;
        [SerializeField] float turnLightsBlinkingSpeed = 1.0f;

        [SerializeField] string brakeLightPropertyName;
        [SerializeField] float brakeLightMultiplier = 1.0f;

        [SerializeField] string parkingLightPropertyName;
        [SerializeField] float parkingLightMultiplier = 1.0f;

        [SerializeField] string headLightPropertyName;
        [SerializeField] float headLightMultiplier = 1.0f;

        [Header("Главные светильники")]
        [SerializeField] MeshLightController lightHeadFront;

        [Header("Боковые светильники")]
        [SerializeField] MeshLightController lightSideFrontLeft;
        [SerializeField] MeshLightController lightSideFrontRight;
        [SerializeField] MeshLightController lightSideBackLeft;
        [SerializeField] MeshLightController lightSideBackRight;


        [Header("Инфо")]
        public bool lightsCanWork = false;

        [Space(10)]
        public bool leftTurnIsOn = false;
        public bool rightTurnIsOn = false;
        public bool alarmIsOn = false;
        public bool brakeIsOn = false;
        public bool parkingIsOn = false;
        public bool headIsOn = false;

        TractorEngine tractorEngine;

        float turnLightsRelayValue = 0;
        float turnLightsRelayTime = 0;

        void Update()
        {
            if (!tractorEngine.Mass)
            {
                lightsCanWork = false;
                return;
            }
            else
                lightsCanWork = true;

            UpdateTurnLightsRelay();
            UpdateBrakeLightsMaterials();
            UpdateParkingLightsMaterials();
            UpdateHeadLightsMaterials();
        }

        public void Init(TractorInput INtractorInput, TractorEngine INtractorEngine)
        {
            if (!INtractorInput || !INtractorEngine)
                return;

            tractorEngine = INtractorEngine;
        }

        void UpdateTurnLightsRelay()
        {
            if (!lightsCanWork)
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
            float value = turnLightsRelayValue * turnLightMultiplier;

            if (leftTurnIsOn)
            {
                lightSideFrontLeft.UpdateVisuals(turnLightPropertyName, value);
                lightSideBackLeft.UpdateVisuals(turnLightPropertyName, value);
                lightSideFrontRight.UpdateVisuals(turnLightPropertyName, 0);
                lightSideBackRight.UpdateVisuals(turnLightPropertyName, 0);
            }
            else if (rightTurnIsOn)
            {
                lightSideFrontLeft.UpdateVisuals(turnLightPropertyName, 0);
                lightSideBackLeft.UpdateVisuals(turnLightPropertyName, 0);
                lightSideFrontRight.UpdateVisuals(turnLightPropertyName, value);
                lightSideBackRight.UpdateVisuals(turnLightPropertyName, value);
            }
            else if (alarmIsOn)
            {
                lightSideFrontLeft.UpdateVisuals(turnLightPropertyName, value);
                lightSideBackLeft.UpdateVisuals(turnLightPropertyName, value);
                lightSideFrontRight.UpdateVisuals(turnLightPropertyName, value);
                lightSideBackRight.UpdateVisuals(turnLightPropertyName, value);
            }
            else
            {
                lightSideFrontRight.UpdateVisuals(turnLightPropertyName, 0);
                lightSideBackRight.UpdateVisuals(turnLightPropertyName, 0);
                lightSideFrontLeft.UpdateVisuals(turnLightPropertyName, 0);
                lightSideBackLeft.UpdateVisuals(turnLightPropertyName, 0);
            }
        }

        void UpdateBrakeLightsMaterials()
        {
            if (!lightsCanWork)
                return;

            if (brakeIsOn)
            {
                lightSideBackLeft.UpdateVisuals(brakeLightPropertyName, brakeLightMultiplier);
                lightSideBackRight.UpdateVisuals(brakeLightPropertyName, brakeLightMultiplier);
            }
            else
            {
                lightSideBackLeft.UpdateVisuals(brakeLightPropertyName, 0);
                lightSideBackRight.UpdateVisuals(brakeLightPropertyName, 0);
            }
        }

        void UpdateParkingLightsMaterials()
        {
            if (!lightsCanWork)
                return;

            if (parkingIsOn)
            {
                lightSideFrontLeft.UpdateVisuals(parkingLightPropertyName, parkingLightMultiplier);
                lightSideBackLeft.UpdateVisuals(parkingLightPropertyName, parkingLightMultiplier);
                lightSideFrontRight.UpdateVisuals(parkingLightPropertyName, parkingLightMultiplier);
                lightSideBackRight.UpdateVisuals(parkingLightPropertyName, parkingLightMultiplier);
            }
            else
            {
                lightSideFrontLeft.UpdateVisuals(parkingLightPropertyName, 0);
                lightSideBackLeft.UpdateVisuals(parkingLightPropertyName, 0);
                lightSideFrontRight.UpdateVisuals(parkingLightPropertyName, 0);
                lightSideBackRight.UpdateVisuals(parkingLightPropertyName, 0);
            }
        }

        void UpdateHeadLightsMaterials()
        {
            if (!lightsCanWork)
                return;

            if (headIsOn)
                lightHeadFront.UpdateVisuals(headLightPropertyName, headLightMultiplier);
            else
                lightHeadFront.UpdateVisuals(headLightPropertyName, 0);
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

        public void ActivateBrakeLights(bool state)
        {
            brakeIsOn = state;
        }

        public void TurnOnParkingLights()
        {
            parkingIsOn = true;
        }

        public void TurnOffParkingLights()
        {
            parkingIsOn = false;
        }

        public void TurnOnHeadLights()
        {
            headIsOn = true;
        }

        public void TurnOffHeadLights()
        {
            headIsOn = false;
        }
    }
}
