using System.Collections.Generic;
using UnityEngine;

namespace Tractor
{
    public class ExercisesController : MonoBehaviour
    {
        public static ExercisesController Instance;

        public List<Exercise> exercises = new List<Exercise>();

        [Header("Info")]
        public Exercise currentExercise;
        public int currentExerciseId = -1;
        public ExerciseStep currentStep;
        public int currentStepId = -1;

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

        void Update()
        {
            if (!IsExercise())
                return;

            if (!currentExercise)
                return;

            if (currentExercise.GetCurrentExerciseStep())
            {
                currentStepId = currentExercise.GetCurrentExerciseStepId();
                currentStep = currentExercise.GetCurrentExerciseStep();

                if (currentStep.IsCompleted())
                    currentExercise.StartNextStep();
            }
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

            currentStep = null;
            currentStepId = -1;
        }

        public bool IsExercise()
        {
            if (currentExerciseId != -1)
                return true;
            else
                return false;
        }
    }
}
