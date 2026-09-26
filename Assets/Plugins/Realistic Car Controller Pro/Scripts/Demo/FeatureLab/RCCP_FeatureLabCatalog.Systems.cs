//----------------------------------------------
//        Realistic Car Controller Pro
//
// Copyright © 2014 - 2026 BoneCracker Games
// https://www.bonecrackergames.com
// Ekrem Bugra Ozdoganlar
//
//----------------------------------------------

using System;
using System.Collections.Generic;
using UnityEngine;

public static partial class RCCP_FeatureLabCatalog {

    //  Scene-authored anchors for Respawn Vehicle / Teleport To Trailer live on RCCP_FeatureLab
    //  (RCCP_FeatureLab.Instance.spawnAnchor / trailerAnchor) — RCCP has no spawn-point or
    //  trailer-point registry (RCCP.Transport(vehicle, pos, rot), RCCP.cs:474, needs a target
    //  Transform from somewhere). Null by default — assign from a scene bootstrap component (or the
    //  vehicle selector, for spawnAnchor); both actions report "unavailable" (not a crash) until
    //  then, same pattern as any other missing-addon gate.
    static partial void BuildSystems(List<RCCP_FeatureLabEntry> entries) {

        //  Any vehicle; requires an active player vehicle registered with RCCP_SceneManager.
        entries.Add(A("photo-enter", RCCP_FeatureLabCategory.Systems, "Enter Photo Mode",
            "Pause the action and fly a free camera around your car to line up the perfect shot. The regular driving UI hides while photo mode is on.",
            "Enter Photo Mode",
            c => { RCCP.EnterPhotoMode(); },
            status: c => "" + (RCCP_PhotoMode.IsActive ? "Active" : "Off"),
            avail: c => (!RCCP_PhotoMode.IsActive && c.V != null), availReason: "Photo mode is a no-op while already active, and EnterPhotoMode warn-aborts when no active player vehicle exists."));

        entries.Add(A("photo-capture", RCCP_FeatureLabCategory.Systems, "Capture Photo",
            "Save a high-resolution screenshot of the current photo-mode view. Photos are written to the game's data folder.",
            "Capture Photo",
            c => { RCCP.CapturePhoto(); },
            avail: c => (RCCP_PhotoMode.IsActive), availReason: "Capture only makes sense from the photo camera; outside photo mode it would shoot the normal gameplay view with HUD."));

        entries.Add(A("photo-exit", RCCP_FeatureLabCategory.Systems, "Exit Photo Mode",
            "Leave photo mode and resume driving. Game speed and audio return to exactly how they were before.",
            "Exit Photo Mode",
            c => { RCCP.ExitPhotoMode(); },
            avail: c => (RCCP_PhotoMode.IsActive), availReason: "Nothing to exit when photo mode is not active."));

        //  Vehicle needs an RCCP_Recorder under its OtherAddons node (most RCCP demo vehicles ship with one).
        entries.Add(A("recorder-record", RCCP_FeatureLabCategory.Systems, "Record Drive",
            "Start recording your driving; press again to stop and save the clip. Saved clips can be replayed as a ghost of your run.",
            "Record Drive",
            c => { RCCP.StartStopRecord(c.V); },
            status: c => "" + ((c.V.OtherAddonsManager != null && c.V.OtherAddonsManager.Recorder != null && c.V.OtherAddonsManager.Recorder.mode == RCCP_Recorder.RecorderMode.Record) ? "Recording" : "Idle"),
            avail: c => (c.V != null && c.V.OtherAddonsManager != null && c.V.OtherAddonsManager.Recorder != null), availReason: "Requires the Recorder addon under OtherAddons; the facade warn-and-noops without it, so hide the button instead."));

        //  Same Recorder addon requirement as Record Drive; replay something recorded this session.
        entries.Add(A("recorder-replay", RCCP_FeatureLabCategory.Systems, "Replay Last Clip",
            "Play back the last recorded clip; press again to stop. The car drives itself from your recorded inputs and control returns when the replay ends.",
            "Replay Last Clip",
            c => { RCCP.StartStopReplay(c.V); },
            status: c => "" + ((c.V.OtherAddonsManager != null && c.V.OtherAddonsManager.Recorder != null && c.V.OtherAddonsManager.Recorder.mode == RCCP_Recorder.RecorderMode.Play) ? "Replaying" : "Idle"),
            avail: c => (c.V != null && c.V.OtherAddonsManager != null && c.V.OtherAddonsManager.Recorder != null), availReason: "Requires the Recorder addon under OtherAddons (facade warn-and-noops without it)."));

        entries.Add(A("recorder-stop", RCCP_FeatureLabCategory.Systems, "Stop Record / Replay",
            "Stop any recording or replay that is currently running and hand control back to you.",
            "Stop Record / Replay",
            c => { RCCP.StopRecordReplay(c.V); },
            avail: c => (c.V != null && c.V.OtherAddonsManager != null && c.V.OtherAddonsManager.Recorder != null && c.V.OtherAddonsManager.Recorder.mode != RCCP_Recorder.RecorderMode.Neutral), availReason: "Only useful while the recorder is actually in Record or Play; also requires the Recorder addon."));

        entries.Add(R("recorder-mode", RCCP_FeatureLabCategory.Systems, "Recorder State",
            "Shows what the recorder is doing right now: Neutral (idle), Record, or Play.",
            c => "" + (c.V.OtherAddonsManager.Recorder.mode.ToString()),
            avail: c => (c.V != null && c.V.OtherAddonsManager != null && c.V.OtherAddonsManager.Recorder != null), availReason: "The lamp reads RCCP_Recorder.mode, which only exists when the Recorder addon is present on the vehicle."));

        entries.Add(T("telemetry-toggle", RCCP_FeatureLabCategory.Systems, "Telemetry Panel",
            "Show or hide the on-screen telemetry panel with live wheel, engine, and input numbers. Handy for seeing how your tweaks change the physics.",
            c => (UnityEngine.Object.FindAnyObjectByType<RCCP_Telemetry>(FindObjectsInactive.Include) is RCCP_Telemetry tI && tI.gameObject.activeInHierarchy),
            (c, x) => { RCCP_Telemetry t = UnityEngine.Object.FindAnyObjectByType<RCCP_Telemetry>(FindObjectsInactive.Include); if (t == null && x && c.S.RCCPTelemetry != null) t = UnityEngine.Object.Instantiate(c.S.RCCPTelemetry, Vector3.zero, Quaternion.identity).GetComponentInChildren<RCCP_Telemetry>(true); if (t != null) t.gameObject.SetActive(x); },
            avail: c => (c.S.RCCPTelemetry != null || UnityEngine.Object.FindAnyObjectByType<RCCP_Telemetry>(FindObjectsInactive.Include) != null), availReason: "Needs either the telemetry prefab reference on the settings clone (to spawn one) or an already-spawned scene instance (when RCCP_Settings.useTelemetry was on at SceneManager Awake)."));

        //  Use a truck + trailer demo pair; the entry stays hidden until a trailer is actually coupled.
        entries.Add(A("trailer-detach", RCCP_FeatureLabCategory.Systems, "Detach Trailer",
            "Unhook the trailer currently attached to your vehicle. To hook it back up, just reverse slowly into the trailer's coupling.",
            "Detach Trailer",
            c => { if (c.V.ConnectedTrailer != null) c.V.ConnectedTrailer.DetachTrailer(); },
            status: c => "" + (c.V.ConnectedTrailer != null ? "Attached" : "None"),
            avail: c => (c.V != null && c.V.ConnectedTrailer != null), availReason: "ConnectedTrailer is null unless a trailer is trigger-attached right now; there is no attach API, so the button only appears while towing."));

        //  Point the anchor at the demo trailer in the Feature Lab scene.
        entries.Add(A("teleport-trailer", RCCP_FeatureLabCategory.Systems, "Teleport To Trailer",
            "Instantly move your vehicle to the trailer parking spot so you can try towing. The car arrives stopped, lined up with the marker.",
            "Teleport To Trailer",
            c => { Transform a = RCCP_FeatureLab.Instance != null ? RCCP_FeatureLab.Instance.trailerAnchor : null; if (a == null) return; RCCP.Transport(c.V, a.position, a.rotation); },
            avail: c => (RCCP_FeatureLab.Instance != null && RCCP_FeatureLab.Instance.trailerAnchor != null && c.V != null), availReason: "Requires the scene-authored trailerAnchor Transform serialized on the Feature Lab manager; hidden when the scene has no trailer area."));

        entries.Add(A("respawn", RCCP_FeatureLabCategory.Systems, "Respawn Vehicle",
            "Teleport your vehicle back to the starting point and stop it there. Useful if you get stuck or flip over.",
            "Respawn Vehicle",
            c => { Transform a = RCCP_FeatureLab.Instance != null ? RCCP_FeatureLab.Instance.spawnAnchor : null; if (a == null) return; RCCP.Transport(c.V, a.position, a.rotation); },
            avail: c => (RCCP_FeatureLab.Instance != null && RCCP_FeatureLab.Instance.spawnAnchor != null && c.V != null), availReason: "Requires the scene-authored spawnAnchor Transform serialized on the Feature Lab manager."));

        entries.Add(S("time-scale", RCCP_FeatureLabCategory.Systems, "Slow Motion",
            "Slow the whole game down for a closer look at the physics. 1 is normal speed; lower values are slow motion.",
            0.05f, 1f,
            c => (Time.timeScale),
            (c, x) => { if (!RCCP_PhotoMode.IsActive) Time.timeScale = Mathf.Clamp(x, 0.05f, 1f); }, unit: "x",
            avail: c => (!RCCP_PhotoMode.IsActive), availReason: "Photo mode owns Time.timeScale while active (sets 0, restores its cached value on exit) — the slider must not fight it."));

        entries.Add(T("mobile-enabled", RCCP_FeatureLabCategory.Systems, "Touch Controls",
            "Switch between keyboard/gamepad input and on-screen touch controls. The touch UI appears as soon as this is on.",
            c => (c.S.mobileControllerEnabled),
            (c, x) => { c.S.mobileControllerEnabled = x; }));

        entries.Add(E("mobile-scheme", RCCP_FeatureLabCategory.Systems, "Touch Control Style",
            "Choose how the touch controls steer the car: buttons, phone tilt, an on-screen wheel, or a joystick.",
            new string[] { "Touch Screen", "Gyro / Tilt", "Steering Wheel", "Joystick" },
            c => ((int)c.S.mobileController),
            (c, x) => { RCCP.SetMobileController((RCCP_Settings.MobileController)x); },
            avail: c => (c.S.mobileControllerEnabled), availReason: "The scheme only has a visible effect while touch controls are enabled; hiding it avoids a dead dropdown."));

        entries.Add(T("engine-running", RCCP_FeatureLabCategory.Systems, "Engine",
            "Start or stop the engine. With the engine off the car ignores the throttle but still steers and brakes.",
            c => (c.V.engineRunning),
            (c, x) => { RCCP.SetEngine(c.V, x); },
            avail: c => (c.V != null && c.V.Engine != null), availReason: "SetEngine routes to Engine.engineRunning; without an RCCP_Engine the facade warn-and-noops, so hide the toggle."));

        //  RCCP_Events.Event_OnBehaviorChanged() — included in setExpr; forces every enabled vehicle's CheckBehavior to re-apply the (fresh-clone) preset one WaitForFixedUpdate later
        entries.Add(A("factory-reset", RCCP_FeatureLabCategory.Systems, "Reset All Settings",
            "Reset every setting in this lab back to its shipped default and re-apply to all vehicles. Your car's position and damage stay as they are.",
            "Reset All Settings",
            c => { RCCP_RuntimeSettings.Clear(); RCCP_Events.Event_OnBehaviorChanged(); }));

    }

}
