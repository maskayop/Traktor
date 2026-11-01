using UnityEngine;

namespace Tractor
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

        public enum NeedleMode { UI, World3D }
        public NeedleMode needleMode = NeedleMode.UI;

        [Header("UI Элементы")]
        public RectTransform needleTransform;      // для UI

        [Header("3D Элементы")]
        public Transform needle3DTransform;        // для 3D
        public Vector3 rotationAxis = Vector3.forward; // ось вращения (по умолчанию Z)

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
            if (!isInitialized || targetCheckable == null)
                return;

            float targetValue;
            if (useDefaultValue)
            {
                float condition = targetCheckable.GetVariableValue(variableName);
                targetValue = condition > 0.5f ? defaultValue : minValue;
            }
            else
                targetValue = targetCheckable.GetVariableValue(variableName);

            currentDisplayValue = Mathf.Lerp(currentDisplayValue, targetValue, smoothSpeed);

            float angle = CalculateAngle(currentDisplayValue);
            ApplyAngle(angle);
        }

        public void Init()
        {
            if (!tractorController)
                tractorController = FindAnyObjectByType<TractorController>();

            if (!tractorController) return;

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

            if (maxValue <= minValue)
            {
                maxValue = minValue + 0.01f;
                Debug.LogWarning($"SmoothNeedle: maxValue было меньше или равно minValue, исправлено на {maxValue}");
            }

            if (useCurveValue)
            {
                if (scaleCurve == null || scaleCurve.length == 0)
                {
                    Debug.LogError("SmoothNeedle: scaleCurve пустая, но useCurveValue включён!");
                    return;
                }
            }

            if (needleMode == NeedleMode.UI && !needleTransform)
                return;
            else if (needleMode == NeedleMode.World3D && !needle3DTransform)
                return;

            isInitialized = true;
        }

        float CalculateAngle(float value)
        {
            float normalizedValue = Mathf.InverseLerp(minValue, maxValue, value);
            float angleT = useCurveValue ? scaleCurve.Evaluate(normalizedValue) : normalizedValue;
            return Mathf.Lerp(minAngle, maxAngle, angleT);
        }

        void ApplyAngle(float angle)
        {
            if (needleMode == NeedleMode.UI)
            {
                if (needleTransform != null)
                    needleTransform.localEulerAngles = new Vector3(0f, 0f, angle);
            }
            else
            {
                if (needle3DTransform != null)
                    needle3DTransform.localEulerAngles = rotationAxis * angle;
            }
        }
    }
}
