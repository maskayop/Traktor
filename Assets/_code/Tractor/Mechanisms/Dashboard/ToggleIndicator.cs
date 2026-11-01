using UnityEngine;

namespace Tractor
{
    public class ToggleIndicator : MonoBehaviour
    {
        [Header("Источник данных")]
        public TractorController.TractorBehavior tractorBehavior = TractorController.TractorBehavior.Main;
        public string variableName = "";

        [Header("Объекты состояния")]
        public GameObject onObject;     // показывается, когда переменная == 1
        public GameObject offObject;    // показывается, когда переменная == 0
        public bool invertCondition = false; // если true — логика меняется местами

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

            bool state = targetCheckable.GetVariableValue(variableName) > 0.5f;

            if (invertCondition)
                state = !state;

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

            // Инициализируем начальное состояние сразу
            bool state = targetCheckable.GetVariableValue(variableName) > 0.5f;
            if (invertCondition) state = !state;
            lastState = state;
            ApplyState(state);
        }
    }
}
