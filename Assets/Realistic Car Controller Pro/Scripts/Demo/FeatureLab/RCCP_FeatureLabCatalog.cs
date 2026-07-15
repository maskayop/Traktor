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

/// <summary>
/// The Feature Lab catalog. Each category lives in its own partial file
/// (RCCP_FeatureLabCatalog.Assists.cs etc). Adding a feature in a future RCCP
/// version = one factory call in the matching partial.
/// </summary>
public static partial class RCCP_FeatureLabCatalog {

    public static List<RCCP_FeatureLabEntry> Build() {

        List<RCCP_FeatureLabEntry> entries = new List<RCCP_FeatureLabEntry>(96);

        BuildAssists(entries);
        BuildEngine(entries);
        BuildDrivetrain(entries);
        BuildPhysicsAero(entries);
        BuildWheels(entries);
        BuildCamera(entries);
        BuildLights(entries);
        BuildAudioVfx(entries);
        BuildDamage(entries);
        BuildSystems(entries);

        return entries;

    }

    //  Factory helpers. Tasks 4-13 use these verbatim — keep signatures stable.

    internal static RCCP_FeatureLabToggle T(string id, RCCP_FeatureLabCategory cat, string name, string desc,
        Func<RCCP_FeatureLabContext, bool> get, Action<RCCP_FeatureLabContext, bool> set,
        Func<RCCP_FeatureLabContext, bool> avail = null, string availReason = "", string hint = "") {

        return new RCCP_FeatureLabToggle { id = id, category = cat, name = name, description = desc, get = get, set = set, availability = avail, availabilityReason = availReason, vehicleHint = hint };

    }

    internal static RCCP_FeatureLabSlider S(string id, RCCP_FeatureLabCategory cat, string name, string desc,
        float min, float max, Func<RCCP_FeatureLabContext, float> get, Action<RCCP_FeatureLabContext, float> set,
        string unit = "", string format = "0.##",
        Func<RCCP_FeatureLabContext, bool> avail = null, string availReason = "", string hint = "") {

        return new RCCP_FeatureLabSlider { id = id, category = cat, name = name, description = desc, min = min, max = max, get = get, set = set, unit = unit, format = format, availability = avail, availabilityReason = availReason, vehicleHint = hint };

    }

    internal static RCCP_FeatureLabEnum E(string id, RCCP_FeatureLabCategory cat, string name, string desc,
        string[] labels, Func<RCCP_FeatureLabContext, int> get, Action<RCCP_FeatureLabContext, int> set,
        Func<RCCP_FeatureLabContext, bool> avail = null, string availReason = "", string hint = "") {

        return new RCCP_FeatureLabEnum { id = id, category = cat, name = name, description = desc, labels = labels, get = get, set = set, availability = avail, availabilityReason = availReason, vehicleHint = hint };

    }

    internal static RCCP_FeatureLabAction A(string id, RCCP_FeatureLabCategory cat, string name, string desc,
        string buttonLabel, Action<RCCP_FeatureLabContext> invoke, Func<RCCP_FeatureLabContext, string> status = null,
        Func<RCCP_FeatureLabContext, bool> avail = null, string availReason = "", string hint = "") {

        return new RCCP_FeatureLabAction { id = id, category = cat, name = name, description = desc, buttonLabel = buttonLabel, invoke = invoke, status = status, availability = avail, availabilityReason = availReason, vehicleHint = hint };

    }

    internal static RCCP_FeatureLabReadout R(string id, RCCP_FeatureLabCategory cat, string name, string desc,
        Func<RCCP_FeatureLabContext, string> read,
        Func<RCCP_FeatureLabContext, bool> avail = null, string availReason = "", string hint = "") {

        return new RCCP_FeatureLabReadout { id = id, category = cat, name = name, description = desc, read = read, availability = avail, availabilityReason = availReason, vehicleHint = hint };

    }

    //  Category partials. Each Task 4-13 file implements exactly one of these.

    static partial void BuildAssists(List<RCCP_FeatureLabEntry> entries);
    static partial void BuildEngine(List<RCCP_FeatureLabEntry> entries);
    static partial void BuildDrivetrain(List<RCCP_FeatureLabEntry> entries);
    static partial void BuildPhysicsAero(List<RCCP_FeatureLabEntry> entries);
    static partial void BuildWheels(List<RCCP_FeatureLabEntry> entries);
    static partial void BuildCamera(List<RCCP_FeatureLabEntry> entries);
    static partial void BuildLights(List<RCCP_FeatureLabEntry> entries);
    static partial void BuildAudioVfx(List<RCCP_FeatureLabEntry> entries);
    static partial void BuildDamage(List<RCCP_FeatureLabEntry> entries);
    static partial void BuildSystems(List<RCCP_FeatureLabEntry> entries);

}
