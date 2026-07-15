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

    static partial void BuildAudioVfx(List<RCCP_FeatureLabEntry> entries) {

        //  None — each layer's maxVolume is read every frame in RCCP_Audio.Engine(); live-safe.
        //  Proportional write: layer 0 is the master reference; the other layers scale by the same ratio so the
        //  authored per-layer mix (e.g. M3_E46 0.65/0.75/0.75/0.65) is preserved. Uniform fallback when rising from zero.
        entries.Add(S("audio-engine-volume", RCCP_FeatureLabCategory.AudioVfx, "Engine Volume",
            "How loud the engine sounds. Slide to zero to silence the motor while keeping every other vehicle sound playing. Other engine sound layers scale proportionally.",
            0f, 1f,
            c => (c.V.Audio.engineSounds[0].maxVolume),
            (c, x) => {

                RCCP_Audio.EngineSound[] sounds = c.V.Audio.engineSounds;

                if (sounds == null || sounds.Length == 0 || sounds[0] == null)
                    return;

                float current = sounds[0].maxVolume;

                if (current < .001f) {

                    for (int i = 0; i < sounds.Length; i++) { if (sounds[i] != null) sounds[i].maxVolume = x; }

                } else {

                    float ratio = x / current;

                    for (int i = 0; i < sounds.Length; i++) { if (sounds[i] != null) sounds[i].maxVolume = Mathf.Clamp01(sounds[i].maxVolume * ratio); }

                }

            }, format: "0.00",
            avail: c => (c.V.Audio != null && c.V.Audio.engineSounds != null && c.V.Audio.engineSounds.Length > 0 && c.V.Audio.engineSounds[0] != null), availReason: "RCCP_Audio is an optional child component (lazy TryGetComponentInChildren, null when absent) and engineSounds can be empty on custom vehicles."));

        //  None — OnDisable re-collects and deactivates all child AudioSource GameObjects (snapshotting states); OnEnable restores them.
        entries.Add(T("audio-vehicle-sound", RCCP_FeatureLabCategory.AudioVfx, "Vehicle Sound",
            "Master switch for this vehicle's sounds. Off silences engine, wind, brake and crash audio; on restores everything exactly as it was.",
            c => (c.V.Audio.enabled),
            (c, x) => { c.V.Audio.enabled = x; },
            avail: c => (c.V.Audio != null), availReason: "RCCP_Audio is an optional child component; null on audio-less vehicles."));

        //  None — flameOnCutOff is read every Update() in Flame(); live-safe.
        //  Needs a vehicle with RCCP_Exhaust components; rev past ~5000 RPM then release throttle to see pops.
        entries.Add(T("fx-exhaust-flames", RCCP_FeatureLabCategory.AudioVfx, "Exhaust Flames",
            "Pops flames from the exhaust when you lift off the throttle at high revs. Flames are red normally and blend to blue while nitro is boosting.",
            c => (c.V.OtherAddonsManager.Exhausts.Exhaust[0].flameOnCutOff),
            (c, x) => { foreach (RCCP_Exhaust ex in c.V.OtherAddonsManager.Exhausts.Exhaust) { if (ex != null) ex.flameOnCutOff = x; }; },
            avail: c => (c.V.OtherAddonsManager != null && c.V.OtherAddonsManager.Exhausts != null && c.V.OtherAddonsManager.Exhausts.Exhaust != null && c.V.OtherAddonsManager.Exhausts.Exhaust.Length > 0), availReason: "Exhausts live under the optional OtherAddons manager; RCCP_Exhausts caches its RCCP_Exhaust[] in Awake (GetAllExhausts) — vehicles without exhausts yield null manager or empty array."));

        //  None — read per-frame in Flame(); live-safe.
        entries.Add(S("fx-backfire-min-rpm", RCCP_FeatureLabCategory.AudioVfx, "Backfire Min RPM",
            "The engine speed where lift-off flames start appearing. Lower it to get pops from gentler revs; raise it so only hard redline lifts spit fire.",
            1000f, 8000f,
            c => (c.V.OtherAddonsManager.Exhausts.Exhaust[0].backfireMinRPM),
            (c, x) => { foreach (RCCP_Exhaust ex in c.V.OtherAddonsManager.Exhausts.Exhaust) { if (ex != null) ex.backfireMinRPM = x; }; }, unit: "RPM", format: "0",
            avail: c => (c.V.OtherAddonsManager != null && c.V.OtherAddonsManager.Exhausts != null && c.V.OtherAddonsManager.Exhausts.Exhaust != null && c.V.OtherAddonsManager.Exhausts.Exhaust.Length > 0), availReason: "Same exhaust-enumeration chain as the flames toggle; empty on exhaust-less vehicles."));

        //  None — read per-frame in Flame(); live-safe.
        entries.Add(S("fx-backfire-max-rpm", RCCP_FeatureLabCategory.AudioVfx, "Backfire Max RPM",
            "The engine speed where lift-off flames stop. Raise it toward redline so flames keep popping all the way to the limiter.",
            1000f, 9000f,
            c => (c.V.OtherAddonsManager.Exhausts.Exhaust[0].backfireMaxRPM),
            (c, x) => { foreach (RCCP_Exhaust ex in c.V.OtherAddonsManager.Exhausts.Exhaust) { if (ex != null) ex.backfireMaxRPM = x; }; }, unit: "RPM", format: "0",
            avail: c => (c.V.OtherAddonsManager != null && c.V.OtherAddonsManager.Exhausts != null && c.V.OtherAddonsManager.Exhausts.Exhaust != null && c.V.OtherAddonsManager.Exhausts.Exhaust.Length > 0), availReason: "Same exhaust-enumeration chain as the flames toggle; empty on exhaust-less vehicles."));

        //  One-shot; nothing to re-apply.
        entries.Add(A("fx-clean-skidmarks", RCCP_FeatureLabCategory.AudioVfx, "Clean Skidmarks",
            "Instantly erases every tire mark drawn on the road. Handy after a burnout session to start over with clean asphalt.",
            "Clean Skidmarks",
            c => { RCCP.CleanSkidmarks(); }));

        //  Reload() is folded into setExpr on disable — REQUIRED: disabling only stops LateUpdate and would leave blur meshes frozen visible at their last intensity.
        entries.Add(T("fx-wheel-blur", RCCP_FeatureLabCategory.AudioVfx, "Wheel Blur",
            "Shows a soft motion-blur disc over the wheels once they spin fast. Turn off for crisp wheel rims at any speed.",
            c => (c.V.OtherAddonsManager.WheelBlur.enabled),
            (c, x) => { c.V.OtherAddonsManager.WheelBlur.enabled = x; if (!x) c.V.OtherAddonsManager.WheelBlur.Reload(); },
            avail: c => (c.V.OtherAddonsManager != null && c.V.OtherAddonsManager.WheelBlur != null), availReason: "RCCP_WheelBlur is an optional addon resolved lazily via OtherAddonsManager; null when the vehicle has none."));

        //  None — read per-frame in LateUpdate; live-safe.
        //  Drive at speed (or burnout) so the blur disc is visible while sliding this.
        entries.Add(S("fx-wheel-blur-speed", RCCP_FeatureLabCategory.AudioVfx, "Wheel Blur Spin Speed",
            "How fast the blur texture spins compared to the real wheel. Higher looks more frantic; zero keeps the blur disc static.",
            0f, 5f,
            c => (c.V.OtherAddonsManager.WheelBlur.rotationSpeed),
            (c, x) => { c.V.OtherAddonsManager.WheelBlur.rotationSpeed = x; }, format: "0.00",
            avail: c => (c.V.OtherAddonsManager != null && c.V.OtherAddonsManager.WheelBlur != null), availReason: "Same optional RCCP_WheelBlur addon as the blur toggle."));

        //  N/A — read-only.
        entries.Add(R("fx-ground-surface", RCCP_FeatureLabCategory.AudioVfx, "Ground Surface",
            "Live readout of what the tires are rolling on right now — asphalt, grass, sand and so on. Drive across different terrain to watch it change.",
            c => "" + (RCCP_RuntimeSettings.RCCPGroundMaterialsInstance.frictions[Mathf.Clamp(c.V.AllWheelColliders[0].groundIndex, 0, RCCP_RuntimeSettings.RCCPGroundMaterialsInstance.frictions.Length - 1)].groundMaterial != null ? RCCP_RuntimeSettings.RCCPGroundMaterialsInstance.frictions[Mathf.Clamp(c.V.AllWheelColliders[0].groundIndex, 0, RCCP_RuntimeSettings.RCCPGroundMaterialsInstance.frictions.Length - 1)].groundMaterial.name : ("Surface " + c.V.AllWheelColliders[0].groundIndex)),
            avail: c => (c.V.AllWheelColliders != null && c.V.AllWheelColliders.Length > 0 && RCCP_RuntimeSettings.RCCPGroundMaterialsInstance != null && RCCP_RuntimeSettings.RCCPGroundMaterialsInstance.frictions != null && RCCP_RuntimeSettings.RCCPGroundMaterialsInstance.frictions.Length > 0), availReason: "Needs at least one RCCP_WheelCollider and the ground-materials runtime clone with a non-empty frictions array; either can be absent on malformed setups."));

    }

}
