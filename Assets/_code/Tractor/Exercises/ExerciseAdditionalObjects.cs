using System.Collections.Generic;
using UnityEngine;

namespace Tractor
{
    public class ExerciseAdditionalObjects : MonoBehaviour
    {
        public GameObject destinationMarker;
        public List<GameObject> gameObjectsToEnable = new List<GameObject>();

        [Header("Прицеп")]
        [SerializeField] GameObject trailerObject;
        [SerializeField] Transform trailerSpawnPoint;

        [Header("Дополнительные условия")]
        public List<ExerciseStep> additionalConditions = new List<ExerciseStep>();

        public void EnableDestinationMarker(bool state)
        {
            if (!destinationMarker)
                return;

            destinationMarker.SetActive(state);
        }

        public void EnableAdditionalGameObjects(bool state)
        {
            for (int i = 0; i < gameObjectsToEnable.Count; i++)
                if (gameObjectsToEnable[i])
                    gameObjectsToEnable[i].SetActive(state);

            EnableTrailer(state);
            EnableDestinationMarker(state);
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

        public void ResetAllAdditionalConditions()
        {
            for (int i = 0; i < additionalConditions.Count; i++)
            {
                additionalConditions[i].result = ExerciseStep.StepResult.Done;
                additionalConditions[i].SetCurrentStepTime(0);
            }
        }

        public void ResetTrailer()
        {
            if (trailerObject == null) return;

            // Отцепить, если прицеплен
            var rccpTrailer = trailerObject.GetComponent<RCCP_TrailerController>();
            if (rccpTrailer != null)
                rccpTrailer.DetachTrailer();

            // Телепортировать на спаун-поинт
            if (trailerSpawnPoint != null)
            {
                trailerObject.transform.position = trailerSpawnPoint.position;
                trailerObject.transform.rotation = trailerSpawnPoint.rotation;
            }
        }

        public void EnableTrailer(bool state)
        {
            if (trailerObject == null) return;

            if (state)
            {
                ResetTrailer();
            }
            else
            {
                // Перед выключением — отцепить, иначе трактор может улететь
                var rccpTrailer = trailerObject.GetComponent<RCCP_TrailerController>();
                if (rccpTrailer != null)
                    rccpTrailer.DetachTrailer();
            }

            trailerObject.SetActive(state);
        }
    }
}
