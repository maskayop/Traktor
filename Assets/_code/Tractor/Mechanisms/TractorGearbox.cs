using UnityEngine;

namespace Tractor
{
    public class TractorGearbox : MonoBehaviour
    {
        public bool isGearLevel1 = true;
        public int currentRange = 0;
        public int currentGear = 0;

        TractorInput tractorInput;
        RCCP_Gearbox gearbox;

        public void Init(TractorInput tractorInput)
        {
            if (!tractorInput)
                return;

            gearbox = tractorInput.RCCP_Vehicle.Gearbox;

            ChangeGearLevel(true);
            gearbox.ShiftToGear(0);
            currentGear = 1;
        }

        void Update()
        {
            if (!tractorInput || !gearbox)
                return;
        }

        public void ShiftToGear(float input, int value)
        {
            if (input >= 0.5f)
                gearbox.ShiftToGear(value - 1);

            currentGear = value;
        }

        public void ChangeGearLevel(bool isFirstLevel)
        {
            isGearLevel1 = isFirstLevel;
            ChangeGearRange(1);
        }

        public void ChangeGearRange(int value)
        {
            if (value == 1)
            {
                if (isGearLevel1)
                    currentRange = 1;
                else
                    currentRange = 2;
            }
            else if (value == 2)
            {
                if (isGearLevel1)
                    currentRange = 3;
                else
                    currentRange = 4;
            }
            else if (value == -1)
            {
                if (isGearLevel1)
                    currentRange = -1;
                else
                    currentRange = -2;
            }
            else
                currentRange = 0;
        }
    }
}
