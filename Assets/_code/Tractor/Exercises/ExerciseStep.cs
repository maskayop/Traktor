using UnityEngine;
using static Tractor.TractorController;

namespace Tractor
{
    public class ExerciseStep : MonoBehaviour
    {
        public TractorBehavior tractorBehavior;
        public string variableName;
        public float conditionValue;

        public enum ConditionComparison { Equal, NotEqual, Less, LessOrEqual, Greater, GreaterOrEqual };
        public ConditionComparison comparison = ConditionComparison.Equal;

        [Header("Описание шага")]

        [TextArea(1, 50)]
        public string stepDescription;

        TractorController tractorController;
        ExerciseAdditionalObjects additionalObjects;

        void Start()
        {
            additionalObjects = GetComponent<ExerciseAdditionalObjects>();
            ResetAllAdditionalGameObjects();
        }

        public void Init(TractorController targetController)
        {
            if (targetController == null)
                return;

            tractorController = targetController;

            ResetAllAdditionalGameObjects();
            InitAdditionalConditions();
        }

        public bool IsCompleted()
        {
            // Проверяем, что targetObject не null и он реализует ICheckable
            if (tractorController == null)
            {
                Debug.LogError("Целевой объект не назначен!");
                return false;
            }

            // Пытаемся привести targetObject к интерфейсу ICheckable
            ICheckable checkable = tractorController.GetMonoBehaviour(tractorBehavior) as ICheckable;

            if (checkable == null)
            {
                Debug.LogError($"Объект {tractorController.GetMonoBehaviour(tractorBehavior).name} не реализует ICheckable!");
                return false;
            }

            // Поверяем условие
            switch (comparison)
            {
                case ConditionComparison.Equal:
                    if (checkable.GetVariableValue(variableName) == conditionValue)
                        return true;
                    else
                        return false;
                case ConditionComparison.NotEqual:
                    if (checkable.GetVariableValue(variableName) != conditionValue)
                        return true;
                    else
                        return false;
                case ConditionComparison.Less:
                    if (checkable.GetVariableValue(variableName) < conditionValue)
                        return true;
                    else
                        return false;
                case ConditionComparison.LessOrEqual:
                    if (checkable.GetVariableValue(variableName) <= conditionValue)
                        return true;
                    else
                        return false;
                case ConditionComparison.Greater:
                    if (checkable.GetVariableValue(variableName) > conditionValue)
                        return true;
                    else
                        return false;
                case ConditionComparison.GreaterOrEqual:
                    if (checkable.GetVariableValue(variableName) >= conditionValue)
                        return true;
                    else
                        return false;
                default:
                    return false;
            }
        }

        public ExerciseAdditionalObjects GetAdditionalObjects()
        {
            return additionalObjects;
        }

        public void EnableDestinationMarker(bool state)
        {
            if (additionalObjects)
                additionalObjects.EnableDestinationMarker(state);
        }

        public void EnableAdditionalGameObjects(bool state)
        {
            if (additionalObjects)
                additionalObjects.EnableAdditionalGameObjects(state);
        }

        public void ResetAllAdditionalGameObjects()
        {
            EnableDestinationMarker(false);
            EnableAdditionalGameObjects(false);
        }

        public void InitAdditionalConditions()
        {
            if (additionalObjects)
                additionalObjects.InitAdditionalConditions(tractorController);
        }
    }
}
