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

    static partial void BuildLights(List<RCCP_FeatureLabEntry> entries) {

        //  none — each RCCP_Light reads the flag every frame and lerps its intensity
        //  Any RCCP demo vehicle with the Lights addon (all shipped demo vehicles have full light sets).
        entries.Add(T("lights-low-beam", RCCP_FeatureLabCategory.Lights, "Low Beam Headlights",
            "Turns the headlights on or off. Drive into the tunnel with these on to see the beams light up the road.",
            c => (c.V.Lights.lowBeamHeadlights),
            (c, x) => { c.V.Lights.lowBeamHeadlights = x; },
            avail: c => (c.V.Lights != null), availReason: "RCCP_Lights is an optional addon; RCCP_MainComponent.Lights lazily resolves via GetComponentInChildren and returns null when the vehicle has no lights manager."));

        //  none — read per-frame by each RCCP_Light high-beam branch
        //  Vehicle with a dedicated Headlight_HighBeam RCCP_Light for a visible difference.
        entries.Add(T("lights-high-beam", RCCP_FeatureLabCategory.Lights, "High Beam Headlights",
            "Switches the brighter long-range beams on or off. Best seen at night or inside the tunnel.",
            c => (c.V.Lights.highBeamHeadlights),
            (c, x) => { c.V.Lights.highBeamHeadlights = x; },
            avail: c => (c.V.Lights != null), availReason: "Lights manager is optional; null when the vehicle has no RCCP_Lights component."));

        //  none — read per-frame by IndicatorLeftLight RCCP_Lights; blink timer runs automatically while any indicator bool is true
        entries.Add(T("lights-indicator-left", RCCP_FeatureLabCategory.Lights, "Left Turn Signal",
            "Blinks the left turn signals once per second. Turning it on switches off the right signal and hazards, like a real car stalk.",
            c => (c.V.Lights.indicatorsLeft),
            (c, x) => { c.V.Lights.indicatorsLeft = x; if (x) { c.V.Lights.indicatorsRight = false; c.V.Lights.indicatorsAll = false; }; },
            avail: c => (c.V.Lights != null), availReason: "Lights manager is optional; null when the vehicle has no RCCP_Lights component.", hint: "Any demo vehicle with indicator lights."));

        //  none — read per-frame by IndicatorRightLight RCCP_Lights
        entries.Add(T("lights-indicator-right", RCCP_FeatureLabCategory.Lights, "Right Turn Signal",
            "Blinks the right turn signals once per second. Turning it on switches off the left signal and hazards.",
            c => (c.V.Lights.indicatorsRight),
            (c, x) => { c.V.Lights.indicatorsRight = x; if (x) { c.V.Lights.indicatorsLeft = false; c.V.Lights.indicatorsAll = false; }; },
            avail: c => (c.V.Lights != null), availReason: "Lights manager is optional; null when the vehicle has no RCCP_Lights component.", hint: "Any demo vehicle with indicator lights."));

        //  none — each RCCP_Light indicator branch checks (side bool || indicatorsAll) per frame
        entries.Add(T("lights-hazards", RCCP_FeatureLabCategory.Lights, "Hazard Lights",
            "Flashes all turn signals on both sides at once, like pressing the red triangle button. Turning it on cancels any single-side signal.",
            c => (c.V.Lights.indicatorsAll),
            (c, x) => { c.V.Lights.indicatorsAll = x; if (x) { c.V.Lights.indicatorsLeft = false; c.V.Lights.indicatorsRight = false; }; },
            avail: c => (c.V.Lights != null), availReason: "Lights manager is optional; null when the vehicle has no RCCP_Lights component.", hint: "Any demo vehicle with indicator lights."));

        //  n/a — readout
        //  Any vehicle — press brake or shift to reverse to see the lamps react.
        entries.Add(R("lights-brake-reverse-status", RCCP_FeatureLabCategory.Lights, "Brake / Reverse Lamps",
            "Shows whether the brake and reverse lights are currently lit. These follow the pedals and gearbox automatically and cannot be forced on.",
            c => "" + ("Brake: " + (c.V.Lights.brakeLights ? "ON" : "off") + "   Reverse: " + (c.V.Lights.reverseLights ? "ON" : "off")),
            avail: c => (c.V.Lights != null), availReason: "Lights manager is optional; null when the vehicle has no RCCP_Lights component."));

        //  none — checked per collision event on each light
        //  Vehicle with RCCP_Damage enabled plus lightDamage=true, so breakage is observable; crash front-first into a wall.
        entries.Add(T("lights-breakable", RCCP_FeatureLabCategory.Lights, "Breakable Lights",
            "When on, lights can shatter in hard crashes and stay dark until the car is repaired. Turn off to make every light crash-proof.",
            c => (c.V.Lights.lights.Exists(l => l != null && l.isBreakable)),
            (c, x) => { foreach (RCCP_Light l in c.V.Lights.lights) { if (l != null) l.isBreakable = x; }; },
            avail: c => (c.V.Lights != null && c.V.Lights.lights != null && c.V.Lights.lights.Count > 0), availReason: "Needs the lights manager AND at least one registered RCCP_Light; the lights list is auto-populated at runtime via RCCP_Component.Register (RCCP_Component.cs:262) and null-pruned by CheckLights() each Update."));

    }

}
