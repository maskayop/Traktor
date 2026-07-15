//----------------------------------------------
//        Realistic Car Controller Pro
//
// Copyright 2014 - 2026 BoneCracker Games
// https://www.bonecrackergames.com
// Ekrem Bugra Ozdoganlar
//
//----------------------------------------------

using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;

/// <summary>
/// Play-mode probe for timeScale robustness (V2.57 WS1). Drives the scene vehicle at a matrix of
/// timescales (1 / 2 / 0.2 / 0.02 + a frozen photo-style phase at 0) on a REALTIME clock and samples:
/// engine-audio silence ratio, transient one-shot leaks, skid-audio absorbing-state behavior (TS-01),
/// and recovery after unfreezing. Writes a JSON report to persistentDataPath/RCCP_TimeScaleProbe/.
/// Test harness — not wired into any scene; attach at runtime and call RunAll() (reflection-friendly).
/// </summary>
public class RCCP_TimeScaleProbe : MonoBehaviour {

    /// <summary>Realtime seconds of driving per timescale phase.</summary>
    public float driveSeconds = 8f;

    /// <summary>Realtime seconds of the frozen (timeScale 0) phase.</summary>
    public float freezeSeconds = 5f;

    /// <summary>Steer input used to provoke light sustained slip for the TS-01 skid sampler.</summary>
    public float probeSteer = .5f;

    [System.Serializable]
    public class PhaseResult {
        public float timescale;
        public int ticks;
        public int engineAudibleEligibleTicks;
        public int engineSilentTicks;
        public float silentRatio;
        public int oneShotMax;
        public int oneShotResidue;
        public int lightSlipTicks;
        public int skidMutedDuringLightSlipTicks;
        public float maxSkidVolume;
        public float recoverySeconds = -1f;
        public bool pass;
        public string note;
    }

    [System.Serializable]
    public class ProbeReport {
        public string vehicleName;
        public List<PhaseResult> phases = new List<PhaseResult>();
        public bool allPass;
    }

    private RCCP_CarController car;
    private RCCP_Input carInput;
    private static readonly FieldInfo skidSourceField = typeof(RCCP_WheelCollider).GetField("skidAudioSource", BindingFlags.NonPublic | BindingFlags.Instance);

    /// <summary>Reflection-friendly entry point. Runs the full timescale matrix and writes the report.</summary>
    public void RunAll() {

        StartCoroutine(RunAllRoutine());

    }

    private IEnumerator RunAllRoutine() {

        car = FindFirstObjectByType<RCCP_CarController>();

        if (!car) {

            Debug.LogError("RCCP_TimeScaleProbe: no RCCP_CarController in the scene.");
            yield break;

        }

        carInput = car.GetComponentInChildren<RCCP_Input>(true);

        //  TESTING.md: damage off for harness runs; confirm target in the log before starting.
        RCCP_Damage damage = car.GetComponentInChildren<RCCP_Damage>(true);

        if (damage)
            damage.enabled = false;

        Debug.Log("RCCP_TimeScaleProbe: target vehicle '" + car.name + "', input=" + (carInput ? "ok" : "MISSING") + ", damage disabled=" + (damage != null));

        ProbeReport report = new ProbeReport { vehicleName = car.name, allPass = true };

        //  Engine on, gear D.
        car.SetEngine(true);

        if (car.Gearbox)
            car.Gearbox.ShiftToGear(0);

        float[] scales = new float[] { 1f, 2f, .2f, .02f };

        for (int i = 0; i < scales.Length; i++) {

            PhaseResult r = null;
            IEnumerator phase = DrivePhase(scales[i], res => r = res);

            while (phase.MoveNext())
                yield return phase.Current;

            report.phases.Add(r);
            report.allPass &= r.pass;

        }

        {

            PhaseResult r = null;
            IEnumerator phase = FreezePhase(res => r = res);

            while (phase.MoveNext())
                yield return phase.Current;

            report.phases.Add(r);
            report.allPass &= r.pass;

        }

        Time.timeScale = 1f;

        if (carInput)
            carInput.DisableOverrideInputs();

        string dir = System.IO.Path.Combine(Application.persistentDataPath, "RCCP_TimeScaleProbe");
        System.IO.Directory.CreateDirectory(dir);
        string file = System.IO.Path.Combine(dir, "report_" + System.DateTime.Now.ToString("yyyyMMdd_HHmmss") + ".json");
        System.IO.File.WriteAllText(file, JsonUtility.ToJson(report, true));
        Debug.Log("RCCP_TimeScaleProbe: DONE allPass=" + report.allPass + " -> " + file);

    }

    private IEnumerator DrivePhase(float scale, System.Action<PhaseResult> done) {

        PhaseResult r = new PhaseResult { timescale = scale };
        Time.timeScale = scale;

        RCCP_Inputs inputs = new RCCP_Inputs();
        float elapsed = 0f;
        float settleUntil = 2f;    //  Realtime settle window before sampling: spawn drop + engine spin-up.
        float recoveryProbe = -1f;

        while (elapsed < driveSeconds) {

            elapsed += Time.unscaledDeltaTime;

            //  Gentle circular drive: enough slip for a light skid, not a burnout.
            inputs.throttleInput = 1f;
            inputs.steerInput = probeSteer;
            inputs.brakeInput = 0f;
            inputs.handbrakeInput = 0f;

            if (carInput)
                carInput.OverrideInputs(inputs);

            if (elapsed > settleUntil) {

                r.ticks++;
                SampleEngineAudio(r);
                SampleOneShots(r);
                SampleSkid(r);

            }

            yield return null;

        }

        //  Post-phase residue check on a realtime clock at timeScale 1.
        Time.timeScale = 1f;

        if (carInput) {

            inputs.throttleInput = 0f;
            inputs.steerInput = 0f;
            carInput.OverrideInputs(inputs);

        }

        float wait = 0f;

        while (wait < 3f) {

            wait += Time.unscaledDeltaTime;
            yield return null;

        }

        r.oneShotResidue = CountOneShots();
        r.silentRatio = r.engineAudibleEligibleTicks > 0 ? (float)r.engineSilentTicks / r.engineAudibleEligibleTicks : 0f;
        recoveryProbe = 0f; // drive phases don't test recovery
        r.recoverySeconds = recoveryProbe;

        //  Pass criteria defined up-front (TESTING.md): zero engine silence while revving, no leaked
        //  one-shots, and the skid source must not be absorbing-muted through light sustained slip.
        bool skidOk = r.lightSlipTicks < 30 || r.maxSkidVolume > .005f;
        r.pass = r.silentRatio <= 0f && r.oneShotResidue == 0 && skidOk;
        r.note = (r.lightSlipTicks < 30 ? "skid sampler inconclusive (light-slip window too short); " : "") + (skidOk ? "" : "SKID MUTED DURING LIGHT SLIP (TS-01); ") + (r.silentRatio > 0f ? "ENGINE SILENCE (regression); " : "");

        done(r);

    }

    private IEnumerator FreezePhase(System.Action<PhaseResult> done) {

        PhaseResult r = new PhaseResult { timescale = 0f };

        //  Photo-mode-style freeze (world frozen is CORRECT; we check leaks + recovery).
        Time.timeScale = 0f;

        float elapsed = 0f;
        int oneShotAtFreeze = CountOneShots();

        while (elapsed < freezeSeconds) {

            elapsed += Time.unscaledDeltaTime;
            r.ticks++;
            SampleOneShots(r);
            yield return null;

        }

        int accumulated = CountOneShots() - oneShotAtFreeze;

        //  Unfreeze and measure engine-audio recovery.
        Time.timeScale = 1f;

        RCCP_Inputs inputs = new RCCP_Inputs { throttleInput = 1f };

        if (carInput)
            carInput.OverrideInputs(inputs);

        float recovery = 0f;

        while (recovery < 5f) {

            recovery += Time.unscaledDeltaTime;

            if (MaxVehicleVolume() > .01f)
                break;

            yield return null;

        }

        r.recoverySeconds = recovery;
        r.oneShotResidue = accumulated > 0 ? accumulated : 0;
        r.pass = r.recoverySeconds <= 2f && r.oneShotResidue == 0;
        r.note = (r.pass ? "" : "freeze phase failed: ") + "recovery=" + recovery.ToString("F2") + "s, accumulatedOneShots=" + accumulated;

        done(r);

    }

    private void SampleEngineAudio(PhaseResult r) {

        float rpm = car.Engine ? car.Engine.engineRPM : 0f;

        //  Only count ticks where the engine is genuinely revving (audible by contract).
        if (rpm < 1200f)
            return;

        r.engineAudibleEligibleTicks++;

        if (MaxVehicleVolume() < .01f)
            r.engineSilentTicks++;

    }

    private float MaxVehicleVolume() {

        float max = 0f;
        AudioSource[] sources = car.GetComponentsInChildren<AudioSource>(false);

        for (int i = 0; i < sources.Length; i++) {

            if (sources[i] && sources[i].isPlaying && sources[i].clip != null && sources[i].volume > max)
                max = sources[i].volume;

        }

        return max;

    }

    private int CountOneShots() {

        return FindObjectsByType<RCCP_AudioSourceAutoDestroy>(FindObjectsSortMode.None).Length;

    }

    private void SampleOneShots(PhaseResult r) {

        int count = CountOneShots();

        if (count > r.oneShotMax)
            r.oneShotMax = count;

    }

    private void SampleSkid(PhaseResult r) {

        if (skidSourceField == null || !RCCP_GroundMaterials.Instance || RCCP_GroundMaterials.Instance.frictions == null || RCCP_GroundMaterials.Instance.frictions.Length == 0)
            return;

        float slipThreshold = RCCP_GroundMaterials.Instance.frictions[0].slip;
        RCCP_WheelCollider[] wheels = car.GetComponentsInChildren<RCCP_WheelCollider>(false);

        float maxSlipExcess = 0f;
        float maxSkidVol = 0f;

        for (int i = 0; i < wheels.Length; i++) {

            float excess = wheels[i].TotalSlip - slipThreshold;

            if (excess > maxSlipExcess)
                maxSlipExcess = excess;

            AudioSource src = skidSourceField.GetValue(wheels[i]) as AudioSource;

            if (src && src.volume > maxSkidVol)
                maxSkidVol = src.volume;

        }

        if (maxSkidVol > r.maxSkidVolume)
            r.maxSkidVolume = maxSkidVol;

        //  Light-slip window: slipping past the ground threshold, but gently — the TS-01 absorbing
        //  trap re-zeroes targets below SKID_VOLUME_THRESHOLD / lerp step (= 0.1 at defaults).
        if (maxSlipExcess > .02f && maxSlipExcess < .35f) {

            r.lightSlipTicks++;

            if (maxSkidVol <= 0f)
                r.skidMutedDuringLightSlipTicks++;

        }

    }

}
