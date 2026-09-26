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

    static partial void BuildWheels(List<RCCP_FeatureLabEntry> entries) {

        //  None needed — SetSuspensionForces writes PhysX JointSpring immediately. Writing BaseSuspensionSpring FIRST makes the slider preset-survivable: behavior re-apply (RCCP_Events.Event_OnBehaviorChanged) re-bakes Base x BehaviorType.suspensionSpringMultiplier.
        entries.Add(S("wheels-suspension-spring", RCCP_FeatureLabCategory.Wheels, "Suspension Spring",
            "Stiffness of the suspension springs on all wheels. Higher = firmer, sportier ride that resists body roll; lower = softer with more body movement.",
            10000f, 150000f,
            c => (c.V.AllWheelColliders[0].WheelCollider.suspensionSpring.spring),
            (c, x) => { foreach (RCCP_WheelCollider w in c.V.AllWheelColliders) { w.BaseSuspensionSpring = x; w.SetSuspensionForces(x, w.WheelCollider.suspensionSpring.damper); }; }, unit: "N/m", format: "0",
            avail: c => (c.V.AllWheelColliders != null && c.V.AllWheelColliders.Length > 0 && c.V.AllWheelColliders[0].WheelCollider != null), availReason: "Needs at least one RCCP_WheelCollider with a Unity WheelCollider present (both lazy accessors; AllWheelColliders can be an empty array on a malformed vehicle)."));

        //  None needed — applies to PhysX immediately. BaseSuspensionDamper write keeps it preset-survivable (behavior re-apply bakes Base x suspensionDamperMultiplier).
        entries.Add(S("wheels-suspension-damper", RCCP_FeatureLabCategory.Wheels, "Suspension Damper",
            "How quickly suspension bounce settles. Higher = tight, controlled rebound after bumps; lower = floaty, boat-like body motion.",
            500f, 10000f,
            c => (c.V.AllWheelColliders[0].WheelCollider.suspensionSpring.damper),
            (c, x) => { foreach (RCCP_WheelCollider w in c.V.AllWheelColliders) { w.BaseSuspensionDamper = x; w.SetSuspensionForces(w.WheelCollider.suspensionSpring.spring, x); }; }, unit: "N·s/m", format: "0",
            avail: c => (c.V.AllWheelColliders != null && c.V.AllWheelColliders.Length > 0 && c.V.AllWheelColliders[0].WheelCollider != null), availReason: "Needs at least one RCCP_WheelCollider with a Unity WheelCollider present."));

        //  None — PhysX accepts live suspensionDistance writes and nothing in RCCP rewrites it per-frame or on behavior apply (SetSuspensionForces deliberately preserves it, verified in its body).
        entries.Add(S("wheels-suspension-distance", RCCP_FeatureLabCategory.Wheels, "Suspension Travel",
            "How far each wheel's suspension can travel, in meters. Longer soaks up big bumps like an off-roader; shorter feels low and stiff like a race car.",
            0.05f, 0.5f,
            c => (c.V.AllWheelColliders[0].WheelCollider.suspensionDistance),
            (c, x) => { foreach (RCCP_WheelCollider w in c.V.AllWheelColliders) { w.WheelCollider.suspensionDistance = x; }; }, unit: "m", format: "0.00",
            avail: c => (c.V.AllWheelColliders != null && c.V.AllWheelColliders.Length > 0 && c.V.AllWheelColliders[0].WheelCollider != null), availReason: "Needs at least one RCCP_WheelCollider with a Unity WheelCollider present."));

        //  None — never touched per-frame or by behavior apply (SetSuspensionForces preserves it).
        entries.Add(S("wheels-suspension-target-pos", RCCP_FeatureLabCategory.Wheels, "Ride Height (Rest Position)",
            "Where the suspension rests within its travel. Higher raises the car up on its springs; lower lets it sit slammed and compressed.",
            0f, 1f,
            c => (c.V.AllWheelColliders[0].WheelCollider.suspensionSpring.targetPosition),
            (c, x) => { foreach (RCCP_WheelCollider w in c.V.AllWheelColliders) { JointSpring js = w.WheelCollider.suspensionSpring; js.targetPosition = Mathf.Clamp01(x); w.WheelCollider.suspensionSpring = js; }; }, format: "0.00",
            avail: c => (c.V.AllWheelColliders != null && c.V.AllWheelColliders.Length > 0 && c.V.AllWheelColliders[0].WheelCollider != null), availReason: "Needs at least one RCCP_WheelCollider with a Unity WheelCollider present."));

        //  None — grip multiplies ground-material friction stiffness inside Frictions() EVERY FixedUpdate; this is the only durable live grip dial (direct WheelFrictionCurve.stiffness writes are recomputed away within one physics step). Not written by behavior-preset apply.
        entries.Add(S("wheels-grip", RCCP_FeatureLabCategory.Wheels, "Tire Grip",
            "Tire grip multiplier for all wheels. 1 = normal, below 1 slides like ice, up to 2 = glued to the road.",
            0f, 2f,
            c => (c.V.AllWheelColliders[0].grip),
            (c, x) => { foreach (RCCP_WheelCollider w in c.V.AllWheelColliders) { w.grip = x; }; }, unit: "x", format: "0.00",
            avail: c => (c.V.AllWheelColliders != null && c.V.AllWheelColliders.Length > 0), availReason: "Needs at least one RCCP_WheelCollider (grip is an RCCP field, no Unity WheelCollider access needed in the lambda)."));

        //  None — consumed every Update in WheelAlign() (wheel model tilt) and per-frame by RCCP_Caliper. Fully live.
        entries.Add(S("wheels-camber", RCCP_FeatureLabCategory.Wheels, "Camber (Visual)",
            "Tilts the wheel models inward or outward, in degrees, for stance/styling. Purely visual — it does not change grip or handling in this controller.",
            -10f, 10f,
            c => (c.V.AllWheelColliders[0].camber),
            (c, x) => { foreach (RCCP_WheelCollider w in c.V.AllWheelColliders) { w.camber = x; }; }, unit: "°", format: "0.0",
            avail: c => (c.V.AllWheelColliders != null && c.V.AllWheelColliders.Length > 0), availReason: "Needs at least one RCCP_WheelCollider."));

        //  None — Deflate() caches defRadius on first call, sets radius = defRadius * deflatedRadiusMultiplier (0.8 default), adds a random lateral impulse, fires CarController.OnWheelDeflated() (audio) and marks Damage.repaired = false.
        entries.Add(A("wheels-deflate", RCCP_FeatureLabCategory.Wheels, "Deflate Tires",
            "Pops all tires: wheels shrink and lose grip, making the car wobble and slide. Use Inflate Tires to undo.",
            "Deflate Tires",
            c => { foreach (RCCP_WheelCollider w in c.V.AllWheelColliders) { w.Deflate(); }; },
            status: c => "" + (System.Linq.Enumerable.Count(c.V.AllWheelColliders, w => w.deflated) + "/" + c.V.AllWheelColliders.Length + " flat"),
            avail: c => (c.V.AllWheelColliders != null && c.V.AllWheelColliders.Length > 0), availReason: "Needs at least one RCCP_WheelCollider. Deflate() itself no-ops safely on detached wheels (early-returns when WheelCollider.enabled == false) and on already-deflated wheels — the loop needs no extra guards."));

        //  None — restores the cached defRadius and fires CarController.OnWheelInflated(). Immediate.
        entries.Add(A("wheels-inflate", RCCP_FeatureLabCategory.Wheels, "Inflate Tires",
            "Re-inflates any flat tires, restoring normal wheel size and grip.",
            "Inflate Tires",
            c => { foreach (RCCP_WheelCollider w in c.V.AllWheelColliders) { w.Inflate(); }; },
            status: c => "" + (System.Linq.Enumerable.Count(c.V.AllWheelColliders, w => w.deflated) + "/" + c.V.AllWheelColliders.Length + " flat"),
            avail: c => (c.V.AllWheelColliders != null && c.V.AllWheelColliders.Length > 0), availReason: "Needs at least one RCCP_WheelCollider. Inflate() no-ops on detached (disabled) wheels and on wheels that are not deflated."));

        //  None for the write itself (checked every FixedUpdate in SkidMarks()). CLOBBER: if the vehicle has an RCCP_Lod component, it REWRITES drawSkid on LOD level transitions (RCCP_Lod.cs:175/212/249/286) — either strip/disable RCCP_Lod on the lab vehicle or re-assert the toggle after LOD changes.
        entries.Add(T("wheels-draw-skid", RCCP_FeatureLabCategory.Wheels, "Skidmarks",
            "Leave black rubber skidmarks on the road when tires slip. Turn off for a clean track.",
            c => (c.V.AllWheelColliders[0].drawSkid),
            (c, x) => { foreach (RCCP_WheelCollider w in c.V.AllWheelColliders) { w.drawSkid = x; }; },
            avail: c => (c.V.AllWheelColliders != null && c.V.AllWheelColliders.Length > 0), availReason: "Needs at least one RCCP_WheelCollider. This is the ONLY skidmark switch — RCCP_SkidmarksManager has no global on/off API (only CleanSkidmarks())."));

    }

}
