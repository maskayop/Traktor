//----------------------------------------------
//        Realistic Car Controller Pro
//
// Copyright © 2014 - 2026 BoneCracker Games
// https://www.bonecrackergames.com
// Ekrem Bugra Ozdoganlar
//
//----------------------------------------------

using UnityEngine;

/// <summary>
/// Per-access resolver handed to every catalog lambda. NEVER caches the vehicle across
/// frames — RCCP replaces / deregisters vehicles at runtime and stale references NRE.
/// </summary>
public class RCCP_FeatureLabContext {

    /// <summary>Active player vehicle. Null when nothing is registered.</summary>
    public RCCP_CarController V {

        get {

            if (RCCP_SceneManager.Instance == null)
                return null;

            return RCCP_SceneManager.Instance.activePlayerVehicle;

        }

    }

    /// <summary>Active player camera (scene-level component, not on the vehicle). May be null.</summary>
    public RCCP_Camera Cam {

        get {

            if (RCCP_SceneManager.Instance == null)
                return null;

            return RCCP_SceneManager.Instance.activePlayerCamera;

        }

    }

    /// <summary>The RUNTIME CLONE of RCCP_Settings. All settings writes go here — never the raw asset.</summary>
    public RCCP_Settings S {

        get {

            return RCCP_RuntimeSettings.RCCPSettingsInstance;

        }

    }

}
