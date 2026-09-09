using TMPro;
using UnityEngine;

namespace Tractor.UI
{
    public class UIExerciseStepText : MonoBehaviour
    {
        [SerializeField] GameObject completed;
        [SerializeField] TextMeshProUGUI descriptionText;

        bool isComplete = false;

        Exercise exercise;
        ExerciseStep step;
        public ExerciseStep Step { get { return step; } }

        public void Init(Exercise INexercise, ExerciseStep INstep)
        {
            exercise = INexercise;
            step = INstep;
            descriptionText.text = step.stepDescription;

            Complete(false);
        }

        public void Complete(bool state)
        {
            isComplete = state;
            completed.SetActive(isComplete);
        }
    }
}
