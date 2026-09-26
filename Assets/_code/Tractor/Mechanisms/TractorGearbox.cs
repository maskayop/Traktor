using System;
using System.Collections.Generic;
using UnityEngine;

namespace Tractor
{
    [Serializable]
    public class GearModification
    {
        public string name;
        public float maxSpeed;
        public float[] finalDriveOverrides;
    }

    public class TractorGearbox : MonoBehaviour, ICheckable
    {
        [Header("Состояние передач")]
        //Уровень
        public bool isGearLevel1 = true;

        //Диапазон
        public int currentRange = 0;
        public int rangeValue = 0;
        public int RangeValue { get { return rangeValue; } set { rangeValue = value; } }
        public int previousRange = -99;

        //Передача
        public int currentGear = 0;
        public int gearValue = 0;
        public int GearValue { get { return gearValue; } set { gearValue = value; } }
        public int previousGear = -99;

        [Header("Модификаторы Передач")]
        [SerializeField] List<float> defaultGearRatios = new List<float>();
        [SerializeField] List<GearModification> gearModifications = new List<GearModification>();
        [SerializeField] float defaultFinalDrive = 0;

        int currentGearModification = 0;

        RCCP_Gearbox gearbox;
        RCCP_Engine engine;
        RCCP_Differential[] differentials;

        public float GetVariableValue(string conditionName)
        {
            switch (conditionName)
            {
                case "gearLevel":
                    if (isGearLevel1)
                        return 1;
                    else
                        return 2;
                case "isGearLevel1":
                    if (isGearLevel1)
                        return 1;
                    else
                        return 2;
                case "currentRange":
                    return currentRange;
                case "currentGear":
                    return gearValue;
                default:
                    Debug.LogWarning($"Неизвестное условие: {conditionName}");
                    return 0;
            }
        }

        public void Init(TractorInput tractorInput)
        {
            if (!tractorInput)
                return;

            gearbox = tractorInput.RCCP_Vehicle.Gearbox;
            engine = tractorInput.RCCP_Vehicle.Engine;
            differentials = tractorInput.RCCP_Vehicle.Differentials;

            for (int i = 0; i < gearbox.gearRatios.Length; i++)
                defaultGearRatios[i] = gearbox.gearRatios[i];

            ResetGearbox();
        }

        void Update()
        {
            if (previousRange != rangeValue)
                ChangeGear(rangeValue, currentGear);
            
            if (previousGear != gearValue)
                ChangeGear(currentRange, gearValue);

            previousRange = rangeValue;
            previousGear = gearValue;
        }

        public void ResetGearbox()
        {
            previousRange = -99;
            previousGear = -99;

            ChangeGearLevel(true);
            ChangeGear(0, gearValue);
        }

        public void ShiftToGear()
        {
            if (currentRange < 0)
                gearbox.forceToRGear = true;
            else
                gearbox.forceToRGear = false;

            if (currentGear != 0)
            {
                gearbox.forceToNGear = false;
                gearbox.ShiftToGear(currentGear - 1);
            }
            else
                gearbox.forceToNGear = true;

            if (currentRange == 0)
                gearbox.forceToNGear = true;
        }

        public void ChangeGearLevel(bool isFirstLevel)
        {
            isGearLevel1 = isFirstLevel;

            if (currentRange < 0)
                ChangeGear(-1, gearValue);
            else if (currentRange == 1 || currentRange == 2)
                ChangeGear(1, gearValue);
            else if (currentRange == 3 || currentRange == 4)
                ChangeGear(2, gearValue);
            else if (currentRange == 0)
                ChangeGear(0, gearValue);
        }

        public void ChangeGear(int range, int gear)
        {
            if (range == 0)
            {
                currentRange = 0;
                currentGearModification = 0;
            }
            else if (range == 1)
            {
                if (isGearLevel1)
                {
                    currentRange = 1;
                    currentGearModification = 0;
                }
                else
                {
                    currentRange = 2;
                    currentGearModification = 1;
                }
            }
            else if (range == 2)
            {
                if (isGearLevel1)
                {
                    currentRange = 3;
                    currentGearModification = 2;
                }
                else
                {
                    currentRange = 4;
                    currentGearModification = 3;
                }
            }
            else if (range == -1)
            {
                if (isGearLevel1)
                {
                    currentRange = -1;
                    currentGearModification = 4;
                }
                else
                {
                    currentRange = -2;
                    currentGearModification = 5;
                }
            }

            if (range == 0 || currentRange == 0)
                currentGear = 0;
            else
                currentGear = gear;

            ShiftToGear();
            UpdateGearRatios();
        }

        void UpdateGearRatios()
        {
            if (gearValue == 0 || rangeValue == 0)
            {
                foreach (var d in differentials)
                    d.finalDriveRatio = defaultFinalDrive;

                return;
            }

            float mSpeed = gearModifications[currentGearModification].maxSpeed;

            if (mSpeed < 0)
                engine.maximumSpeed = -gearModifications[currentGearModification].maxSpeed;
            else
                engine.maximumSpeed = gearModifications[currentGearModification].maxSpeed;

            foreach (var d in differentials)
                d.finalDriveRatio = gearModifications[currentGearModification].finalDriveOverrides[gearValue - 1];
        }
    }
}
