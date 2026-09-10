using System.Collections.Generic;
using UnityEngine;

namespace Tractor
{
    public class Exercise : MonoBehaviour
    {
        public int id;

        [Header("Трансформы")]
        public Transform spawnPoint;

        [Header("Описание задания")]
        public string exerciseName;

        [TextArea(1, 50)]
        public string exerciseDescription;

        [Header("Шаги")]
        public List<ExerciseStep> steps = new List<ExerciseStep>();

        ExercisesController exercisesController;
        ExerciseStep currentStep;
        int currentStepId;

        TractorController tractorController;

        void Start()
        {
            exercisesController = ExercisesController.Instance;
            exercisesController?.AddExercise(this);
        }

        public void StartExercise()
        {
            if (steps.Count == 0)
                return;

            tractorController = FindAnyObjectByType<TractorController>();

            if (tractorController == null)
                return;

            foreach (var step in steps)
                step.Init(tractorController);

            currentStepId = 0;
            currentStep = steps[currentStepId];

            tractorController.ResetTractor();
            RCCP.Transport(tractorController.CarController, spawnPoint.position, spawnPoint.rotation);
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
