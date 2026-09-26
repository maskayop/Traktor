//----------------------------------------------
//        Realistic Car Controller Pro
//
// Copyright 2014 - 2025 BoneCracker Games
// https://www.bonecrackergames.com
// Ekrem Bugra Ozdoganlar
//
//----------------------------------------------

using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.Rendering;

/// <summary>
/// Manages exhaust smoke and flame effects. Can optionally produce a flame when the throttle is cut off (e.g., engine backfire) or under NOS boost.
/// </summary>
[AddComponentMenu("BoneCracker Games/Realistic Car Controller Pro/Misc/RCCP Exhaust")]
public class RCCP_Exhaust : RCCP_Component {

    /// <summary>
    /// Main camera used for calculating the intesity of the lensflare.
    /// </summary>
    private Camera mainCam;

    /// <summary>
    /// If true, triggers a flame effect when throttle is cut off at high RPM.
    /// </summary>
    [Header("Flame")]
    [Tooltip("Triggers a flame effect when throttle is cut off at high RPM (engine backfire).")]
    public bool flameOnCutOff = false;

    /// <summary>
    /// Pops flames and backfire audio while the engine's launch control is actively cutting fuel.
    /// Only relevant when launch control is enabled on RCCP_Engine (default off there).
    /// </summary>
    [Tooltip("Pops flames and backfire audio while the engine's launch control is actively cutting fuel.")]
    public bool flameOnLaunchControl = true;

    /// <summary>
    /// V2.51 (T1-17): RPM window in which throttle-cut backfire flames pop. Defaults (5000-5500) preserve
    /// prior hardcoded behavior; raise these to match a high-revving engine that previously never popped.
    /// </summary>
    [Tooltip("Lowest engine RPM at which throttle-cut backfire flames pop. Default 5000 = prior hardcoded value.")]
    public float backfireMinRPM = 5000f;

    [Tooltip("Highest engine RPM at which throttle-cut backfire flames pop. Default 5500 = prior hardcoded value.")]
    public float backfireMaxRPM = 5500f;

    /// <summary>
    /// Primary smoke ParticleSystem.
    /// </summary>
    private ParticleSystem particle;

    /// <summary>
    /// Emission module for the smoke ParticleSystem.
    /// </summary>
    private ParticleSystem.EmissionModule emission;

    /// <summary>
    /// Flame ParticleSystem for backfire or boost flame.
    /// </summary>
    [Tooltip("Particle system used for the backfire or boost flame effect.")]
    public ParticleSystem flame;

    /// <summary>
    /// Emission module for the flame ParticleSystem.
    /// </summary>
    private ParticleSystem.EmissionModule subEmission;

    /// <summary>
    /// Light component used to illuminate the flame effect.
    /// </summary>
    private Light flameLight;

#if !BCG_URP && !BCG_HDRP
    /// <summary>
    /// Optional LensFlare for the flame effect.
    /// </summary>
    private LensFlare lensFlare;
#else
    /// <summary>
    /// Optional LensFlare for the flame effect.
    /// </summary>
    private LensFlareComponentSRP lensFlare_SRP;
#endif

    /// <summary>
    /// Multiplier for flare brightness.
    /// </summary>
    [Header("Lens Flare")]
    [Min(0f), Tooltip("Multiplier applied to the lens flare brightness from the flame effect.")]
    public float flareBrightness = 1f;

    /// <summary>
    /// Final computed brightness for the LensFlare, based on camera angle and distance.
    /// </summary>
    [Min(0f)] private float finalFlareBrightness;

    /// <summary>
    /// Timer for how long the flame effect remains active when triggered.
    /// </summary>
    [Min(0f), Tooltip("Duration timer controlling how long the flame remains active after triggering.")]
    public float flameTime = 0f;

    /// <summary>
    /// Primary color of the flame effect (e.g., a typical orange/red backfire).
    /// </summary>
    [Tooltip("Primary color of the backfire flame (typically orange or red).")]
    public Color flameColor = Color.red;

    /// <summary>
    /// Alternate flame color when under NOS boost.
    /// </summary>
    [Tooltip("Flame color used when nitrous oxide boost is active.")]
    public Color boostFlameColor = Color.blue;

    /// <summary>
    /// If true, the flame light will be configured as a pixel light to cast dynamic lighting on the ground/bumper.
    /// </summary>
    [Header("Dynamic Flame Light")]
    [Tooltip("If true, the flame light will be configured as a pixel light to cast dynamic lighting on the ground/bumper.")]
    public bool usePixelLight = true;

    /// <summary>
    /// Range/radius of the point light.
    /// </summary>
    [Tooltip("Range/radius of the point light.")]
    public float flameLightRange = 3f;

    /// <summary>
    /// Multiplier applied to the light intensity (HDR).
    /// </summary>
    [Tooltip("Multiplier applied to the light intensity (HDR).")]
    public float flameLightIntensityMultiplier = 8f;

    /// <summary>
    /// Duration of the ignition flash in seconds.
    /// </summary>
    [Tooltip("Duration of the ignition flash in seconds.")]
    public float flameLightFlashDuration = 0.2f;

    /// <summary>
    /// Curve defining the light intensity over the flash duration.
    /// </summary>
    [Tooltip("Curve defining the light intensity over the flash duration.")]
    public AnimationCurve flameLightCurve = new AnimationCurve(new Keyframe(0f, 0f), new Keyframe(0.05f, 1f), new Keyframe(0.2f, 0f));

    /// <summary>
    /// Base intensity for sustained flame (e.g. NOS boost) after the initial flash.
    /// </summary>
    [Tooltip("Base intensity for sustained flame (e.g. NOS boost) after the initial flash.")]
    public float flameLightBaseIntensity = 0.3f;

    /// <summary>
    /// Flicker range for sustained flame light.
    /// </summary>
    [Tooltip("Flicker range for sustained flame light.")]
    public float flameLightFlicker = 0.1f;

    /// <summary>
    /// Flicker frequency/speed of Perlin noise for sustained flame light.
    /// </summary>
    [Tooltip("Flicker frequency/speed of Perlin noise for sustained flame light.")]
    public float flameLightFlickerFrequency = 15f;

    /// <summary>
    /// If true, a spatialized 3D pop sound will be played at the exhaust tip when a backfire/NOS ignition occurs.
    /// </summary>
    [Header("Audio")]
    [Tooltip("If true, a spatialized 3D pop sound will be played at the exhaust tip when a backfire/NOS ignition occurs.")]
    public bool playBackfireSound = true;

    /// <summary>
    /// Audio clips for the backfire pop. If empty, the clips from global RCCP Settings will be used.
    /// </summary>
    [Tooltip("Audio clips for the backfire pop. If empty, the clips from global RCCP Settings will be used.")]
    public AudioClip[] backfireClips;

    /// <summary>
    /// Volume level of the backfire sound.
    /// </summary>
    [Range(0f, 1f), Tooltip("Volume level of the backfire sound.")]
    public float backfireSoundVolume = 0.5f;

    /// <summary>
    /// Min distance for 3D sound attenuation.
    /// </summary>
    [Tooltip("Min distance for 3D sound attenuation.")]
    public float backfireSoundMinDistance = 1f;

    /// <summary>
    /// Max distance for 3D sound attenuation.
    /// </summary>
    [Tooltip("Max distance for 3D sound attenuation.")]
    public float backfireSoundMaxDistance = 15f;

    /// <summary>
    /// If true, a brief camera shake will be triggered on backfire pops and NOS ignition.
    /// </summary>
    [Header("Camera Shake")]
    [Tooltip("If true, a brief camera shake will be triggered on backfire pops and NOS ignition.")]
    public bool triggerCameraShake = true;

    /// <summary>
    /// Shake intensity multiplier for backfire pops.
    /// </summary>
    [Tooltip("Shake intensity multiplier for backfire pops.")]
    public float backfireShakeIntensity = 0.5f;

    /// <summary>
    /// Shake intensity multiplier for NOS ignition.
    /// </summary>
    [Tooltip("Shake intensity multiplier for NOS ignition.")]
    public float nosIgnitionShakeIntensity = 0.3f;

    /// <summary>
    /// Duration in seconds to smoothly transition from standard backfire color to NOS boost color on NOS ignition.
    /// </summary>
    [Header("NOS Color Transition")]
    [Tooltip("Duration in seconds to smoothly transition from standard backfire color to NOS boost color on NOS ignition.")]
    public float nosColorTransitionDuration = 0.25f;

    /// <summary>
    /// Minimum smoke emission rate.
    /// </summary>
    [Header("Smoke / Particles")]
    [Min(0f), Tooltip("Lowest smoke particle emission rate at idle throttle.")]
    public float minEmission = 5f;

    /// <summary>
    /// Maximum smoke emission rate.
    /// </summary>
    [Min(0f), Tooltip("Highest smoke particle emission rate at full throttle.")]
    public float maxEmission = 20f;

    /// <summary>
    /// Minimum smoke particle size.
    /// </summary>
    [Min(0f), Tooltip("Smallest smoke particle size at idle throttle.")]
    public float minSize = 1f;

    /// <summary>
    /// Maximum smoke particle size.
    /// </summary>
    [Min(0f), Tooltip("Largest smoke particle size at full throttle.")]
    public float maxSize = 4f;

    /// <summary>
    /// Minimum smoke particle speed.
    /// </summary>
    [Min(0f), Tooltip("Slowest initial velocity of smoke particles at idle throttle.")]
    public float minSpeed = .1f;

    /// <summary>
    /// Maximum smoke particle speed.
    /// </summary>
    [Min(0f), Tooltip("Fastest initial velocity of smoke particles at full throttle.")]
    public float maxSpeed = 1f;

    /// <summary>
    /// True if the flame (popping/backfire) is currently active.
    /// </summary>
    [Tooltip("True while the exhaust flame (backfire pop) is currently active.")]
    public bool popping = false;

    private float currentFlashTime = 0f;
    private bool wasPopping = false;
    private bool wasNosActive = false;

    private AudioSource audioSource;
    private float noiseOffset;
    private float nosActiveTime = 0f;

    public override void Start() {

        base.Start();

        // Get the main exhaust ParticleSystem.
        TryGetComponent(out particle);

        if (!particle) {

            Debug.LogError("No ParticleSystem found on this exhaust named " + transform.name + ", disabling this script!");
            enabled = false;
            return;

        }

        emission = particle.emission;

        // If a flame ParticleSystem is assigned, set up references.
        if (flame) {

            subEmission = flame.emission;
            flameLight = flame.GetComponentInChildren<Light>();

            // If a flame light exists, configure range and pixel rendering mode.
            if (flameLight) {
                flameLight.range = flameLightRange;
                if (usePixelLight)
                    flameLight.renderMode = LightRenderMode.ForcePixel;
                else
                    flameLight.renderMode = LightRenderMode.ForceVertex;
            }

        }

        InvokeRepeating(nameof(FindMainCamera), 0f, 1f);

        noiseOffset = UnityEngine.Random.Range(0f, 1000f);

        if (playBackfireSound) {

            audioSource = gameObject.AddComponent<AudioSource>();
            audioSource.playOnAwake = false;
            audioSource.spatialBlend = 1f; // Force 3D spatial blend
            audioSource.minDistance = backfireSoundMinDistance;
            audioSource.maxDistance = backfireSoundMaxDistance;
            audioSource.rolloffMode = AudioRolloffMode.Logarithmic;

            // Assign mixer group from RCCP_Audio if available
            if (CarController) {

                var rccpAudio = CarController.GetComponentInChildren<RCCP_Audio>();
                if (rccpAudio != null && rccpAudio.audioMixer != null) {

                    audioSource.outputAudioMixerGroup = rccpAudio.audioMixer;

                }

            }

            // If custom backfire clips are not assigned, pull from global RCCP_Settings
            if (backfireClips == null || backfireClips.Length == 0) {

                if (RCCP_Settings.Instance != null)
                    backfireClips = RCCP_Settings.Instance.exhaustFlameClips;

            }

        }

#if !BCG_URP && !BCG_HDRP
        // Attempt to find a LensFlare in this object�s children.
        lensFlare = GetComponentInChildren<LensFlare>();
#else
        // Attempt to find a LensFlare in this object�s children.
        lensFlare_SRP = GetComponentInChildren<LensFlareComponentSRP>();
#endif

        // Disable the built-in flare on the Light if it exists.
        if (flameLight && flameLight.flare != null)
            flameLight.flare = null;

    }

    /// <summary>Finds and caches the main camera reference for distance-based particle culling.</summary>
    public void FindMainCamera() {

        mainCam = Camera.main;

    }

    private void Update() {

        // If no ParticleSystem is found (or it was disabled), skip.
        if (!particle)
            return;

        // Keep the cached emission module bound to the LIVE ParticleSystem. Holding the EmissionModule
        // struct across a vehicle respawn/teardown can leave it referencing a destroyed system, which throws
        // "Do not create your own module instances" on the next .enabled access. Re-deriving from the
        // null-checked particle each frame is alloc-free and keeps the struct valid.
        emission = particle.emission;

        // If no Engine component found, disable exhaust emission.
        if (!CarController.Engine) {

            if (emission.enabled)
                emission.enabled = false;

            return;

        }

        Smoke();
        Flame();

        // Optional lens flare adjustments if present.
#if !BCG_URP && !BCG_HDRP
        // Built-in pipeline lens flare
        if (lensFlare)
            LensFlare();
#else
        // URP/HDRP pipeline lens flare
        if (lensFlare_SRP)
            LensFlare_SRP();
#endif

    }

    /// <summary>
    /// Manages the smoke particle emission, size, and speed based on engine state and throttle input.
    /// </summary>
    private void Smoke() {

        // Only emit smoke if the engine is running and the vehicle is below ~20 km/h. 
        if (CarController.Engine.engineRunning) {

            var main = particle.main;

            if (CarController.absoluteSpeed > 25f) {

                if (emission.enabled)
                    emission.enabled = false;

                return;

            }

            if (!emission.enabled)
                emission.enabled = true;

            emission.rateOverTime = Mathf.Clamp(maxEmission * CarController.throttleInput_V, minEmission, maxEmission);
            main.startSpeed = Mathf.Clamp(maxSpeed * CarController.throttleInput_V, minSpeed, maxSpeed);
            main.startSize = Mathf.Clamp(maxSize * CarController.throttleInput_V, minSize, maxSize);

        } else {

            if (emission.enabled)
                emission.enabled = false;

        }

    }

    /// <summary>
    /// Manages flame/backfire effects, switching color if NOS is in use.
    /// </summary>
    private void Flame() {

        // No flame system assigned -> nothing to do (smoke emission is managed by Smoke()). Guards against
        // dereferencing flame / its cached subEmission module after a respawn or when flame is unset.
        if (!flame)
            return;

        // Re-derive the flame's emission module from the live system for the same teardown-safety reason as
        // the smoke module above.
        subEmission = flame.emission;

        if (!CarController.Engine.engineRunning) {

            if (emission.enabled)
                emission.enabled = false;

            subEmission.enabled = false;

            if (flameLight)
                flameLight.intensity = 0f;

            wasPopping = false;
            wasNosActive = false;
            return;

        }

        var main = flame.main;

        // Reset flame timer if throttle is above ~25%.
        if (CarController.throttleInput_V >= .25f)
            flameTime = 0f;

        bool isNosActive = CarController.nosInput_V >= .75f;
        bool isBackfireActive = flameOnCutOff && (CarController.engineRPM >= backfireMinRPM && CarController.engineRPM <= backfireMaxRPM && CarController.throttleInput_V <= .25f && flameTime <= .5f);
        bool isLaunchPopActive = flameOnLaunchControl && CarController.Engine.launchControlActive && CarController.Engine.cutFuel;
        bool shouldPop = isBackfireActive || isNosActive || isLaunchPopActive;

        // Trigger ignition flash and play pop sound and camera shake
        if (shouldPop && (!wasPopping || (isNosActive && !wasNosActive))) {
            currentFlashTime = 0f;

            if (playBackfireSound && audioSource && backfireClips != null && backfireClips.Length > 0) {

                audioSource.clip = backfireClips[Random.Range(0, backfireClips.Length)];
                audioSource.volume = backfireSoundVolume;
                audioSource.pitch = Random.Range(0.85f, 1.15f); // Subtle pitch variation
                audioSource.Play();

            }

            if (triggerCameraShake) {

                float shakeIntensity = isNosActive ? nosIgnitionShakeIntensity : backfireShakeIntensity;
                TriggerCameraShake(shakeIntensity);

            }

            // Apply randomized flame speed multiplier on ignition
            main.startSpeedMultiplier = Random.Range(0.85f, 1.25f);
        }

        wasPopping = shouldPop;
        wasNosActive = isNosActive;

        if (isNosActive) {
            nosActiveTime += Time.deltaTime;
        } else {
            nosActiveTime = 0f;
        }

        if (shouldPop) {

            popping = true;
            flameTime += Time.deltaTime;
            subEmission.enabled = true;

            currentFlashTime += Time.deltaTime;

            float intensityMultiplier = 1f;
            if (currentFlashTime < flameLightFlashDuration) {
                intensityMultiplier = flameLightCurve.Evaluate(currentFlashTime / flameLightFlashDuration);
            } else {
                float noise = Mathf.PerlinNoise(Time.time * flameLightFlickerFrequency, noiseOffset);
                intensityMultiplier = flameLightBaseIntensity + (noise * 2f - 1f) * flameLightFlicker;
            }

            // Interpolate colors smoothly if NOS is active
            if (isNosActive) {

                float t = nosColorTransitionDuration > 0f ? Mathf.Clamp01(nosActiveTime / nosColorTransitionDuration) : 1f;
                Color blendedColor = Color.Lerp(flameColor, boostFlameColor, t);
                main.startColor = blendedColor;
                if (flameLight)
                    flameLight.color = blendedColor;

            } else {

                main.startColor = flameColor;
                if (flameLight)
                    flameLight.color = flameColor;

            }

            if (flameLight) {

                flameLight.intensity = intensityMultiplier * flameLightIntensityMultiplier;

            }

        } else {

            popping = false;
            subEmission.enabled = false;
            nosActiveTime = 0f;
            main.startSpeedMultiplier = 1f;

            if (flameLight)
                flameLight.intensity = 0f;

        }

    }

#if !BCG_URP && !BCG_HDRP
    /// <summary>
    /// Built-in pipeline lens flare logic. Adjusts brightness based on camera distance and angle relative to the light.
    /// </summary>
    private void LensFlare() {

        if (!mainCam || !flameLight) {

            finalFlareBrightness = 0f;
            lensFlare.brightness = finalFlareBrightness * flameLight.intensity;
            lensFlare.color = flameLight.color;
            return;

        }

        Vector3 transformPos = transform.position;
        Vector3 transformDir = transform.forward;
        Vector3 camPos = mainCam.transform.position;

        float distanceTocam = Vector3.Distance(transformPos, camPos);
        float angle = Vector3.Angle(transformDir, camPos - transformPos);

        if (!Mathf.Approximately(angle, 0f))
            finalFlareBrightness = flareBrightness * (4f / distanceTocam) * ((300f - (3f * angle)) / 300f) / 3f;
        else
            finalFlareBrightness = flareBrightness;

        if (finalFlareBrightness < 0)
            finalFlareBrightness = 0f;

        lensFlare.brightness = finalFlareBrightness * flameLight.intensity;
        lensFlare.color = flameLight.color;

    }
#else
    /// <summary>
    /// URP/HDRP SRP lens flare logic. Adjusts brightness based on camera distance and angle relative to the light.
    /// </summary>
    private void LensFlare_SRP() {

        if (!mainCam || !flameLight) {

            finalFlareBrightness = 0f;
            lensFlare_SRP.intensity = finalFlareBrightness;
            return;

        }

        float distanceTocam = Vector3.Distance(transform.position, mainCam.transform.position);
        float angle = Vector3.Angle(transform.forward, mainCam.transform.position - transform.position);

        if (!Mathf.Approximately(angle, 0f))
            finalFlareBrightness = flareBrightness * (8f / distanceTocam) * ((300f - (3f * angle)) / 300f) / 3f;
        else
            finalFlareBrightness = flareBrightness;

        if (finalFlareBrightness < 0)
            finalFlareBrightness = 0f;

        lensFlare_SRP.attenuationByLightShape = false;
        lensFlare_SRP.intensity = finalFlareBrightness * flameLight.intensity;

    }
#endif

    /// <summary>
    /// Triggers a brief collision displacement shake on the active player camera if this vehicle is the player's active vehicle.
    /// </summary>
    /// <param name="intensity">Strength of the shake.</param>
    private void TriggerCameraShake(float intensity) {

        if (CarController != null && RCCP_SceneManager.Instance != null && RCCP_SceneManager.Instance.activePlayerVehicle == CarController) {

            if (RCCP_SceneManager.Instance.activePlayerCamera != null) {

                RCCP_SceneManager.Instance.activePlayerCamera.TriggerCollisionShake(intensity);

            }

        }

    }

}
