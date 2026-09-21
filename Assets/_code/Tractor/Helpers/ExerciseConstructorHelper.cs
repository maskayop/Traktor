using UnityEngine;

namespace Tractor
{
    public class ExerciseConstructorHelper : MonoBehaviour
    {
#if UNITY_EDITOR
        Exercise exercise;

        void Reset()
        {

            if (!GetComponent<Exercise>())
                return;
            else
                exercise = GetComponent<Exercise>();

            ResetSteps();
        }

        void ResetSteps()
        {
            exercise.steps.Clear();

            foreach (Transform t in transform)
                if (t.GetComponent<ExerciseStep>())
                    exercise.steps.Add(t.GetComponent<ExerciseStep>());
        }
#endif
    }
}
