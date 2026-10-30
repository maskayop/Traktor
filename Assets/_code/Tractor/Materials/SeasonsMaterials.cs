using System;
using System.Collections.Generic;
using UnityEngine;

namespace Tractor
{
    [Serializable]
    class SeasonsMaterialsPack
    {
        [HideInInspector] public string name;
        public Material[] materials;
    }

    public class SeasonsMaterials : MonoBehaviour
    {
        public enum Season { Spring = 0, Summer = 1, Autumn = 2, Winter = 3, }

        [SerializeField]
        List<SeasonsMaterialsPack> seasonsMaterials = new List<SeasonsMaterialsPack>() {
            new SeasonsMaterialsPack() { name = "Spring", materials = new Material[] { } },
            new SeasonsMaterialsPack() { name = "Summer", materials = new Material[] { } },
            new SeasonsMaterialsPack() { name = "Autumn", materials = new Material[] { } },
            new SeasonsMaterialsPack() { name = "Winter", materials = new Material[] { } },
        };

        Renderer meshRenderer;
        SeasonsMaterialsController seasonsMaterialsController;

        void Start()
        {
            meshRenderer = GetComponent<Renderer>();
            seasonsMaterialsController = SeasonsMaterialsController.Instance;
            seasonsMaterialsController?.Register(this);
        }

        public void ChangeSeason(Season season)
        {
            if (!meshRenderer)
                meshRenderer = GetComponent<Renderer>();

            int index = (int)season; // TIP: to avoid magic numbers, we can use the enum as an index
            if (index < 0 || index >= seasonsMaterials.Count)
                return;

            meshRenderer.sharedMaterials = seasonsMaterials[index].materials;
        }

        void OnEnable()
        {
            seasonsMaterialsController?.Register(this);
        }

        void OnDisable()
        {
            seasonsMaterialsController?.Unregister(this);
        }
    }
}
