//----------------------------------------------
//        Realistic Car Controller Pro
//
// Copyright 2014 - 2026 BoneCracker Games
// https://www.bonecrackergames.com
// Ekrem Bugra Ozdoganlar
//
//----------------------------------------------

#if UNITY_EDITOR

using UnityEngine;
using UnityEngine.UIElements;
using UnityEditor;

/// <summary>
/// Demos tab content for the RCCP Welcome Window.
/// Provides demo scene loading buttons with conditional addon support.
/// </summary>
public class RCCP_WelcomeTab_Demos : IRCCP_WelcomeTabContent {

    private readonly RCCP_WelcomeWindowController controller;

    public RCCP_WelcomeTab_Demos(RCCP_WelcomeWindowController controller) {
        this.controller = controller;
    }

    public VisualElement CreateContent() {

        var root = new VisualElement();

        // Demo content status.
        var demoSection = RCCP_WelcomeWindowUI.CreateSection("Demo Scenes");

#if !RCCP_DEMO

        demoSection.Add(RCCP_WelcomeWindowUI.CreateHelpBox(
            "Demo assets are not imported. Import them from the Addons tab first.",
            "info"
        ));

        demoSection.Add(RCCP_WelcomeWindowUI.CreateButton("Import Demo Content", () => {
            bool decision = EditorUtility.DisplayDialog(
                "Realistic Car Controller Pro | Import Demo Content",
                "Do you want to import demo assets to your project? This will increase your build size even if you don't use them.",
                "Yes, import demo assets", "No");
            if (decision)
                RCCP_WelcomeWindowController.ImportPackageSafe(RCCP_AddonPackages.Instance.demoPackage, "Demo Content");
        }, "primary"));

#else

        demoSection.Add(RCCP_WelcomeWindowUI.CreateHelpBox(
            "Demo assets are installed. You can open demo scenes below.",
            "success"
        ));

#endif

        root.Add(demoSection);

        // Build settings warning.
        root.Add(RCCP_WelcomeWindowUI.CreateHelpBox(
            "The AIO demo loads multiple scenes at runtime. Add all demo scenes to Build Settings — the AIO menu scene will be placed at index 0 so it runs first.",
            "warning"
        ));

        root.Add(RCCP_WelcomeWindowUI.CreateButton("Add Demo Scenes to Build Settings", () => {

            var result = controller.AddDemoScenesToBuildSettings();

            string body;

            if (result.added == 0 && result.skipped == 0)
                body = "No demo scenes were found. Import the Demo Content package first.";
            else if (result.added == 0)
                body = $"All {result.skipped} demo scene(s) were already in Build Settings.";
            else if (result.skipped == 0)
                body = $"Added {result.added} demo scene(s) to Build Settings.";
            else
                body = $"Added {result.added} demo scene(s). {result.skipped} scene(s) were already present.";

            if (result.aioMovedToFirst)
                body += "\n\nThe AIO menu scene was moved to index 0 — required because it loads every other demo scene by name at runtime.";

            EditorUtility.DisplayDialog(
                "Realistic Car Controller Pro | Build Settings Updated",
                body,
                "OK"
            );

        }, "success"));

        // Core RCCP scenes — ship with the asset regardless of the Demo Content addon.
        var coreTitle = new Label("Core Scenes");
        coreTitle.AddToClassList("rccp-welcome-section__title");
        root.Add(coreTitle);

        var coreEssentialRow = new VisualElement();
        coreEssentialRow.AddToClassList("rccp-welcome-scene-card-row");

        coreEssentialRow.Add(RCCP_WelcomeWindowUI.CreateSceneCard("Prototype",
            "Blank prototype scene for quick testing",
            () => OpenDemoScene(d => d.path_demo_protototype, "Prototype")));
        // NOTE: the Prototype card stays enabled without the Demo Content addon (the scene
        // ships with core RCCP), so OpenDemoScene must tolerate a missing demo registry.

        root.Add(coreEssentialRow);

        // Demo content scenes — all live under Addons/Installed/Demo Content/ and require the
        // Demo Content addon to be imported, so the entire row is disabled when RCCP_DEMO is off.
        var demoTitle = new Label("Demo Scenes");
        demoTitle.AddToClassList("rccp-welcome-section__title");
        root.Add(demoTitle);

        var coreWrapper = new VisualElement();

#if !RCCP_DEMO
        coreWrapper.SetEnabled(false);
#endif

        var coreRow = new VisualElement();
        coreRow.AddToClassList("rccp-welcome-scene-card-row");

        coreRow.Add(RCCP_WelcomeWindowUI.CreateSceneCard("City AIO",
            "Hub menu that loads every demo scene",
            () => OpenDemoScene(d => d.path_city_AIO, "City AIO")));

        coreRow.Add(RCCP_WelcomeWindowUI.CreateSceneCard("City",
            "City environment driving demo",
            () => OpenDemoScene(d => d.path_demo_City, "City")));

        coreRow.Add(RCCP_WelcomeWindowUI.CreateSceneCard("City AI",
            "City with AI-driven vehicles on waypoints",
            () => OpenDemoScene(d => d.path_demo_CityWithAI, "City AI")));

        coreRow.Add(RCCP_WelcomeWindowUI.CreateSceneCard("Car Selection",
            "Menu scene for vehicle picker",
            () => OpenDemoScene(d => d.path_demo_CarSelection, "Car Selection")));

        coreRow.Add(RCCP_WelcomeWindowUI.CreateSceneCard("Blank API",
            "Minimal scene for API testing",
            () => OpenDemoScene(d => d.path_demo_APIBlank, "Blank API")));

        coreRow.Add(RCCP_WelcomeWindowUI.CreateSceneCard("Blank Mobile",
            "Mobile-ready blank canvas",
            () => OpenDemoScene(d => d.path_demo_BlankMobile, "Blank Mobile")));

        coreRow.Add(RCCP_WelcomeWindowUI.CreateSceneCard("Damage",
            "Crash and deformation demo",
            () => OpenDemoScene(d => d.path_demo_Damage, "Damage")));

        coreRow.Add(RCCP_WelcomeWindowUI.CreateSceneCard("Customization",
            "Paint, wheels, and upgrades demo",
            () => OpenDemoScene(d => d.path_demo_Customization, "Customization")));

        coreRow.Add(RCCP_WelcomeWindowUI.CreateSceneCard("Override Inputs",
            "Custom input mapping demo",
            () => OpenDemoScene(d => d.path_demo_OverrideInputs, "Override Inputs")));

        coreRow.Add(RCCP_WelcomeWindowUI.CreateSceneCard("Transport",
            "Vehicle plus trailer towing demo",
            () => OpenDemoScene(d => d.path_demo_Transport, "Transport")));

        coreWrapper.Add(coreRow);
        root.Add(coreWrapper);

        // Enter/Exit scenes.
        var eeTitle = new Label("Enter / Exit Scenes");
        eeTitle.AddToClassList("rccp-welcome-section__title");
        root.Add(eeTitle);

#if BCG_ENTEREXIT

        // BCG_DemoScenes ships with BCG Shared Assets alongside the define, but guard
        // anyway — these dereferences run at tab BUILD time, and a partially deleted
        // Shared Assets install would otherwise break the entire Demos tab.
        BCG_DemoScenes bcgScenes = BCG_DemoScenes.Instance;

        if (bcgScenes != null) {

            var eeRow = new VisualElement();
            eeRow.AddToClassList("rccp-welcome-scene-card-row");

            if (bcgScenes.demo_BlankFPS)
                eeRow.Add(RCCP_WelcomeWindowUI.CreateSceneCard("Blank FPS",
                    "First-person enter/exit blank scene",
                    () => {
                        bcgScenes.GetPaths();
                        RCCP_WelcomeWindowController.OpenDemoSceneSafe(bcgScenes.path_demo_BlankFPS, "Blank FPS");
                    }));

            if (bcgScenes.demo_BlankTPS)
                eeRow.Add(RCCP_WelcomeWindowUI.CreateSceneCard("Blank TPS",
                    "Third-person enter/exit blank scene",
                    () => {
                        bcgScenes.GetPaths();
                        RCCP_WelcomeWindowController.OpenDemoSceneSafe(bcgScenes.path_demo_BlankTPS, "Blank TPS");
                    }));

#if RCCP_DEMO
            if (bcgScenes.demo_CityFPS)
                eeRow.Add(RCCP_WelcomeWindowUI.CreateSceneCard("City FPS",
                    "First-person city enter/exit demo",
                    () => {
                        bcgScenes.GetPaths();
                        RCCP_WelcomeWindowController.OpenDemoSceneSafe(bcgScenes.path_demo_CityFPS, "City FPS");
                    }));

            if (bcgScenes.demo_CityTPS)
                eeRow.Add(RCCP_WelcomeWindowUI.CreateSceneCard("City TPS",
                    "Third-person city enter/exit demo",
                    () => {
                        bcgScenes.GetPaths();
                        RCCP_WelcomeWindowController.OpenDemoSceneSafe(bcgScenes.path_demo_CityTPS, "City TPS");
                    }));
#endif

            root.Add(eeRow);

        } else {

            root.Add(RCCP_WelcomeWindowUI.CreateHelpBox(
                "BCG demo scene registry not found. Reimport BCG Shared Assets from the Addons tab.",
                "warning"
            ));

        }

#else

        root.Add(RCCP_WelcomeWindowUI.CreateHelpBox(
            "Enter / Exit vehicle scenes require BCG Shared Assets. Install it from the Addons tab.",
            "info"
        ));

#endif

        // Traffic (RTC) scenes.
        var trafficTitle = new Label("Traffic Scenes");
        trafficTitle.AddToClassList("rccp-welcome-section__title");
        root.Add(trafficTitle);

#if BCG_RTRC

        var trafficRow = new VisualElement();
        trafficRow.AddToClassList("rccp-welcome-scene-card-row");

        trafficRow.Add(RCCP_WelcomeWindowUI.CreateSceneCard("City RTC",
            "City with Realistic Traffic Controller AI traffic",
            () => OpenDemoScene(d => d.path_demo_CityWithTraffic, "City RTC")));

        root.Add(trafficRow);

#else

        root.Add(RCCP_WelcomeWindowUI.CreateHelpBox(
            "AI traffic scenes require the Realistic Traffic Controller integration. Install it from the Addons tab.",
            "info"
        ));

#endif

        // Photon scenes.
        var photonTitle = new Label("Photon PUN2 Scenes");
        photonTitle.AddToClassList("rccp-welcome-section__title");
        root.Add(photonTitle);

#if RCCP_PHOTON && PHOTON_UNITY_NETWORKING

        var photonRow = new VisualElement();
        photonRow.AddToClassList("rccp-welcome-scene-card-row");

        photonRow.Add(RCCP_WelcomeWindowUI.CreateSceneCard("Lobby Photon",
            "Multiplayer lobby with room browser",
            () => {
                var photonScenes = RCCP_DemoScenes_Photon.Instance;
                if (photonScenes != null)
                    photonScenes.GetPaths();
                RCCP_WelcomeWindowController.OpenDemoSceneSafe(photonScenes != null ? photonScenes.path_demo_PUN2Lobby : null, "Lobby Photon");
            }));

        photonRow.Add(RCCP_WelcomeWindowUI.CreateSceneCard("Blank Photon",
            "Minimal multiplayer gameplay scene",
            () => {
                var photonScenes = RCCP_DemoScenes_Photon.Instance;
                if (photonScenes != null)
                    photonScenes.GetPaths();
                RCCP_WelcomeWindowController.OpenDemoSceneSafe(photonScenes != null ? photonScenes.path_demo_PUN2City : null, "Blank Photon");
            }));

        root.Add(photonRow);

#else

        root.Add(RCCP_WelcomeWindowUI.CreateHelpBox(
            "Photon PUN2 multiplayer scenes require the Photon integration. Install it from the Addons tab.",
            "info"
        ));

#endif

        // Mirror scenes.
        var mirrorTitle = new Label("Mirror Scenes");
        mirrorTitle.AddToClassList("rccp-welcome-section__title");
        root.Add(mirrorTitle);

#if RCCP_MIRROR && MIRROR

        var mirrorRow = new VisualElement();
        mirrorRow.AddToClassList("rccp-welcome-scene-card-row");

        mirrorRow.Add(RCCP_WelcomeWindowUI.CreateSceneCard("Blank Mirror",
            "Minimal Mirror multiplayer scene",
            () => {
                var mirrorScenes = RCCP_DemoScenes_Mirror.Instance;
                if (mirrorScenes != null)
                    mirrorScenes.GetPaths();
                RCCP_WelcomeWindowController.OpenDemoSceneSafe(mirrorScenes != null ? mirrorScenes.path_Demo_Blank_Mirror : null, "Blank Mirror");
            }));

        root.Add(mirrorRow);

#else

        root.Add(RCCP_WelcomeWindowUI.CreateHelpBox(
            "Mirror networking scenes require the Mirror integration. Install it from the Addons tab.",
            "info"
        ));

#endif

        return root;

    }

    /// <summary>
    /// Opens a demo scene resolved from the demo registry. The selector keeps the
    /// RCCP_DemoScenes.Instance dereference inside the null guard — the registry asset
    /// only exists when the Demo Content addon is imported. With a missing registry the
    /// call routes into OpenDemoSceneSafe's explanatory dialog instead of throwing.
    /// </summary>
    private void OpenDemoScene(System.Func<RCCP_DemoScenes, string> pathSelector, string displayName) {

        RCCP_DemoScenes demoScenes = RCCP_DemoScenes.Instance;

        if (demoScenes != null)
            demoScenes.GetPaths();

        RCCP_WelcomeWindowController.OpenDemoSceneSafe(demoScenes != null ? pathSelector(demoScenes) : null, displayName);

    }

    public void OnActivated() { }
    public void OnDeactivated() { }

}

#endif
