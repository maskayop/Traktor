using UnityEngine;

namespace Tractor
{
    public class TractorAudio : MonoBehaviour, ICheckable
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

        [SerializeField] AudioSource audioSource;
        [SerializeField] AudioClip hornSignalClip;

        public bool hornIsPlaying = false;

        public bool signalsCanWork = false;

        TractorEngine tractorEngine;

        void Update()
        {
            if (!tractorEngine.Mass)
            {
                signalsCanWork = false;
                return;
            }
            else
                signalsCanWork = true;

            if (!signalsCanWork)
                audioSource.volume = 0;

            if (hornIsPlaying)
                audioSource.volume = 1;
            else
                audioSource.volume = 0;
        }

        public void Init(TractorInput INtractorInput, TractorEngine INtractorEngine)
        {
            if (!INtractorInput || !INtractorEngine)
                return;

            tractorEngine = INtractorEngine;
        }

        public void ActivateHornSignal(bool state)
        {
            hornIsPlaying = state;
        }
    }
}
