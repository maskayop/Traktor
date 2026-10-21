#if UNITY_EDITOR
using UnityEngine;

namespace Vopere.Editor
{
    public class ChildTransformsRandomizerHelper : MonoBehaviour
    {
        [Header("Scale")]
        public Vector3 scaleMin = Vector3.one;
        public Vector3 scaleMax = Vector3.one;

        [Header("Rotation")]
        public Vector3 rotationMin = Vector3.zero;
        public Vector3 rotationMax = Vector3.zero;
    }
}
#endif
