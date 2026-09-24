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
        ExerciseAdditionalObjects exerciseAdditionalObjects;

        void Start()
        {
            exerciseAdditionalObjects = GetComponent<ExerciseAdditionalObjects>();
            EnableAdditionalGameObjects(false);
        }

        public void Init(ExercisesController INexercisesController)
        {
            exercisesController = INexercisesController;

            if (!exercisesController || !tractorController)
                return;

            InitSteps();
        }

        public void StartExercise()
        {
            if (steps.Count == 0)
                return;

            tractorController = FindAnyObjectByType<TractorController>();

            if (tractorController == null)
                return;

            currentStepId = 0;
            currentStep = steps[currentStepId];

            tractorController.ResetTractor();
            tractorController.PlaceTractor(spawnPoint);

            EnableAdditionalGameObjects(true);
            InitSteps();
        }

        public void StartNextStep()
        {
            currentStepId++;

            if (currentStepId < steps.Count)
                currentStep = steps[currentStepId];
            else
                CompleteExercise();

            InitSteps();
        }

        public void CompleteExercise()
        {
            currentStep = null;
            currentStepId = -1;

            exercisesController.CompleteExercise();
        }

        void InitSteps()
        {
            foreach (var step in steps)
                step.Init(tractorController);

            if (!tractorController)
                return;

            if (currentStepId == -1)
                return;

            if (steps[currentStepId].tractorBehavior == TractorController.TractorBehavior.Main)
            {
                Transform destinationTransform = null;

                if (steps[currentStepId].GetAdditionalObjects())
                    destinationTransform = steps[currentStepId].GetAdditionalObjects().GetDestinationMarkerTransform();

                if (destinationTransform)
                    tractorController.DestinationTransform = destinationTransform;
                else
                    tractorController.DestinationTransform = null;
            }
            else
                tractorController.DestinationTransform = null;

            steps[currentStepId].EnableDestinationMarker(true);
            steps[currentStepId].EnableAdditionalGameObjects(true);
        }

        public ExerciseStep GetCurrentExerciseStep()
        {
            return currentStep;
        }

        public int GetCurrentExerciseStepId()
        {
            return currentStepId;
        }

        public void EnableAdditionalGameObjects(bool state)
        {
            if (!exerciseAdditionalObjects)
                return;

            exerciseAdditionalObjects.EnableAdditionalGameObjects(state);
        }
    }
}
