using TMPro;
using UnityEngine;

namespace Tractor.UI
{
    public class UIExerciseButton : MonoBehaviour
    {
        [SerializeField] GameObject selected;
        [SerializeField] TextMeshProUGUI nameText;

        bool isSelected = false;
        public bool IsSelected { get { return isSelected; } }

        UIExerciseWindow exerciseWindow;
        Exercise exercise;
        public Exercise Exercise { get { return exercise; } }
        string exerciseName;
        public string ExerciseName { get { return exerciseName; } }

        public void Init(Exercise ex)
        {
            exerciseWindow = UIExerciseWindow.Instance;
            exercise = ex;
            exerciseName = ex.exerciseName;
            nameText.text = exerciseName;
            Select(false);
        }

        public void Init(string name)
        {
            exerciseWindow = UIExerciseWindow.Instance;
            exerciseName = name;
            nameText.text = name;
            Select(false);
        }

        public void Select(bool state)
        {
            isSelected = state;
            if (selected) selected.SetActive(state);
        }

        public void SelectExercise()
        {
            Select(true);
            exerciseWindow?.SelectExerciseByName(exerciseName);
        }
    }
}
