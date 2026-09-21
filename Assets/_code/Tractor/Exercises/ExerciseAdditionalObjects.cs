using System.Collections.Generic;
using UnityEngine;

namespace Tractor
{
    public class ExerciseAdditionalObjects : MonoBehaviour
    {
        public GameObject destinationMarker;
        public List<GameObject> gameObjectsToEnable = new List<GameObject>();
        public List<GameObject> gameObjectsToDisable = new List<GameObject>();
    }
}
