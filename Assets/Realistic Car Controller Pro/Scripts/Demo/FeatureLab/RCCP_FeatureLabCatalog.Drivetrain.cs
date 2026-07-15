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

    static partial void BuildDrivetrain(List<RCCP_FeatureLabEntry> entries) {

        //  None — transmissionType is branched on every FixedUpdate (live).
        entries.Add(E("drivetrain-transmission-type", RCCP_FeatureLabCategory.Drivetrain, "Transmission Type",
            "How gears change: Manual (you shift), Automatic (shifts itself), or Automatic with a D/N/R/P selector like a real automatic car.",
            new string[] { "Manual", "Automatic", "Automatic (D/N/R/P)" },
            c => ((int)c.V.Gearbox.transmissionType),
            (c, x) => { c.V.Gearbox.transmissionType = (RCCP_Gearbox.TransmissionType)x; },
            avail: c => (c.V.Gearbox != null), availReason: "Vehicle has no gearbox component."));

        //  None — read every FixedUpdate (live).
        entries.Add(S("drivetrain-shift-up-rpm", RCCP_FeatureLabCategory.Drivetrain, "Shift-Up RPM",
            "Engine speed where the automatic gearbox shifts to the next gear. Higher = revvier, sportier shifts; lower = relaxed early shifts.",
            3000f, 8000f,
            c => (c.V.Gearbox.shiftUpRPM),
            (c, x) => { c.V.Gearbox.shiftUpRPM = x; }, unit: "RPM",
            avail: c => (c.V.Gearbox != null), availReason: "Vehicle has no gearbox component."));

        //  None — read every FixedUpdate (live).
        entries.Add(S("drivetrain-shift-down-rpm", RCCP_FeatureLabCategory.Drivetrain, "Shift-Down RPM",
            "Engine speed where the automatic gearbox drops a gear. Higher = eager downshifts when slowing; lower = holds the current gear longer.",
            1000f, 5000f,
            c => (c.V.Gearbox.shiftDownRPM),
            (c, x) => { c.V.Gearbox.shiftDownRPM = x; }, unit: "RPM",
            avail: c => (c.V.Gearbox != null), availReason: "Vehicle has no gearbox component."));

        //  None — read every FixedUpdate (live).
        entries.Add(S("drivetrain-shift-threshold", RCCP_FeatureLabCategory.Drivetrain, "Shift Threshold",
            "How much of each gear's speed range is used before shifting up. Higher = holds gears longer for a sportier feel; lower = short-shifts early.",
            0.1f, 0.9f,
            c => (c.V.Gearbox.shiftThreshold),
            (c, x) => { c.V.Gearbox.shiftThreshold = x; }, unit: "×",
            avail: c => (c.V.Gearbox != null), availReason: "Vehicle has no gearbox component."));

        //  None needed — read at the start of the NEXT ShiftTo coroutine, so changes apply from the next shift.
        entries.Add(S("drivetrain-shift-time", RCCP_FeatureLabCategory.Drivetrain, "Shift Time",
            "How long a gear change takes. Lower = instant race-style shifts; higher = slow, deliberate shifts with a torque gap.",
            0f, 1.5f,
            c => (c.V.Gearbox.shiftingTime),
            (c, x) => { c.V.Gearbox.shiftingTime = x; }, unit: "s",
            avail: c => (c.V.Gearbox != null), availReason: "Vehicle has no gearbox component."));

        //  None — read every FixedUpdate while automaticClutch is on (live).
        entries.Add(S("drivetrain-clutch-inertia", RCCP_FeatureLabCategory.Drivetrain, "Clutch Inertia",
            "How quickly the automatic clutch engages and releases. Lower = snappy, aggressive engagement; higher = smooth and gradual.",
            0f, 0.9f,
            c => (c.V.Clutch.clutchInertia),
            (c, x) => { c.V.Clutch.clutchInertia = x; }, unit: "×",
            avail: c => (c.V.Clutch != null), availReason: "Vehicle has no clutch component."));

        //  None — differentialType is switched on every FixedUpdate in CalculateSlip() (live).
        entries.Add(E("drivetrain-differential-type", RCCP_FeatureLabCategory.Drivetrain, "Differential Type",
            "How engine power splits between left and right wheels. Open = easy cruising, Limited = balanced grip, Locked/Direct = both wheels spin together (drifty).",
            new string[] { "Open", "Limited Slip", "Fully Locked", "Direct" },
            c => ((int)c.V.Differentials[0].differentialType),
            (c, x) => { foreach (RCCP_Differential d in c.V.Differentials) d.differentialType = (RCCP_Differential.DifferentialType)x; },
            avail: c => (c.V.Differentials != null && c.V.Differentials.Length > 0), availReason: "Vehicle has no differential component."));

        //  None after set — consumed live every FixedUpdate in DistributeTorque(). setExpr deliberately disables Engine.autoCalculateDifferentialRatio first so the value is not clobbered.
        entries.Add(S("drivetrain-final-drive-ratio", RCCP_FeatureLabCategory.Drivetrain, "Final Drive Ratio",
            "Overall gearing multiplier. Higher = quicker acceleration but lower top speed; lower = taller gearing for a higher top speed.",
            2f, 6f,
            c => (c.V.Differentials[0].finalDriveRatio),
            (c, x) => { if (c.V.Engine != null) c.V.Engine.autoCalculateDifferentialRatio = false; foreach (RCCP_Differential d in c.V.Differentials) d.finalDriveRatio = x; }, unit: ":1",
            avail: c => (c.V.Differentials != null && c.V.Differentials.Length > 0), availReason: "Vehicle has no differential component."));

        entries.Add(R("drivetrain-drive-type", RCCP_FeatureLabCategory.Drivetrain, "Drive Type",
            "Which wheels are powered right now: front (FWD), rear (RWD), or all (AWD). Read live from the vehicle's drivetrain.",
            c => "" + ((c.V.FrontAxle != null && c.V.FrontAxle.isPower) ? ((c.V.RearAxle != null && c.V.RearAxle.isPower) ? "AWD" : "FWD") : ((c.V.RearAxle != null && c.V.RearAxle.isPower) ? "RWD" : (c.V.PoweredAxles.Count > 0 ? "Custom" : "None"))),
            avail: c => (c.V.AxleManager != null), availReason: "Vehicle has no axle manager."));

        entries.Add(R("drivetrain-gear-readout", RCCP_FeatureLabCategory.Drivetrain, "Current Gear",
            "The gear the vehicle is in right now: P, R, N, or the forward gear number. Shows '...' while a shift is in progress.",
            c => "" + (c.V.Gearbox.currentGearState.gearState == RCCP_Gearbox.CurrentGearState.GearState.Park ? "P" : c.V.Gearbox.currentGearState.gearState == RCCP_Gearbox.CurrentGearState.GearState.Neutral ? "N" : c.V.Gearbox.currentGearState.gearState == RCCP_Gearbox.CurrentGearState.GearState.InReverseGear ? "R" : (c.V.Gearbox.shiftingNow ? "..." : (c.V.Gearbox.currentGear + 1).ToString())),
            avail: c => (c.V.Gearbox != null), availReason: "Vehicle has no gearbox component."));

        entries.Add(A("drivetrain-shift-up-action", RCCP_FeatureLabCategory.Drivetrain, "Shift Up",
            "Shift to the next gear. Works in any transmission mode; from reverse it goes to 1st.",
            "Shift Up",
            c => { c.V.Gearbox.ShiftUp(); },
            status: c => "" + (c.V.Gearbox.shiftingNow ? "SHIFTING" : "READY"),
            avail: c => (c.V.Gearbox != null), availReason: "Vehicle has no gearbox component."));

        entries.Add(A("drivetrain-shift-down-action", RCCP_FeatureLabCategory.Drivetrain, "Shift Down",
            "Drop one gear. At 1st gear this tries reverse instead, which only works near standstill.",
            "Shift Down",
            c => { c.V.Gearbox.ShiftDown(); },
            status: c => "" + (c.V.Gearbox.shiftingNow ? "SHIFTING" : "READY"),
            avail: c => (c.V.Gearbox != null), availReason: "Vehicle has no gearbox component."));

        entries.Add(A("drivetrain-shift-reverse-action", RCCP_FeatureLabCategory.Drivetrain, "Shift to Reverse",
            "Put the gearbox in reverse. Quietly refused while still rolling forward faster than about 20 km/h — slow down first.",
            "Shift to Reverse",
            c => { c.V.Gearbox.ShiftReverse(); },
            status: c => "" + (c.V.Gearbox.currentGearState.gearState == RCCP_Gearbox.CurrentGearState.GearState.InReverseGear ? "IN REVERSE" : (c.V.CarController.speed > c.V.Gearbox.maxSpeedToShiftReverse ? "TOO FAST" : "READY")),
            avail: c => (c.V.Gearbox != null), availReason: "Vehicle has no gearbox component."));

        //  Only vehicles with a NOS component under OtherAddons (most RCCP demo sports cars).
        entries.Add(R("drivetrain-nos-amount", RCCP_FeatureLabCategory.Drivetrain, "NOS Remaining",
            "How much nitrous boost is left, as a share of a full tank. Refills on its own after a short pause when boost is not being used.",
            c => "" + (c.V.OtherAddonsManager.Nos.timer / Mathf.Max(0.01f, c.V.OtherAddonsManager.Nos.durationTime)),
            avail: c => (c.V.OtherAddonsManager != null && c.V.OtherAddonsManager.Nos != null), availReason: "Vehicle has no NOS addon."));

    }

}
