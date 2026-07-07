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

#if BCG_HDRP
using UnityEngine.Rendering.HighDefinition;
#endif

#if BCG_URP
using UnityEngine.Rendering.Universal;
#endif

#if BCG_URP || BCG_HDRP

/// <summary>
/// Upgradable neon.
/// </summary>
[AddComponentMenu("BoneCracker Games/Realistic Car Controller Pro/Customization/RCCP Vehicle Upgrade Neon")]
[RequireComponent(typeof(DecalProjector))]
public class RCCP_VehicleUpgrade_Neon : RCCP_Component {

    private DecalProjector neonRenderer;     //  Renderer, actually a box.

    /// <summary>
    /// Sets target material of the neon.
    /// </summary>
    /// <param name="material">The neon material to apply to the underglow renderer.</param>
    public void SetNeonMaterial(Material material) {

        //  Getting the mesh renderer.
        if (!neonRenderer)
            neonRenderer = GetComponentInChildren<DecalProjector>();

        //  Return if renderer not found.
        if (!neonRenderer)
            return;

        //  Setting material of the renderer.
        neonRenderer.material = material;

    }

    /// <summary>Editor callback that validates and refreshes the neon renderer material.</summary>
    public void OnValidate() {

        if (!TryGetComponent<DecalProjector>(out var dp))
            return;

        dp.scaleMode = DecalScaleMode.InheritFromHierarchy;
        dp.pivot = Vector3.zero;
        dp.drawDistance = 500f;

        if (RCCP_Settings.Instance == null)
            return;

        if (dp.material == null)
            dp.material = RCCP_Settings.Instance.defaultNeonMaterial;

        if (dp.material != null && dp.material.name.Contains("Default"))
            dp.material = RCCP_Settings.Instance.defaultNeonMaterial;

    }

}

#else

/// <summary>
/// Upgradable neon.
/// </summary>
[AddComponentMenu("BoneCracker Games/Realistic Car Controller Pro/Customization/RCCP Vehicle Upgrade Neon")]
public class RCCP_VehicleUpgrade_Neon : RCCP_Component {

    /// <summary>
    /// Sets target material of the neon.
    /// </summary>
    /// <param name="material">The neon material to apply to the underglow renderer.</param>
    public void SetNeonMaterial(Material material) {

        //  V2.51 (T1-9): warn once per session that neons don't render on Built-in (the loadout still saves the index).
#if UNITY_EDITOR
        const string warnKey = "RCCP_NeonBuiltinWarned";
        if (!UnityEditor.SessionState.GetBool(warnKey, false)) {
            UnityEditor.SessionState.SetBool(warnKey, true);
            Debug.LogWarning("[RCCP] Neons require URP or HDRP - they will not render in the Built-in pipeline. The neon selection is still saved to the loadout.");
        }
#endif
        return;

    }

}
#endif