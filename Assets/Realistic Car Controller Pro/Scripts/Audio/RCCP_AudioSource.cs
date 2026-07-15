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
using UnityEngine.Audio;

/// <summary>
/// Creates new audiosource with specified settings.
/// </summary>
public class RCCP_AudioSource {

    /// <summary>
    /// Creates and configures a new audio source on the target GameObject.
    /// </summary>
    /// <param name="audioMixer">The audio mixer group to route output through, or null for default.</param>
    /// <param name="go">The target GameObject to parent the audio source under.</param>
    /// <param name="audioName">Name for the created audio source GameObject.</param>
    /// <param name="minDistance">Minimum distance for 3D sound attenuation. Set both min and max to 0 for 2D.</param>
    /// <param name="maxDistance">Maximum distance for 3D sound attenuation.</param>
    /// <param name="volume">Playback volume from 0 to 1.</param>
    /// <param name="audioClip">The audio clip to assign to the source.</param>
    /// <param name="loop">Whether the audio should loop.</param>
    /// <param name="playNow">Whether to start playback immediately.</param>
    /// <param name="destroyAfterFinished">Whether to destroy the audio source after the clip finishes playing.</param>
    /// <returns>The created AudioSource component.</returns>
    public static AudioSource NewAudioSource(AudioMixerGroup audioMixer, GameObject go, string audioName, float minDistance, float maxDistance, float volume, AudioClip audioClip, bool loop, bool playNow, bool destroyAfterFinished) {

        GameObject audioSourceObject = new GameObject(audioName);

        if (go.transform.Find("All Audio Sources")) {

            audioSourceObject.transform.SetParent(go.transform.Find("All Audio Sources"));

        } else {

            GameObject allAudioSources = new GameObject("All Audio Sources");
            allAudioSources.transform.SetParent(go.transform, false);
            audioSourceObject.transform.SetParent(allAudioSources.transform, false);

        }

        audioSourceObject.transform.SetPositionAndRotation(go.transform.position, go.transform.rotation);

        AudioSource source = audioSourceObject.AddComponent<AudioSource>();

        if (audioMixer)
            source.outputAudioMixerGroup = audioMixer;

        source.minDistance = minDistance;
        source.maxDistance = maxDistance;
        source.volume = volume;
        source.clip = audioClip;
        source.loop = loop;
        source.dopplerLevel = .5f;
        source.ignoreListenerPause = false;
        source.ignoreListenerVolume = false;
        source.rolloffMode = AudioRolloffMode.Logarithmic;

        if (minDistance == 0 && maxDistance == 0)
            source.spatialBlend = 0f;
        else
            source.spatialBlend = 1f;

        if (playNow) {

            source.playOnAwake = true;
            source.Play();

        } else {

            source.playOnAwake = false;

        }

        if (destroyAfterFinished) {

            //  Delayed Object.Destroy runs on scaled time while audio plays in realtime, which
            //  truncates one-shots at Time.timeScale > 1 and leaks them at timeScale = 0. The
            //  helper component destroys based on actual playback state instead.
            if (audioClip)
                audioSourceObject.AddComponent<RCCP_AudioSourceAutoDestroy>();
            else
                Object.Destroy(audioSourceObject);

        }

        return source;

    }

    /// <summary>
    /// Creates and configures a new audio source on the target GameObject without an audio mixer group.
    /// </summary>
    /// <param name="go">The target GameObject to parent the audio source under.</param>
    /// <param name="audioName">Name for the created audio source GameObject.</param>
    /// <param name="minDistance">Minimum distance for 3D sound attenuation. Set both min and max to 0 for 2D.</param>
    /// <param name="maxDistance">Maximum distance for 3D sound attenuation.</param>
    /// <param name="volume">Playback volume from 0 to 1.</param>
    /// <param name="audioClip">The audio clip to assign to the source.</param>
    /// <param name="loop">Whether the audio should loop.</param>
    /// <param name="playNow">Whether to start playback immediately.</param>
    /// <param name="destroyAfterFinished">Whether to destroy the audio source after the clip finishes playing.</param>
    /// <returns>The created AudioSource component.</returns>
    public static AudioSource NewAudioSource(GameObject go, string audioName, float minDistance, float maxDistance, float volume, AudioClip audioClip, bool loop, bool playNow, bool destroyAfterFinished) {

        GameObject audioSourceObject = new GameObject(audioName);

        if (go.transform.Find("All Audio Sources")) {

            audioSourceObject.transform.SetParent(go.transform.Find("All Audio Sources"));

        } else {

            GameObject allAudioSources = new GameObject("All Audio Sources");
            allAudioSources.transform.SetParent(go.transform, false);
            audioSourceObject.transform.SetParent(allAudioSources.transform, false);

        }

        audioSourceObject.transform.SetPositionAndRotation(go.transform.position, go.transform.rotation);

        AudioSource source = audioSourceObject.AddComponent<AudioSource>();

        source.minDistance = minDistance;
        source.maxDistance = maxDistance;
        source.volume = volume;
        source.clip = audioClip;
        source.loop = loop;
        source.dopplerLevel = .5f;
        source.ignoreListenerPause = false;
        source.ignoreListenerVolume = false;
        source.rolloffMode = AudioRolloffMode.Logarithmic;

        if (minDistance == 0 && maxDistance == 0)
            source.spatialBlend = 0f;
        else
            source.spatialBlend = 1f;

        if (playNow) {

            source.playOnAwake = true;
            source.Play();

        } else {

            source.playOnAwake = false;

        }

        if (destroyAfterFinished) {

            //  Delayed Object.Destroy runs on scaled time while audio plays in realtime, which
            //  truncates one-shots at Time.timeScale > 1 and leaks them at timeScale = 0. The
            //  helper component destroys based on actual playback state instead.
            if (audioClip)
                audioSourceObject.AddComponent<RCCP_AudioSourceAutoDestroy>();
            else
                Object.Destroy(audioSourceObject);

        }

        return source;

    }

    /// <summary>
    /// Adds a high-pass audio filter to the given AudioSource.
    /// </summary>
    /// <param name="source">The AudioSource to attach the filter to.</param>
    /// <param name="freq">Cutoff frequency in Hz for the high-pass filter.</param>
    /// <param name="level">Resonance quality factor for the high-pass filter.</param>
    public static void NewHighPassFilter(AudioSource source, float freq, int level) {

        if (source == null)
            return;

        AudioHighPassFilter highFilter = source.gameObject.AddComponent<AudioHighPassFilter>();
        highFilter.cutoffFrequency = freq;
        highFilter.highpassResonanceQ = level;

    }

    /// <summary>
    /// Adds a low-pass audio filter to the given AudioSource.
    /// </summary>
    /// <param name="source">The AudioSource to attach the filter to.</param>
    /// <param name="freq">Cutoff frequency in Hz for the low-pass filter.</param>
    public static void NewLowPassFilter(AudioSource source, float freq) {

        if (source == null)
            return;

        AudioLowPassFilter lowFilter = source.gameObject.AddComponent<AudioLowPassFilter>();
        lowFilter.cutoffFrequency = freq;

    }

}

/// <summary>
/// Destroys a transient one-shot audio source GameObject once playback has actually finished.
/// Runtime-created by RCCP_AudioSource.NewAudioSource for destroyAfterFinished sources; not meant
/// to be added manually. Tracks realtime playback state instead of a scaled-time Destroy delay, so
/// one-shots are neither cut off at Time.timeScale > 1 nor kept alive forever at timeScale = 0.
/// </summary>
[AddComponentMenu("")]
public class RCCP_AudioSourceAutoDestroy : MonoBehaviour {

    private AudioSource source;
    private bool hasPlayed = false;
    private float spawnRealtime = 0f;
    private bool quitting = false;

    private void Awake() {

        source = GetComponent<AudioSource>();
        spawnRealtime = Time.realtimeSinceStartup;

    }

    private void Update() {

        if (!source) {

            Destroy(gameObject);
            return;

        }

        //  Global audio pause suspends playback without finishing it. Don't reap while paused.
        if (AudioListener.pause)
            return;

        if (source.isPlaying) {

            hasPlayed = true;
            return;

        }

        //  Finished playing, or never started within the legacy clip-length lifetime window.
        float lifetime = source.clip ? source.clip.length : 0f;

        if (hasPlayed || Time.realtimeSinceStartup - spawnRealtime >= lifetime)
            Destroy(gameObject);

    }

    private void OnApplicationQuit() {

        quitting = true;

    }

    //  Fire-and-forget one-shots are garbage once deactivated: Update no longer runs (would leak),
    //  and playOnAwake would replay the clip on reactivation. Reap immediately instead.
    private void OnDisable() {

        if (!quitting)
            Destroy(gameObject);

    }

}
