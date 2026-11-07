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

        [Header("Переключение передач")]
        [SerializeField] string gearSwitchWithoutClutchMessage;
        [SerializeField] string rangeSwitchOnSpeedMessage;

        [Header("Столкновения")]
        [SerializeField] string collisionMessage;
        [SerializeField] float minImpactImpulse = 5f;

        ErrorMessage errorMessage;
        public ErrorMessage ErrorMessage { get { return errorMessage; } set { errorMessage = value; } }

        GameController gameController;
        ExercisesController exercisesController;

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
            gameController = GameController.Instance;
            exercisesController = ExercisesController.Instance;

            if (!gameController) return;

            tractorController = gameController.GetTractorController();
            if (!tractorController) return;

            tractorGearbox = tractorController.tractorGearbox;

            currentGear = tractorGearbox.currentGear;
            currentRange = tractorGearbox.currentRange;
            isGearLevel1 = tractorGearbox.isGearLevel1;

            errorMessage = null;
        }

        void CheckGearbox()
        {
            if (!tractorGearbox || !tractorController) return;
            errorMessage = null;

            // Проверка переключения передачи без сцепления
            if (currentGear != tractorGearbox.currentGear)
            {
                // Нейтраль можно включать без сцепления
                if (tractorGearbox.currentGear != 0 && tractorGearbox.currentClutch < 0.5f)
                {
                    errorMessage = new ErrorMessage
                    {
                        errorState = ErrorState.Error,
                        message = gearSwitchWithoutClutchMessage
                    };
                }
            }
            currentGear = tractorGearbox.currentGear;

            // Проверка переключения диапазона/уровня на ходу
            bool rangeChanged = currentRange != tractorGearbox.currentRange;
            bool levelChanged = isGearLevel1 != tractorGearbox.isGearLevel1;

            if ((rangeChanged || levelChanged) && Mathf.Abs(tractorController.speed) > 0.5f)
            {
                errorMessage = new ErrorMessage
                {
                    errorState = ErrorState.Error,
                    message = rangeSwitchOnSpeedMessage
                };
            }

            currentRange = tractorGearbox.currentRange;
            isGearLevel1 = tractorGearbox.isGearLevel1;

            if (errorMessage != null)
                ReportError(errorMessage);
        }

        void OnEnable()
        {
            RCCP_Events.OnRCCPImpact += OnImpact;
        }

        void OnDisable()
        {
            RCCP_Events.OnRCCPImpact -= OnImpact;
        }

        void OnImpact(RCCP_CarController car, float impulse)
        {
            // Проверяем, что удар пришёлся именно по нашему трактору
            if (tractorController == null || car != tractorController.CarController)
                return;

            // Проверяем силу удара (можно настроить в инспекторе)
            if (impulse < minImpactImpulse)
                return;

            // Выдаём ошибку
            errorMessage = new ErrorMessage
            {
                errorState = ErrorState.Error,
                message = collisionMessage
            };

            ReportError(errorMessage);
        }

        void ReportError(ErrorMessage msg)
        {
            if (msg == null) return;

            exercisesController?.AddErrorToReport(msg.errorState, msg.message);
        }
    }
}
