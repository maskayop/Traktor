//----------------------------------------------
//        Realistic Car Controller Pro
//
// Copyright (c) 2014 - 2025 BoneCracker Games
// https://www.bonecrackergames.com
// Ekrem Bugra Ozdoganlar
//
//----------------------------------------------

#if UNITY_EDITOR

using UnityEditor;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;
using UnityEngine;

[InitializeOnLoad]
public class RCCP_MobilePlatformNotifier : IActiveBuildTargetChanged {

    private const string PopupShownKey = "RCCP_MobilePlatformNotifier";

    static RCCP_MobilePlatformNotifier() {

        // Reset the popup flag when the editor is started
        EditorPrefs.SetBool(PopupShownKey, false);

    }

    public int callbackOrder => 0;

    public void OnActiveBuildTargetChanged(BuildTarget previousTarget, BuildTarget newTarget) {

        // Check if the new build target is a mobile platform
        if ((newTarget == BuildTarget.Android || newTarget == BuildTarget.iOS))
            ShowMobilePlatformInfo();

        ShowListInfo();

    }

    private static void ShowMobilePlatformInfo() {

        //  V2.51 (T3-5): offer one-click enable of the mobile controller, mirroring RCCP_CheckBeforePlay's
        //  Play-time prompt. The touch canvas still has to be added to the scene manually — note that caveat.
        bool enableNow = EditorUtility.DisplayDialog(

            "Realistic Car Controller Pro | Mobile Platform Detected",
            "You have switched the build platform to a mobile platform (Android/iOS).\n\nDo you want to enable the mobile controller now? (You still need to add the RCCP UI Canvas to your scene via Tools --> BoneCracker Games --> RCCP --> Add to Scene --> UI Canvas for the touch controls to appear.)\n\nIf you're using URP, consider disabling additional lights and shadows for better performance on older devices.",
            "Enable Mobile Controller",
            "Not Now"

        );

        if (enableNow && RCCP_Settings.Instance != null) {

            RCCP_Settings.Instance.mobileControllerEnabled = true;
            EditorUtility.SetDirty(RCCP_Settings.Instance);
            Debug.Log("RCCP: mobile controller enabled. Remember to add the RCCP UI Canvas to your scene.");

        }

    }

    private static void ShowListInfo() {

        EditorUtility.DisplayDialog(

    "Realistic Car Controller Pro | Build Platform Changed",
    "You have switched the build platform. If you are having compiler errors related to RCCP after changing it, most likely scripting define symbol list (Edit --> Project Settings --> Player) in your project settings has old keys. This happens if you import an addon package, and delete after a while.\n\nBe sure to have proper keys in the list. Remove keys from the list if your project doesn't have that addon.\n\nMore info can be found in the documentation.",
    "OK"

);

    }

}

#endif
