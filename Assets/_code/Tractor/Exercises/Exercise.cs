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

        void Start()
        {
            exercisesController = ExercisesController.Instance;
            exercisesController?.AddExercise(this);
        }
    }
}
