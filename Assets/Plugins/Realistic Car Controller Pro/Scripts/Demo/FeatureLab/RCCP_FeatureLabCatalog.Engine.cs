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

    static partial void BuildEngine(List<RCCP_FeatureLabEntry> entries) {

        //  none — consumed every FixedUpdate in GenerateKW()/NegativeFeedback(); NMCurve is normalized 0-1 so no curve regen is needed
        entries.Add(S("engine-max-torque", RCCP_FeatureLabCategory.Engine, "Max Torque",
            "Peak pulling power of the engine in newton-meters. Higher = harder acceleration; lower makes the car feel tame.",
            50f, 1000f,
            c => (c.V.Engine.maximumTorqueAsNM),
            (c, x) => { c.V.Engine.maximumTorqueAsNM = x; }, unit: "Nm",
            avail: c => (c.V.Engine != null), availReason: "Engine is a lazy property on RCCP_MainComponent that returns null when the vehicle has no RCCP_Engine child.", hint: "Any RCCP vehicle (all have an engine)."));

        //  CheckAndCreateNMCurve() (curve only auto-builds in Start) PLUS UpdateMaximumSpeed() when autoCalculateDifferentialRatio is on — the Update() change-detector watches only maximumSpeed, so the diff ratio silently drifts off the top-speed target otherwise.
        entries.Add(S("engine-max-rpm", RCCP_FeatureLabCategory.Engine, "Max Engine RPM",
            "The engine's redline. Higher lets each gear stretch further before the limiter kicks in and reshapes the power band toward the top end.",
            5000f, 10000f,
            c => (c.V.Engine.maxEngineRPM),
            (c, x) => { c.V.Engine.maxEngineRPM = x; c.V.Engine.CheckAndCreateNMCurve(); if (c.V.Engine.autoCalculateDifferentialRatio) c.V.Engine.UpdateMaximumSpeed(); }, unit: "RPM",
            avail: c => (c.V.Engine != null), availReason: "Engine sub-component may be missing (lazy null-returning property).", hint: "Any RCCP vehicle."));

        //  CheckAndCreateNMCurve() — the torque curve is only auto-generated in Start(); the idle band itself (min ±10%) is read live every FixedUpdate.
        entries.Add(S("engine-min-rpm", RCCP_FeatureLabCategory.Engine, "Min Engine RPM (Idle)",
            "Idle speed of the engine. Higher keeps the engine spinning faster at rest and fattens low-end response; lower gives a lazier idle.",
            400f, 1500f,
            c => (c.V.Engine.minEngineRPM),
            (c, x) => { c.V.Engine.minEngineRPM = x; c.V.Engine.CheckAndCreateNMCurve(); }, unit: "RPM",
            avail: c => (c.V.Engine != null), availReason: "Engine sub-component may be missing (lazy null-returning property).", hint: "Any RCCP vehicle."));

        //  none — checked every FixedUpdate; false also forces cutFuel=false, so the Rev Cut lamp goes dark
        entries.Add(T("engine-rev-limiter", RCCP_FeatureLabCategory.Engine, "Rev Limiter",
            "Cuts fuel just below the redline to protect the engine — the classic stutter at max revs. Off removes the stutter and lets revs pin at the ceiling.",
            c => (c.V.Engine.engineRevLimiter),
            (c, x) => { c.V.Engine.engineRevLimiter = x; },
            avail: c => (c.V.Engine != null), availReason: "Engine sub-component may be missing (lazy null-returning property).", hint: "Any RCCP vehicle."));

        //  none — LIVE, checked every FixedUpdate in LaunchControl() (runs after RevLimiter inside Inputs())
        //  Best demoed from a standstill with full throttle.
        entries.Add(T("engine-launch-control", RCCP_FeatureLabCategory.Engine, "Launch Control",
            "Holds engine revs at a launch-friendly RPM while you stand on the throttle at a standstill, for consistent hard launches. Arms automatically.",
            c => (c.V.Engine.launchControlEnabled),
            (c, x) => { c.V.Engine.launchControlEnabled = x; },
            avail: c => (c.V.Engine != null), availReason: "Engine sub-component may be missing (lazy null-returning property)."));

        //  Any RCCP vehicle with Launch Control toggled on.
        entries.Add(R("engine-launch-armed-lamp", RCCP_FeatureLabCategory.Engine, "Launch ARMED",
            "Lights while launch control is holding the revs for a launch. Stand still and hold the throttle to arm it — it engages on its own.",
            c => "" + (c.V.Engine.launchControlActive),
            avail: c => (c.V.Engine != null), availReason: "Engine sub-component may be missing (lazy null-returning property)."));

        //  none — read every FixedUpdate via CalculateEngineFriction() (RPM- and temperature-scaled)
        //  Any RCCP vehicle; most audible when lifting off at high RPM in neutral.
        entries.Add(S("engine-friction", RCCP_FeatureLabCategory.Engine, "Engine Friction",
            "How much internal drag the engine has. Higher makes revs fall faster when you lift off the throttle; lower keeps the engine spinning freely.",
            0f, 1f,
            c => (c.V.Engine.engineFriction),
            (c, x) => { c.V.Engine.engineFriction = x; },
            avail: c => (c.V.Engine != null), availReason: "Engine sub-component may be missing (lazy null-returning property)."));

        //  none — read every FixedUpdate in CalculateDynamicInertia(); final SmoothDamp time = dynamicInertia*0.35
        //  Any RCCP vehicle; rev the engine in neutral to feel it.
        entries.Add(S("engine-inertia", RCCP_FeatureLabCategory.Engine, "Engine Inertia",
            "How heavy the engine's spinning parts feel. Lower = revs jump instantly with the pedal; higher = slow, truck-like rev buildup.",
            0.01f, 1f,
            c => (c.V.Engine.engineInertia),
            (c, x) => { c.V.Engine.engineInertia = x; },
            avail: c => (c.V.Engine != null), availReason: "Engine sub-component may be missing (lazy null-returning property)."));

        //  UpdateMaximumSpeed() on enable is REQUIRED: Update() syncs maximumSpeed_Old unconditionally every frame (line 386), so a Max Speed moved while the flag was off never re-fires the change-detector on its own — the explicit call snaps the diff ratios to the current target (mirrors Awake behavior).
        entries.Add(T("engine-auto-diff-ratio", RCCP_FeatureLabCategory.Engine, "Auto Gearing (Top Speed Match)",
            "Automatically re-gears the car so its top speed matches the Max Speed slider. Off lets you hand-tune gearing; Max Speed then becomes a label.",
            c => (c.V.Engine.autoCalculateDifferentialRatio),
            (c, x) => { c.V.Engine.autoCalculateDifferentialRatio = x; if (x) c.V.Engine.UpdateMaximumSpeed(); },
            avail: c => (c.V.Engine != null), availReason: "Engine sub-component may be missing (lazy null-returning property).", hint: "Any RCCP vehicle."));

        //  none — Update() (render rate) compares against hidden maximumSpeed_Old and calls UpdateMaximumSpeed() automatically on change; a plain field write is the whole binding.
        //  Any RCCP vehicle; long straight recommended to verify the new top speed.
        entries.Add(S("engine-max-speed", RCCP_FeatureLabCategory.Engine, "Max Speed",
            "Target top speed in km/h. The car re-gears itself on the fly to top out at this value. Only active while Auto Gearing is on.",
            80f, 400f,
            c => (c.V.Engine.maximumSpeed),
            (c, x) => { c.V.Engine.maximumSpeed = x; }, unit: "km/h",
            avail: c => (c.V.Engine != null && c.V.Engine.autoCalculateDifferentialRatio), availReason: "With autoCalculateDifferentialRatio OFF the field is INERT at runtime (Update() detector skips it), so the slider grays out rather than lying; Engine itself may also be missing."));

        //  Any RCCP vehicle; hold full throttle in a low gear to see it strobe at redline.
        entries.Add(R("engine-rev-cut-lamp", RCCP_FeatureLabCategory.Engine, "Rev Cut",
            "Lights whenever fuel is being cut — by the rev limiter at redline, by launch control holding revs, or by a speed limiter.",
            c => "" + (c.V.Engine.cutFuel),
            avail: c => (c.V.Engine != null), availReason: "Engine sub-component may be missing (lazy null-returning property)."));

    }

}
