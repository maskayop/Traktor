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
        List<UIExerciseButton> exerciseButtons = new List<UIExerciseButton>();

        [Header("Окно текущего задания")]
        [SerializeField] GameObject currentExerciseWindow;
        [SerializeField] TextMeshProUGUI currentExerciseNameText;
        [SerializeField] GameObject exerciseStepTextPrefab;
        [SerializeField] RectTransform exerciseStepTextsContainer;
        List<UIExerciseStepText> exerciseStepText = new List<UIExerciseStepText>();

        bool preparingWindowIsOpen = false;
        public bool PreparingWindowIsOpen { get { return preparingWindowIsOpen; } }

        bool currentExerciseWindowIsOpen = false;
        public bool CurrentExerciseWindowIsOpen { get { return currentExerciseWindowIsOpen; } }

        ExercisesController exercisesController;
        Exercise currentExercise;

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

        }

        public void Init()
        {
            exercisesController = ExercisesController.Instance;

            CreateExerciseButtons();

            CloseCurrentExerciseWindow();
            ClosePreparingWindow();
        }

        public void OpenPreparingWindow()
        {
            preparingWindowIsOpen = true;
            preparingWindow.SetActive(true);

            CloseCurrentExerciseWindow();
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

        public void SelectExercise(Exercise exercise)
        {
            if (exercise == null)
            {
                foreach (var eb in exerciseButtons)
                    eb.Select(false);

                return;
            }

            exercisesController.SelectExercise(exercise);

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
            exercisesController.ExerciseForceExit();
            CloseCurrentExerciseWindow();
            SelectExercise(null);
        }

        void CreateExerciseStepTexts()
        {
            exerciseStepText.Clear();

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
                exerciseStepText.Add(est);
            }
        }
    }
}
