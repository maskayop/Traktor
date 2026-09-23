using System.Collections.Generic;
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
        [SerializeField] Color failedColor = Color.white;
        [SerializeField] TextMeshProUGUI descriptionText;

        ExerciseStep step;
        public ExerciseStep Step { get { return step; } }

        List<UIExerciseStepText> subStepTexts = new List<UIExerciseStepText>();

        public void Init(Exercise INexercise, ExerciseStep INstep)
        {
            step = INstep;
            descriptionText.text = step.stepDescription;

            defaultColor = image.color;

            SetCompleted(false);
            SetCurrent(false);
            SetFailed(false);

            subStepTexts.Clear();
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
            if (state)
                image.color = completedColor;
            else
                image.color = defaultColor;
        }

        public void SetFailed(bool state)
        {
            if (state)
                image.color = failedColor;
            else
                image.color = defaultColor;
        }

        public void AddSubStepText(UIExerciseStepText item)
        {
            subStepTexts.Add(item);
        }

        public List<UIExerciseStepText> GetSubStepTexts()
        {
            return subStepTexts;
        }
    }
}
