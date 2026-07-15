//----------------------------------------------
//        Realistic Car Controller Pro
//
// Copyright © 2014 - 2026 BoneCracker Games
// https://www.bonecrackergames.com
// Ekrem Bugra Ozdoganlar
//
//----------------------------------------------

using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Feature Lab manager. Owns the catalog lifecycle: rebinding on vehicle change,
/// default snapshots (captured AFTER the behavior-preset bake settles), the reset
/// system, and the 10 Hz UI refresh tick. UI layers subscribe to OnRebound / OnTick.
/// </summary>
public class RCCP_FeatureLab : MonoBehaviour {

    public static RCCP_FeatureLab Instance { get; private set; }

    [Tooltip("Scene anchor used by the Systems category's Respawn action. Wired by the scene (or the vehicle selector).")]
    public Transform spawnAnchor;

    [Tooltip("Scene anchor used by the Systems category's Teleport To Trailer action. Wired by the scene.")]
    public Transform trailerAnchor;

    public RCCP_FeatureLabContext Context { get; private set; }
    public List<RCCP_FeatureLabEntry> Entries { get; private set; }

    /// <summary>Full refresh signal: vehicle changed, behavior re-baked, defaults captured, or reset applied.</summary>
    public event Action OnRebound;

    /// <summary>~10 Hz on unscaled time while enabled and photo mode is inactive. Widgets refresh getters here.</summary>
    public event Action OnTick;

    private readonly Dictionary<string, object> capturedDefaults = new Dictionary<string, object>();
    private Coroutine captureRoutine;

    public bool HasVehicle {

        get {

            return Context != null && Context.V != null;

        }

    }

    public bool HasDefaultFor(string entryId) {

        return capturedDefaults.ContainsKey(entryId);

    }

    private void Awake() {

        if (Instance != null && Instance != this) {

            Destroy(gameObject);
            return;

        }

        Instance = this;
        Context = new RCCP_FeatureLabContext();
        Entries = RCCP_FeatureLabCatalog.Build();

    }

    private void OnEnable() {

        RCCP_Events.OnVehicleChangedToVehicle += OnVehicleChanged;
        RCCP_Events.OnBehaviorChanged += OnBehaviorChanged;

        StartCoroutine(TickLoop());

        if (HasVehicle)
            ScheduleCapture();

    }

    private void OnDisable() {

        RCCP_Events.OnVehicleChangedToVehicle -= OnVehicleChanged;
        RCCP_Events.OnBehaviorChanged -= OnBehaviorChanged;

        StopAllCoroutines();
        captureRoutine = null;

        //  The lab's time-scale entry (Systems category) is the only scaled-time writer besides
        //  photo mode. Photo mode restores its own cached value; we restore ours here.
        if (!RCCP_PhotoMode.IsActive && !Mathf.Approximately(Time.timeScale, 1f))
            Time.timeScale = 1f;

    }

    private void OnDestroy() {

        if (Instance == this)
            Instance = null;

    }

    private void OnVehicleChanged(RCCP_CarController newVehicle) {

        //  newVehicle is NULL on deregister — that's a valid state, not an error.
        capturedDefaults.Clear();

        if (newVehicle != null)
            ScheduleCapture();

        OnRebound?.Invoke();

    }

    private void OnBehaviorChanged() {

        if (isActiveAndEnabled)
            StartCoroutine(ReboundAfterBake());

    }

    private IEnumerator ReboundAfterBake() {

        //  CheckBehaviorDelayed runs one WaitForFixedUpdate after the event; wait two so
        //  the refresh reads post-bake component state.
        yield return new WaitForFixedUpdate();
        yield return new WaitForFixedUpdate();

        OnRebound?.Invoke();

    }

    private void ScheduleCapture() {

        if (captureRoutine != null)
            StopCoroutine(captureRoutine);

        captureRoutine = StartCoroutine(CaptureDefaults());

    }

    private IEnumerator CaptureDefaults() {

        //  Snapshot AFTER the behavior-preset bake settles, so Reset restores the
        //  as-configured vehicle, not mid-bake garbage.
        yield return new WaitForFixedUpdate();
        yield return new WaitForFixedUpdate();

        if (Context.V == null)
            yield break;

        for (int i = 0; i < Entries.Count; i++) {

            RCCP_FeatureLabEntry entry = Entries[i];

            if (!entry.IsAvailable(Context))
                continue;

            try {

                object value = entry.CaptureValue(Context);

                if (value != null)
                    capturedDefaults[entry.id] = value;

            } catch (Exception e) {

                Debug.LogWarning("RCCP_FeatureLab: default capture failed for '" + entry.id + "': " + e.Message);

            }

        }

        captureRoutine = null;
        OnRebound?.Invoke();

    }

    private IEnumerator TickLoop() {

        WaitForSecondsRealtime wait = new WaitForSecondsRealtime(.1f);

        while (true) {

            yield return wait;

            if (!RCCP_PhotoMode.IsActive)
                OnTick?.Invoke();

        }

    }

    public bool ResetEntry(RCCP_FeatureLabEntry entry) {

        if (entry == null || !HasVehicle)
            return false;

        object value;

        if (!capturedDefaults.TryGetValue(entry.id, out value))
            return false;

        try {

            entry.RestoreValue(Context, value);

        } catch (Exception e) {

            Debug.LogWarning("RCCP_FeatureLab: reset failed for '" + entry.id + "': " + e.Message);
            return false;

        }

        OnRebound?.Invoke();
        return true;

    }

    public int ResetCategory(RCCP_FeatureLabCategory category) {

        if (!HasVehicle)
            return 0;

        int count = 0;

        for (int i = 0; i < Entries.Count; i++) {

            RCCP_FeatureLabEntry entry = Entries[i];

            if (entry.category != category)
                continue;

            object value;

            if (!capturedDefaults.TryGetValue(entry.id, out value))
                continue;

            try {

                entry.RestoreValue(Context, value);
                count++;

            } catch (Exception e) {

                Debug.LogWarning("RCCP_FeatureLab: reset failed for '" + entry.id + "': " + e.Message);

            }

        }

        if (count > 0)
            OnRebound?.Invoke();

        return count;

    }

    public int ResetAll() {

        if (!HasVehicle)
            return 0;

        int count = 0;

        for (int i = 0; i < Entries.Count; i++) {

            RCCP_FeatureLabEntry entry = Entries[i];
            object value;

            if (!capturedDefaults.TryGetValue(entry.id, out value))
                continue;

            try {

                entry.RestoreValue(Context, value);
                count++;

            } catch (Exception e) {

                Debug.LogWarning("RCCP_FeatureLab: reset failed for '" + entry.id + "': " + e.Message);

            }

        }

        if (count > 0)
            OnRebound?.Invoke();

        return count;

    }

}
