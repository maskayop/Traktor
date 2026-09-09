using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Tractor.UI
{
    public class UIExerciseStepText : MonoBehaviour
    {
        [SerializeField] Image image;
        [SerializeField] Color defaultColor = Color.white;
        [SerializeField] Color currentColor = Color.white;
        [SerializeField] Color completedColor = Color.white;
        [SerializeField] TextMeshProUGUI descriptionText;

        bool isComplete = false;

        ExerciseStep step;
        public ExerciseStep Step { get { return step; } }

        public void Init(Exercise INexercise, ExerciseStep INstep)
        {
            step = INstep;
            descriptionText.text = step.stepDescription;

            defaultColor = image.color;

            SetCompleted(false);
            SetCurrent(false);
        }

        public void SetCurrent(bool state)
        {
            if (state)
                image.color = currentColor;
            else
                image.color = defaultColor;
        }

        public void SetCompleted(bool state)
        {
            isComplete = state;

            if (state)
                image.color = completedColor;
            else
                image.color = defaultColor;
        }
    }
}
