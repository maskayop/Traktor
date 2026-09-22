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

        void Update()
        {

        }

        public void Init(TractorInput tractorInput)
        {
            if (!tractorInput)
                return;
        }
    }
}
