//----------------------------------------------
//        Realistic Car Controller Pro
//
// Copyright © 2014 - 2026 BoneCracker Games
// https://www.bonecrackergames.com
// Ekrem Bugra Ozdoganlar
//
//----------------------------------------------

using System;
using UnityEngine;

/// <summary>
/// Feature Lab category. Order defines the rail order in the UI.
/// </summary>
public enum RCCP_FeatureLabCategory {

    Assists = 0,
    Engine = 1,
    Drivetrain = 2,
    PhysicsAero = 3,
    Wheels = 4,
    Camera = 5,
    Lights = 6,
    AudioVfx = 7,
    Damage = 8,
    Systems = 9

}

/// <summary>
/// One live-tunable feature in the Feature Lab catalog. Concrete subclasses carry typed
/// getter / setter lambdas; setters bake in any re-apply call the RCCP API needs
/// (RecomputeInertia, SetSuspensionForces, ResetCamera, ...).
/// </summary>
public abstract class RCCP_FeatureLabEntry {

    /// <summary>Stable kebab-case id. Used as the default-snapshot dictionary key.</summary>
    public string id;

    /// <summary>Display name shown on the row.</summary>
    public string name;

    /// <summary>1-2 line plain-English description shown on the expanded card.</summary>
    public string description;

    public RCCP_FeatureLabCategory category;

    /// <summary>Optional "best tried with: X" chip text. Empty = no chip.</summary>
    public string vehicleHint = "";

    /// <summary>Shown on the greyed card when availability returns false.</summary>
    public string availabilityReason = "";

    /// <summary>Null = always available. Runs with a valid context but possibly-missing sub-components.</summary>
    public Func<RCCP_FeatureLabContext, bool> availability;

    public bool IsAvailable(RCCP_FeatureLabContext ctx) {

        if (ctx == null || ctx.V == null)
            return false;

        if (availability == null)
            return true;

        try {
            return availability(ctx);
        } catch {
            return false;
        }

    }

    /// <summary>Snapshot the current value for the reset system. Null = not resettable (actions, readouts).</summary>
    public abstract object CaptureValue(RCCP_FeatureLabContext ctx);

    /// <summary>Write a snapshot back through the setter.</summary>
    public abstract void RestoreValue(RCCP_FeatureLabContext ctx, object value);

}

public sealed class RCCP_FeatureLabToggle : RCCP_FeatureLabEntry {

    public Func<RCCP_FeatureLabContext, bool> get;
    public Action<RCCP_FeatureLabContext, bool> set;

    public override object CaptureValue(RCCP_FeatureLabContext ctx) { return get(ctx); }

    public override void RestoreValue(RCCP_FeatureLabContext ctx, object value) { set(ctx, (bool)value); }

}

public sealed class RCCP_FeatureLabSlider : RCCP_FeatureLabEntry {

    public Func<RCCP_FeatureLabContext, float> get;
    public Action<RCCP_FeatureLabContext, float> set;
    public float min;
    public float max;

    /// <summary>Unit suffix shown after the value ("km/h", "Hz", "°"...). Empty = none.</summary>
    public string unit = "";

    /// <summary>float.ToString format for the value label.</summary>
    public string format = "0.##";

    public override object CaptureValue(RCCP_FeatureLabContext ctx) { return get(ctx); }

    public override void RestoreValue(RCCP_FeatureLabContext ctx, object value) { set(ctx, (float)value); }

}

public sealed class RCCP_FeatureLabEnum : RCCP_FeatureLabEntry {

    /// <summary>Display labels in index order. set/get speak indices into this array.</summary>
    public string[] labels;

    public Func<RCCP_FeatureLabContext, int> get;
    public Action<RCCP_FeatureLabContext, int> set;

    public override object CaptureValue(RCCP_FeatureLabContext ctx) { return get(ctx); }

    public override void RestoreValue(RCCP_FeatureLabContext ctx, object value) { set(ctx, (int)value); }

}

public sealed class RCCP_FeatureLabAction : RCCP_FeatureLabEntry {

    /// <summary>Button caption ("Repair", "Capture Photo"...).</summary>
    public string buttonLabel = "Run";

    public Action<RCCP_FeatureLabContext> invoke;

    /// <summary>Optional status lamp text next to the button ("ARMED", "Recording"...). Null = no lamp.</summary>
    public Func<RCCP_FeatureLabContext, string> status;

    public override object CaptureValue(RCCP_FeatureLabContext ctx) { return null; }

    public override void RestoreValue(RCCP_FeatureLabContext ctx, object value) { }

}

public sealed class RCCP_FeatureLabReadout : RCCP_FeatureLabEntry {

    public Func<RCCP_FeatureLabContext, string> read;

    public override object CaptureValue(RCCP_FeatureLabContext ctx) { return null; }

    public override void RestoreValue(RCCP_FeatureLabContext ctx, object value) { }

}
