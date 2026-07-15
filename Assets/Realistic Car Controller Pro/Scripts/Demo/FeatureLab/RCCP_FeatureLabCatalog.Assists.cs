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

    static partial void BuildAssists(List<RCCP_FeatureLabEntry> entries) {

        entries.Add(T("abs", RCCP_FeatureLabCategory.Assists, "ABS",
            "Anti-lock braking. Stops the wheels locking up under hard braking so you keep steering control. Off = wheels can lock and skid.",
            c => (c.V.Stability.ABS),
            (c, x) => { c.V.Stability.ABS = x; },
            avail: c => (c.V.Stability != null), availReason: "RCCP_Stability is an optional child component; RCCP_MainComponent.Stability lazily resolves via GetComponentInChildren and returns null when absent."));

        entries.Add(S("abs-intensity", RCCP_FeatureLabCategory.Assists, "ABS Strength",
            "How strongly the anti-lock system releases the brakes when a wheel starts to lock. 0 = barely helps, 1 = full assist.",
            0f, 1f,
            c => (c.V.Stability.ABSIntensity),
            (c, x) => { c.V.Stability.ABSIntensity = x; },
            avail: c => (c.V.Stability != null), availReason: "Requires the optional RCCP_Stability component."));

        entries.Add(T("esp", RCCP_FeatureLabCategory.Assists, "Stability Control (ESP)",
            "Electronic stability. Brakes individual wheels to stop the car spinning out or plowing wide in corners.",
            c => (c.V.Stability.ESP),
            (c, x) => { c.V.Stability.ESP = x; },
            avail: c => (c.V.Stability != null), availReason: "Requires the optional RCCP_Stability component."));

        entries.Add(S("esp-intensity", RCCP_FeatureLabCategory.Assists, "ESP Strength",
            "How hard stability control fights a slide. Low = subtle nudges, high = firm corrections. Also scales the engine power cut in Normal mode.",
            0f, 1f,
            c => (c.V.Stability.ESPIntensity),
            (c, x) => { c.V.Stability.ESPIntensity = x; },
            avail: c => (c.V.Stability != null), availReason: "Requires the optional RCCP_Stability component."));

        entries.Add(E("esp-mode", RCCP_FeatureLabCategory.Assists, "ESP Mode",
            "Normal cuts engine power during corrections. Sport keeps power on and only brakes wheels, letting the car slide a little more before help arrives.",
            new string[] { "Normal", "Sport" },
            c => ((int)c.V.Stability.espMode),
            (c, x) => { c.V.Stability.espMode = (RCCP_Stability.ESPMode)x; },
            avail: c => (c.V.Stability != null), availReason: "Requires the optional RCCP_Stability component."));

        entries.Add(S("esp-preserve-speed", RCCP_FeatureLabCategory.Assists, "ESP Speed Preservation",
            "How much speed the car keeps while stability control brakes. 0 = realistic (corrections slow you down), 1 = arcade (no speed loss).",
            0f, 1f,
            c => (c.V.Stability.preserveSpeedFactor),
            (c, x) => { c.V.Stability.preserveSpeedFactor = x; },
            avail: c => (c.V.Stability != null), availReason: "Requires the optional RCCP_Stability component."));

        entries.Add(T("tcs", RCCP_FeatureLabCategory.Assists, "Traction Control (TCS)",
            "Cuts power when the driven wheels spin up under throttle, keeping launches and corner exits clean.",
            c => (c.V.Stability.TCS),
            (c, x) => { c.V.Stability.TCS = x; },
            avail: c => (c.V.Stability != null), availReason: "Requires the optional RCCP_Stability component."));

        entries.Add(S("tcs-intensity", RCCP_FeatureLabCategory.Assists, "TCS Strength",
            "How aggressively traction control trims power when the wheels spin. Higher = cleaner grip, lower = more wheelspin allowed.",
            0f, 1f,
            c => (c.V.Stability.TCSIntensity),
            (c, x) => { c.V.Stability.TCSIntensity = x; },
            avail: c => (c.V.Stability != null), availReason: "Requires the optional RCCP_Stability component."));

        entries.Add(T("steering-helper", RCCP_FeatureLabCategory.Assists, "Steering Helper",
            "Gently steers the car back into line when the rear starts to slide — like an invisible co-driver on the wheel.",
            c => (c.V.Stability.steeringHelper),
            (c, x) => { c.V.Stability.steeringHelper = x; },
            avail: c => (c.V.Stability != null), availReason: "Requires the optional RCCP_Stability component."));

        entries.Add(S("steering-helper-strength", RCCP_FeatureLabCategory.Assists, "Steering Helper Strength",
            "How much the steering helper corrects your line. Higher = more stable but less lively feel.",
            0f, 1f,
            c => (c.V.Stability.steerHelperStrength),
            (c, x) => { c.V.Stability.steerHelperStrength = x; },
            avail: c => (c.V.Stability != null), availReason: "Requires the optional RCCP_Stability component."));

        entries.Add(T("traction-helper", RCCP_FeatureLabCategory.Assists, "Traction Helper",
            "Stiffens the front tires' side grip when the car leans hard, reducing wobble and fishtailing at speed.",
            c => (c.V.Stability.tractionHelper),
            (c, x) => { c.V.Stability.tractionHelper = x; },
            avail: c => (c.V.Stability != null), availReason: "Requires the optional RCCP_Stability component."));

        entries.Add(S("traction-helper-strength", RCCP_FeatureLabCategory.Assists, "Traction Helper Strength",
            "How much extra front-tire side grip the helper adds under load. Higher = calmer and more planted at speed.",
            0f, 1f,
            c => (c.V.Stability.tractionHelperStrength),
            (c, x) => { c.V.Stability.tractionHelperStrength = x; },
            avail: c => (c.V.Stability != null), availReason: "Requires the optional RCCP_Stability component."));

        //  OFF has no native restore — RCCP leaves the last lerped angularDamping on the Rigidbody, so setExpr resets it explicitly.
        entries.Add(T("angular-drag-helper", RCCP_FeatureLabCategory.Assists, "Spin Damping Helper",
            "Damps fast rotation as speed rises so the car resists sudden spins. Turning it off restores normal rotation damping.",
            c => (c.V.Stability.angularDragHelper),
            (c, x) => { c.V.Stability.angularDragHelper = x; if (!x) { RCCP_Settings.BehaviorType bt = c.V.GetVehicleBehaviorType(); c.V.Rigid.angularDamping = bt != null ? bt.angularDrag : 0.1f; } },
            avail: c => (c.V.Stability != null), availReason: "Requires the optional RCCP_Stability component; Rigid is [RequireComponent]-backed and safe once c.V is non-null."));

        entries.Add(S("angular-drag-helper-strength", RCCP_FeatureLabCategory.Assists, "Spin Damping Strength",
            "How much rotation damping is added as speed rises. Higher = harder to spin the car out.",
            0f, 1f,
            c => (c.V.Stability.angularDragHelperStrength),
            (c, x) => { c.V.Stability.angularDragHelperStrength = x; },
            avail: c => (c.V.Stability != null), availReason: "Requires the optional RCCP_Stability component."));

        //  Skipped entirely while RCCP_Input.overrideExternalInputs is true (AI/harness control) — toggle appears dead in that state.
        entries.Add(T("counter-steering", RCCP_FeatureLabCategory.Assists, "Auto Counter-Steer",
            "Automatically steers slightly against a slide for you, making drifts easier to hold and recover.",
            c => (c.V.Inputs.counterSteering),
            (c, x) => { c.V.Inputs.counterSteering = x; },
            avail: c => (c.V.Inputs != null), availReason: "RCCP_Input is an optional child component; RCCP_MainComponent.Inputs returns null when absent."));

        entries.Add(S("counter-steer-factor", RCCP_FeatureLabCategory.Assists, "Counter-Steer Amount",
            "How much automatic opposite-lock is applied during a slide. 0 = none, 1 = strong correction.",
            0f, 1f,
            c => (c.V.Inputs.counterSteerFactor),
            (c, x) => { c.V.Inputs.counterSteerFactor = x; },
            avail: c => (c.V.Inputs != null), availReason: "Requires the optional RCCP_Input component."));

        //  No visible effect below 15 km/h; skipped while overrideExternalInputs is true.
        entries.Add(T("steering-limiter", RCCP_FeatureLabCategory.Assists, "Steering Limiter",
            "Limits how far you can steer at speed so the front tires don't scrub and tuck. Only active above 15 km/h.",
            c => (c.V.Inputs.steeringLimiter),
            (c, x) => { c.V.Inputs.steeringLimiter = x; },
            avail: c => (c.V.Inputs != null), availReason: "Requires the optional RCCP_Input component."));

        //  SELF-CLEARING: driver brakeInput >= 0.1 flips it false inside CruiseControl() — the UI must poll getExpr every frame; a write-only binding drifts out of sync.
        entries.Add(T("cruise-control", RCCP_FeatureLabCategory.Assists, "Cruise Control",
            "Holds your chosen speed with automatic throttle. Cancels itself the moment you press the brake.",
            c => (c.V.Inputs.cruiseControl),
            (c, x) => { c.V.Inputs.SetCruiseControl(x); },
            avail: c => (c.V.Inputs != null), availReason: "Requires the optional RCCP_Input component."));

        entries.Add(S("cruise-target-speed", RCCP_FeatureLabCategory.Assists, "Cruise Target Speed",
            "The speed cruise control tries to hold. It only adds throttle — above target it simply coasts back down.",
            10f, 240f,
            c => (c.V.Inputs.cruiseTargetSpeed),
            (c, x) => { c.V.Inputs.cruiseTargetSpeed = x; }, unit: "km/h",
            avail: c => (c.V.Inputs != null), availReason: "Requires the optional RCCP_Input component."));

        //  Only engages when nearly stopped (<= 1 km/h), grounded, in a forward gear, on a slope >= 2 degrees.
        entries.Add(T("hill-start-assist", RCCP_FeatureLabCategory.Assists, "Hill-Start Assist",
            "Holds the brakes for you when stopped on a slope, releasing as you step on the throttle. Works in forward gears.",
            c => (c.V.Inputs.hillStartAssist),
            (c, x) => { c.V.Inputs.hillStartAssist = x; },
            avail: c => (c.V.Inputs != null), availReason: "Requires the optional RCCP_Input component."));

        entries.Add(R("hill-hold-lamp", RCCP_FeatureLabCategory.Assists, "Hill Hold Active",
            "Lights up while hill-start assist is actively holding the car on a slope.",
            c => "" + (c.V.Inputs.hillHoldActive),
            avail: c => (c.V.Inputs != null), availReason: "Requires the optional RCCP_Input component."));

        entries.Add(R("speed-limiter-status", RCCP_FeatureLabCategory.Assists, "Speed Limiter Active",
            "Shows when the per-gear speed limiter is actively holding the car below its cap for the current gear.",
            c => "" + (c.V.OtherAddonsManager.Limiter.limitingNow),
            avail: c => (c.V.OtherAddonsManager != null && c.V.OtherAddonsManager.Limiter != null), availReason: "RCCP_Limiter is an optional addon reached only via RCCP_OtherAddons — no Limiter accessor exists on the controller itself; both links can be null."));

        //  Only observable with an Automatic gearbox; skipped while overrideExternalInputs is true.
        entries.Add(T("auto-reverse", RCCP_FeatureLabCategory.Assists, "Auto Reverse",
            "With an automatic gearbox, holding the brake at a stop shifts into reverse, and pressing forward again shifts back out.",
            c => (c.V.Inputs.autoReverse),
            (c, x) => { c.V.Inputs.autoReverse = x; },
            avail: c => (c.V.Inputs != null), availReason: "Requires the optional RCCP_Input component."));

        //  Inert on manual/DCT gearboxes; skipped while overrideExternalInputs is true.
        entries.Add(T("inverse-throttle-on-reverse", RCCP_FeatureLabCategory.Assists, "Swap Pedals In Reverse",
            "While reversing, swaps the pedals so throttle means go faster backwards and brake slows you down. Automatic gearbox only.",
            c => (c.V.Inputs.inverseThrottleBrakeOnReverse),
            (c, x) => { c.V.Inputs.inverseThrottleBrakeOnReverse = x; },
            avail: c => (c.V.Inputs != null), availReason: "Requires the optional RCCP_Input component."));

        entries.Add(R("assist-authority", RCCP_FeatureLabCategory.Assists, "Assist Authority",
            "How much authority the driving assists have right now. Pulling the handbrake fades ABS, ESP, TCS and helpers toward zero — by design.",
            c => "" + (Mathf.Clamp01(1f - c.V.handbrakeInput_V)),
            avail: c => (c.V.Stability != null), availReason: "The factor only shapes behavior when RCCP_Stability exists; the source field handbrakeInput_V lives on the controller and is always present."));

    }

}
