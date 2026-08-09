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

    public class TractorGearbox : MonoBehaviour
    {
        public bool isGearLevel1 = true;
        public int currentRange = 0;
        public int currentGear = 0;

        [Header("Модификаторы Передач")]
        [SerializeField] List<float> defaultGearRatios = new List<float>();
        [SerializeField] List<GearModification> gearModifications = new List<GearModification>();

        int currentGearModification = 0;

        RCCP_Gearbox gearbox;
        RCCP_Engine engine;
        RCCP_Differential[] differentials;

        public void Init(TractorInput tractorInput)
        {
            if (!tractorInput)
                return;

            gearbox = tractorInput.RCCP_Vehicle.Gearbox;
            engine = tractorInput.RCCP_Vehicle.Engine;
            differentials = tractorInput.RCCP_Vehicle.Differentials;

            for (int i = 0; i < gearbox.gearRatios.Length; i++)
                defaultGearRatios[i] = gearbox.gearRatios[i];

            ChangeGearLevel(true);
            gearbox.ShiftToGear(0);
            currentGear = 1;
            ChangeGearRange(1);
        }

        public void ShiftToGear(float input, int value)
        {
            currentGear = value;
            UpdateGearRatios();

            if (currentRange < 0)
                return;

            if (input >= 0.5f)
                gearbox.ShiftToGear(value - 1);
        }

        public void ChangeGearLevel(bool isFirstLevel)
        {
            isGearLevel1 = isFirstLevel;

            if (currentRange < 0)
                ChangeGearRange(-1);
            else
            {
                if (currentRange == 1 || currentRange == 2)
                    ChangeGearRange(1);
                else if (currentRange == 3 || currentRange == 4)
                    ChangeGearRange(2);
            }
        }

        public void ChangeGearRange(int value)
        {
            if (value == 1)
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
            else if (value == 2)
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
            else if (value == -1)
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
            else
            {
                currentRange = 0;
                currentGearModification = 0;
            }

            if (currentRange < 0)
                gearbox.forceToRGear = true;
            else
            {
                gearbox.forceToRGear = false;
                gearbox.ShiftToGear(currentGear - 1);
            }

            UpdateGearRatios();
        }

        void UpdateGearRatios()
        {
            float mSpeed = gearModifications[currentGearModification].maxSpeed;

            if (mSpeed < 0)
                engine.maximumSpeed = -gearModifications[currentGearModification].maxSpeed;
            else
                engine.maximumSpeed = gearModifications[currentGearModification].maxSpeed;

            foreach (var d in differentials)
                d.finalDriveRatio = gearModifications[currentGearModification].finalDriveOverrides[currentGear - 1];
        }
    }
}
