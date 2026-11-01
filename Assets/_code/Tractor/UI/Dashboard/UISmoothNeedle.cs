using UnityEngine;

namespace Tractor.UI
{
    public class UISmoothNeedle : MonoBehaviour
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

        [Header("Диапазон шкалы")]
        public float minValue = 0f;
        public float maxValue = 10f;

        [Header("Фейковый режим")]
        public bool useDefaultValue = false;
        public float defaultValue = 0f;

        [Header("Нелинейная шкала")]
        public bool useCurveValue = false;
        public AnimationCurve scaleCurve = AnimationCurve.Linear(0f, 0f, 1f, 1f);

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

            float targetValue;

            if (useDefaultValue)
            {
                // Читаем переменную (например, "IsRunning").
                // Если она == 1 → показываем defaultValue, иначе 0.
                float condition = targetCheckable.GetVariableValue(variableName);
                targetValue = condition > 0.5f ? defaultValue : minValue;
            }
            else
            {
                // Обычный режим: показываем саму переменную
                targetValue = targetCheckable.GetVariableValue(variableName);
            }

            currentDisplayValue = Mathf.Lerp(currentDisplayValue, targetValue, smoothSpeed);

            float normalizedValue = Mathf.InverseLerp(minValue, maxValue, currentDisplayValue);
            float angleT;

            if (useCurveValue)
                angleT = scaleCurve.Evaluate(normalizedValue);
            else
                angleT = normalizedValue;

            float angle = Mathf.Lerp(minAngle, maxAngle, angleT);
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
                Debug.LogError($"UISmoothNeedle: модуль {tractorBehavior} не найден!");
                return;
            }

            targetCheckable = targetMono as ICheckable;

            if (targetCheckable == null)
            {
                Debug.LogError($"UISmoothNeedle: модуль {tractorBehavior} не реализует ICheckable!");
                return;
            }

            if (string.IsNullOrEmpty(variableName))
            {
                Debug.LogError("UISmoothNeedle: variableName пустой!");
                return;
            }

            if (!targetCheckable.HasVariable(variableName))
            {
                Debug.LogError($"UISmoothNeedle: переменная '{variableName}' не найдена в модуле {tractorBehavior}!");
                return;
            }

            if (maxValue <= minValue)
            {
                maxValue = minValue + 0.01f;
                Debug.LogWarning($"UISmoothNeedle: maxValue было меньше или равно minValue, исправлено на {maxValue}");
            }

            if (useCurveValue)
            {
                if (scaleCurve == null || scaleCurve.length == 0)
                {
                    Debug.LogError("UISmoothNeedle: scaleCurve пустая, но useCurveValue включён!");
                    return;
                }
            }

            isInitialized = true;
        }
    }
}
