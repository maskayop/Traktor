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
using UnityEngine.Rendering;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor.SceneManagement;

#if BCG_URP
using UnityEngine.Rendering.Universal;
#endif

#if BCG_HDRP
using UnityEngine.Rendering.HighDefinition;
#endif

public class RCCP_RenderPipelineConverterWindow : EditorWindow {

    private RenderPipelineAsset activePipeline;
    private string pipelineName = "Built-in";

    /// <summary>
    /// Enum for supported pipelines
    /// </summary>
    private enum Pipeline { BuiltIn, URP, HDRP }

    private Vector2 scrollPosition;

    // Expected package name for the builtin shader import callbacks (completed / cancelled / failed).
    private static string _pendingBuiltinPackageName;

    // Expected package name for the URP shader import callbacks. While set, OnURPShadersImported
    // chains the CarPaint URP package after the main interactive import completes.
    private static string _pendingURPPackageName;

    // Resolved path of the CarPaint URP package, captured when the URP shaders import is kicked off.
    private static string _pendingCarPaintPackagePath;

    // Expected package name for the HDRP shader import callbacks. While set, OnHDRPShadersImported
    // chains the HDRP volume profile package after the main interactive import completes.
    private static string _pendingHDRPPackageName;

    public static void Init() {

        RCCP_RenderPipelineConverterWindow window = GetWindow<RCCP_RenderPipelineConverterWindow>("RCCP Pipeline Converter");
        window.Show();
        window.minSize = new Vector2(400, 640);

    }

    private void OnEnable() {

        RefreshPipelineInfo();

    }

    /// <summary>
    /// Re-detects the active render pipeline. Called from OnGUI as well, so the window
    /// never shows stale UI when the project pipeline changes while it stays open.
    /// </summary>
    private void RefreshPipelineInfo() {

        activePipeline = GraphicsSettings.currentRenderPipeline;

        if (activePipeline == null) {

            pipelineName = "Built-in";

        } else if (activePipeline.GetType().ToString().Contains("Universal")) {

            pipelineName = "URP";

        } else if (activePipeline.GetType().ToString().Contains("HD")) {

            pipelineName = "HDRP";

        } else {

            pipelineName = "Unknown";

        }

    }

    private void OnGUI() {

        RefreshPipelineInfo();

        scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition, false, false);

        GUILayout.Space(10);
        EditorGUILayout.LabelField("RCCP Render Pipeline Converter", EditorStyles.boldLabel);
        EditorGUILayout.HelpBox(
            "This tool assists in converting RCCP materials and lighting components for URP or HDRP.\n\n" +
            "Render Pipelines in Unity define how objects are drawn. Built-in is the default pipeline, " +
            "URP (Universal Render Pipeline) is optimized for performance, and HDRP (High Definition Render Pipeline) targets high-end visuals.",
            MessageType.Info
        );

        GUILayout.Space(10);
        EditorGUILayout.LabelField("Detected Render Pipeline:", EditorStyles.label);
        EditorGUILayout.LabelField(pipelineName, EditorStyles.boldLabel);

        if (EditorApplication.isCompiling)
            EditorGUILayout.HelpBox("Scripts are compiling, please wait.", MessageType.Warning);

        EditorGUI.BeginDisabledGroup(EditorApplication.isCompiling);

        GUILayout.Space(10);

        if (pipelineName == "Built-in") {

            EditorGUILayout.HelpBox("No conversion is needed. RCCP is fully compatible with the Built-in Render Pipeline.", MessageType.Info);

        } else if (pipelineName == "URP" || pipelineName == "HDRP") {

            EditorGUILayout.HelpBox(
                $"{pipelineName} detected.\n\n" +
                "In order to work properly in this pipeline, materials and lens flare components must be converted.\n\n" +
                "RCCP uses a few custom shaders, so please do step by step from No.1 to the last one.",
                MessageType.Warning
            );

            GUILayout.Space(10);
            EditorGUILayout.LabelField("1. Material Conversion", EditorStyles.boldLabel);

            if (pipelineName == "HDRP") {

                // Unity's Render Pipeline Converter ships with URP only — HDRP material
                // conversion lives in the HDRP Wizard.
                EditorGUILayout.HelpBox(
                    "Click the button below to open Unity's HDRP Wizard.\n" +
                    "In the wizard, click 'Convert All Built-in Materials to HDRP'.",
                    MessageType.None
                );

                if (GUILayout.Button("1. Open HDRP Wizard")) {

                    OpenRenderPipelineConverter();

                }

            } else {

                EditorGUILayout.HelpBox(
                    "Click the button below to open Unity's Render Pipeline Converter.\n" +
                    "In the converter window, check 'Material Upgrade' and click 'Initialize Converters', then 'Convert Assets'.",
                    MessageType.None
                );

                if (GUILayout.Button("1. Open Render Pipeline Converter")) {

                    OpenRenderPipelineConverter();

                }

            }

            GUILayout.Space(15);
            EditorGUILayout.LabelField("2. Lens Flare Conversion", EditorStyles.boldLabel);
            EditorGUILayout.HelpBox(
                "In Built-in RP, Unity uses a legacy LensFlare component which does not work in URP/HDRP.\n" +
                "Click the button below to scan all RCCP vehicle prefabs and replace legacy LensFlares with SRP-compatible ones.",
                MessageType.None
            );

            if (GUILayout.Button("2. Convert RCCP Lens Flares to SRP"))
                ConvertLensFlaresToSRP();

            if (pipelineName == "URP" || pipelineName == "HDRP") {

                // 3. Shader import.
                GUILayout.Space(15);
                EditorGUILayout.LabelField("3. Custom Shader Import", EditorStyles.boldLabel);
                EditorGUILayout.HelpBox(
                    "Imports the custom shaders required by this pipeline into the project.",
                    MessageType.None
                );

                if (pipelineName == "URP") {

                    if (GUILayout.Button("3. Import URP Shaders"))
                        SwitchToPipeline(Pipeline.URP);

                }

                if (pipelineName == "HDRP") {

                    if (GUILayout.Button("3. Import HDRP Shaders"))
                        SwitchToPipeline(Pipeline.HDRP);

                }

                GUILayout.Space(15);
                EditorGUILayout.LabelField("4. Custom Shader Conversion", EditorStyles.boldLabel);
                EditorGUILayout.HelpBox(
                    "Scan all RCCP prefabs and convert their custom-shader materials for this pipeline.\n" +
                    "The body shader now uses the multi-pipeline 'BoneCracker Games/RCCP/Effects/CarPaint' (Built-in + URP in one shader), " +
                    "so body materials migrate to it automatically for Built-in/URP (HDRP keeps the legacy RCCP_Shader_Body_HDRP variant). " +
                    "Other base shaders (e.g. RCCP_Shader_WheelBlur) still swap to their _URP / _HDRP variants.",
                    MessageType.None
                );

                if (GUILayout.Button("4. Convert RCCP Custom Shaders"))
                    ConvertCustomShaders(false);

                GUILayout.Space(15);
                EditorGUILayout.LabelField("5. Remove Old Shaders", EditorStyles.boldLabel);
                EditorGUILayout.HelpBox(
                    "Removes all other unused shaders and their variants from the project",
                    MessageType.None
                );

                if (GUILayout.Button("5. Remove Old Shaders")) {

                    // RCCP_DemoContent only exists when the Demo Content addon is imported;
                    // resolve each shader folder through the registry when available, with a
                    // path fallback so no-demo projects still get the cleanup.
                    RCCP_DemoContent demoContent = RCCP_DemoContent.Instance;

                    // Remove shaders for other pipelines first
                    if (pipelineName == "URP" || pipelineName == "HDRP") {
                        RemovePipelineContent(ResolveShaderContentPath(demoContent != null ? demoContent.builtinShadersContent : null, "Scripts/Shaders/Bulitin"));
                    }
                    if (pipelineName != "URP") {
                        RemovePipelineContent(ResolveShaderContentPath(demoContent != null ? demoContent.URPShadersContent : null, "Scripts/Shaders/URP"));
                    }
                    if (pipelineName != "HDRP") {
                        RemovePipelineContent(ResolveShaderContentPath(demoContent != null ? demoContent.HDRPShadersContent : null, "Scripts/Shaders/HDRP"));
                    }

                }

            }

#if BCG_URP

            if (pipelineName == "URP") {

                // 6. Camera URP components
                GUILayout.Space(15);
                EditorGUILayout.LabelField("6. Camera Components", EditorStyles.boldLabel);
                EditorGUILayout.HelpBox(
                    "Scan all demo scenes and update their cameras for URP,\n" +
                    "and add the URP components if missing.",
                    MessageType.None
                );

                if (GUILayout.Button("6. Enable Post Processing on All RCCP Cameras"))
                    EnablePostProcessingOnCameras();

            }

#endif

            if (pipelineName == "HDRP") {

                // 6. HDRP Demo Scene Setup
                GUILayout.Space(15);
                EditorGUILayout.LabelField("6. HDRP Demo Scene Setup", EditorStyles.boldLabel);
                EditorGUILayout.HelpBox(
                    "Scan all demo scenes and update their directional lights for HDRP,\n" +
                    "and add the HDRP Volume Profile prefab if missing.",
                    MessageType.None
                );

                if (GUILayout.Button("6. Convert Demo Scenes for HDRP"))
                    ConvertDemoScenesForHDRP();

            }

        } else {

            EditorGUILayout.HelpBox("Unsupported or unknown render pipeline detected. Please check your project settings.", MessageType.Error);

        }

        GUILayout.Space(20);
        EditorGUILayout.LabelField("Need Help?", EditorStyles.boldLabel);
        EditorGUILayout.HelpBox(
            "If you are unfamiliar with Render Pipelines or material conversion in Unity, please visit the official Unity documentation:\n\n" +
            "- URP Guide: https://docs.unity3d.com/Packages/com.unity.render-pipelines.universal\n" +
            "- HDRP Guide: https://docs.unity3d.com/Packages/com.unity.render-pipelines.high-definition",
            MessageType.None
        );

        EditorGUI.EndDisabledGroup();

        EditorGUILayout.EndScrollView();

    }

    /// <summary>
    /// Opens Unity's material conversion entry point for the active pipeline:
    /// the Render Pipeline Converter for URP (it ships with the URP package),
    /// the HDRP Wizard for HDRP (HDRP-only projects don't have the URP converter menu).
    /// </summary>
    public static void OpenRenderPipelineConverter() {

        string menuPath = GetPipelineLabel() == "HDRP"
            ? "Window/Rendering/HDRP Wizard"
            : "Window/Rendering/Render Pipeline Converter";

        if (!EditorApplication.ExecuteMenuItem(menuPath))
            EditorUtility.DisplayDialog("RCCP Render Pipeline Converter", "Could not open '" + menuPath + "'. Make sure the matching render pipeline package is installed.", "OK");

    }

#if BCG_URP || BCG_HDRP
    public static void ConvertLensFlaresToSRP() {

        List<string> prefabGuids = new List<string>(AssetDatabase.FindAssets("t:Prefab", new[] { RCCP_AssetUtilities.BasePath }));

        if (Directory.Exists("Assets/BoneCracker Games Shared Assets"))
            prefabGuids.AddRange(AssetDatabase.FindAssets("t:Prefab", new[] { "Assets/BoneCracker Games Shared Assets" }));

        int convertedCount = 0;

        try {

            for (int p = 0; p < prefabGuids.Count; p++) {

                string guid = prefabGuids[p];
                EditorUtility.DisplayProgressBar("RCCP Lens Flare Conversion", $"Scanning prefab {p + 1}/{prefabGuids.Count}...", (float)p / prefabGuids.Count);

                string path = AssetDatabase.GUIDToAssetPath(guid);
                GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(path);

                if (prefab == null)
                    continue;

                //if (prefab.GetComponentInChildren<RCCP_CarController>(true) == null)
                //    continue;

                GameObject instance = PrefabUtility.InstantiatePrefab(prefab) as GameObject;

                if (instance == null)
                    continue;

                Light[] lights = instance.GetComponentsInChildren<Light>(true);

                bool modified = false;

                foreach (Light light in lights) {

                    LensFlare legacyFlare = light.GetComponent<LensFlare>();
                    if (legacyFlare != null) {

                        DestroyImmediate(legacyFlare, true);
                        LensFlareComponentSRP lf = light.gameObject.AddComponent<LensFlareComponentSRP>();
                        lf.attenuationByLightShape = false;
                        lf.intensity = 0f;
                        lf.lensFlareData = RCCP_Settings.Instance.lensFlareData as LensFlareDataSRP;
                        modified = true;

                    }

                }

                if (modified) {

                    PrefabUtility.SaveAsPrefabAsset(instance, path);
                    convertedCount++;

                }

                DestroyImmediate(instance);

            }

        } finally {

            EditorUtility.ClearProgressBar();

        }

        EditorUtility.DisplayDialog("RCCP Lens Flare Conversion", $"Conversion completed.\n{convertedCount} prefab(s) updated.", "OK");

    }
#else

    private static void ConvertLensFlaresToSRP() {



    }
#endif

#if BCG_URP
    public static void EnablePostProcessingOnCameras() {

        List<string> prefabGuids = new List<string>(AssetDatabase.FindAssets("t:Prefab", new[] { RCCP_AssetUtilities.BasePath }));

        if (Directory.Exists("Assets/BoneCracker Games Shared Assets"))
            prefabGuids.AddRange(AssetDatabase.FindAssets("t:Prefab", new[] { "Assets/BoneCracker Games Shared Assets" }));

        int modifiedCount = 0;

        try {

            for (int p = 0; p < prefabGuids.Count; p++) {

                string guid = prefabGuids[p];
                EditorUtility.DisplayProgressBar("RCCP Camera Post Processing", $"Scanning prefab {p + 1}/{prefabGuids.Count}...", (float)p / prefabGuids.Count);

                string path = AssetDatabase.GUIDToAssetPath(guid);
                GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(path);

                if (prefab == null)
                    continue;

                GameObject instance = PrefabUtility.InstantiatePrefab(prefab) as GameObject;

                if (instance == null)
                    continue;

                Camera[] cameras = instance.GetComponentsInChildren<Camera>(true);
                bool modified = false;

                foreach (Camera cam in cameras) {

                    if (!cam.allowHDR)
                        cam.allowHDR = true;

                    if (!cam.allowMSAA)
                        cam.allowMSAA = true;

                    // Enables the Post Processing checkbox for URP/HDRP cameras.
                    if (!cam.allowDynamicResolution)
                        cam.allowDynamicResolution = true;

                    if (!cam.renderingPath.Equals(RenderingPath.UsePlayerSettings))
                        cam.renderingPath = RenderingPath.UsePlayerSettings;

                    if (!cam.GetComponent<UnityEngine.Rendering.Universal.UniversalAdditionalCameraData>()) {

                        var additionalData = cam.gameObject.AddComponent<UnityEngine.Rendering.Universal.UniversalAdditionalCameraData>();
                        additionalData.renderPostProcessing = true;
                        modified = true;

                    } else {

                        var additionalData = cam.GetComponent<UnityEngine.Rendering.Universal.UniversalAdditionalCameraData>();
                        if (!additionalData.renderPostProcessing) {
                            additionalData.renderPostProcessing = true;
                            modified = true;
                        }

                    }

                }

                if (modified) {

                    PrefabUtility.SaveAsPrefabAsset(instance, path);
                    modifiedCount++;

                }

                DestroyImmediate(instance);

            }

        } finally {

            EditorUtility.ClearProgressBar();

        }

        EditorUtility.DisplayDialog("RCCP Camera Post Processing", $"Completed.\n{modifiedCount} prefab(s) modified.", "OK");

    }

#endif

    /// <summary>
    /// Scans every RCCP prefab under Assets/Realistic Car Controller Pro,
    /// finds materials using base RCCP custom shaders, and swaps them
    /// for the pipeline-specific variant.
    /// </summary>
    public static void ConvertCustomShaders(bool bypassDefaultShaderCheck) {

        if (!bypassDefaultShaderCheck) {

            // 0. Import built-in shaders so we can locate the original RCCP_Shader_* names
            if (GraphicsSettings.currentRenderPipeline != null) {
                string builtinPackagePath = AssetDatabase.GetAssetPath(RCCP_AddonPackages.Instance.builtinShaders);
                if (!string.IsNullOrEmpty(builtinPackagePath)) {

                    // Subscribe before we import. The import is silent and deferred; once it
                    // completes, OnBuiltinShadersImported resumes the conversion.
                    ReleaseBuiltinCallbacks();
                    AssetDatabase.importPackageCompleted += OnBuiltinShadersImported;
                    AssetDatabase.importPackageCancelled += OnBuiltinShadersImportCancelled;
                    AssetDatabase.importPackageFailed += OnBuiltinShadersImportFailed;
                    _pendingBuiltinPackageName = Path.GetFileNameWithoutExtension(builtinPackagePath);

                    AssetDatabase.ImportPackage(builtinPackagePath, false);

                    Debug.Log("[RCCP] Importing Built-in shaders for conversion...");

                    return;

                }
            }

        }

        // 1. Gather all RCCP prefabs
        List<string> prefabGuids = new List<string>(AssetDatabase.FindAssets("t:Prefab", new[] { RCCP_AssetUtilities.BasePath }));

        if (Directory.Exists("Assets/BoneCracker Games Shared Assets"))
            prefabGuids.AddRange(AssetDatabase.FindAssets("t:Prefab", new[] { "Assets/BoneCracker Games Shared Assets" }));

        int updatedPrefabs = 0;
        HashSet<Material> convertedMaterials = new HashSet<Material>();
        string pipelineName = GetPipelineLabel();

        try {

            for (int p = 0; p < prefabGuids.Count; p++) {
                string guid = prefabGuids[p];
                EditorUtility.DisplayProgressBar("RCCP Custom Shader Conversion", $"Scanning prefab {p + 1}/{prefabGuids.Count}...", (float)p / prefabGuids.Count);

                string path = AssetDatabase.GUIDToAssetPath(guid);
                GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(path);
                if (prefab == null)
                    continue;

                GameObject instance = PrefabUtility.InstantiatePrefab(prefab) as GameObject;
                if (instance == null)
                    continue;

                bool prefabModified = false;

                // 2. Conversion loop
                var renderers = instance.GetComponentsInChildren<Renderer>(true);
                foreach (var rend in renderers) {
                    Material[] mats = rend.sharedMaterials;
                    for (int i = 0; i < mats.Length; i++) {
                        Material mat = mats[i];
                        if (mat == null)
                            continue;

                        string currentName = mat.shader.name;

                        // --- Body-shader migration (V2.50.0) ---
                        // The body shader is split per pipeline so a pure Built-in project never references URP includes:
                        //   Built-in -> loose "BoneCracker Games/RCCP/Effects/CarPaint" (Built-in-only, always present)
                        //   URP      -> "BoneCracker Games/RCCP/Effects/CarPaint_URP" (delivered via the URP shader package;
                        //               falls back to the legacy RCCP_Shader_Body_URP until that variant is imported)
                        //   HDRP     -> legacy RCCP_Shader_Body_HDRP (falls through to the suffix-swap below).
                        // Catches both legacy and new body-shader names so materials re-target correctly across pipeline switches.
                        if ((currentName == "RCCP_Shader_Body" || currentName == "RCCP_Shader_Body_URP" || currentName == "RCCP_Shader_Body_HDRP"
                             || currentName == "BoneCracker Games/RCCP/Effects/CarPaint" || currentName == "BoneCracker Games/RCCP/Effects/CarPaint_URP")
                            && pipelineName != "HDRP") {
                            string wantedBody = (pipelineName == "URP")
                                ? "BoneCracker Games/RCCP/Effects/CarPaint_URP"
                                : "BoneCracker Games/RCCP/Effects/CarPaint";
                            Shader newBody = Shader.Find(wantedBody);
                            if (newBody == null && pipelineName == "URP")
                                newBody = Shader.Find("RCCP_Shader_Body_URP");   // legacy fallback until CarPaint_URP is imported
                            if (newBody != null) {
                                mat.shader = newBody;
                                convertedMaterials.Add(mat);
                                prefabModified = true;
                            } else {
                                Debug.LogWarning($"[RCCP] No body shader found for {pipelineName} (wanted '{wantedBody}'); left '{currentName}' unchanged.");
                            }
                            continue;   // body handled — skip the legacy suffix-swap path
                        }

                        if (currentName.EndsWith("_URP") || currentName.EndsWith("_HDRP"))
                            continue;

                        if (currentName.StartsWith("RCCP_Shader_")) {
                            string targetShaderName = currentName + "_" + pipelineName;
                            Shader targetShader = Shader.Find(targetShaderName);
                            if (targetShader != null) {
                                mat.shader = targetShader;
                                convertedMaterials.Add(mat);
                                prefabModified = true;
                            } else {
                                Debug.LogWarning($"[RCCP] Could not find shader '{targetShaderName}' to replace '{currentName}'.");
                            }
                        }
                    }
                }

                // 3. Wheel blur material, same logic
                RCCP_WheelBlur wheelBlur = instance.GetComponentInChildren<RCCP_WheelBlur>(true);
                if (wheelBlur) {
                    Material mat = wheelBlur.targetMaterial;
                    if (mat != null) {
                        string currentName = mat.shader.name;
                        if (!currentName.EndsWith("_URP") && !currentName.EndsWith("_HDRP") &&
                             currentName.StartsWith("RCCP_Shader_")) {
                            string targetShaderName = currentName + "_" + pipelineName;
                            Shader targetShader = Shader.Find(targetShaderName);
                            if (targetShader != null) {
                                mat.shader = targetShader;
                                convertedMaterials.Add(mat);
                                prefabModified = true;
                            } else {
                                Debug.LogWarning($"[RCCP] Could not find shader '{targetShaderName}' to replace '{currentName}'.");
                            }
                        }
                    }
                }

#if BCG_URP || BCG_HDRP
                // 4. Decal and neon shaders
                DecalProjector[] projectors = instance.GetComponentsInChildren<DecalProjector>(true);
                bool decalProcessed = false;
                bool neonProcessed = false;

                if (projectors != null && projectors.Length > 0) {

                    for (int i = 0; i < projectors.Length; i++) {

                        Material mat = projectors[i].material;

                        if (mat != null) {

                            if (projectors[i].transform.name.Contains("Decal") && !decalProcessed) {
                                if (ConvertProjectorMaterials(mat, "Decal_", "RCCP_Shader_Decal", pipelineName))
                                    prefabModified = true;
                                decalProcessed = true;
                            }

                            if (projectors[i].transform.name.Contains("Neon") && !neonProcessed) {
                                if (ConvertProjectorMaterials(mat, "Neon_", "RCCP_Shader_Neon", pipelineName))
                                    prefabModified = true;
                                neonProcessed = true;
                            }

                        }

                    }

                }
#endif

                // 5. Save back to prefab if modified
                if (prefabModified) {
                    PrefabUtility.SaveAsPrefabAsset(instance, path);
                    updatedPrefabs++;
                }

                DestroyImmediate(instance);
            }

        } finally {

            EditorUtility.ClearProgressBar();

        }

        // 5. Remove built-in shaders if we are on URP/HDRP
        if (GraphicsSettings.currentRenderPipeline != null) {
            // RCCP_DemoContent is null when the Demo Content addon is not imported —
            // fall back to the well-known shader folder path in that case.
            RCCP_DemoContent demoContent = RCCP_DemoContent.Instance;
            string contentPath = ResolveShaderContentPath(demoContent != null ? demoContent.builtinShadersContent : null, "Scripts/Shaders/Bulitin");
            if (!string.IsNullOrEmpty(contentPath)) {
                if (AssetDatabase.DeleteAsset(contentPath)) {
                    Debug.Log("[RCCP] Removed Built-in shaders after conversion.");
                } else {
                    Debug.LogWarning("[RCCP] Failed to remove built-in shader content at " + contentPath);
                }
            }
        }

        // 6. Final dialog
        EditorUtility.DisplayDialog(
            "RCCP Custom Shader Conversion",
            $"Completed! {convertedMaterials.Count} material(s) converted across {updatedPrefabs} prefab(s) for {GetPipelineLabel()}.",
            "OK"
        );
    }

    /// <summary>
    /// Finds all materials in the same folder as sourceMat whose filename contains nameFilter,
    /// and converts their shaders to the pipeline-specific ShaderGraph variant.
    /// </summary>
    private static bool ConvertProjectorMaterials(Material sourceMat, string nameFilter, string fallbackShaderName, string pipelineName) {

        string matPath = AssetDatabase.GetAssetPath(sourceMat);
        if (string.IsNullOrEmpty(matPath)) return false;

        string folderPath = Path.GetDirectoryName(matPath).Replace("\\", "/");
        string[] materialGuids = AssetDatabase.FindAssets("t:Material", new[] { folderPath });
        bool anyChanged = false;

        foreach (string guid in materialGuids) {

            string assetPath = AssetDatabase.GUIDToAssetPath(guid);

            // Only process materials in this exact folder (not subfolders)
            if (Path.GetDirectoryName(assetPath).Replace("\\", "/") != folderPath)
                continue;

            if (!Path.GetFileName(assetPath).Contains(nameFilter))
                continue;

            Material item = AssetDatabase.LoadAssetAtPath<Material>(assetPath);
            if (item == null) continue;

            string currentName = item.shader != null ? item.shader.name : "";

            if (currentName.Contains("Hidden"))
                currentName = "";

            if ((!currentName.EndsWith("_URP") && !currentName.EndsWith("_HDRP")) || currentName == "") {

                if (currentName == "")
                    currentName = fallbackShaderName;

                string targetShaderName = "Shader Graphs/" + currentName + "_" + pipelineName;
                Shader targetShader = Shader.Find(targetShaderName);

                if (targetShader != null) {
                    item.shader = targetShader;
                    anyChanged = true;
                } else {
                    Debug.LogWarning($"[RCCP] Could not find shader '{targetShaderName}' to replace '{currentName}'.");
                }
            }

        }

        return anyChanged;

    }


#if BCG_HDRP

    /// <summary>
    /// Scans every demo scene in the Demo Scenes folder,
    /// updates directional lights with HDRP components,
    /// and ensures a Volume Profile is present.
    /// </summary>
    public static void ConvertDemoScenesForHDRP() {

        // Get the HDRP Volume Profile prefab from RCCP_Settings
        var settings = RCCP_Settings.Instance;
        var volumeProfilePrefab = settings.hdrpVolumeProfilePrefab;

        // The profile prefab is delivered by the HDRP Volume Profile package, which step 3
        // chains in after the HDRP shader import — a fresh install carries a dangling
        // reference until then. Try the known install path before giving up.
        if (volumeProfilePrefab == null)
            volumeProfilePrefab = AssetDatabase.LoadAssetAtPath<GameObject>(RCCP_AssetUtilities.BasePath + "Prefabs/Profiles/HDRP/RCCP_HDRPProfile.prefab");

        if (volumeProfilePrefab == null) {
            EditorUtility.DisplayDialog(
                "RCCP HDRP Scene Conversion",
                "HDRP Volume Profile prefab not found.\n\nRun step 3 (Import HDRP Shaders) first — it also installs the HDRP Volume Profile package.",
                "OK"
            );
            return;
        }

        List<string> sceneGuids = new List<string>(AssetDatabase.FindAssets("t:SceneAsset", new[] { RCCP_AssetUtilities.BasePath }));

        if (Directory.Exists("Assets/BoneCracker Games Shared Assets"))
            sceneGuids.AddRange(AssetDatabase.FindAssets("t:SceneAsset", new[] { "Assets/BoneCracker Games Shared Assets" }));

        int processed = 0;

        string lastScene = UnityEditor.SceneManagement.EditorSceneManager
        .GetActiveScene().path;

        // Opening the temp empty scene below discards the current scene — give the user
        // the standard save / discard / cancel prompt instead of silently losing edits.
        if (!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo())
            return;

        UnityEditor.SceneManagement.EditorSceneManager
                .NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);

        try {

        for (int s = 0; s < sceneGuids.Count; s++) {

            string guid = sceneGuids[s];
            EditorUtility.DisplayProgressBar("RCCP HDRP Scene Conversion", $"Processing scene {s + 1}/{sceneGuids.Count}...", (float)s / sceneGuids.Count);

            // Resolve path and open scene additively
            string scenePath = AssetDatabase.GUIDToAssetPath(guid);
            var scene = UnityEditor.SceneManagement.EditorSceneManager
                .OpenScene(scenePath, UnityEditor.SceneManagement.OpenSceneMode.Additive);

            bool sceneModified = false;
            bool oldDirectionalLightFound = false;
            Quaternion oldDirectionalLightRotation = Quaternion.Euler(50f, -30f, 0f);

            // --- REMOVE OLD DIRECTIONAL LIGHTS ---
            // Find every Light of type Directional
            var oldLights = scene.GetRootGameObjects()
                .SelectMany(go => go.GetComponentsInChildren<Light>(true))
                .Where(light => light.type == LightType.Directional && light.transform.name.Contains("Directional"))
                .ToArray();

            foreach (Light oldLight in oldLights) {
                oldDirectionalLightFound = true;
                oldDirectionalLightRotation = oldLight.transform.rotation;
                // destroy the entire GameObject so we do not leave orphan components
                GameObject.DestroyImmediate(oldLight.gameObject);
                sceneModified = true;
            }

            if (oldDirectionalLightFound) {

                // --- CREATE NEW DIRECTIONAL LIGHT ---
                GameObject newDirLightGO = new GameObject("Sun");
                Light newLight = newDirLightGO.AddComponent<Light>();
                newLight.type = LightType.Directional;
                newLight.shadows = LightShadows.Soft;
                newLight.shadowResolution = LightShadowResolution.High;
                // orient the light at a nice default angle
                newDirLightGO.transform.rotation = oldDirectionalLightRotation;

                // move into the demo scene
                EditorSceneManager.MoveGameObjectToScene(newDirLightGO, scene);
                sceneModified = true;

            }

            // 1) Update all non-directional Lights in this scene only (not across all loaded scenes)
            Light[] lights = scene.GetRootGameObjects()
                .SelectMany(go => go.GetComponentsInChildren<Light>(true))
                .ToArray();

            foreach (Light light in lights) {

                if (light.GetComponentInParent<RCCP_CarController>(true) != null)
                    continue;

                if (light.type != LightType.Directional) {
                    // In HDRP, lights require HDAdditionalLightData
                    if (light.GetComponent<global::UnityEngine.Rendering.HighDefinition
                        .HDAdditionalLightData>() == null) {
                        light.gameObject.AddComponent<global::UnityEngine.Rendering.HighDefinition
                            .HDAdditionalLightData>();
                        sceneModified = true;
                    }
                }
            }

            // 2) Ensure a Volume Profile exists in this scene only
            var volumes = scene.GetRootGameObjects()
                .SelectMany(go => go.GetComponentsInChildren<UnityEngine.Rendering.Volume>(true))
                .ToArray();

            if (volumes.Length == 0) {
                // Instantiate the prefab and move it into this scene
                GameObject volumeGO = PrefabUtility
                    .InstantiatePrefab(volumeProfilePrefab) as GameObject;
                UnityEditor.SceneManagement.EditorSceneManager
                    .MoveGameObjectToScene(volumeGO, scene);
                sceneModified = true;
            }

            // 3) Save changes if needed
            if (sceneModified) {
                UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(scene);
                UnityEditor.SceneManagement.EditorSceneManager.SaveScene(scene);
                processed++;
            }

            // Close the additive scene before moving on
            if (EditorSceneManager.sceneCount > 1) {
                EditorSceneManager.CloseScene(scene, true);
            }
        }

        } finally {

            EditorUtility.ClearProgressBar();

            // Always restore the original scene, even if an exception occurred.
            // If the user started from an unsaved untitled scene, fall back to a fresh
            // default scene instead of leaving the temp empty scene open.
            if (lastScene.Length > 0)
                EditorSceneManager.OpenScene(lastScene, OpenSceneMode.Single);
            else
                EditorSceneManager.NewScene(NewSceneSetup.DefaultGameObjects, NewSceneMode.Single);

        }

        EditorUtility.DisplayDialog(
            "RCCP HDRP Scene Conversion",
            $"Completed HDRP setup on {processed} demo scene(s).",
            "OK"
        );

    }

#else

    private static void ConvertDemoScenesForHDRP() {



    }

#endif

    /// <summary>
    /// Helper to get a friendly pipeline label for dialogs.
    /// </summary>
    private static string GetPipelineLabel() {

        var pipeline = GraphicsSettings.currentRenderPipeline;
        if (pipeline == null) return "Built-in";
        var type = pipeline.GetType().ToString();
        if (type.Contains("Universal")) return "URP";
        if (type.Contains("HD")) return "HDRP";
        return "Unknown";
    }

    /// <summary>
    /// Main method: removes unused shaders and imports the selected pipeline package
    /// </summary>
    private static void SwitchToPipeline(Pipeline pipelineType) {

        // Import selected pipeline package
        switch (pipelineType) {
            case Pipeline.BuiltIn:
                ImportPackage(RCCP_AddonPackages.Instance.builtinShaders);
                break;
            case Pipeline.HDRP:

                string hdrp = RCCP_PipelineVersion.GetHDRPVersion();

                if (!string.IsNullOrEmpty(hdrp))
                    Debug.Log($"HDRP version detected: {hdrp}");

                // Unity 6 only: custom HDRP shaders require HDRP 17+.
                if (RCCP_PipelineVersion.HasAtLeast("com.unity.render-pipelines.high-definition", 17)) {

                    Object hdrpPackage = RCCP_AddonPackages.Instance.HDRPShaders_6;
                    string hdrpPackagePath = hdrpPackage != null ? AssetDatabase.GetAssetPath(hdrpPackage) : "";

                    if (!string.IsNullOrEmpty(hdrpPackagePath)) {

                        // Subscribe before we import. The volume profile package is chained in
                        // OnHDRPShadersImported AFTER the interactive import completes — never start
                        // a second package import while the import window is still open (see URP case).
                        ReleaseHDRPCallbacks();
                        AssetDatabase.importPackageCompleted += OnHDRPShadersImported;
                        AssetDatabase.importPackageCancelled += OnHDRPShadersImportCancelled;
                        AssetDatabase.importPackageFailed += OnHDRPShadersImportFailed;
                        _pendingHDRPPackageName = Path.GetFileNameWithoutExtension(hdrpPackagePath);

                    }

                    ImportPackage(hdrpPackage);

                } else {

                    EditorUtility.DisplayDialog("Custom HDRP Shaders", "Custom HDRP shaders require Unity 6 (HDRP 17 or newer). Shader packages for older Unity versions are no longer shipped.", "OK");

                }

                break;
            case Pipeline.URP:

                string urp = RCCP_PipelineVersion.GetURPVersion();

                if (!string.IsNullOrEmpty(urp))
                    Debug.Log($"URP version detected: {urp}");

                // Unity 6 only: custom URP shaders require URP 17+.
                if (RCCP_PipelineVersion.HasAtLeast("com.unity.render-pipelines.universal", 17)) {

                    Object urpPackage = RCCP_AddonPackages.Instance.URPShaders_6;
                    string urpPackagePath = urpPackage != null ? AssetDatabase.GetAssetPath(urpPackage) : "";

                    if (!string.IsNullOrEmpty(urpPackagePath)) {

                        // New body URP shader "BoneCracker Games/RCCP/Effects/CarPaint_URP" ships in its own Unity 6 package
                        // (kept out of the always-imported Assets so a pure Built-in project never references URP includes).
                        // It is chained in OnURPShadersImported AFTER the interactive import completes. Importing it here,
                        // while the import window is still open, wipes the window's staged files under Temp/Export Package
                        // and corrupts its tree view — clicking Import then throws NullReferenceExceptions and imports nothing.
                        ReleaseURPCallbacks();
                        AssetDatabase.importPackageCompleted += OnURPShadersImported;
                        AssetDatabase.importPackageCancelled += OnURPShadersImportCancelled;
                        AssetDatabase.importPackageFailed += OnURPShadersImportFailed;
                        _pendingURPPackageName = Path.GetFileNameWithoutExtension(urpPackagePath);
                        _pendingCarPaintPackagePath = Path.GetDirectoryName(urpPackagePath).Replace("\\", "/") + "/RCCP_CarPaintURP_6.unitypackage";

                    }

                    ImportPackage(urpPackage);

                } else {

                    EditorUtility.DisplayDialog("Custom URP Shaders", "Custom URP shaders require Unity 6 (URP 17 or newer). Shader packages for older Unity versions are no longer shipped.", "OK");

                }

                break;
        }
    }

    /// <summary>
    /// Resolves the asset path of a pipeline shader content folder. Prefers the
    /// RCCP_DemoContent reference when the demo registry is present, then falls back to
    /// the well-known folder under the RCCP install path so the converter keeps working
    /// in projects without the Demo Content addon. Returns an empty string when neither
    /// resolves (e.g. that pipeline's shaders were never imported).
    /// </summary>
    private static string ResolveShaderContentPath(Object contentReference, string relativeFolder) {

        if (contentReference != null) {

            string path = AssetDatabase.GetAssetPath(contentReference);

            if (!string.IsNullOrEmpty(path))
                return path;

        }

        string fallbackPath = RCCP_AssetUtilities.BasePath + relativeFolder;

        if (AssetDatabase.IsValidFolder(fallbackPath))
            return fallbackPath;

        return "";

    }

    /// <summary>
    /// Deletes all assets at the given content path
    /// </summary>
    private static void RemovePipelineContent(string path) {

        if (string.IsNullOrEmpty(path)) {
            Debug.LogWarning("Shader content path could not be resolved, skipping removal.");
            return;
        }

        // If folder, delete folder, else delete asset file
        if (AssetDatabase.IsValidFolder(path)) {
            if (AssetDatabase.DeleteAsset(path)) {
                Debug.Log("Deleted folder at " + path);
            } else {
                Debug.LogError("Failed to delete folder at " + path);
            }
        } else {
            if (AssetDatabase.DeleteAsset(path)) {
                Debug.Log("Deleted asset at " + path);
            } else {
                Debug.LogError("Failed to delete asset at " + path);
            }
        }
    }

    /// <summary>
    /// Called when a package import completes. Only proceeds if it matches our expected builtin shader package.
    /// </summary>
    private static void OnBuiltinShadersImported(string packageName) {

        // Ignore unrelated package imports - stay subscribed until our package arrives
        if (_pendingBuiltinPackageName != null && packageName != _pendingBuiltinPackageName)
            return;

        ReleaseBuiltinCallbacks();

        // ConvertCustomShaders handles built-in shader deletion and shows a completion dialog
        ConvertCustomShaders(true);

    }

    /// <summary>
    /// Called when the user cancels the builtin shader package import. Releases the pending callbacks.
    /// </summary>
    private static void OnBuiltinShadersImportCancelled(string packageName) {

        if (_pendingBuiltinPackageName != null && packageName != _pendingBuiltinPackageName)
            return;

        ReleaseBuiltinCallbacks();

    }

    /// <summary>
    /// Called when the builtin shader package import fails. Releases the pending callbacks and logs the error.
    /// </summary>
    private static void OnBuiltinShadersImportFailed(string packageName, string errorMessage) {

        if (_pendingBuiltinPackageName != null && packageName != _pendingBuiltinPackageName)
            return;

        ReleaseBuiltinCallbacks();
        Debug.LogError("[RCCP] Built-in shader package import failed: " + errorMessage);

    }

    /// <summary>
    /// Removes all builtin shader import callbacks and clears the pending package name.
    /// </summary>
    private static void ReleaseBuiltinCallbacks() {

        AssetDatabase.importPackageCompleted -= OnBuiltinShadersImported;
        AssetDatabase.importPackageCancelled -= OnBuiltinShadersImportCancelled;
        AssetDatabase.importPackageFailed -= OnBuiltinShadersImportFailed;
        _pendingBuiltinPackageName = null;

    }

    /// <summary>
    /// Called when a package import completes. Only proceeds if it matches our expected URP shader package,
    /// then chains the CarPaint URP body shader package now that the import window is closed.
    /// </summary>
    private static void OnURPShadersImported(string packageName) {

        // Ignore unrelated package imports - stay subscribed until our URP shader package arrives
        if (_pendingURPPackageName != null && packageName != _pendingURPPackageName)
            return;

        string carPaintPackagePath = _pendingCarPaintPackagePath;
        ReleaseURPCallbacks();

        // Import the CarPaint URP body shader package silently. Safe now - the interactive import is finished.
        if (!string.IsNullOrEmpty(carPaintPackagePath) && File.Exists(carPaintPackagePath))
            AssetDatabase.ImportPackage(carPaintPackagePath, false);
        else
            Debug.LogWarning("[RCCP] CarPaint URP shader package not found at " + carPaintPackagePath);

    }

    /// <summary>
    /// Called when the user cancels the URP shader package import. Releases the pending callbacks
    /// without importing the CarPaint URP package.
    /// </summary>
    private static void OnURPShadersImportCancelled(string packageName) {

        if (_pendingURPPackageName != null && packageName != _pendingURPPackageName)
            return;

        ReleaseURPCallbacks();

    }

    /// <summary>
    /// Called when the URP shader package import fails. Releases the pending callbacks and logs the error.
    /// </summary>
    private static void OnURPShadersImportFailed(string packageName, string errorMessage) {

        if (_pendingURPPackageName != null && packageName != _pendingURPPackageName)
            return;

        ReleaseURPCallbacks();
        Debug.LogError("[RCCP] URP shader package import failed: " + errorMessage);

    }

    /// <summary>
    /// Removes all URP shader import callbacks and clears the pending package state.
    /// </summary>
    private static void ReleaseURPCallbacks() {

        AssetDatabase.importPackageCompleted -= OnURPShadersImported;
        AssetDatabase.importPackageCancelled -= OnURPShadersImportCancelled;
        AssetDatabase.importPackageFailed -= OnURPShadersImportFailed;
        _pendingURPPackageName = null;
        _pendingCarPaintPackagePath = null;

    }

    /// <summary>
    /// Called when a package import completes. Only proceeds if it matches our expected HDRP shader package,
    /// then chains the HDRP volume profile package now that the import window is closed.
    /// </summary>
    private static void OnHDRPShadersImported(string packageName) {

        // Ignore unrelated package imports - stay subscribed until our HDRP shader package arrives
        if (_pendingHDRPPackageName != null && packageName != _pendingHDRPPackageName)
            return;

        ReleaseHDRPCallbacks();

        ImportPackage(RCCP_AddonPackages.Instance.HDRPVolumeProfile);

    }

    /// <summary>
    /// Called when the user cancels the HDRP shader package import. Releases the pending callbacks
    /// without importing the volume profile package.
    /// </summary>
    private static void OnHDRPShadersImportCancelled(string packageName) {

        if (_pendingHDRPPackageName != null && packageName != _pendingHDRPPackageName)
            return;

        ReleaseHDRPCallbacks();

    }

    /// <summary>
    /// Called when the HDRP shader package import fails. Releases the pending callbacks and logs the error.
    /// </summary>
    private static void OnHDRPShadersImportFailed(string packageName, string errorMessage) {

        if (_pendingHDRPPackageName != null && packageName != _pendingHDRPPackageName)
            return;

        ReleaseHDRPCallbacks();
        Debug.LogError("[RCCP] HDRP shader package import failed: " + errorMessage);

    }

    /// <summary>
    /// Removes all HDRP shader import callbacks and clears the pending package name.
    /// </summary>
    private static void ReleaseHDRPCallbacks() {

        AssetDatabase.importPackageCompleted -= OnHDRPShadersImported;
        AssetDatabase.importPackageCancelled -= OnHDRPShadersImportCancelled;
        AssetDatabase.importPackageFailed -= OnHDRPShadersImportFailed;
        _pendingHDRPPackageName = null;

    }

    /// <summary>
    /// Imports a .unitypackage from the path of the given package object
    /// </summary>
    private static void ImportPackage(Object packageObject) {
        if (packageObject == null) {
            Debug.LogError("Package object is null, cannot import.");
            return;
        }

        var packagePath = AssetDatabase.GetAssetPath(packageObject);

        if (string.IsNullOrEmpty(packagePath)) {
            Debug.LogError("Could not find package path for object: " + packageObject.name);
            return;
        }

        AssetDatabase.ImportPackage(packagePath, true);
        Debug.Log("Imported package: " + packagePath);
    }

    /// <summary>
    /// Quick helpers for querying installed render-pipeline package versions.
    /// Works only in the Editor; returns an empty string in a built player.
    /// </summary>
    public static class RCCP_PipelineVersion {

        /// <summary>
        /// Returns the full version of Universal Render Pipeline
        /// (e.g. "17.0.3"), or <c>string.Empty</c> if URP is not installed.
        /// </summary>
        public static string GetURPVersion() {

            return GetPackageVersion("com.unity.render-pipelines.universal");

        }

        /// <summary>
        /// Returns the full version of High Definition Render Pipeline
        /// (e.g. "17.0.3"), or <c>string.Empty</c> if HDRP is not installed.
        /// </summary>
        public static string GetHDRPVersion() {

            return GetPackageVersion("com.unity.render-pipelines.high-definition");

        }

        /// <summary>
        /// Looks up a package by its name and returns its <see cref="PackageInfo.version"/>.
        /// </summary>
        private static string GetPackageVersion(string packageName) {

#if UNITY_EDITOR

            // This is a very fast, synchronous lookup that does not allocate
            // a full Client.List() request.
            var packageInfo = UnityEditor.PackageManager.PackageInfo.FindForAssetPath($"Packages/{packageName}");

            if (packageInfo != null)
                return packageInfo.version;

#endif
            return string.Empty;

        }

        /// <summary>
        /// Returns <c>true</c> if the given pipeline is installed and its
        /// <see cref="PackageInfo.major"/> is at least <paramref name="minimumMajor"/>.
        /// Handy when you need to branch on URP 17 vs URP 15, etc.
        /// </summary>
        public static bool HasAtLeast(string packageName, int minimumMajor) {

            string ver = GetPackageVersion(packageName);

            if (string.IsNullOrEmpty(ver))
                return false;

            // Package versions use "major.minor.patch", so split on '.'
            if (int.TryParse(ver.Split('.')[0], out int major))
                return major >= minimumMajor;

            return false;

        }

    }

}
#endif
