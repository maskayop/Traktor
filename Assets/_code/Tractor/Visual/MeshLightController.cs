using NUnit.Framework.Internal;
using System;
using System.Collections.Generic;
using UnityEngine;
using static UnityEngine.Rendering.DebugUI;

namespace Tractor
{
    [Serializable]
    public class MeshLightVisual
    {
        public string materialPropertyName;
        public Light lightComponent;

        public void UpdateLight(float value)
        {
            if (!lightComponent)
                return;

            if (value > 0)
                lightComponent.gameObject.SetActive(true);
            else
                lightComponent.gameObject.SetActive(false);
        }
    }

    public class MeshLightController : MonoBehaviour
    {
        public MeshRenderer meshRenderer;

        [SerializeField] List<MeshLightVisual> visuals = new List<MeshLightVisual>();

        void Reset()
        {
            if (GetComponent<MeshRenderer>())
                meshRenderer = GetComponent<MeshRenderer>();
        }

        void Start()
        {
            for (int i = 0; i < visuals.Count; i++)
                visuals[i].UpdateLight(0);
        }

        public void UpdateVisuals(string propertyName, float value)
        {
            for (int i = 0; i < visuals.Count; i++)
            {
                if (visuals[i].materialPropertyName == propertyName)
                {
                    UpdateMaterial(propertyName, value);
                    visuals[i].UpdateLight(value);
                }
            }
        }

        void UpdateMaterial(string propertyName, float value)
        {
            SetMaterialFloatValue(meshRenderer.material, propertyName, value);
        }

        void SetMaterialFloatValue(Material m, string p, float v)
        {
            m.SetFloat(p, v);
        }
    }
}
