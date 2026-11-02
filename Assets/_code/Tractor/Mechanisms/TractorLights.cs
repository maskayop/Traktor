using UnityEngine;

namespace Tractor
{
    public class TractorLights : MonoBehaviour, ICheckable
    {
        public bool HasVariable(string conditionName)
        {
            switch (conditionName)
            {
                case "leftTurnIsOn":
                case "leftTurn":
                case "rightTurnIsOn":
                case "rightTurn":
                case "alarmIsOn":
                case "turnRelay":
                case "alarm":
                case "leftTurnBlink":
                case "rightTurnBlink":
                case "alarmBlink":
                case "parkingIsOn":
                case "parkingLight":
                case "headIsOn":
                case "headLight":
                case "highBeamIsOn":
                case "highBeamLight":
                    return true;
                default:
                    return false;
            }
        }

        public float GetVariableValue(string conditionName)
        {
            switch (conditionName)
            {
                case "leftTurnIsOn":
                case "leftTurn":
                    return leftTurnIsOn ? 1f : 0f;
                case "rightTurnIsOn":
                case "rightTurn":
                    return rightTurnIsOn ? 1f : 0f;
                case "alarmIsOn":
                case "alarm":
                    return alarmIsOn ? 1f : 0f;
                case "turnRelay":
                    return turnLightsRelayValue;
                case "leftTurnBlink":
                    return (leftTurnIsOn && turnLightsRelayValue > 0.5f) ? 1f : 0f;
                case "rightTurnBlink":
                    return (rightTurnIsOn && turnLightsRelayValue > 0.5f) ? 1f : 0f;
                case "alarmBlink":
                    return (alarmIsOn && turnLightsRelayValue > 0.5f) ? 1f : 0f;
                case "parkingIsOn":
                    return parkingIsOn ? 1f : 0f;
                case "parkingLight":
                    return (parkingIsOn && lightsCanWork) ? 1f : 0f;
                case "headIsOn":
                    return headIsOn ? 1f : 0f;
                case "headLight":
                    return (headIsOn && lightsCanWork) ? 1f : 0f;
                case "highBeamIsOn":
                    return highBeamIsOn ? 1f : 0f;
                case "highBeamLight":
                    return (highBeamIsOn && lightsCanWork) ? 1f : 0f;
                default:
                    Debug.LogWarning($"Неизвестное условие: {conditionName}");
                    return 0f;
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
        [SerializeField] string highBeamLightPropertyName;
        [SerializeField] float highBeamLightMultiplier = 1.0f;

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
        public bool highBeamIsOn = false;

        TractorEngine tractorEngine;

        float turnLightsRelayValue = 0;
        float turnLightsRelayTime = 0;

        void Update()
        {
            lightsCanWork = tractorEngine.Mass;

            if (!leftTurnIsOn && !rightTurnIsOn && !alarmIsOn)
            {
                turnLightsRelayValue = 0;
                turnLightsRelayTime = 0;
            }

            UpdateTurnLightsRelay();
            UpdateBrakeLightsMaterials();
            UpdateParkingLightsMaterials();
            UpdateHeadLightsMaterials();
            UpdateHighBeamLightsMaterials();
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
            if (brakeIsOn && lightsCanWork)
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
            if (parkingIsOn && lightsCanWork)
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
            if (headIsOn && !highBeamIsOn && lightsCanWork)
                lightHeadFront.UpdateVisuals(headLightPropertyName, headLightMultiplier);
            else
                lightHeadFront.UpdateVisuals(headLightPropertyName, 0);
        }

        void UpdateHighBeamLightsMaterials()
        {
            if (highBeamIsOn && lightsCanWork)
                lightHeadFront.UpdateVisuals(highBeamLightPropertyName, highBeamLightMultiplier);
            else
                lightHeadFront.UpdateVisuals(highBeamLightPropertyName, 0);
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

        public void TurnOnHighBeamLights()
        {
            highBeamIsOn = true;
        }

        public void TurnOffHighBeamLights()
        {
            highBeamIsOn = false;
        }
    }
}
