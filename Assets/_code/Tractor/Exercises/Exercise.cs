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
        ExerciseStep currentStep;
        int currentStepId;

        void Start()
        {
            exercisesController = ExercisesController.Instance;
            exercisesController?.AddExercise(this);
        }

        public void StartExercise()
        {
            if (steps.Count == 0)
                return;

            TractorController tractorController = FindAnyObjectByType<TractorController>();

            if (tractorController == null)
                return;

            foreach (var step in steps)
                step.Init(tractorController);

            currentStepId = 0;
            currentStep = steps[currentStepId];
        }

        public void StartNextStep()
        {
            currentStepId++;

            if (currentStepId < steps.Count)
                currentStep = steps[currentStepId];
            else
                CompleteExercise();
        }

        public void CompleteExercise()
        {
            currentStep = null;
            currentStepId = -1;

            exercisesController.CompleteExercise();
        }

        public ExerciseStep GetCurrentExerciseStep()
        {
            return currentStep;
        }

        public int GetCurrentExerciseStepId()
        {
            return currentStepId;
        }
    }
}
