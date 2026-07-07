//----------------------------------------------
//        Realistic Car Controller Pro
//
// Copyright 2014 - 2026 BoneCracker Games
// https://www.bonecrackergames.com
// Ekrem Bugra Ozdoganlar
//
//----------------------------------------------

using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// Editor automation that keeps a single <see cref="RCCP_SceneManager"/> present whenever a loaded scene
/// contains an <see cref="RCCP_CarController"/> — <b>selection-independent</b>. Previously the scene manager
/// was only created as a side-effect of <see cref="RCCP_SceneManager"/>.Instance being read inside the
/// car-controller custom editor, which meant a vehicle had to be SELECTED before the manager appeared. This
/// hook removes that requirement; the singleton itself is unchanged (still the runtime source of truth).
/// <para>
/// Triggers on scene open, on hierarchy changes, and once after each domain reload. CREATES at most ONE
/// manager — if any already exists in the loaded scenes it reuses that one and creates nothing (it does not
/// remove pre-existing duplicates an author may have placed by hand; a second one would double-register every
/// vehicle and fight over the camera/UI binding at runtime). Opt out via <c>Tools ▸ BoneCracker Games ▸ Realistic Car Controller Pro ▸
/// Add to Scene ▸ Auto-Create Scene Manager</c>. Skips Play Mode, prefab-stage editing, re-entrancy, and
/// (via <see cref="Suppressed"/>) automated test runs.
/// </para>
/// </summary>
[InitializeOnLoad]
public static class RCCP_SceneManagerAutoCreate {

    private const string EnabledKey = "RCCP_AutoCreateSceneManager";
    private const string MenuPath = "Tools/BoneCracker Games/Realistic Car Controller Pro/Add to Scene/Auto-Create Scene Manager";

    // Re-entrancy guard: creating the manager fires hierarchyChanged again — ignore that nested call.
    private static bool processing;

    /// <summary>
    /// When true, all auto-creation is suppressed. Provided so an automated test fixture can disable scene
    /// mutation for the whole run (RCCP core ships no EditMode test asmdef today, but the hook stays parity
    /// with the RHCP source). Editor-only static — never part of the runtime build.
    /// </summary>
    public static bool Suppressed { get; set; }

    /// <summary>Whether auto-creation is enabled (EditorPrefs-backed, default on). Toggled from the Tools menu.</summary>
    public static bool Enabled {
        get => EditorPrefs.GetBool(EnabledKey, true);
        set => EditorPrefs.SetBool(EnabledKey, value);
    }

    static RCCP_SceneManagerAutoCreate() {

        EditorApplication.hierarchyChanged += AutoEnsure;
        EditorSceneManager.sceneOpened += OnSceneOpened;
        // Handle scenes already open when this script (re)loads, after the in-flight import/reload settles.
        EditorApplication.delayCall += AutoEnsure;

    }

    private static void OnSceneOpened(Scene scene, OpenSceneMode mode) => AutoEnsure();

    /// <summary>Auto path: honours the <see cref="Enabled"/> opt-out, then runs the guarded ensure.</summary>
    private static void AutoEnsure() {

        if (!Enabled)
            return;

        EnsureSceneManager(select: false, requireVehicle: true);

    }

    /// <summary>
    /// Ensures a single <see cref="RCCP_SceneManager"/> exists across the loaded scenes. Returns the existing
    /// or newly created manager, or null when skipped (Play Mode, prefab stage, suppressed, or
    /// <paramref name="requireVehicle"/> with no vehicle present).
    /// </summary>
    /// <param name="select">When true (manual menu action), selects/pings the manager.</param>
    /// <param name="requireVehicle">When true (auto path), only creates if an RCCP_CarController is present.</param>
    public static RCCP_SceneManager EnsureSceneManager(bool select, bool requireVehicle) {

        if (processing || Suppressed)
            return null;

        if (EditorApplication.isPlayingOrWillChangePlaymode)
            return null;

        if (PrefabStageUtility.GetCurrentPrefabStage() != null)
            return null; // never inject a scene manager into prefab-stage editing

        // One manager across ALL loaded scenes — a second would double-register every vehicle and fight over
        // the camera/UI binding at runtime. If one already exists, that's the singleton-like result.
        RCCP_SceneManager[] existing = Object.FindObjectsByType<RCCP_SceneManager>(FindObjectsInactive.Include, FindObjectsSortMode.None);

        if (existing.Length > 0) {

            if (select)
                Selection.activeGameObject = existing[0].gameObject;

            return existing[0];

        }

        RCCP_CarController[] vehicles = Object.FindObjectsByType<RCCP_CarController>(FindObjectsInactive.Include, FindObjectsSortMode.None);

        if (requireVehicle && vehicles.Length == 0)
            return null; // no vehicle → nothing to manage

        // Land it in the vehicle's scene (multi-scene-friendly), falling back to the active scene.
        Scene target = default;

        if (vehicles.Length > 0 && vehicles[0] != null)
            target = vehicles[0].gameObject.scene;

        if (!target.IsValid() || !target.isLoaded)
            target = SceneManager.GetActiveScene();

        processing = true;

        try {

            GameObject go = new GameObject("RCCP_SceneManager", typeof(RCCP_SceneManager));

            if (go.scene != target && target.IsValid() && target.isLoaded)
                SceneManager.MoveGameObjectToScene(go, target);

            Undo.RegisterCreatedObjectUndo(go, "Create RCCP Scene Manager");
            EditorSceneManager.MarkSceneDirty(go.scene);

            if (select)
                Selection.activeGameObject = go;

            return go.GetComponent<RCCP_SceneManager>();

        } finally {

            processing = false;

        }

    }

    // ------------------------------------------------------------------
    // Auto-create toggle — checkmarked, EditorPrefs-backed. When on, a scene that contains a vehicle
    // automatically gets a single RCCP_SceneManager (no selection needed). The runtime singleton is unchanged.
    // ------------------------------------------------------------------

    [MenuItem(MenuPath, false, 41)]
    private static void ToggleAutoCreate() => Enabled = !Enabled;

    [MenuItem(MenuPath, true)]
    private static bool ToggleAutoCreateValidate() {

        Menu.SetChecked(MenuPath, Enabled);
        return true;

    }

}
