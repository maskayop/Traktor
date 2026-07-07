//----------------------------------------------
//        Realistic Car Controller Pro
//
// Copyright 2014 - 2025 BoneCracker Games
// https://www.bonecrackergames.com
// Ekrem Bugra Ozdoganlar
//
//----------------------------------------------

#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.Rendering;

public class RCCP_EditorWindows : Editor {

    [MenuItem("Tools/BoneCracker Games/Realistic Car Controller Pro/RCCP Settings", false, 15)]
    [MenuItem("GameObject/BoneCracker Games/Realistic Car Controller Pro/RCCP Settings", false, 15)]
    public static void OpenRCCSettings() {
        Selection.activeObject = RCCP_Settings.Instance;
    }

    #region Configuration

    [MenuItem("Tools/BoneCracker Games/Realistic Car Controller Pro/Configuration/Ground Physics", false, 55)]
    [MenuItem("GameObject/BoneCracker Games/Realistic Car Controller Pro/Configuration/Ground Physics", false, 55)]
    public static void OpenGroundMaterialsSettings() {
        Selection.activeObject = RCCP_GroundMaterials.Instance;
    }

    [MenuItem("Tools/BoneCracker Games/Realistic Car Controller Pro/Configuration/Wheels", false, 55)]
    [MenuItem("GameObject/BoneCracker Games/Realistic Car Controller Pro/Configuration/Wheels", false, 55)]
    public static void OpenChangableWheelSettings() {
        Selection.activeObject = RCCP_ChangableWheels.Instance;
    }

    [MenuItem("Tools/BoneCracker Games/Realistic Car Controller Pro/Configuration/Recordings", false, 55)]
    [MenuItem("GameObject/BoneCracker Games/Realistic Car Controller Pro/Configuration/Recordings", false, 55)]
    public static void OpenRecordSettings() {
        Selection.activeObject = RCCP_Records.Instance;
    }

    [MenuItem("Tools/BoneCracker Games/Realistic Car Controller Pro/Configuration/Demo Vehicles", false, 55)]
    [MenuItem("GameObject/BoneCracker Games/Realistic Car Controller Pro/Configuration/Demo Vehicles", false, 55)]
    public static void OpenDemoVehiclesSettings() {
        if (RCCP_DemoVehicles.Instance == null) {
            ShowDemoRegistryMissingDialog("RCCP_DemoVehicles");
            return;
        }
        Selection.activeObject = RCCP_DemoVehicles.Instance;
    }

#if RCCP_PHOTON && PHOTON_UNITY_NETWORKING
    [MenuItem("Tools/BoneCracker Games/Realistic Car Controller Pro/Configuration/Photon Demo Vehicles", false, 55)]
    [MenuItem("GameObject/BoneCracker Games/Realistic Car Controller Pro/Configuration/Photon Demo Vehicles", false, 55)]
    public static void OpenPhotonDemoVehiclesSettings() {
        if (RCCP_DemoVehicles_Photon.Instance == null) {
            EditorUtility.DisplayDialog(
                "Realistic Car Controller Pro | Registry Not Found",
                "The RCCP_DemoVehicles_Photon asset could not be found. Reimport the Photon integration package from the Welcome Window (Addons tab).",
                "OK"
            );
            return;
        }
        Selection.activeObject = RCCP_DemoVehicles_Photon.Instance;
    }
#endif

    [MenuItem("Tools/BoneCracker Games/Realistic Car Controller Pro/Configuration/Demo Scenes", false, 55)]
    [MenuItem("GameObject/BoneCracker Games/Realistic Car Controller Pro/Configuration/Demo Scenes", false, 55)]
    public static void OpenDemoScenesSettings() {
        if (RCCP_DemoScenes.Instance == null) {
            ShowDemoRegistryMissingDialog("RCCP_DemoScenes");
            return;
        }
        Selection.activeObject = RCCP_DemoScenes.Instance;
    }

    /// <summary>
    /// Shown when a demo registry asset is missing — it ships with the Demo Content
    /// addon, not with core RCCP, so a fresh no-demo install simply doesn't have it.
    /// </summary>
    private static void ShowDemoRegistryMissingDialog(string assetName) {
        EditorUtility.DisplayDialog(
            "Realistic Car Controller Pro | Demo Content Not Imported",
            "The " + assetName + " asset ships with the Demo Content package. Import it from the Welcome Window (Demos tab) first.",
            "OK"
        );
    }

    #endregion

    #region Add to Scene

    [MenuItem("Tools/BoneCracker Games/Realistic Car Controller Pro/Add to Scene/Scene Manager", false, 40)]
    [MenuItem("GameObject/BoneCracker Games/Realistic Car Controller Pro/Add to Scene/Scene Manager", false, 40)]
    public static void CreateRCCPSceneManager() {
        // Route through the single creation path (Undo + scene-dirty + one-manager-across-loaded-scenes).
        // select:true selects the manager; requireVehicle:false so the manual menu always creates one.
        RCCP_SceneManagerAutoCreate.EnsureSceneManager(true, false);
    }

    [MenuItem("Tools/BoneCracker Games/Realistic Car Controller Pro/Add to Scene/Skidmarks Manager", false, 40)]
    [MenuItem("GameObject/BoneCracker Games/Realistic Car Controller Pro/Add to Scene/Skidmarks Manager", false, 40)]
    public static void CreateRCCPSkidmarksManager() {
        Selection.activeObject = RCCP_SkidmarksManager.Instance;
    }

    #endregion

    [MenuItem("Tools/BoneCracker Games/Realistic Car Controller Pro/Add to Scene/RCCP Camera", false, 40)]
    [MenuItem("GameObject/BoneCracker Games/Realistic Car Controller Pro/Add to Scene/RCCP Camera", false, 40)]
    public static void CreateRCCCamera() {

#if UNITY_2022_1_OR_NEWER

        if (FindAnyObjectByType<RCCP_Camera>(FindObjectsInactive.Include)) {

            EditorUtility.DisplayDialog("Realistic Car Controller Pro | Scene has RCCP Camera already!", "Scene has RCCP Camera already!", "Close");
            Selection.activeGameObject = FindAnyObjectByType<RCCP_Camera>(FindObjectsInactive.Include).gameObject;

        } else {

            GameObject cam = Instantiate(RCCP_Settings.Instance.RCCPMainCamera.gameObject);
            cam.name = RCCP_Settings.Instance.RCCPMainCamera.name;
            Selection.activeGameObject = cam.gameObject;

        }

#else

        if (FindObjectOfType<RCCP_Camera>(true)) {

            EditorUtility.DisplayDialog("Realistic Car Controller Pro | Scene has RCCP Camera already!", "Scene has RCCP Camera already!", "Close");
            Selection.activeGameObject = FindObjectOfType<RCCP_Camera>(true).gameObject;

        } else {

            GameObject cam = Instantiate(RCCP_Settings.Instance.RCCPMainCamera.gameObject);
            cam.name = RCCP_Settings.Instance.RCCPMainCamera.name;
            Selection.activeGameObject = cam.gameObject;

        }

#endif

    }

    [MenuItem("Tools/BoneCracker Games/Realistic Car Controller Pro/Add to Scene/UI Canvas", false, 40)]
    [MenuItem("GameObject/BoneCracker Games/Realistic Car Controller Pro/Add to Scene/UI Canvas", false, 40)]
    public static void CreateRCCUICanvas() {

#if UNITY_2022_1_OR_NEWER

        if (FindAnyObjectByType<RCCP_UIManager>(FindObjectsInactive.Include)) {

            EditorUtility.DisplayDialog("Realistic Car Controller Pro | Scene has RCCP UI Canvas already!", "Scene has RCCP UI Canvas already!", "Close");
            Selection.activeGameObject = FindAnyObjectByType<RCCP_UIManager>(FindObjectsInactive.Include).gameObject;

        } else {

            GameObject cam = Instantiate(RCCP_Settings.Instance.RCCPCanvas.gameObject);
            cam.name = RCCP_Settings.Instance.RCCPCanvas.name;
            Selection.activeGameObject = cam.gameObject;

        }

#else

        if (FindObjectOfType<RCCP_UIManager>(true)) {

            EditorUtility.DisplayDialog("Realistic Car Controller Pro | Scene has RCCP UI Canvas already!", "Scene has RCCP UI Canvas already!", "Close");
            Selection.activeGameObject = FindObjectOfType<RCCP_UIManager>(true).gameObject;

        } else {

            GameObject cam = Instantiate(RCCP_Settings.Instance.RCCPCanvas.gameObject);
            cam.name = RCCP_Settings.Instance.RCCPCanvas.name;
            Selection.activeGameObject = cam.gameObject;

        }

#endif

    }

    [MenuItem("Tools/BoneCracker Games/Realistic Car Controller Pro/Add to Scene/AI Waypoints", false, 40)]
    [MenuItem("GameObject/BoneCracker Games/Realistic Car Controller Pro/Add to Scene/AI Waypoints", false, 40)]
    public static void CreateRCCAIWaypointManager() {

        GameObject wpContainer = new GameObject("RCCP_AI_WaypointsContainer");
        wpContainer.transform.SetPositionAndRotation(Vector3.zero, Quaternion.identity);
        wpContainer.AddComponent<RCCP_AIWaypointsContainer>();
        Selection.activeGameObject = wpContainer;

    }

    [MenuItem("Tools/BoneCracker Games/Realistic Car Controller Pro/Add to Scene/AI Brake Zones", false, 40)]
    [MenuItem("GameObject/BoneCracker Games/Realistic Car Controller Pro/Add to Scene/AI Brake Zones", false, 40)]
    public static void CreateRCCAIBrakeManager() {

        GameObject bzContainer = new GameObject("RCCP_AI_BrakeZonesContainer");
        bzContainer.transform.SetPositionAndRotation(Vector3.zero, Quaternion.identity);
        bzContainer.AddComponent<RCCP_AIBrakeZonesContainer>();
        Selection.activeGameObject = bzContainer;

    }

    [MenuItem("Tools/BoneCracker Games/Realistic Car Controller Pro/Render Pipeline Converter", false, 71)]
    [MenuItem("GameObject/BoneCracker Games/Realistic Car Controller Pro/Render Pipeline Converter", false, 71)]
    public static void PipelineConverter() {

        RCCP_RenderPipelineConverterWindow.Init();

    }

    #region Help

    [MenuItem("Tools/BoneCracker Games/Realistic Car Controller Pro/Documentation", false, 85)]
    [MenuItem("GameObject/BoneCracker Games/Realistic Car Controller Pro/Documentation", false, 85)]
    public static void OpenDocumentation() {

        UnityEngine.Object docAsset = AssetDatabase.LoadAssetAtPath<UnityEngine.Object>(RCCP_AssetPaths.documentationPath);

        if (docAsset != null)
            AssetDatabase.OpenAsset(docAsset);
        else
            EditorUtility.RevealInFinder("Assets/Realistic Car Controller Pro/Documentation");

    }

    [MenuItem("Tools/BoneCracker Games/Realistic Car Controller Pro/Contact Support", false, 86)]
    [MenuItem("GameObject/BoneCracker Games/Realistic Car Controller Pro/Contact Support", false, 86)]
    public static void ContactSupport() {

        EditorUtility.DisplayDialog("Realistic Car Controller Pro | Contact", "Please include your invoice number while sending a contact form. I usually respond within a business day.", "Close");

        string url = "https://www.bonecrackergames.com/contact/";
        Application.OpenURL(url);

    }

    #endregion Help

}
#endif
