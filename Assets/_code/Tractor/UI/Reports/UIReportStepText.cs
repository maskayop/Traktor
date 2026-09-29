using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Tractor.UI
{
    public class UIReportStepText : MonoBehaviour
    {
        [Header("Colors")]
        [SerializeField] Image image;
        [SerializeField] Color doneColor = Color.white;
        [SerializeField] Color warningColor = Color.white;
        [SerializeField] Color errorColor = Color.white;
        [SerializeField] Color penaltyColor = Color.white;

        [Header("Texts")]
        [SerializeField] TextMeshProUGUI timeText;
        [SerializeField] TextMeshProUGUI resultText;
        [SerializeField] TextMeshProUGUI descriptionText;

        Color defaultColor = Color.white;

        ExercisesController exercisesController;

        public void Init(string INtime, string INresult, string INdescription)
        {
            exercisesController = ExercisesController.Instance;

            if (!exercisesController)
                return;

            timeText.text = INtime;
            resultText.text = INresult;
            descriptionText.text = INdescription;

            ExerciseStep.StepResult result = exercisesController.GetStepResultByString(INresult);

            if (result == ExerciseStep.StepResult.No)
                SetDone(false);
            else if (result == ExerciseStep.StepResult.Done)
                SetDone(true);
            else if (result == ExerciseStep.StepResult.Warning)
                SetWarning(true);
            else if (result == ExerciseStep.StepResult.Error)
                SetError(true);
            else if (result == ExerciseStep.StepResult.Penalty)
                SetPenalty(true);
        }

        public void SetDone(bool state)
        {
            if (state)
                image.color = doneColor;
            else
                image.color = defaultColor;
        }

        public void SetWarning(bool state)
        {
            if (state)
                image.color = warningColor;
            else
                image.color = defaultColor;
        }

        public void SetError(bool state)
        {
            if (state)
                image.color = errorColor;
            else
                image.color = defaultColor;
        }

        public void SetPenalty(bool state)
        {
            if (state)
                image.color = penaltyColor;
            else
                image.color = defaultColor;
        }
    }
}
