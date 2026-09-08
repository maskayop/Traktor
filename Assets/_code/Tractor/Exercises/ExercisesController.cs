using System.Collections.Generic;
using UnityEngine;

namespace Tractor
{
    public class ExercisesController : MonoBehaviour
    {
        public static ExercisesController Instance;

        public List<Exercise> exercises = new List<Exercise>();

        Exercise currentExercise;
        int currentExerciseId = -1;

        void Awake()
        {
            if (Instance != null)
            {
                Debug.LogWarning("Cannot create ExercisesController");
                Destroy(gameObject);
                return;
            }

            Instance = this;
        }

        void Start()
        {
            Init();
        }

        public void Init()
        {
            exercises.Clear();
        }

        public void AddExercise(Exercise exercise)
        {
            for (int i = 0; i <= exercise.id; i++)
            {
                if (exercises.Count < exercise.id)
                    exercises.Add(null);
            }

            exercises[exercise.id - 1] = exercise;
        }

        public void SelectExercise(Exercise exercise)
        {
            currentExercise = exercise;

            for (int i = 0; i < exercises.Count; i++)
                if (exercises[i] == exercise)
                    currentExerciseId = i;
        }

        public void StartExercise()
        {
            exercises[currentExerciseId].StartExercise();
        }

        public Exercise GetCurrentExercise()
        {
            return currentExercise;
        }

        public void ExerciseForceExit()
        {
            currentExercise = null;
            currentExerciseId = -1;
        }
    }
}
