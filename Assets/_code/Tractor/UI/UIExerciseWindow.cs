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
            if (exercisesController.IsCompleted)
                OpenExerciseCompleteWindow();

            if (!exercisesController.IsExercise())
                return;

            if (currentExercise == null)
                return;

            if (currentExercise.GetCurrentExerciseStep() == null)
                return;

            currentStep = currentExercise.GetCurrentExerciseStepId();

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

            currentExercise = exercisesController.GetCurrentExercise();
            currentExerciseNameText.text = currentExercise.exerciseName;

            CreateExerciseStepTexts();
        }

        void CreateExerciseButtons()
        {
            exerciseButtons.Clear();

            foreach (Transform t in exerciseButtonsContainer)
                Destroy(t.gameObject);

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
            CloseCurrentExerciseWindow();
            SelectExercise(null);
        }

        void CreateExerciseStepTexts()
        {
            exerciseStepTexts.Clear();

            foreach (Transform t in exerciseStepTextsContainer)
                Destroy(t.gameObject);

            if (currentExercise == null)
                return;

            for (int i = 0; i < currentExercise.steps.Count; i++)
            {
                GameObject go = Instantiate(exerciseStepTextPrefab, exerciseStepTextsContainer);
                go.name = currentExercise.steps[i].stepDescription;

                UIExerciseStepText est = go.GetComponent<UIExerciseStepText>();
                est.Init(currentExercise, currentExercise.steps[i]);
                exerciseStepTexts.Add(est);

                ExerciseAdditionalObjects add = currentExercise.steps[i].GetComponent<ExerciseAdditionalObjects>();

                if (add)
                    CreateSubStepTexts(est, add);
            }
        }

        void CreateSubStepTexts(UIExerciseStepText stepText, ExerciseAdditionalObjects additional)
        {
            for (int i = 0; i < additional.additionalConditions.Count; i++)
            {
                GameObject go = Instantiate(exerciseSubStepTextPrefab, exerciseStepTextsContainer);
                go.name = additional.additionalConditions[i].stepDescription;

                UIExerciseStepText est = go.GetComponent<UIExerciseStepText>();
                est.Init(currentExercise, additional.additionalConditions[i]);

                stepText.AddSubStepText(est);
            }
        }

        void ChangeStep()
        {
            exerciseStepTexts[currentStep].SetCurrent(true);

            List<UIExerciseStepText> subStepsList = exerciseStepTexts[currentStep].GetSubStepTexts();

            for (int i = 0; i < subStepsList.Count; i++)
                subStepsList[i].SetCurrent(true);

            if (currentStep - 1 >= 0)
            {
                exerciseStepTexts[currentStep - 1].SetCompleted(true);
                subStepsList = exerciseStepTexts[currentStep - 1].GetSubStepTexts();

                for (int i = 0; i < subStepsList.Count; i++)
                {
                    //bool ok = subStepsList[i].Step.IsCompleted();
                    bool ok = true;

                    if (ok)
                        subStepsList[i].SetCompleted(true);
                    else
                        subStepsList[i].SetFailed(true);
                }
            }
        }
    }
}
