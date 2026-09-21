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

            InitStepAdditionalObjects();
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

            InitStepAdditionalObjects();
        }

        public void CompleteExercise()
        {
            currentStep = null;
            currentStepId = -1;

            exercisesController.CompleteExercise();
        }

        void InitStepAdditionalObjects()
        {
            foreach (var step in steps)
            {
                if (step.GetComponent<ExerciseStepAdditionalObjects>())
                    if (step.GetComponent<ExerciseStepAdditionalObjects>().destinationMarker)
                        step.GetComponent<ExerciseStepAdditionalObjects>().destinationMarker.SetActive(false);
            }

            if (!tractorController)
                return;

            if (currentStepId == -1)
                return;

            if (steps[currentStepId].tractorBehavior == TractorController.TractorBehavior.Main)
            {
                ExerciseStepAdditionalObjects additional = steps[currentStepId].GetComponent<ExerciseStepAdditionalObjects>();

                if (additional)
                {
                    if (additional.destinationMarker)
                    {
                        tractorController.DestinationTransform = additional.destinationMarker.transform;
                        additional.destinationMarker.SetActive(true);
                    }
                }
                else
                    tractorController.DestinationTransform = null;
            }
            else
                tractorController.DestinationTransform = null;
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
