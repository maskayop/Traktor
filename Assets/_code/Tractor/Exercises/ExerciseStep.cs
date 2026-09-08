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

        public void Init(TractorController targetController)
        {
            if (targetController == null)
                return;

            tractorController = targetController;
        }

        public void StartStep()
        {

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
    }
}
