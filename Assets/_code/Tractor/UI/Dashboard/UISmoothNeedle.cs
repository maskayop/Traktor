using UnityEngine;

namespace Tractor.UI
{
    public class SmoothNeedle : MonoBehaviour
    {
        [Header("Настройки плавности")]
        [Range(0f, 1f)]
        public float smoothSpeed = 0.1f;

        [Header("Источник данных")]
        public TractorController.TractorBehavior tractorBehavior = TractorController.TractorBehavior.Main;
        public string variableName = "speed";

        TractorController tractorController;

        [Header("UI Элементы")]
        public RectTransform needleTransform;

        [Header("Диапазон стрелки")]
        public float minAngle = 0f;
        public float maxAngle = -180f;
        public float maxSpeed = 40f;

        ICheckable targetCheckable;
        float currentDisplayValue = 0f;
        bool isInitialized = false;

        void Start()
        {
            Init();
        }

        void Update()
        {
            if (!isInitialized || !needleTransform || targetCheckable == null)
                return;

            float targetValue = targetCheckable.GetVariableValue(variableName);
            currentDisplayValue = Mathf.Lerp(currentDisplayValue, targetValue, smoothSpeed);

            float normalizedValue = Mathf.Clamp01(currentDisplayValue / maxSpeed);
            float angle = Mathf.Lerp(minAngle, maxAngle, normalizedValue);
            needleTransform.localEulerAngles = new Vector3(0f, 0f, angle);
        }

        public void Init()
        {
            if (!tractorController)
                tractorController = FindAnyObjectByType<TractorController>();

            if (!tractorController) return;
            if (!needleTransform) return;

            MonoBehaviour targetMono = tractorController.GetMonoBehaviour(tractorBehavior);

            if (!targetMono)
            {
                Debug.LogError($"SmoothNeedle: модуль {tractorBehavior} не найден!");
                return;
            }

            targetCheckable = targetMono as ICheckable;

            if (targetCheckable == null)
            {
                Debug.LogError($"SmoothNeedle: модуль {tractorBehavior} не реализует ICheckable!");
                return;
            }

            if (string.IsNullOrEmpty(variableName))
            {
                Debug.LogError("SmoothNeedle: variableName пустой!");
                return;
            }

            if (!targetCheckable.HasVariable(variableName))
            {
                Debug.LogError($"SmoothNeedle: переменная '{variableName}' не найдена в модуле {tractorBehavior}!");
                return;
            }

            if (Mathf.Approximately(maxSpeed, 0f))
            {
                Debug.LogError("SmoothNeedle: maxSpeed не может быть 0!");
                return;
            }

            isInitialized = true;
        }
    }
}
