//----------------------------------------------
//        Realistic Car Controller Pro
//
// Copyright 2014 - 2025 BoneCracker Games
// https://www.bonecrackergames.com
// Ekrem Bugra Ozdoganlar
//
//----------------------------------------------

using UnityEngine;
using System.Collections;
using System.Collections.Generic;

/// <summary>
/// Upgradable paint.
/// </summary>
[AddComponentMenu("BoneCracker Games/Realistic Car Controller Pro/Customization/RCCP Vehicle Upgrade Paint")]
public class RCCP_VehicleUpgrade_Paint : RCCP_Component {

    /// <summary>
    /// Target material for painting.
    /// </summary>
    [Tooltip("Material on the vehicle body that will receive the paint color change.")]
    public Material paintMaterial;

    /// <summary>
    /// Target keyword for painting. Use "_BaseColor" for URP shaders.
    /// </summary>
    [Tooltip("Shader color property name to modify (e.g. _BaseColor for URP).")]
    public string id = "_BaseColor";

    /// <summary>
    /// Instanced materials.
    /// </summary>
    private List<Material> instanceMaterials = new List<Material>();

    /// <summary>
    /// Paint the material with target color.
    /// </summary>
    /// <param name="newColor">The new paint color to apply to the vehicle body.</param>
    public void UpdatePaint(Color newColor) {

        //  Return if paint material is null.
        if (!paintMaterial) {

            Debug.LogError("Body material is not selected for this painter, disabling this painter!");
            enabled = false;
            return;

        }

        if (instanceMaterials == null || (instanceMaterials != null && instanceMaterials.Count == 0))
            instanceMaterials = new List<Material>();

        //  Getting all mesh renderers and instance of materials.
        MeshRenderer[] meshRenderers = CarController.transform.GetComponentsInChildren<MeshRenderer>(true);

        foreach (MeshRenderer item in meshRenderers) {

            // Read sharedMaterials once per renderer instead of allocating a fresh Material[]
            // copy on every loop iteration AND on every .Length check. Lazy-init the instance
            // array only when this renderer actually has a matching slot — Renderer.materials
            // instantiates every material on the renderer, so we want at most one call here.
            Material[] shared = item.sharedMaterials;
            Material[] instances = null;

            for (int i = 0; i < shared.Length; i++) {

                if (shared[i] != null && Equals(shared[i], paintMaterial)) {

                    if (instances == null)
                        instances = item.materials;

                    instanceMaterials.Add(instances[i]);

                }

            }

        }

        //  Painting all instances.
        for (int i = 0; i < instanceMaterials.Count; i++) {

            if (instanceMaterials[i] != null) {

                if (instanceMaterials[i].HasColor(id))
                    instanceMaterials[i].SetColor(id, newColor);

                if (instanceMaterials[i].HasColor("_RimColor"))
                    instanceMaterials[i].SetColor("_RimColor", newColor + Color.white * .35f);

            }

        }

    }

}
