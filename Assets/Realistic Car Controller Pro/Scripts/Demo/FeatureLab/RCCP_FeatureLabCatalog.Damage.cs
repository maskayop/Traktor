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

    static partial void BuildDamage(List<RCCP_FeatureLabEntry> entries) {

        //  Most RCCP demo vehicles carry RCCP_Damage; validator flags its absence.
        entries.Add(T("damage-enabled", RCCP_FeatureLabCategory.Damage, "Collision Damage",
            "Master switch for crash damage. Off stops new dents, wheel and light damage; it does not smooth out dents you already have — use Repair Vehicle for that.",
            c => (c.V.Damage.enabled),
            (c, x) => { c.V.Damage.enabled = x; },
            avail: c => (c.V.Damage != null), availReason: "Vehicle has no RCCP_Damage component (optional add-on)."));

        entries.Add(S("damage-deformation-multiplier", RCCP_FeatureLabCategory.Damage, "Deformation Strength",
            "How strongly crashes dent the bodywork. 1 = subtle dents, 10 = full-strength deformation, 0 = no denting at all. Only affects new impacts.",
            0f, 10f,
            c => (c.V.Damage.deformationMultiplier),
            (c, x) => { c.V.Damage.deformationMultiplier = x; }, unit: "x", format: "F1",
            avail: c => (c.V.Damage != null), availReason: "Vehicle has no RCCP_Damage component (optional add-on)."));

        entries.Add(A("damage-repair", RCCP_FeatureLabCategory.Damage, "Repair Vehicle",
            "Fixes everything in one click: body dents, broken lights, detached parts, and flat tires all return to factory condition.",
            "Repair Vehicle",
            c => { RCCP.Repair(c.V); },
            status: c => "" + (c.V.Damage.repaired ? "Intact" : "Damaged"),
            avail: c => (c.V.Damage != null), availReason: "Repair routes through RCCP_Damage; vehicle has no damage component."));

        //  Only vehicles with the Fuel Tank add-on under Other Addons; add via BoneCracker Games > RCCP > Other Addons > Fuel Tank.
        entries.Add(A("fuel-refill", RCCP_FeatureLabCategory.Damage, "Refuel Tank",
            "Fills the fuel tank back to 100% instantly. The engine burns fuel while running and shuts off when the tank runs dry.",
            "Refuel Tank",
            c => { c.V.OtherAddonsManager.FuelTank.Refill(); },
            avail: c => (c.V.OtherAddonsManager != null && c.V.OtherAddonsManager.FuelTank != null), availReason: "Vehicle has no Fuel Tank add-on (RCCP_OtherAddons > RCCP_FuelTank)."));

        entries.Add(R("fuel-level", RCCP_FeatureLabCategory.Damage, "Fuel Level",
            "Fuel left in the tank, shown in liters and percent. It drains faster at high revs and full throttle.",
            c => "" + (c.V.OtherAddonsManager.FuelTank.fuelTankCapacity.ToString("F1") + " L (" + Mathf.RoundToInt(c.V.OtherAddonsManager.FuelTank.fuelTankFillAmount * 100f) + "%)"),
            avail: c => (c.V.OtherAddonsManager != null && c.V.OtherAddonsManager.FuelTank != null), availReason: "Vehicle has no Fuel Tank add-on (RCCP_OtherAddons > RCCP_FuelTank)."));

        entries.Add(R("world-stations", RCCP_FeatureLabCategory.Damage, "Demo World Stations",
            "Where to find the drive-in stations in the demo world and what each one does.",
            c => "" + ("Drive into the marked zones in the demo scene: GAS STATIONS refuel the tank over time, REPAIR STATIONS fix all damage instantly, and CUSTOMIZATION STATIONS open the mod shop.")));

        entries.Add(R("world-spike-strip", RCCP_FeatureLabCategory.Damage, "Spike Strips & Flat Tires",
            "What spike strips do to your tires and how to recover from a flat.",
            c => "" + ("Spike strips puncture any tire that rolls over them — the wheel deflates and loses grip. Re-inflate with the Inflate Tires control in the Wheels category, or press Repair Vehicle (repair also re-inflates all tires).")));

    }

}
