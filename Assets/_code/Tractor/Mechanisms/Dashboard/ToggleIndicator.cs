using UnityEngine;

namespace Tractor
{
    public class ToggleIndicator : MonoBehaviour
    {
        [Header("Источник данных")]
        public TractorController.TractorBehavior tractorBehavior = TractorController.TractorBehavior.Main;
        public string variableName = "";

        [Header("Объекты состояния")]
        public GameObject offObject;    // показывается, когда переменная == 0
        public GameObject onObject;     // показывается, когда переменная == 1
        public bool invertCondition = false; // если true — логика меняется местами

        public enum CompareMode { Boolean, EqualsTo }

        [Header("Режим сравнения")]
        public CompareMode compareMode = CompareMode.Boolean;
        public float targetValue = 0f;   // используется только в режиме EqualsTo

        TractorController tractorController;
        ICheckable targetCheckable;
        bool isInitialized = false;
        bool lastState = false;

        void Start()
        {
            Init();
        }

        void Update()
        {
            if (!isInitialized || targetCheckable == null)
                return;

            bool state = EvaluateState();

            if (state != lastState)
            {
                lastState = state;
                ApplyState(state);
            }
        }

        void ApplyState(bool state)
        {
            if (onObject != null)
                onObject.SetActive(state);

            if (offObject != null)
                offObject.SetActive(!state);
        }

        public void Init()
        {
            if (!tractorController)
                tractorController = FindAnyObjectByType<TractorController>();

            if (!tractorController) return;

            MonoBehaviour targetMono = tractorController.GetMonoBehaviour(tractorBehavior);

            if (!targetMono)
            {
                Debug.LogError($"UIToggleIndicator: модуль {tractorBehavior} не найден!");
                return;
            }

            targetCheckable = targetMono as ICheckable;

            if (targetCheckable == null)
            {
                Debug.LogError($"UIToggleIndicator: модуль {tractorBehavior} не реализует ICheckable!");
                return;
            }

            if (string.IsNullOrEmpty(variableName))
            {
                Debug.LogError("UIToggleIndicator: variableName пустой!");
                return;
            }

            if (!targetCheckable.HasVariable(variableName))
            {
                Debug.LogError($"UIToggleIndicator: переменная '{variableName}' не найдена в модуле {tractorBehavior}!");
                return;
            }

            isInitialized = true;

            lastState = EvaluateState();
            ApplyState(lastState);
        }

        bool EvaluateState()
        {
            float value = targetCheckable.GetVariableValue(variableName);

            bool state;

            if (compareMode == CompareMode.Boolean)
                state = value > 0.5f;
            else
                state = Mathf.Approximately(value, targetValue);

            if (invertCondition)
                state = !state;

            return state;
        }
    }
}
