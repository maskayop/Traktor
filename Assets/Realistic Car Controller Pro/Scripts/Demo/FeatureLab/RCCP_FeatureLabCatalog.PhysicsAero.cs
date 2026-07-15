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

    static partial void BuildPhysicsAero(List<RCCP_FeatureLabEntry> entries) {

        //  none — read every FixedUpdate, fully live
        entries.Add(S("phys-downforce", RCCP_FeatureLabCategory.PhysicsAero, "Downforce",
            "Presses the car onto the road as speed builds. Higher = more grip in fast corners; the push grows with the square of your speed.",
            0f, 100f,
            c => (c.V.AeroDynamics.downForce),
            (c, x) => { c.V.AeroDynamics.downForce = x; },
            avail: c => (c.V.AeroDynamics != null), availReason: "RCCP_AeroDynamics is an optional child component; RCCP_MainComponent.AeroDynamics returns null when absent.", hint: "Per-vehicle (active player vehicle)."));

        //  n/a (readout)
        entries.Add(R("phys-downforce-applied", RCCP_FeatureLabCategory.PhysicsAero, "Applied Downforce",
            "How much downward force the car is getting right now. Grows with the square of your speed and drops to zero while airborne.",
            c => "" + ((c.V.IsGrounded ? c.V.AeroDynamics.downForce * Mathf.Pow(Mathf.Abs(c.V.speed) / 3.6f, 2f) * 0.15f : 0f)),
            avail: c => (c.V.AeroDynamics != null), availReason: "Needs the aerodynamics component for the downForce coefficient."));

        //  none — Rigidbody property, live
        entries.Add(S("phys-air-drag", RCCP_FeatureLabCategory.PhysicsAero, "Air Drag (Body Damping)",
            "Overall movement damping on the car body. Higher = the car coasts to a stop sooner and tops out at a lower speed.",
            0f, 1f,
            c => (c.V.Rigid.linearDamping),
            (c, x) => { c.V.Rigid.linearDamping = x; },
            avail: c => (c.V.AeroDynamics == null || !c.V.AeroDynamics.ignoreRigidbodyDragOnAccelerate), availReason: "While ignoreRigidbodyDragOnAccelerate is ON, RCCP_AeroDynamics.FixedUpdate overwrites Rigid.linearDamping every physics step (defaultDrag*(1-throttle)) — the slider would silently fight it, so it is disabled."));

        //  none — checked every FixedUpdate
        entries.Add(T("phys-ignore-drag-on-accel", RCCP_FeatureLabCategory.PhysicsAero, "Cut Drag On Throttle",
            "Fades body drag out while you hold the throttle, so acceleration feels punchier. Drag returns as you lift off the pedal.",
            c => (c.V.AeroDynamics.ignoreRigidbodyDragOnAccelerate),
            (c, x) => { c.V.AeroDynamics.ignoreRigidbodyDragOnAccelerate = x; },
            avail: c => (c.V.AeroDynamics != null), availReason: "Field lives on the optional RCCP_AeroDynamics component."));

        //  SetCOMOffset() pushes Rigid.centerOfMass immediately; RecomputeInertia() included to re-anchor a frozen inertia tensor (frozen-tensor policy — harmless no-op when override is off)
        entries.Add(S("phys-com-height", RCCP_FeatureLabCategory.PhysicsAero, "Center of Mass Height",
            "Raises or lowers the car's balance point. Lower = flatter, more stable cornering; higher = more body roll and easier rollovers.",
            -0.5f, 1.5f,
            c => (c.V.AeroDynamics.COM.localPosition.y),
            (c, x) => { Vector3 comOfs = c.V.AeroDynamics.COM.localPosition; comOfs.y = x; c.V.AeroDynamics.SetCOMOffset(comOfs); c.V.AeroDynamics.RecomputeInertia(); },
            avail: c => (c.V.AeroDynamics != null), availReason: "COM handling lives on RCCP_AeroDynamics; the COM property auto-creates a 'COM' child if missing, but the component itself may be absent."));

        //  RecomputeInertia() — included; the bool is read ONLY inside that method, so toggling without it is inert. Disabling REQUIRES this final call (it runs rb.ResetInertiaTensor() to hand control back to Unity).
        entries.Add(T("phys-inertia-override", RCCP_FeatureLabCategory.PhysicsAero, "Custom Rotation Inertia",
            "Takes manual control of how easily the car rotates. Turn on to use the Pitch/Yaw/Roll sliders below; off returns to Unity's automatic value.",
            c => (c.V.AeroDynamics.overrideInertiaTensor),
            (c, x) => { c.V.AeroDynamics.overrideInertiaTensor = x; c.V.AeroDynamics.RecomputeInertia(); },
            avail: c => (c.V.AeroDynamics != null), availReason: "Inertia tuning (V2.53) lives on RCCP_AeroDynamics."));

        //  RecomputeInertia() — included in every set; fields are read ONLY inside it (frozen-tensor policy)
        entries.Add(S("phys-inertia-pitch", RCCP_FeatureLabCategory.PhysicsAero, "Pitch Inertia (X)",
            "Resistance to nose-diving and squatting. Lower = livelier pitch under braking and throttle; higher = a calmer, heavier feel.",
            0.25f, 3f,
            c => (c.V.AeroDynamics.inertiaTensorScale.x),
            (c, x) => { Vector3 its = c.V.AeroDynamics.inertiaTensorScale; its.x = x; c.V.AeroDynamics.inertiaTensorScale = its; c.V.AeroDynamics.inertiaTensorMode = RCCP_AeroDynamics.InertiaTensorMode.Multiplier; c.V.AeroDynamics.RecomputeInertia(); }, unit: "x",
            avail: c => (c.V.AeroDynamics != null && c.V.AeroDynamics.overrideInertiaTensor), availReason: "inertiaTensorScale is consumed only while the override is active — slider is meaningless (inert) with the override off."));

        //  RecomputeInertia() — included in every set
        entries.Add(S("phys-inertia-yaw", RCCP_FeatureLabCategory.PhysicsAero, "Yaw Inertia (Y)",
            "Resistance to turning left/right. Lower = snappier direction changes and easier spins; higher = more stable and slower to rotate.",
            0.25f, 3f,
            c => (c.V.AeroDynamics.inertiaTensorScale.y),
            (c, x) => { Vector3 its = c.V.AeroDynamics.inertiaTensorScale; its.y = x; c.V.AeroDynamics.inertiaTensorScale = its; c.V.AeroDynamics.inertiaTensorMode = RCCP_AeroDynamics.InertiaTensorMode.Multiplier; c.V.AeroDynamics.RecomputeInertia(); }, unit: "x",
            avail: c => (c.V.AeroDynamics != null && c.V.AeroDynamics.overrideInertiaTensor), availReason: "Consumed only while overrideInertiaTensor is on."));

        //  RecomputeInertia() — included in every set
        entries.Add(S("phys-inertia-roll", RCCP_FeatureLabCategory.PhysicsAero, "Roll Inertia (Z)",
            "Resistance to leaning side to side in corners. Lower = quicker, livelier body lean; higher = slower, more damped roll.",
            0.25f, 3f,
            c => (c.V.AeroDynamics.inertiaTensorScale.z),
            (c, x) => { Vector3 its = c.V.AeroDynamics.inertiaTensorScale; its.z = x; c.V.AeroDynamics.inertiaTensorScale = its; c.V.AeroDynamics.inertiaTensorMode = RCCP_AeroDynamics.InertiaTensorMode.Multiplier; c.V.AeroDynamics.RecomputeInertia(); }, unit: "x",
            avail: c => (c.V.AeroDynamics != null && c.V.AeroDynamics.overrideInertiaTensor), availReason: "Consumed only while overrideInertiaTensor is on."));

        //  none — raw Unity WheelCollider property, PhysX reads it every step; NOTHING in RCCP overwrites it during play (set once in editor-only Reset())
        entries.Add(S("phys-force-app-point", RCCP_FeatureLabCategory.PhysicsAero, "Tire Force Height",
            "Where tire forces push on the body. 0 = realistic body sway in corners; 0.25 = most stable with almost no lean.",
            0f, 0.25f,
            c => ((c.V.AllWheelColliders[0].WheelCollider != null ? c.V.AllWheelColliders[0].WheelCollider.forceAppPointDistance : 0.1f)),
            (c, x) => { for (int i = 0; i < c.V.AllWheelColliders.Length; i++) { if (c.V.AllWheelColliders[i] != null && c.V.AllWheelColliders[i].WheelCollider != null) c.V.AllWheelColliders[i].WheelCollider.forceAppPointDistance = x; }; }, unit: "m",
            avail: c => (c.V.AllWheelColliders != null && c.V.AllWheelColliders.Length > 0), availReason: "Needs at least one RCCP_WheelCollider; the array getter re-scans children when empty."));

        //  none — Rigidbody property, live
        entries.Add(S("phys-max-angular-velocity", RCCP_FeatureLabCategory.PhysicsAero, "Max Spin Rate",
            "Caps how fast the car can rotate overall. Lower stops violent spins after big hits; higher allows wilder spin-outs.",
            0.5f, 20f,
            c => (c.V.Rigid.maxAngularVelocity),
            (c, x) => { c.V.Rigid.maxAngularVelocity = x; }, unit: "rad/s"));

        //  direct WheelCollider.ConfigureVehicleSubsteps(threshold, below, above) call included in setExpr — deliberately BYPASSES the private RCCP ConfigureWheelSubsteps()/Event_OnBehaviorChanged path, which would re-bake the entire behavior preset (clobbering manual tuning) and doesn't run at all when no behavior resolves
        entries.Add(E("phys-wheel-substeps", RCCP_FeatureLabCategory.PhysicsAero, "Wheel Simulation Quality",
            "How finely the wheel physics is simulated. Realistic suits road cars, OffRoad helps on bumpy ground, HighSpeed for very fast racing.",
            new string[] { "Realistic", "Arcade", "OffRoad", "HighSpeed" },
            c => ((int)c.V.wheelSubstepProfile),
            (c, x) => { c.V.overrideWheelSubstepProfile = true; c.V.wheelSubstepProfile = (RCCP_WheelSubstepProfile)x; float sTh = (x == 1) ? 20f : ((x == 3) ? 30f : 10f); int sBelow = (x == 0) ? 12 : ((x == 1) ? 10 : ((x == 2) ? 14 : 22)); int sAbove = (x == 0) ? 8 : ((x == 1) ? 6 : ((x == 2) ? 10 : 16)); for (int i = 0; i < c.V.AllWheelColliders.Length; i++) { if (c.V.AllWheelColliders[i] != null && c.V.AllWheelColliders[i].WheelCollider != null) { c.V.AllWheelColliders[i].WheelCollider.ConfigureVehicleSubsteps(sTh, sBelow, sAbove); break; } }; },
            avail: c => (c.V.AllWheelColliders != null && c.V.AllWheelColliders.Length > 0), availReason: "ConfigureVehicleSubsteps must be invoked on a live WheelCollider (per-Rigidbody in PhysX — one wheel configures the whole vehicle).", hint: "Per-vehicle (per-Rigidbody in PhysX)."));

        //  RCCP.SetBehavior(int) writes the clone (overrideBehavior=true + index) and fires Event_OnBehaviorChanged; vehicles bake the preset via CheckBehaviorDelayed one WaitForFixedUpdate later (expect 1-2 frame latency)
        //  GLOBAL — affects every vehicle in the scene, not just the active one (vehicles with Lock Manual Tuning ON are excluded).

        //  Preset labels resolve from the runtime clone at build time — preset names are user-editable.
        RCCP_Settings labelSource = RCCP_RuntimeSettings.RCCPSettingsInstance;
        string[] presetLabels;

        if (labelSource != null && labelSource.behaviorTypes != null && labelSource.behaviorTypes.Length > 0) {

            presetLabels = new string[labelSource.behaviorTypes.Length + 1];

            for (int i = 0; i < labelSource.behaviorTypes.Length; i++)
                presetLabels[i] = labelSource.behaviorTypes[i].behaviorName;

            presetLabels[labelSource.behaviorTypes.Length] = "Off";

        } else {

            presetLabels = new string[] { "Off" };

        }

        entries.Add(E("phys-behavior-preset", RCCP_FeatureLabCategory.PhysicsAero, "Handling Preset",
            "Applies a whole handling personality (grip, helpers, suspension feel) to every car in the scene. Off stops preset control but keeps current values.",
            presetLabels,
            c => ((c.S.overrideBehavior && c.S.behaviorSelectedIndex >= 0 && c.S.behaviorSelectedIndex < c.S.behaviorTypes.Length) ? c.S.behaviorSelectedIndex : c.S.behaviorTypes.Length),
            (c, x) => { if (x >= 0 && x < c.S.behaviorTypes.Length) { RCCP.SetBehavior(x); } else { RCCP.ClearBehavior(); }; },
            avail: c => (c.S != null && c.S.behaviorTypes != null && c.S.behaviorTypes.Length > 0), availReason: "Needs the behaviorTypes preset array on the runtime settings clone."));

        //  none — CheckBehavior reads it at apply time (early-return when true)
        entries.Add(T("phys-lock-manual-tuning", RCCP_FeatureLabCategory.PhysicsAero, "Lock Manual Tuning",
            "Protects this car's slider tweaks from being overwritten when a handling preset is applied. Turn on before hand-tuning.",
            c => (c.V.ineffectiveBehavior),
            (c, x) => { c.V.ineffectiveBehavior = x; }));

        //  n/a (readout)
        entries.Add(R("phys-mass", RCCP_FeatureLabCategory.PhysicsAero, "Vehicle Mass",
            "The car's total weight in kilograms. Heavier cars accelerate and stop more slowly but carry more momentum through impacts.",
            c => "" + (c.V.Rigid.mass)));

    }

}
