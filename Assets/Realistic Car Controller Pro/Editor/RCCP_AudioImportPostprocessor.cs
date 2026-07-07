//----------------------------------------------
//        Realistic Car Controller Pro
//
// Copyright 2014 - 2026 BoneCracker Games
// https://www.bonecrackergames.com
// Ekrem Bugra Ozdoganlar
//
//----------------------------------------------

#if UNITY_EDITOR
using System;
using System.IO;
using UnityEditor;
using UnityEngine;

/// <summary>
/// Enforces recommended audio import settings on clips under the Realistic Car Controller Pro
/// folder. Customer audio outside the RCCP path is not touched.
///
/// Project Auditor flags PAA4008 (PreloadAudioData enabled), PAA4004 (DecompressOnLoad on
/// medium/large clips), and PAA4000 (long clips not Streaming). This postprocessor applies the
/// recommended fixes at import time so newly-added RCCP audio auto-conforms.
///
/// Existing clips keep their stored settings until they're reimported. To force-apply across
/// every RCCP clip, run Tools &gt; BoneCracker Games &gt; Realistic Car Controller Pro &gt;
/// Audio &gt; Reimport All RCCP Audio Clips.
/// </summary>
public class RCCP_AudioImportPostprocessor : AssetPostprocessor {

    private const string RCCP_AUDIO_PATH_PREFIX = "Assets/Realistic Car Controller Pro/";

    // Disk size thresholds for choosing loadType. Tuned for RCCP's mix of short SFX
    // (gear shifts, crashes) and long looping clips (engine layers, wind).
    private const long SIZE_THRESHOLD_COMPRESSED = 50 * 1024;     // 50 KB
    private const long SIZE_THRESHOLD_STREAMING = 500 * 1024;    // 500 KB

    /// <summary>Runs after default postprocessors so we override Unity's defaults last.</summary>
    public override int GetPostprocessOrder() => 100;

    private void OnPreprocessAudio() {

        if (string.IsNullOrEmpty(assetPath))
            return;

        if (!assetPath.StartsWith(RCCP_AUDIO_PATH_PREFIX, StringComparison.OrdinalIgnoreCase))
            return;

        AudioImporter importer = assetImporter as AudioImporter;
        if (importer == null)
            return;

        AudioImporterSampleSettings settings = importer.defaultSampleSettings;
        bool changed = false;

        if (settings.preloadAudioData) {
            settings.preloadAudioData = false;
            changed = true;
        }

        AudioClipLoadType targetLoadType = ResolveLoadType(assetPath);
        if (settings.loadType != targetLoadType) {
            settings.loadType = targetLoadType;
            changed = true;
        }

        if (changed)
            importer.defaultSampleSettings = settings;

        if (!importer.loadInBackground)
            importer.loadInBackground = true;

    }

    /// <summary>
    /// Picks a loadType from on-disk file size. We don't crack the audio header for duration —
    /// file size is close enough for the small/medium/large bucket choice.
    /// </summary>
    private static AudioClipLoadType ResolveLoadType(string assetPath) {

        try {
            FileInfo info = new FileInfo(assetPath);
            if (!info.Exists)
                return AudioClipLoadType.DecompressOnLoad;

            if (info.Length > SIZE_THRESHOLD_STREAMING)
                return AudioClipLoadType.Streaming;

            if (info.Length > SIZE_THRESHOLD_COMPRESSED)
                return AudioClipLoadType.CompressedInMemory;

            return AudioClipLoadType.DecompressOnLoad;
        }
        catch {
            return AudioClipLoadType.DecompressOnLoad;
        }

    }

    [MenuItem("Tools/BoneCracker Games/Realistic Car Controller Pro/Audio/Reimport All RCCP Audio Clips")]
    private static void ReimportAllRCCPAudio() {

        if (!EditorUtility.DisplayDialog(
            "Reimport RCCP Audio Clips",
            "This will reimport every audio clip under " + RCCP_AUDIO_PATH_PREFIX + " with the optimized import settings (PreloadAudioData=off, LoadInBackground=on, size-based loadType).\n\nMany .meta files will be updated. Continue?",
            "Reimport",
            "Cancel"))
            return;

        string[] guids = AssetDatabase.FindAssets("t:AudioClip", new[] { "Assets/Realistic Car Controller Pro" });
        int total = guids.Length;
        int processed = 0;

        try {
            AssetDatabase.StartAssetEditing();

            for (int i = 0; i < total; i++) {

                string path = AssetDatabase.GUIDToAssetPath(guids[i]);

                if (EditorUtility.DisplayCancelableProgressBar(
                    "Reimporting RCCP audio",
                    path,
                    total == 0 ? 0f : (float)i / total))
                    break;

                AssetDatabase.ImportAsset(path, ImportAssetOptions.ForceUpdate);
                processed++;

            }
        }
        finally {
            AssetDatabase.StopAssetEditing();
            EditorUtility.ClearProgressBar();
            AssetDatabase.SaveAssets();
        }

        Debug.Log($"[RCCP] Reimported {processed}/{total} RCCP audio clips with optimized import settings.");

    }

}
#endif
