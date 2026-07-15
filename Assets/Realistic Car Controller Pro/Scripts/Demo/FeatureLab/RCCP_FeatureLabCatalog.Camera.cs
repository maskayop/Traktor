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

    static partial void BuildCamera(List<RCCP_FeatureLabEntry> entries) {

        //  Hood/Wheel require RCCP_HoodCamera / RCCP_WheelCamera mounts on the vehicle; without them ChangeCamera warns and stays in the current mode (all RCCP demo vehicles have both).
        entries.Add(E("cam-mode", RCCP_FeatureLabCategory.Camera, "Camera View",
            "Which view the camera uses to follow the car. Hood and Wheel need camera mounts on the vehicle; towing a trailer switches to a special trailer view automatically.",
            new string[] { "Chase (TPS)", "Hood", "Wheel", "Fixed", "Cinematic", "Top" },
            c => (c.Cam.cameraMode == RCCP_Camera.CameraMode.TRUCKTRAILER ? 0 : (int)c.Cam.cameraMode),
            (c, x) => { c.Cam.ChangeCamera((RCCP_Camera.CameraMode)x); },
            avail: c => (c.Cam != null), availReason: "No RCCP camera in the scene (RCCP_SceneManager.activePlayerCamera is null)."));

        //  TPSAutoFocus forced false in setExpr — otherwise the 2 s AutoFocus smoothstep coroutine rewrites TPSDistance/TPSHeight on every SetTarget()/trailer change and the slider drifts back.
        entries.Add(S("cam-tpsdistance", RCCP_FeatureLabCategory.Camera, "Chase Distance",
            "How far the chase camera sits behind the car, in meters. Lower = closer and more intense, higher = wider view. Turns off auto-fit so your value sticks.",
            0f, 20f,
            c => (c.Cam.TPSDistance),
            (c, x) => { c.Cam.TPSAutoFocus = false; c.Cam.TPSDistance = x; }, unit: "m",
            avail: c => (c.Cam != null), availReason: "No RCCP camera in the scene (RCCP_SceneManager.activePlayerCamera is null)."));

        //  TPSAutoFocus forced false in setExpr — same AutoFocus-clobber hazard as Chase Distance.
        entries.Add(S("cam-tpsheight", RCCP_FeatureLabCategory.Camera, "Chase Height",
            "How high the chase camera sits above the car, in meters. Higher gives a more top-down feel. Turns off auto-fit so your value sticks.",
            0f, 10f,
            c => (c.Cam.TPSHeight),
            (c, x) => { c.Cam.TPSAutoFocus = false; c.Cam.TPSHeight = x; }, unit: "m",
            avail: c => (c.Cam != null), availReason: "No RCCP camera in the scene (RCCP_SceneManager.activePlayerCamera is null)."));

        entries.Add(T("cam-tpsautofocus", RCCP_FeatureLabCategory.Camera, "Auto-Fit Chase Camera",
            "Automatically sizes the chase camera's distance and height to fit the car. Turn off to keep your own distance and height settings.",
            c => (c.Cam.TPSAutoFocus),
            (c, x) => { c.Cam.TPSAutoFocus = x; },
            avail: c => (c.Cam != null), availReason: "No RCCP camera in the scene (RCCP_SceneManager.activePlayerCamera is null)."));

        entries.Add(S("cam-tpsfovmin", RCCP_FeatureLabCategory.Camera, "Chase Zoom (Standing)",
            "Chase camera field of view when the car is standing still. Lower = more zoomed in. The view widens toward the top-speed value as you accelerate.",
            10f, 90f,
            c => (c.Cam.TPSMinimumFOV),
            (c, x) => { c.Cam.TPSMinimumFOV = x; }, unit: "deg",
            avail: c => (c.Cam != null), availReason: "No RCCP camera in the scene (RCCP_SceneManager.activePlayerCamera is null)."));

        entries.Add(S("cam-tpsfovmax", RCCP_FeatureLabCategory.Camera, "Chase Zoom (Top Speed)",
            "Chase camera field of view at very high speed. Higher = wider, faster-feeling view. The camera blends from the standing value to this as speed rises.",
            10f, 160f,
            c => (c.Cam.TPSMaximumFOV),
            (c, x) => { c.Cam.TPSMaximumFOV = x; }, unit: "deg",
            avail: c => (c.Cam != null), availReason: "No RCCP camera in the scene (RCCP_SceneManager.activePlayerCamera is null)."));

        //  c.Cam.ResetCamera() (public, parameterless, idempotent for the current mode) — hoodCameraFOV is read ONLY in ResetCamera()'s FPS case on mode entry; without the call the change waits for the next mode switch.
        //  Effect only visible in Hood view; vehicle needs an RCCP_HoodCamera mount to enter it.
        entries.Add(S("cam-hoodfov", RCCP_FeatureLabCategory.Camera, "Hood View Zoom",
            "Field of view of the hood (driver) camera. Lower = more zoomed in, higher = wider. Applies immediately, even while already in the hood view.",
            10f, 160f,
            c => (c.Cam.hoodCameraFOV),
            (c, x) => { c.Cam.hoodCameraFOV = x; c.Cam.ResetCamera(); }, unit: "deg",
            avail: c => (c.Cam != null), availReason: "No RCCP camera in the scene (RCCP_SceneManager.activePlayerCamera is null)."));

        //  c.Cam.ResetCamera() — wheelCameraFOV is read ONLY in ResetCamera()'s WHEEL case on mode entry.
        //  Effect only visible in Wheel view; vehicle needs an RCCP_WheelCamera mount to enter it.
        entries.Add(S("cam-wheelfov", RCCP_FeatureLabCategory.Camera, "Wheel View Zoom",
            "Field of view of the wheel close-up camera. Lower = more zoomed in, higher = wider. Applies immediately, even while already in the wheel view.",
            10f, 160f,
            c => (c.Cam.wheelCameraFOV),
            (c, x) => { c.Cam.wheelCameraFOV = x; c.Cam.ResetCamera(); }, unit: "deg",
            avail: c => (c.Cam != null), availReason: "No RCCP camera in the scene (RCCP_SceneManager.activePlayerCamera is null)."));

        entries.Add(T("cam-tpsshake", RCCP_FeatureLabCategory.Camera, "Speed Shake",
            "Adds a subtle handheld shake to the chase camera that grows with speed, for a stronger sense of velocity. Off = perfectly steady camera.",
            c => (c.Cam.TPSShake),
            (c, x) => { c.Cam.TPSShake = x; },
            avail: c => (c.Cam != null), availReason: "No RCCP camera in the scene (RCCP_SceneManager.activePlayerCamera is null)."));

        entries.Add(A("cam-testcollisionshake", RCCP_FeatureLabCategory.Camera, "Test Collision Shake",
            "Gives the camera a quick crash-style jolt so you can preview impact feedback without hitting anything. Only visible in the chase (or trailer) view.",
            "Test Collision Shake",
            c => { c.Cam.TriggerCollisionShake(0.5f); },
            status: c => "" + ((c.Cam.cameraMode == RCCP_Camera.CameraMode.TPS || c.Cam.cameraMode == RCCP_Camera.CameraMode.TRUCKTRAILER) ? "Ready" : "Switch to Chase view to see it"),
            avail: c => (c.Cam != null), availReason: "No RCCP camera in the scene (RCCP_SceneManager.activePlayerCamera is null)."));

        entries.Add(T("cam-autochange", RCCP_FeatureLabCategory.Camera, "Auto-Cycle Views",
            "Automatically cycles through the camera views every few seconds, like a replay director. Set the pace with the interval slider.",
            c => (c.Cam.useAutoChangeCamera),
            (c, x) => { c.Cam.useAutoChangeCamera = x; },
            avail: c => (c.Cam != null), availReason: "No RCCP camera in the scene (RCCP_SceneManager.activePlayerCamera is null)."));

        entries.Add(S("cam-autochangeinterval", RCCP_FeatureLabCategory.Camera, "Auto-Cycle Interval",
            "Seconds between automatic view switches when auto-cycle is on. Lower = faster cuts between cameras.",
            1f, 30f,
            c => (c.Cam.autoChangeCameraInterval),
            (c, x) => { c.Cam.autoChangeCameraInterval = x; }, unit: "s",
            avail: c => (c.Cam != null), availReason: "No RCCP camera in the scene (RCCP_SceneManager.activePlayerCamera is null)."));

        entries.Add(T("cam-muffle", RCCP_FeatureLabCategory.Camera, "Interior Audio Muffle",
            "Muffles outside sounds while in the hood or wheel view, as if you're sitting inside the car. Off = full outside audio in every view.",
            c => (c.Cam.useInteriorAudioMuffle),
            (c, x) => { c.Cam.useInteriorAudioMuffle = x; },
            avail: c => (c.Cam != null), availReason: "No RCCP camera in the scene (RCCP_SceneManager.activePlayerCamera is null)."));

        entries.Add(S("cam-mufflecutoff", RCCP_FeatureLabCategory.Camera, "Muffle Strength (Cutoff)",
            "How muffled interior audio sounds, as a frequency cutoff. Lower = heavier, more closed-in muffling; higher = brighter, closer to normal.",
            500f, 22000f,
            c => (c.Cam.interiorLowPassCutoff),
            (c, x) => { c.Cam.interiorLowPassCutoff = x; }, unit: "Hz",
            avail: c => (c.Cam != null), availReason: "No RCCP camera in the scene (RCCP_SceneManager.activePlayerCamera is null)."));

        entries.Add(T("cam-muffleinhood", RCCP_FeatureLabCategory.Camera, "Muffle in Hood View",
            "Applies the interior muffle while in the hood (driver) view. Turn off if you want full outside audio from behind the windshield.",
            c => (c.Cam.muffleInFPSCamera),
            (c, x) => { c.Cam.muffleInFPSCamera = x; },
            avail: c => (c.Cam != null), availReason: "No RCCP camera in the scene (RCCP_SceneManager.activePlayerCamera is null)."));

        entries.Add(T("cam-muffleinwheel", RCCP_FeatureLabCategory.Camera, "Muffle in Wheel View",
            "Applies the interior muffle while in the wheel close-up view.",
            c => (c.Cam.muffleInWheelCamera),
            (c, x) => { c.Cam.muffleInWheelCamera = x; },
            avail: c => (c.Cam != null), availReason: "No RCCP camera in the scene (RCCP_SceneManager.activePlayerCamera is null)."));

        entries.Add(T("cam-occlusion", RCCP_FeatureLabCategory.Camera, "Camera Obstacle Avoidance",
            "Keeps the car visible when something blocks the camera by moving the camera in front of the obstacle. Off = the camera can end up hidden behind walls.",
            c => (c.Cam.useOcclusion),
            (c, x) => { c.Cam.useOcclusion = x; },
            avail: c => (c.Cam != null), availReason: "No RCCP camera in the scene (RCCP_SceneManager.activePlayerCamera is null)."));

    }

}
