using System.Collections.Generic;
using UnityEngine;
using Vopere;

namespace Tractor
{
    public class ExercisesController : MonoBehaviour
    {
        public static ExercisesController Instance;

        public List<Exercise> exercises = new List<Exercise>();

        [Header("Заголовки отчёта")]
        [SerializeField] string reportTimeHeader;
        [SerializeField] string reportStepResultHeader;
        [SerializeField] string reportStepsHeader;

        [Header("Результаты шагов")]
        [SerializeField] string doneResultFormat;
        [SerializeField] string warningResultFormat;
        [SerializeField] string errorResultFormat;
        [SerializeField] string penaltyResultFormat;

        [Header("Инфо")]
        Exercise currentExercise;
        int currentExerciseId = -1;
        ExerciseStep currentStep;

        public bool isCompleted = false;
        public bool IsCompleted { get { return isCompleted; } set { isCompleted = value; } }

        Report currentReport;
        public Report CurrentReport { get { return currentReport; } set { currentReport = value; } }

        float currentExerciseTime = 0;

        ErrorDetector errorDetector;

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
                currentStep = currentExercise.GetCurrentExerciseStep();

                if (currentStep.IsCompleted())
                {
                    currentStep.result = ExerciseStep.StepResult.Done;
                    currentExercise.StartNextStep();
                }
            }

            currentExerciseTime += Time.deltaTime;
        }

        public void Init()
        {
            errorDetector = ErrorDetector.Instance;

            exercises.Clear();

            Exercise[] allExercises = FindObjectsByType<Exercise>();

            for (int i = 0; i < allExercises.Length; i++)
            {
                AddExercise(allExercises[i]);
                allExercises[i].Init(this);
            }

            PrepareReport();
        }

        void AddExercise(Exercise exercise)
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
            isCompleted = false;
            exercises[currentExerciseId].StartExercise();
            errorDetector.Record = true;
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

            for (int i = 0; i < exercises.Count; i++)
                exercises[i].EnableAdditionalGameObjects(false);

            errorDetector.Record = false;
        }

        public bool IsExercise()
        {
            if (currentExerciseId != -1)
                return true;
            else
                return false;
        }

        public void CompleteExercise()
        {
            isCompleted = true;
            CreateReport();
            ExerciseForceExit();
        }

        public float GetCurrentExerciseTime()
        {
            return currentExerciseTime;
        }

        void PrepareReport()
        {
            if (currentReport == null)
                return;

            currentReport.columns.Clear();
            currentExerciseTime = 0;

            Column c1 = new Column();
            c1.header = reportTimeHeader;
            currentReport.columns.Add(c1);

            Column c2 = new Column();
            c2.header = reportStepResultHeader;
            currentReport.columns.Add(c2);

            Column c3 = new Column();
            c3.header = reportStepsHeader;
            currentReport.columns.Add(c3);
        }

        string FormatCurrentExerciseTime(float INtime)
        {
            int totalSeconds = Mathf.FloorToInt(INtime);
            int minutes = totalSeconds / 60;
            int seconds = totalSeconds % 60;

            return string.Format("{0:00}:{1:00}", minutes, seconds);
        }

        void CreateReport()
        {
            for (int i = 0; i < currentExercise.steps.Count; i++)
            {
                AddCurrentStepReport(currentExercise.steps[i]);

                ExerciseAdditionalObjects additional = currentExercise.steps[i].GetAdditionalObjects();

                if (additional)
                    for (int a = 0; a < additional.additionalConditions.Count; a++)
                        AddCurrentStepReport(additional.additionalConditions[a]);
            }
        }

        void AddCurrentStepReport(ExerciseStep step)
        {
            currentReport.columns[0].values.Add(FormatCurrentExerciseTime(step.GetCurrentStepTime()));
            currentReport.columns[1].values.Add(GetStepResultString(step.result));
            currentReport.columns[2].values.Add(step.stepDescription);
        }

        string GetStepResultString(ExerciseStep.StepResult result)
        {
            switch (result)
            {
                case ExerciseStep.StepResult.No:
                    return "";
                case ExerciseStep.StepResult.Done:
                    return doneResultFormat;
                case ExerciseStep.StepResult.Warning:
                    return warningResultFormat;
                case ExerciseStep.StepResult.Error:
                    return errorResultFormat;
                case ExerciseStep.StepResult.Penalty:
                    return penaltyResultFormat;
                default:
                    return "";
            }
        }

        public ExerciseStep.StepResult GetStepResultByString(string result)
        {
            if (result == doneResultFormat)
                return ExerciseStep.StepResult.Done;
            else if (result == warningResultFormat)
                return ExerciseStep.StepResult.Warning;
            else if (result == errorResultFormat)
                return ExerciseStep.StepResult.Error;
            else if (result == penaltyResultFormat)
                return ExerciseStep.StepResult.Penalty;
            else
                return ExerciseStep.StepResult.No;
        }
    }
}
