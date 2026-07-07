//----------------------------------------------
//        Realistic Car Controller Pro
//
// Copyright 2014 - 2026 BoneCracker Games
// https://www.bonecrackergames.com
// Ekrem Bugra Ozdoganlar
//
//----------------------------------------------

using System;
using System.IO;
using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// Photo mode manager. Freezes the simulation and moves a temporary camera around the active player
/// vehicle in one of several modes (Orbit, Free Cam, Auto-Orbit), with adjustable field of view, then
/// captures super-size screenshots to persistentDataPath/Photos. Enter / exit via RCCP.EnterPhotoMode() /
/// RCCP.ExitPhotoMode(), or the RCCP_UI_PhotoMode UI component. Mobile touch input is fed in through
/// touchLeftStick / touchRightStick (set each frame by RCCP_UI_PhotoMode when the on-screen joysticks are used).
/// </summary>
public class RCCP_PhotoMode : RCCP_Singleton<RCCP_PhotoMode> {

    /// <summary>
    /// Available photo camera modes.
    /// Orbit = drag to rotate + scroll to zoom around the car. FreeCam = WASD/QE fly with RMB look.
    /// AutoOrbit = slow automatic turntable rotation around the car.
    /// </summary>
    public enum PhotoCameraMode { Orbit, FreeCam, AutoOrbit }

    /// <summary>
    /// True while photo mode is active.
    /// </summary>
    public bool InPhotoMode { get; private set; }

    /// <summary>
    /// Allocation-free static mirror of InPhotoMode, readable WITHOUT creating the singleton, so per-frame
    /// systems (e.g. RCCP_UIManager) can cheaply skip re-managing HUD panels while photo mode owns them.
    /// </summary>
    public static bool IsActive { get; private set; }

    /// <summary>
    /// Active photo camera mode.
    /// </summary>
    [Tooltip("Active photo camera mode.")]
    public PhotoCameraMode cameraMode = PhotoCameraMode.Orbit;

    [Header("Field Of View")]
    /// <summary>
    /// Current photo camera field of view (degrees). Seeded from the source camera on enter.
    /// </summary>
    [Tooltip("Current photo camera field of view (degrees). Seeded from the source camera on enter.")]
    [Range(10f, 120f)] public float fieldOfView = 60f;

    /// <summary>
    /// Minimum field of view (degrees).
    /// </summary>
    [Tooltip("Minimum field of view (degrees).")]
    [Range(5f, 60f)] public float minFOV = 20f;

    /// <summary>
    /// Maximum field of view (degrees).
    /// </summary>
    [Tooltip("Maximum field of view (degrees).")]
    [Range(60f, 179f)] public float maxFOV = 90f;

    [Header("Orbit / Auto-Orbit")]
    /// <summary>
    /// Orbit sensitivity multiplier applied to the pointer delta.
    /// </summary>
    [Tooltip("Orbit sensitivity multiplier applied to the pointer delta.")]
    [Range(.1f, 5f)] public float orbitSensitivity = .3f;

    /// <summary>
    /// Zoom speed in meters per scroll step (orbit / auto-orbit).
    /// </summary>
    [Tooltip("Zoom speed in meters per scroll step.")]
    [Range(.1f, 5f)] public float zoomSpeed = .3f;

    /// <summary>
    /// Closest orbit distance to the vehicle in meters.
    /// </summary>
    [Tooltip("Closest orbit distance to the vehicle in meters.")]
    [Min(.5f)] public float minDistance = 2.5f;

    /// <summary>
    /// Farthest orbit distance to the vehicle in meters.
    /// </summary>
    [Tooltip("Farthest orbit distance to the vehicle in meters.")]
    [Min(1f)] public float maxDistance = 20f;

    /// <summary>
    /// Auto-orbit rotation speed in degrees per second (turntable mode).
    /// </summary>
    [Tooltip("Auto-orbit rotation speed in degrees per second (turntable mode).")]
    [Range(2f, 90f)] public float autoOrbitSpeed = 20f;

    [Header("Free Cam")]
    /// <summary>
    /// Free-cam move speed in meters per second.
    /// </summary>
    [Tooltip("Free-cam move speed in meters per second (WASD / QE).")]
    [Min(1f)] public float freeCamMoveSpeed = 3.6f;

    /// <summary>
    /// Free-cam boost multiplier while Left Shift is held.
    /// </summary>
    [Tooltip("Free-cam speed multiplier while Left Shift is held.")]
    [Range(1f, 8f)] public float freeCamBoostMultiplier = 3f;

    /// <summary>
    /// Free-cam mouse-look sensitivity (while the right mouse button is held).
    /// </summary>
    [Tooltip("Free-cam mouse-look sensitivity (hold right mouse button to look).")]
    [Range(.02f, 1f)] public float freeCamLookSensitivity = .036f;

    /// <summary>
    /// Maximum distance (meters) the free cam may move from the vehicle. Keeps it from flying off to infinity.
    /// </summary>
    [Tooltip("Maximum distance (m) the free cam can move away from the vehicle.")]
    [Min(5f)] public float freeCamMaxRadius = 40f;

    [Header("Mobile / Touch")]
    /// <summary>
    /// Left on-screen joystick value (-1..1). Orbit: rotates. Free Cam: moves. Set by RCCP_UI_PhotoMode.
    /// </summary>
    [Tooltip("Left on-screen joystick value. Orbit: rotate. Free Cam: move. Fed by RCCP_UI_PhotoMode.")]
    public Vector2 touchLeftStick = Vector2.zero;

    /// <summary>
    /// Right on-screen joystick value (-1..1). Orbit: zoom (Y). Free Cam: look. Set by RCCP_UI_PhotoMode.
    /// </summary>
    [Tooltip("Right on-screen joystick value. Orbit: zoom (Y). Free Cam: look. Fed by RCCP_UI_PhotoMode.")]
    public Vector2 touchRightStick = Vector2.zero;

    /// <summary>
    /// Free-cam vertical (world up/down) elevation input (-1..1), from the on-screen Up/Down buttons.
    /// Set by RCCP_UI_PhotoMode.
    /// </summary>
    [Tooltip("Free-cam vertical elevation input (world up/down), from the on-screen Up/Down buttons.")]
    [Range(-1f, 1f)] public float touchElevation = 0f;

    /// <summary>
    /// Touch orbit speed in degrees per second at full stick deflection.
    /// </summary>
    [Tooltip("Touch orbit speed in degrees per second at full stick deflection.")]
    [Range(20f, 360f)] public float touchOrbitSpeed = 36f;

    /// <summary>
    /// Touch zoom speed in meters per second at full stick deflection.
    /// </summary>
    [Tooltip("Touch zoom speed in meters per second at full stick deflection.")]
    [Range(2f, 40f)] public float touchZoomSpeed = 3.6f;

    /// <summary>
    /// Touch free-cam look speed in degrees per second at full stick deflection.
    /// </summary>
    [Tooltip("Touch free-cam look speed in degrees per second at full stick deflection.")]
    [Range(20f, 360f)] public float touchLookSpeed = 36f;

    [Header("Capture")]
    /// <summary>
    /// Screenshot resolution multiplier passed to ScreenCapture.CaptureScreenshot.
    /// </summary>
    [Tooltip("Screenshot resolution multiplier. 2 on a 1080p screen captures 4K.")]
    [Range(1, 4)] public int superSize = 2;

    private Transform target;
    private RCCP_Camera sourceCamera;
    private GameObject photoCameraGO;
    private Camera photoCamComponent;
    private float previousTimeScale = 1f;
    private bool previousAudioPause = false;

    //  Orbit / auto-orbit state.
    private float orbitYaw, orbitPitch;
    private float distance = 6f;

    //  Free-cam state.
    private float freeYaw, freePitch;

    /// <summary>
    /// Human-readable name of the active camera mode (for UI labels).
    /// </summary>
    public string CurrentModeName {
        get {
            switch (cameraMode) {
                case PhotoCameraMode.FreeCam: return "Free Cam";
                case PhotoCameraMode.AutoOrbit: return "Auto-Orbit";
                default: return "Orbit";
            }
        }
    }

    /// <summary>
    /// Enters photo mode: freezes time, hides the RCCP camera, and spawns the photo camera.
    /// </summary>
    public void EnterPhotoMode() {

        if (InPhotoMode)
            return;

        RCCP_SceneManager sm = RCCP_SceneManager.Instance;
        target = sm.activePlayerVehicle ? sm.activePlayerVehicle.transform : null;

        if (!target) {

            Debug.LogWarning("RCCP_PhotoMode.EnterPhotoMode skipped: no active player vehicle.");
            return;

        }

        sourceCamera = sm.activePlayerCamera;
        Camera sourceCam = sourceCamera && sourceCamera.actualCamera ? sourceCamera.actualCamera : Camera.main;

        if (!sourceCam) {

            Debug.LogWarning("RCCP_PhotoMode.EnterPhotoMode skipped: no camera found.");
            return;

        }

        previousTimeScale = Time.timeScale;
        previousAudioPause = AudioListener.pause;
        Time.timeScale = 0f;
        AudioListener.pause = true;

        photoCameraGO = new GameObject("RCCP_PhotoCamera");
        photoCamComponent = photoCameraGO.AddComponent<Camera>();
        photoCamComponent.CopyFrom(sourceCam);
        photoCameraGO.AddComponent<AudioListener>();
        photoCameraGO.transform.SetPositionAndRotation(sourceCam.transform.position, sourceCam.transform.rotation);

        //  Seed the field of view from the source camera and clamp to the configured range.
        fieldOfView = Mathf.Clamp(sourceCam.fieldOfView, minFOV, maxFOV);

        if (sourceCamera)
            sourceCamera.gameObject.SetActive(false);

        //  Seed both orbit and free-cam state from the current view so entering / switching doesn't snap.
        SeedOrbitFromCurrent();
        SeedFreeCamFromCurrent();

        touchLeftStick = Vector2.zero;
        touchRightStick = Vector2.zero;
        touchElevation = 0f;

        InPhotoMode = true;
        IsActive = true;

    }

    /// <summary>
    /// Exits photo mode and restores timescale, audio, and the RCCP camera.
    /// </summary>
    public void ExitPhotoMode() {

        if (!InPhotoMode)
            return;

        Time.timeScale = previousTimeScale;
        AudioListener.pause = previousAudioPause;

        if (photoCameraGO)
            Destroy(photoCameraGO);

        photoCamComponent = null;

        if (sourceCamera)
            sourceCamera.gameObject.SetActive(true);

        InPhotoMode = false;
        IsActive = false;

    }

    /// <summary>
    /// Switches the photo camera mode, re-seeding the target mode's state from the current view so it doesn't snap.
    /// </summary>
    public void SetCameraMode(PhotoCameraMode mode) {

        if (photoCameraGO != null) {

            if (mode == PhotoCameraMode.FreeCam)
                SeedFreeCamFromCurrent();
            else
                SeedOrbitFromCurrent();

        }

        cameraMode = mode;

    }

    /// <summary>
    /// Cycles to the next photo camera mode (Orbit -> Free Cam -> Auto-Orbit -> Orbit).
    /// </summary>
    public void CyclePhotoCameraMode() {

        SetCameraMode((PhotoCameraMode)(((int)cameraMode + 1) % 3));

    }

    /// <summary>
    /// Sets the photo camera field of view (degrees), clamped to [minFOV, maxFOV].
    /// </summary>
    public void SetFieldOfView(float fov) {

        fieldOfView = Mathf.Clamp(fov, minFOV, maxFOV);

        if (photoCamComponent != null)
            photoCamComponent.fieldOfView = fieldOfView;

    }

    /// <summary>
    /// Captures a super-size screenshot to persistentDataPath/Photos and returns the file path.
    /// Note: captures the whole screen including any visible UI — hide UI before calling
    /// (RCCP_UI_PhotoMode does this automatically).
    /// </summary>
    public string CapturePhoto() {

        string directory = Path.Combine(Application.persistentDataPath, "Photos");

        if (!Directory.Exists(directory))
            Directory.CreateDirectory(directory);

        string file = Path.Combine(directory, "RCCP_Photo_" + DateTime.Now.ToString("yyyy-MM-dd_HH-mm-ss") + ".png");
        ScreenCapture.CaptureScreenshot(file, superSize);
        Debug.Log("RCCP_PhotoMode: capturing screenshot to " + file);
        return file;

    }

    private void Update() {

        if (!InPhotoMode || !photoCameraGO || !target) {

            //  Target vehicle destroyed mid-photo-mode → bail out cleanly.
            if (InPhotoMode)
                ExitPhotoMode();

            return;

        }

        //  Keep the field of view applied (cheap; lets a slider drive it live).
        if (photoCamComponent != null)
            photoCamComponent.fieldOfView = fieldOfView;

        switch (cameraMode) {

            case PhotoCameraMode.FreeCam:
                UpdateFreeCam();
                break;
            case PhotoCameraMode.AutoOrbit:
                UpdateOrbit(true);
                break;
            default:
                UpdateOrbit(false);
                break;

        }

    }

    /// <summary>
    /// Orbit / auto-orbit camera update. Pointer drag or the left touch stick rotates, scroll or the right
    /// touch stick (Y) zooms; auto adds a constant yaw spin.
    /// </summary>
    private void UpdateOrbit(bool auto) {

        //  Orbit with the same pointer-delta action RCCP_Camera uses.
        Vector2 pointerDelta = RCCP_InputManager.Instance.GetInputs().mouseInput;
        orbitYaw += pointerDelta.x * orbitSensitivity;
        orbitPitch -= pointerDelta.y * orbitSensitivity;

        //  Touch orbit (sustained stick value → rate).
        orbitYaw += touchLeftStick.x * touchOrbitSpeed * Time.unscaledDeltaTime;
        orbitPitch -= touchLeftStick.y * touchOrbitSpeed * Time.unscaledDeltaTime;

        if (auto)
            orbitYaw += autoOrbitSpeed * Time.unscaledDeltaTime;

        orbitPitch = Mathf.Clamp(orbitPitch, -10f, 80f);

        //  Zoom: mouse scroll (discrete) + right touch stick Y (continuous).
        float zoomDelta = 0f;

        if (Mouse.current != null) {

            float scroll = Mouse.current.scroll.ReadValue().y;

            if (Mathf.Abs(scroll) > Mathf.Epsilon)
                zoomDelta -= Mathf.Sign(scroll) * zoomSpeed;

        }

        zoomDelta -= touchRightStick.y * touchZoomSpeed * Time.unscaledDeltaTime;
        distance = Mathf.Clamp(distance + zoomDelta, minDistance, maxDistance);

        Vector3 focus = target.position + Vector3.up * 1f;
        Quaternion orbitRotation = Quaternion.Euler(orbitPitch, orbitYaw, 0f);
        Vector3 position = focus + orbitRotation * (Vector3.back * distance);

        photoCameraGO.transform.SetPositionAndRotation(position, Quaternion.LookRotation(focus - position, Vector3.up));

    }

    /// <summary>
    /// Free-fly camera update. Hold RMB (or the right touch stick) to look; WASD / the left touch stick moves,
    /// Q/E descend/ascend, Left Shift boosts. Runs on unscaled time because photo mode freezes Time.timeScale.
    /// </summary>
    private void UpdateFreeCam() {

        Keyboard kb = Keyboard.current;
        Mouse mouse = Mouse.current;

        //  Look while holding the right mouse button (keeps the cursor free for the overlay buttons).
        if (mouse != null && mouse.rightButton.isPressed) {

            Vector2 look = mouse.delta.ReadValue();
            freeYaw += look.x * freeCamLookSensitivity;
            freePitch -= look.y * freeCamLookSensitivity;

        }

        //  Touch look (right stick).
        freeYaw += touchRightStick.x * touchLookSpeed * Time.unscaledDeltaTime;
        freePitch -= touchRightStick.y * touchLookSpeed * Time.unscaledDeltaTime;

        freePitch = Mathf.Clamp(freePitch, -89f, 89f);

        Quaternion rotation = Quaternion.Euler(freePitch, freeYaw, 0f);

        Vector3 move = Vector3.zero;

        if (kb != null) {

            if (kb.wKey.isPressed) move += Vector3.forward;
            if (kb.sKey.isPressed) move += Vector3.back;
            if (kb.aKey.isPressed) move += Vector3.left;
            if (kb.dKey.isPressed) move += Vector3.right;
            if (kb.eKey.isPressed) move += Vector3.up;
            if (kb.qKey.isPressed) move += Vector3.down;

        }

        //  Touch move (left stick): X strafe, Y forward.
        move += Vector3.right * touchLeftStick.x + Vector3.forward * touchLeftStick.y;
        move = Vector3.ClampMagnitude(move, 1f);

        float speed = freeCamMoveSpeed * ((kb != null && kb.leftShiftKey.isPressed) ? freeCamBoostMultiplier : 1f);
        Vector3 worldMove = rotation * move * speed * Time.unscaledDeltaTime;

        //  World-space elevation from the on-screen Up/Down buttons (mobile) — straight up/down regardless of look.
        worldMove += Vector3.up * touchElevation * speed * Time.unscaledDeltaTime;

        photoCameraGO.transform.position += worldMove;

        //  Keep the free cam within a max radius of the vehicle so it can't fly off to infinity.
        Vector3 fromTarget = photoCameraGO.transform.position - target.position;

        if (fromTarget.magnitude > freeCamMaxRadius)
            photoCameraGO.transform.position = target.position + fromTarget.normalized * freeCamMaxRadius;

        photoCameraGO.transform.rotation = rotation;

    }

    /// <summary>
    /// Recomputes orbit yaw / pitch / distance from the photo camera's current position around the target.
    /// </summary>
    private void SeedOrbitFromCurrent() {

        if (photoCameraGO == null || target == null)
            return;

        Vector3 focus = target.position + Vector3.up * 1f;
        Vector3 offset = photoCameraGO.transform.position - focus;
        distance = Mathf.Clamp(offset.magnitude, minDistance, maxDistance);
        orbitYaw = Mathf.Atan2(offset.x, offset.z) * Mathf.Rad2Deg;
        orbitPitch = Mathf.Asin(Mathf.Clamp(offset.y / Mathf.Max(offset.magnitude, .01f), -1f, 1f)) * Mathf.Rad2Deg;

    }

    /// <summary>
    /// Recomputes free-cam yaw / pitch from the photo camera's current rotation.
    /// </summary>
    private void SeedFreeCamFromCurrent() {

        if (photoCameraGO == null)
            return;

        Vector3 e = photoCameraGO.transform.eulerAngles;
        freeYaw = e.y;
        float pitch = e.x;
        if (pitch > 180f) pitch -= 360f;
        freePitch = Mathf.Clamp(pitch, -89f, 89f);

    }

}
