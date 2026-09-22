using System.Collections.Generic;
using UnityEngine;

namespace Tractor
{
    public class ExerciseAdditionalObjects : MonoBehaviour
    {
        public GameObject destinationMarker;
        public List<GameObject> gameObjectsToEnable = new List<GameObject>();

        [Header("Дополнительные условия")]
        public List<ExerciseStep> additionalConditions = new List<ExerciseStep>();

        public void EnableAdditionalGameObjects(bool state)
        {
            for (int i = 0; i < gameObjectsToEnable.Count; i++)
                if (gameObjectsToEnable[i])
                    gameObjectsToEnable[i].SetActive(state);
        }
    }
}
