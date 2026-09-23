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

        public void EnableDestinationMarker(bool state)
        {
            destinationMarker.SetActive(state);
        }

        public void EnableAdditionalGameObjects(bool state)
        {
            for (int i = 0; i < gameObjectsToEnable.Count; i++)
                if (gameObjectsToEnable[i])
                    gameObjectsToEnable[i].SetActive(state);
        }

        public void InitAdditionalConditions(TractorController tractorController)
        {
            for (int i = 0; i < additionalConditions.Count; i++)
                additionalConditions[i].Init(tractorController);
        }

        public Transform GetDestinationMarkerTransform()
        {
            if (destinationMarker)
                return destinationMarker.transform;
            else
                return null;
        }
    }
}
