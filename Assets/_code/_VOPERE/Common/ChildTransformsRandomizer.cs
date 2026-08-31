#if UNITY_EDITOR
using System.Collections.Generic;
using UnityEngine;

namespace Vopere.Editor
{
    [RequireComponent(typeof(ChildTransformsRandomizerHelper))]
    public class ChildTransformsRandomizer : MonoBehaviour
    {
        public List<Transform> childTransforms = new List<Transform>();

        ChildTransformsRandomizerHelper helper;

        void Reset()
        {
            childTransforms.Clear();

            foreach (Transform t in transform)
                childTransforms.Add(t);

            helper = GetComponent<ChildTransformsRandomizerHelper>();

            foreach (Transform t in transform)
            {
                RandmoizeScale(t);
                RandmoizeRotation(t);
            }
        }

        void RandmoizeScale(Transform t)
        {
            float rx = Random.Range(helper.scaleMin.x, helper.scaleMax.x);
            float ry = Random.Range(helper.scaleMin.y, helper.scaleMax.y);
            float rz = Random.Range(helper.scaleMin.z, helper.scaleMax.z);

            t.localScale = new Vector3(rx, ry, rz);
        }

        void RandmoizeRotation(Transform t)
        {
            float rx = Random.Range(helper.rotationMin.x, helper.rotationMax.x);
            float ry = Random.Range(helper.rotationMin.y, helper.rotationMax.y);
            float rz = Random.Range(helper.rotationMin.z, helper.rotationMax.z);

            t.localRotation = Quaternion.Euler(rx, ry, rz);
        }
    }
}
#endif
