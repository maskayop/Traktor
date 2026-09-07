using UnityEngine;

namespace Tractor
{
    public class ExerciseStep : MonoBehaviour
    {
        [Header("Описание шага")]

        [TextArea(1, 50)]
        public string stepDescription;
    }
}
