using System.Collections.Generic;
using TMPro;
using UnityEngine;

namespace Tractor.UI
{
    public class UIExerciseWindow : MonoBehaviour
    {
        public static UIExerciseWindow Instance;

        [Header("Окно выбора задания")]
        [SerializeField] GameObject preparingWindow;
        [SerializeField] GameObject exerciseButtonPrefab;
        [SerializeField] RectTransform exerciseButtonsContainer;
        [SerializeField] GameObject startExerciseButton;
        [SerializeField] GameObject exercisePreparingWindowButton;
        [SerializeField] TextMeshProUGUI exerciseDescriptionText;
        List<UIExerciseButton> exerciseButtons = new List<UIExerciseButton>();

        [Header("Окно текущего задания")]
        [SerializeField] GameObject currentExerciseWindow;
        [SerializeField] TextMeshProUGUI currentExerciseNameText;
        [SerializeField] GameObject exerciseStepTextPrefab;
        [SerializeField] GameObject exerciseSubStepTextPrefab;
        [SerializeField] RectTransform exerciseStepTextsContainer;
        List<UIExerciseStepText> exerciseStepTexts = new List<UIExerciseStepText>();

        [Header("Компактное окно текущего задания")]
        [SerializeField] GameObject currentExerciseWindowCompact;
        [SerializeField] TextMeshProUGUI currentExerciseNameTextCompact;
        [SerializeField] RectTransform exerciseStepTextsContainerCompact;
        List<UIExerciseStepText> exerciseStepTextsCompact = new List<UIExerciseStepText>();

        [Header("Окно завершения задания")]
        [SerializeField] GameObject exerciseCompleteWindow;

        bool preparingWindowIsOpen = false;
        public bool PreparingWindowIsOpen { get { return preparingWindowIsOpen; } }

        bool currentExerciseWindowIsOpen = false;
        public bool CurrentExerciseWindowIsOpen { get { return currentExerciseWindowIsOpen; } }

        ExercisesController exercisesController;
        Exercise currentExercise;

        int currentStep = -1;
        int previousStep = -1;

        void Awake()
        {
            if (Instance != null)
            {
                Debug.LogWarning("Cannot create UIExerciseWindow");
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
            if (!exercisesController)
                return;

            if (exercisesController.IsCompleted)
                OpenExerciseCompleteWindow();

            if (!exercisesController.IsExercise())
                return;

            if (currentExercise == null)
                return;

            if (currentExercise.GetCurrentExerciseStep() == null)
                return;

            currentStep = currentExercise.GetCurrentStepId();

            if (currentStep != previousStep)
                ChangeStep();

            previousStep = currentStep;
        }

        public void Init()
        {
            exercisesController = ExercisesController.Instance;

            CreateExerciseButtons();

            CloseCurrentExerciseWindow();
            ClosePreparingWindow();
            ShowCurrentExerciseWindowCompact(false);

            currentStep = 0;
            previousStep = -1;
        }

        public void OpenPreparingWindow()
        {
            preparingWindowIsOpen = true;
            preparingWindow.SetActive(true);

            CloseCurrentExerciseWindow();
            CloseExerciseCompleteWindow();
            HideShowStartExerciseButton();

            if (exercisesController.GetCurrentExercise() == null)
                startExerciseButton.SetActive(false);

            exercisePreparingWindowButton.SetActive(false);
        }

        public void ClosePreparingWindow()
        {
            preparingWindowIsOpen = false;
            preparingWindow.SetActive(false);
            exercisePreparingWindowButton.SetActive(true);
        }

        public void OpenCurrentExerciseWindow()
        {
            ClosePreparingWindow();
            CloseExerciseCompleteWindow();

            currentExerciseWindowIsOpen = true;
            currentExerciseWindow.SetActive(true);
            exercisePreparingWindowButton.SetActive(false);
        }

        public void CloseCurrentExerciseWindow()
        {
            currentExerciseWindowIsOpen = false;
            currentExerciseWindow.SetActive(false);
            exercisePreparingWindowButton.SetActive(true);
            ShowCurrentExerciseWindowCompact(false);
        }

        public void OpenExerciseCompleteWindow()
        {
            ClosePreparingWindow();
            CloseCurrentExerciseWindow();

            exerciseCompleteWindow.SetActive(true);
        }

        public void CloseExerciseCompleteWindow()
        {
            exerciseCompleteWindow.SetActive(false);
            ShowCurrentExerciseWindowCompact(false);
        }

        public void SelectExercise(Exercise exercise)
        {
            if (exercise == null)
            {
                foreach (var eb in exerciseButtons)
                    eb.Select(false);

                exerciseDescriptionText.text = "";

                return;
            }

            exercisesController.SelectExercise(exercise);
            exerciseDescriptionText.text = exercise.exerciseDescription;

            for (int i = 0; i < exerciseButtons.Count; i++)
            {
                if (exerciseButtons[i].Exercise == exercise)
                    exerciseButtons[i].Select(true);
                else
                    exerciseButtons[i].Select(false);
            }

            HideShowStartExerciseButton();
        }

        public void StartExercise()
        {
            exercisesController.StartExercise();

            ClosePreparingWindow();
            OpenCurrentExerciseWindow();
            ShowCurrentExerciseWindowCompact(false);

            currentExercise = exercisesController.GetCurrentExercise();
            currentExerciseNameText.text = currentExercise.exerciseName;
            currentExerciseNameTextCompact.text = currentExercise.exerciseName;

            CreateExerciseStepTexts();
        }

        void CreateExerciseButtons()
        {
            exerciseButtons.Clear();

            foreach (Transform t in exerciseButtonsContainer)
                Destroy(t.gameObject);

            if (!exercisesController)
                return;

            for (int i = 0; i < exercisesController.exercises.Count; i++)
            {
                GameObject go = Instantiate(exerciseButtonPrefab, exerciseButtonsContainer);
                go.name = exercisesController.exercises[i].exerciseName;

                UIExerciseButton eb = go.GetComponent<UIExerciseButton>();
                eb.Init(exercisesController.exercises[i]);
                exerciseButtons.Add(eb);
            }
        }

        void HideShowStartExerciseButton()
        {
            if (exercisesController.GetCurrentExercise() == null)
                startExerciseButton.SetActive(false);
            else
                startExerciseButton.SetActive(true);
        }

        public void ExerciseForceExit()
        {
            exercisesController.isCompleted = false;
            exercisesController.ExerciseForceExit();
            exercisesController.CurrentReport = null;
            CloseCurrentExerciseWindow();
            SelectExercise(null);
        }

        void CreateExerciseStepTexts()
        {
            exerciseStepTexts.Clear();
            exerciseStepTextsCompact.Clear();

            foreach (Transform t in exerciseStepTextsContainer)
                Destroy(t.gameObject);

            foreach (Transform t in exerciseStepTextsContainerCompact)
                Destroy(t.gameObject);

            if (currentExercise == null)
                return;

            for (int i = 0; i < currentExercise.steps.Count; i++)
            {
                GameObject go = Instantiate(exerciseStepTextPrefab, exerciseStepTextsContainer);
                GameObject goco = Instantiate(exerciseStepTextPrefab, exerciseStepTextsContainerCompact);
                go.name = goco.name = currentExercise.steps[i].stepDescription;

                UIExerciseStepText est = go.GetComponent<UIExerciseStepText>();
                est.Init(currentExercise, currentExercise.steps[i]);
                exerciseStepTexts.Add(est);

                UIExerciseStepText estco = goco.GetComponent<UIExerciseStepText>();
                estco.Init(currentExercise, currentExercise.steps[i]);
                exerciseStepTextsCompact.Add(estco);

                ExerciseAdditionalObjects add = currentExercise.steps[i].GetComponent<ExerciseAdditionalObjects>();

                if (add)
                {
                    CreateSubStepTexts(est, add, exerciseStepTextsContainer);
                    CreateSubStepTexts(estco, add, exerciseStepTextsContainerCompact);
                }
            }
        }

        void CreateSubStepTexts(UIExerciseStepText stepText, ExerciseAdditionalObjects additional, Transform container)
        {
            for (int i = 0; i < additional.additionalConditions.Count; i++)
            {
                GameObject go = Instantiate(exerciseSubStepTextPrefab, container);
                go.name = additional.additionalConditions[i].stepDescription;

                UIExerciseStepText est = go.GetComponent<UIExerciseStepText>();
                est.Init(currentExercise, additional.additionalConditions[i]);

                stepText.AddSubStepText(est);
            }
        }

        void ChangeStep()
        {
            exerciseStepTexts[currentStep].SetCurrent(true);
            exerciseStepTextsCompact[currentStep].SetCurrent(true);

            List<UIExerciseStepText> subStepsList = exerciseStepTexts[currentStep].GetSubStepTexts();
            List<UIExerciseStepText> subStepsListCompact = exerciseStepTextsCompact[currentStep].GetSubStepTexts();

            for (int i = 0; i < subStepsList.Count; i++)
            {
                subStepsList[i].SetCurrent(true);
                subStepsListCompact[i].SetCurrent(true);
            }

            if (currentStep - 1 >= 0)
            {
                float currentTime = exercisesController.GetCurrentExerciseTime();

                exerciseStepTexts[currentStep - 1].SetCompleted(true);
                exerciseStepTextsCompact[currentStep - 1].SetCompleted(true);

                exerciseStepTexts[currentStep - 1].SetCurrentStepTime(currentTime);
                exerciseStepTextsCompact[currentStep - 1].SetCurrentStepTime(currentTime);

                subStepsList = exerciseStepTexts[currentStep - 1].GetSubStepTexts();
                subStepsListCompact = exerciseStepTextsCompact[currentStep - 1].GetSubStepTexts();

                for (int i = 0; i < subStepsList.Count; i++)
                {
                    subStepsList[i].SetCurrentStepTime(currentTime);
                    subStepsListCompact[i].SetCurrentStepTime(currentTime);

                    if (subStepsList[i].Step.IsCompleted())
                    {
                        subStepsList[i].SetCompleted(true);
                        subStepsListCompact[i].SetCompleted(true);

                        subStepsList[i].Step.result = ExerciseStep.StepResult.Done;
                        subStepsListCompact[i].Step.result = ExerciseStep.StepResult.Done;
                    }
                    else
                    {
                        subStepsList[i].SetFailed(true);
                        subStepsListCompact[i].SetFailed(true);

                        subStepsList[i].Step.result = ExerciseStep.StepResult.Penalty;
                        subStepsListCompact[i].Step.result = ExerciseStep.StepResult.Penalty;
                    }
                }
            }

            foreach (var t in exerciseStepTextsCompact)
            {
                foreach (var sst in t.GetSubStepTexts())
                    sst.gameObject.SetActive(false);
            }

            for (int i = 0; i < exerciseStepTextsCompact.Count; i++)
            {
                if (i == currentStep || i == currentStep + 1 || i == currentStep - 1 && currentStep - 1 >= 0)
                {
                    exerciseStepTextsCompact[i].gameObject.SetActive(true);

                    foreach (var sst in exerciseStepTextsCompact[i].GetSubStepTexts())
                        sst.gameObject.SetActive(true);
                }
                else
                    exerciseStepTextsCompact[i].gameObject.SetActive(false);
            }
        }

        public void ShowCurrentExerciseWindow(bool state)
        {
            currentExerciseWindow.SetActive(state);
        }

        public void ShowCurrentExerciseWindowCompact(bool state)
        {
            currentExerciseWindowCompact.SetActive(state);
        }
    }
}
