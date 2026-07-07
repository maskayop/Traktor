//----------------------------------------------
//        Realistic Car Controller Pro
//
// Copyright 2014 - 2026 BoneCracker Games
// https://www.bonecrackergames.com
// Ekrem Bugra Ozdoganlar
//
//----------------------------------------------

using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// UI glue for photo mode. Put this on an ALWAYS-ACTIVE object (e.g. the canvas root). Wire the settings-menu
/// "Photo Mode" button to TogglePhotoMode(), and the in-photo overlay's controls to Capture() / CycleCameraMode() /
/// TogglePhotoMode() (exit) / OnFOVChanged (slider). Entering photo mode hides the assigned gameplay/settings roots,
/// shows the overlay, and (when RCCP_Settings.mobileControllerEnabled) shows on-screen joysticks that drive the
/// photo camera through RCCP_PhotoMode.touchLeftStick / touchRightStick.
/// </summary>
public class RCCP_UI_PhotoMode : RCCP_UIComponent {

    /// <summary>
    /// Gameplay HUD / settings roots hidden while photo mode is active (dashboard, mobile controls, options, etc.).
    /// </summary>
    [Tooltip("Gameplay HUD / settings roots hidden while photo mode is active.")]
    public GameObject[] hideWhilePhotoMode;

    /// <summary>
    /// Minimal overlay shown only while photo mode is active (capture / camera-mode / exit / FOV).
    /// </summary>
    [Tooltip("Minimal overlay shown only while photo mode is active.")]
    public GameObject photoModeOverlay;

    /// <summary>
    /// Optional UI root hidden for the capture frames so it doesn't appear in the screenshot (usually the overlay).
    /// </summary>
    [Tooltip("Optional UI root hidden during capture so it doesn't appear in the screenshot (usually the overlay).")]
    public GameObject hideWhileCapturing;

    /// <summary>
    /// Optional label updated to the active camera mode name ("Camera: Orbit" / "Free Cam" / "Auto-Orbit").
    /// </summary>
    [Tooltip("Optional label updated to the active camera mode name.")]
    public TextMeshProUGUI modeLabel;

    /// <summary>
    /// Optional field-of-view slider. Wire its OnValueChanged to OnFOVChanged.
    /// </summary>
    [Tooltip("Optional field-of-view slider. Wire OnValueChanged -> OnFOVChanged.")]
    public Slider fovSlider;

    [Header("Mobile Joysticks")]
    /// <summary>
    /// Root of the on-screen joysticks, shown only while in photo mode AND RCCP_Settings.mobileControllerEnabled.
    /// </summary>
    [Tooltip("Root of the on-screen joysticks (shown only in photo mode when mobile controller is enabled).")]
    public GameObject mobileJoysticksRoot;

    /// <summary>
    /// Left joystick. Orbit: rotate. Free Cam: move.
    /// </summary>
    [Tooltip("Left joystick. Orbit: rotate. Free Cam: move.")]
    public RCCP_UI_Joystick leftJoystick;

    /// <summary>
    /// Right joystick. Orbit: zoom (Y). Free Cam: look.
    /// </summary>
    [Tooltip("Right joystick. Orbit: zoom (Y). Free Cam: look.")]
    public RCCP_UI_Joystick rightJoystick;

    //  Active state of each hideWhilePhotoMode root before photo mode was entered, so exit restores it exactly.
    private bool[] priorActiveStates;

    //  True while we should feed the on-screen joysticks into RCCP_PhotoMode (avoids creating the singleton early).
    private bool feedingTouch;

    //  Held state of the on-screen elevation (Up / Down) buttons.
    private bool elevateUpHeld, elevateDownHeld;

    /// <summary>
    /// Enters photo mode if inactive (hiding the HUD, showing the overlay + mobile joysticks), exits if active.
    /// </summary>
    public void TogglePhotoMode() {

        RCCP_PhotoMode pm = RCCP_PhotoMode.Instance;

        if (pm.InPhotoMode) {

            pm.ExitPhotoMode();
            SetPhotoUIActive(false);
            feedingTouch = false;

        } else {

            pm.EnterPhotoMode();

            //  Only flip the UI if we actually entered (EnterPhotoMode no-ops without an active player vehicle).
            if (pm.InPhotoMode) {

                SetPhotoUIActive(true);
                RefreshModeLabel();
                InitFovSlider();
                feedingTouch = true;

            }

        }

    }

    /// <summary>
    /// Cycles the photo camera mode (Orbit -> Free Cam -> Auto-Orbit) and refreshes the label.
    /// </summary>
    public void CycleCameraMode() {

        RCCP_PhotoMode.Instance.CyclePhotoCameraMode();
        RefreshModeLabel();

    }

    /// <summary>
    /// Sets orbit camera mode.
    /// </summary>
    public void SetOrbitMode() {

        RCCP_PhotoMode.Instance.SetCameraMode(RCCP_PhotoMode.PhotoCameraMode.Orbit);
        RefreshModeLabel();

    }

    /// <summary>
    /// Sets free-cam camera mode.
    /// </summary>
    public void SetFreeCamMode() {

        RCCP_PhotoMode.Instance.SetCameraMode(RCCP_PhotoMode.PhotoCameraMode.FreeCam);
        RefreshModeLabel();

    }

    /// <summary>
    /// Slider callback: sets the photo camera field of view.
    /// </summary>
    public void OnFOVChanged(float value) {

        RCCP_PhotoMode.Instance.SetFieldOfView(value);

    }

    /// <summary>
    /// Captures a screenshot, hiding the assigned UI root for the capture frames.
    /// </summary>
    public void Capture() {

        StartCoroutine(CaptureRoutine());

    }

    private void Update() {

        if (!feedingTouch)
            return;

        RCCP_PhotoMode pm = RCCP_PhotoMode.Instance;

        if (!pm.InPhotoMode) {

            feedingTouch = false;
            return;

        }

        //  Feed the on-screen joysticks + elevation only while they're actually shown (mobile controller enabled).
        if (mobileJoysticksRoot != null && mobileJoysticksRoot.activeInHierarchy) {

            pm.touchLeftStick = leftJoystick != null ? new Vector2(leftJoystick.inputHorizontal, leftJoystick.inputVertical) : Vector2.zero;
            pm.touchRightStick = rightJoystick != null ? new Vector2(rightJoystick.inputHorizontal, rightJoystick.inputVertical) : Vector2.zero;
            pm.touchElevation = (elevateUpHeld ? 1f : 0f) - (elevateDownHeld ? 1f : 0f);

        } else {

            pm.touchLeftStick = Vector2.zero;
            pm.touchRightStick = Vector2.zero;
            pm.touchElevation = 0f;

        }

    }

    /// <summary>Begins raising the free-cam camera (on-screen Up button pointer-down).</summary>
    public void StartElevateUp() { elevateUpHeld = true; }

    /// <summary>Stops raising the free-cam camera (Up button pointer-up).</summary>
    public void StopElevateUp() { elevateUpHeld = false; }

    /// <summary>Begins lowering the free-cam camera (on-screen Down button pointer-down).</summary>
    public void StartElevateDown() { elevateDownHeld = true; }

    /// <summary>Stops lowering the free-cam camera (Down button pointer-up).</summary>
    public void StopElevateDown() { elevateDownHeld = false; }

    private void SetPhotoUIActive(bool photoActive) {

        if (hideWhilePhotoMode != null) {

            if (photoActive) {

                //  Entering: remember each root's current state, then hide it.
                priorActiveStates = new bool[hideWhilePhotoMode.Length];

                for (int i = 0; i < hideWhilePhotoMode.Length; i++) {

                    GameObject go = hideWhilePhotoMode[i];
                    priorActiveStates[i] = go != null && go.activeSelf;

                    if (go != null)
                        go.SetActive(false);

                }

            } else {

                //  Exiting: restore each root to the state it had before entering.
                for (int i = 0; i < hideWhilePhotoMode.Length; i++) {

                    GameObject go = hideWhilePhotoMode[i];

                    if (go != null)
                        go.SetActive(priorActiveStates != null && i < priorActiveStates.Length ? priorActiveStates[i] : true);

                }

            }

        }

        if (photoModeOverlay != null)
            photoModeOverlay.SetActive(photoActive);

        //  On-screen joysticks only when entering AND the mobile controller is enabled.
        if (mobileJoysticksRoot != null)
            mobileJoysticksRoot.SetActive(photoActive && RCCPSettings != null && RCCPSettings.mobileControllerEnabled);

    }

    private void RefreshModeLabel() {

        if (modeLabel != null)
            modeLabel.text = "Camera: " + RCCP_PhotoMode.Instance.CurrentModeName;

    }

    private void InitFovSlider() {

        if (fovSlider == null)
            return;

        RCCP_PhotoMode pm = RCCP_PhotoMode.Instance;
        fovSlider.minValue = pm.minFOV;
        fovSlider.maxValue = pm.maxFOV;
        fovSlider.SetValueWithoutNotify(pm.fieldOfView);

    }

    private IEnumerator CaptureRoutine() {

        if (hideWhileCapturing)
            hideWhileCapturing.SetActive(false);

        //  Let the hidden UI leave the frame before capturing (frame-based, works at timeScale 0).
        yield return null;

        RCCP_PhotoMode.Instance.CapturePhoto();

        //  Give ScreenCapture time to flush before showing the UI again.
        yield return new WaitForSecondsRealtime(.25f);

        if (hideWhileCapturing)
            hideWhileCapturing.SetActive(true);

    }

}
