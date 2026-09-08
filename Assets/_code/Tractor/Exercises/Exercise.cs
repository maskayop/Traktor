using System.Collections.Generic;
using UnityEngine;

namespace Tractor
{
    public class Exercise : MonoBehaviour
    {
        public int id;

        [Header("Описание задания")]
        public string exerciseName;

        [TextArea(1, 50)]
        public string exerciseDescription;

        [Header("Описание задания")]
        public List<ExerciseStep> steps = new List<ExerciseStep>();

        ExercisesController exercisesController;
        ExerciseStep currentExerciseStep;
        int currentStep;

        void Start()
        {
            exercisesController = ExercisesController.Instance;
            exercisesController?.AddExercise(this);
        }

        public void StartExercise()
        {
            if (steps.Count == 0)
                return;

            currentExerciseStep = steps[0];
            currentStep = 0;
        }

        public void NextSTep()
        {
            currentStep++;

            if (steps.Count < currentStep)
                currentExerciseStep = steps[currentStep];
            else
            {
                currentExerciseStep = null;
                currentStep = 0;
                return;
            }

            steps[currentStep].StartStep();
        }

        public ExerciseStep GetCurrentExerciseStep()
        {
            return currentExerciseStep;
        }
    }
}
