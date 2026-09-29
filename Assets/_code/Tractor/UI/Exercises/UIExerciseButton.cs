using TMPro;
using UnityEngine;

namespace Tractor.UI
{
    public class UIExerciseButton : MonoBehaviour
    {
        [SerializeField] GameObject selected;
        [SerializeField] TextMeshProUGUI nameText;

        bool isSelected = false;

        UIExerciseWindow exerciseWindow;
        Exercise exercise;
        public Exercise Exercise { get { return exercise; } }

        public void Init(Exercise INexercise)
        {
            exerciseWindow = UIExerciseWindow.Instance;

            exercise = INexercise;
            nameText.text = exercise.exerciseName;

            Select(false);
        }

        public void Select(bool state)
        {
            isSelected = state;
            selected.SetActive(isSelected);
        }

        public void SelectExercise()
        {
            Select(true);
            exerciseWindow.SelectExercise(exercise);
        }
    }
}
