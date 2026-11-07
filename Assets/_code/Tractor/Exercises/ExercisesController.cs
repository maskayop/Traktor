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
        public int currentExerciseId = -1;
        ExerciseStep currentStep;

        public bool isCompleted = false;
        public bool IsCompleted { get { return isCompleted; } set { isCompleted = value; } }

        Report currentReport;
        public Report CurrentReport { get { return currentReport; } set { currentReport = value; } }

        float currentExerciseTime = 0;

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
            if (!IsExercise() || !currentExercise)
                return;

            if (currentExercise.GetCurrentExerciseStep())
            {
                currentStep = currentExercise.GetCurrentExerciseStep();

                if (currentStep.IsCompleted())
                {
                    currentStep.result = ExerciseStep.StepResult.Done;
                    currentStep.SetCurrentStepTime(currentExerciseTime);
                    AddCurrentStepReport(currentStep);
                    currentExercise.StartNextStep();
                }
            }

            currentExerciseTime += Time.deltaTime;
        }

        public void Init()
        {
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
            if (currentExerciseId < 0 || currentExerciseId >= exercises.Count)
                return;

            isCompleted = false;
            exercises[currentExerciseId].StartExercise();

            // Сбрасываем флаг "записан" у всех шагов и под-шагов
            for (int i = 0; i < exercises[currentExerciseId].steps.Count; i++)
            {
                var step = exercises[currentExerciseId].steps[i];
                if (step == null) continue;

                step.reported = false;

                var additional = step.GetAdditionalObjects();
                if (additional != null)
                {
                    for (int a = 0; a < additional.additionalConditions.Count; a++)
                    {
                        var subStep = additional.additionalConditions[a];
                        if (subStep != null) subStep.reported = false;
                    }
                }
            }

            currentReport = new Report();
            PrepareReport();
        }

        public Exercise GetCurrentExercise()
        {
            return currentExercise;
        }

        public void ExerciseForceExit()
        {
            ExerciseForceExitInternal(false);
        }

        public void CompleteExercise()
        {
            isCompleted = true;
            ExerciseForceExitInternal(true);
        }

        void ExerciseForceExitInternal(bool keepReport)
        {
            currentExercise = null;
            currentExerciseId = -1;
            currentStep = null;

            if (!keepReport)
                currentReport = null;

            for (int i = 0; i < exercises.Count; i++)
            {
                if (exercises[i] == null) continue;

                exercises[i].EnableAdditionalGameObjects(false);

                for (int s = 0; s < exercises[i].steps.Count; s++)
                {
                    var step = exercises[i].steps[s];
                    if (step == null) continue;

                    step.EnableDestinationMarker(false);
                    step.EnableAdditionalGameObjects(false);
                }
            }
        }

        public bool IsExercise()
        {
            if (currentExerciseId != -1)
                return true;
            else
                return false;
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

        public void AddCurrentStepReport(ExerciseStep step)
        {
            if (step == null || currentReport == null) return;
            if (step.reported) return;

            step.reported = true;

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

        public void AddErrorToReport(ErrorDetector.ErrorState state, string message)
        {
            if (currentReport == null || !IsExercise()) return;

            // Определяем, как записать результат: Warning / Error / Penalty
            string resultString;
            switch (state)
            {
                case ErrorDetector.ErrorState.Warning:
                    resultString = warningResultFormat;
                    break;
                case ErrorDetector.ErrorState.Error:
                    resultString = errorResultFormat;
                    break;
                case ErrorDetector.ErrorState.Penalty:
                    resultString = penaltyResultFormat;
                    break;
                default:
                    resultString = errorResultFormat;
                    break;
            }

            // Добавляем строку в те же колонки, что и шаги
            currentReport.columns[0].values.Add(FormatCurrentExerciseTime(currentExerciseTime));
            currentReport.columns[1].values.Add(resultString);
            currentReport.columns[2].values.Add(message);
        }
    }
}
