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
using System.IO;
using UnityEngine;

/// <summary>
/// Play-mode harness: exercises every available catalog entry on the active vehicle
/// (toggle flip / slider nudge / enum cycle, then restore) and writes a JSON report to
/// persistentDataPath/RCCP_FeatureLab/. Attach at runtime and call RunAll().
/// Actions are NOT invoked unless invokeActions is set (side effects).
/// </summary>
public class RCCP_FeatureLabSelfTest : MonoBehaviour {

    [Serializable]
    public class EntryResult {

        public string id;
        public string kind;
        public string outcome;      //  pass | mismatch | skipped | unavailable | error
        public string detail;

    }

    [Serializable]
    public class Report {

        public string vehicle;
        public string timestamp;
        public int total;
        public int passed;
        public int mismatched;
        public int skipped;
        public int unavailable;
        public int errors;
        public List<EntryResult> results = new List<EntryResult>();

    }

    /// <summary>Invoke action entries too (Repair, photo capture, ...). Off by default — side effects.</summary>
    public bool invokeActions = false;

    /// <summary>Slider read-back tolerance as a fraction of the slider range.</summary>
    public float sliderTolerance = .075f;

    /// <summary>Entry ids whose write→read round-trip is legitimately latent (event-baked). Mismatch → pass-latent.</summary>
    public string[] latencyExemptIds = new string[] { "phys-behavior-preset", "phys-wheel-substeps", "engine-running" };

    public bool IsRunning { get; private set; }
    public string LastReportPath { get; private set; }

    public void RunAll() {

        if (!IsRunning)
            StartCoroutine(RunAllRoutine());

    }

    private IEnumerator RunAllRoutine() {

        IsRunning = true;

        RCCP_FeatureLab lab = RCCP_FeatureLab.Instance;

        if (lab == null) {

            GameObject go = new GameObject("RCCP_FeatureLab (SelfTest)");
            lab = go.AddComponent<RCCP_FeatureLab>();

        }

        //  Wait for a vehicle and for the default-capture pass to have had time to run.
        float deadline = Time.realtimeSinceStartup + 15f;

        while (!lab.HasVehicle && Time.realtimeSinceStartup < deadline)
            yield return null;

        yield return new WaitForFixedUpdate();
        yield return new WaitForFixedUpdate();
        yield return new WaitForFixedUpdate();

        Report report = new Report();
        report.vehicle = lab.HasVehicle ? lab.Context.V.name : "NO VEHICLE";
        report.timestamp = DateTime.Now.ToString("yyyy-MM-dd_HH-mm-ss");

        if (!lab.HasVehicle) {

            WriteReport(report);
            IsRunning = false;
            yield break;

        }

        for (int i = 0; i < lab.Entries.Count; i++) {

            RCCP_FeatureLabEntry entry = lab.Entries[i];
            EntryResult r = new EntryResult();
            r.id = entry.id;
            r.kind = entry.GetType().Name;

            if (!entry.IsAvailable(lab.Context)) {

                r.outcome = "unavailable";
                r.detail = entry.availabilityReason;

            } else {

                ExerciseEntry(lab.Context, entry, r);

            }

            report.results.Add(r);
            Tally(report, r.outcome);

            //  Let physics settle every few writes.
            if (i % 5 == 4)
                yield return new WaitForFixedUpdate();

        }

        report.total = report.results.Count;
        WriteReport(report);
        IsRunning = false;

    }

    private void ExerciseEntry(RCCP_FeatureLabContext ctx, RCCP_FeatureLabEntry entry, EntryResult r) {

        try {

            switch (entry) {

                case RCCP_FeatureLabToggle t: {

                    bool original = t.get(ctx);
                    t.set(ctx, !original);
                    bool readBack = t.get(ctx);
                    t.set(ctx, original);
                    r.outcome = readBack == !original ? "pass" : ResolveMismatch(entry.id, r, "wrote " + (!original) + " read " + readBack);
                    break;

                }

                case RCCP_FeatureLabSlider s: {

                    float original = s.get(ctx);
                    float range = s.max - s.min;
                    float target = Mathf.Abs(original - s.min) > range * .5f ? s.min : s.max;
                    s.set(ctx, target);
                    float readBack = s.get(ctx);
                    s.set(ctx, original);
                    r.outcome = Mathf.Abs(readBack - target) <= range * sliderTolerance ? "pass" : ResolveMismatch(entry.id, r, "wrote " + target + " read " + readBack);
                    break;

                }

                case RCCP_FeatureLabEnum e: {

                    int original = e.get(ctx);
                    int target = (original + 1) % e.labels.Length;
                    e.set(ctx, target);
                    int readBack = e.get(ctx);
                    e.set(ctx, original);
                    r.outcome = readBack == target ? "pass" : ResolveMismatch(entry.id, r, "wrote " + target + " read " + readBack);
                    break;

                }

                case RCCP_FeatureLabAction a: {

                    if (a.status != null)
                        a.status(ctx);   //  status lambda must not throw even when idle

                    if (invokeActions) {

                        a.invoke(ctx);
                        r.outcome = "pass";

                    } else {

                        r.outcome = "skipped";
                        r.detail = "action not invoked (invokeActions=false)";

                    }

                    break;

                }

                case RCCP_FeatureLabReadout ro: {

                    string text = ro.read(ctx);
                    r.outcome = text != null ? "pass" : "mismatch";
                    break;

                }

                default: {

                    r.outcome = "error";
                    r.detail = "unknown entry kind";
                    break;

                }

            }

        } catch (Exception e) {

            r.outcome = "error";
            r.detail = e.GetType().Name + ": " + e.Message;

        }

    }

    private string ResolveMismatch(string id, EntryResult r, string detail) {

        r.detail = detail;

        for (int i = 0; i < latencyExemptIds.Length; i++) {

            if (latencyExemptIds[i] == id)
                return "pass";

        }

        return "mismatch";

    }

    private void Tally(Report report, string outcome) {

        switch (outcome) {

            case "pass": report.passed++; break;
            case "mismatch": report.mismatched++; break;
            case "skipped": report.skipped++; break;
            case "unavailable": report.unavailable++; break;
            default: report.errors++; break;

        }

    }

    private void WriteReport(Report report) {

        string dir = Path.Combine(Application.persistentDataPath, "RCCP_FeatureLab");

        if (!Directory.Exists(dir))
            Directory.CreateDirectory(dir);

        string path = Path.Combine(dir, "selftest_" + report.vehicle.Replace(" ", "_") + "_" + report.timestamp + ".json");
        File.WriteAllText(path, JsonUtility.ToJson(report, true));
        LastReportPath = path;

        Debug.Log("RCCP_FeatureLabSelfTest: " + report.vehicle + " — total " + report.total + ", pass " + report.passed + ", mismatch " + report.mismatched + ", skipped " + report.skipped + ", unavailable " + report.unavailable + ", errors " + report.errors + " → " + path);

    }

}
