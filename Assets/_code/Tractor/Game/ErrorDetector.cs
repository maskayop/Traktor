using System;
using UnityEngine;

namespace Tractor
{
    [Serializable]
    public class ErrorMessage
    {
        public ErrorDetector.ErrorState errorState = ErrorDetector.ErrorState.Warning;
        public string message;
    }

    public class ErrorDetector : MonoBehaviour
    {
        public static ErrorDetector Instance;

        public enum ErrorState { Warning, Error, Penalty };

        [SerializeField] string gearSwitchWithoutClutchMessage;
        [SerializeField] string rangeSwitchOnSpeedMessage;

        ErrorMessage errorMessage;
        public ErrorMessage ErrorMessage { get { return errorMessage; } set { errorMessage = value; } }

        TractorController tractorController;
        TractorGearbox tractorGearbox;

        bool isGearLevel1 = true;
        int currentGear = -1;
        int currentRange = -1;

        void Awake()
        {
            if (Instance != null)
            {
                Debug.LogWarning("Cannot create ErrorDetector");
                Destroy(gameObject);
                return;
            }

            Instance = this;
        }

        void Start()
        {
            Init();
        }

        void Update()
        {
            CheckGearbox();
        }

        public void Init()
        {
            tractorController = FindAnyObjectByType<TractorController>();

            if (!tractorController)
                return;

            tractorGearbox = tractorController.tractorGearbox;

            errorMessage = null;
        }

        void CheckGearbox()
        {
            if (currentGear != tractorGearbox.currentGear)
                if (tractorGearbox.currentClutch < 0.5f)
                    errorMessage = new ErrorMessage
                    {
                        errorState = ErrorState.Error,
                        message = gearSwitchWithoutClutchMessage
                    };

            currentGear = tractorGearbox.currentGear;

            if (currentRange != tractorGearbox.currentRange)
                if (tractorController.speed > 0.5f)
                    errorMessage = new ErrorMessage
                    {
                        errorState = ErrorState.Error,
                        message = rangeSwitchOnSpeedMessage
                    };

            currentRange = tractorGearbox.currentRange;

            if (isGearLevel1 != tractorGearbox.isGearLevel1)
                if (tractorController.speed > 0.5f)
                    errorMessage = new ErrorMessage
                    {
                        errorState = ErrorState.Error,
                        message = rangeSwitchOnSpeedMessage
                    };

            isGearLevel1 = tractorGearbox.isGearLevel1;
        }
    }
}
