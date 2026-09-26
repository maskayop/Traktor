//----------------------------------------------
//        Realistic Car Controller Pro
//
// Copyright 2014 - 2026 BoneCracker Games
// https://www.bonecrackergames.com
// Ekrem Bugra Ozdoganlar
//
//----------------------------------------------

using System.Collections.Generic;
using System.IO;
using UnityEngine;

/// <summary>
/// V2.51 (T2-4): build-safe persistence for RCCP recordings. The ScriptableObject (RCCP_Records) only holds
/// records in the editor; clips saved at runtime in a build are otherwise lost. This writes each clip as JSON to
/// <c>Application.persistentDataPath/Replays/</c> and reads them back. RecordedClip and its frame sub-types are all
/// [Serializable] with JsonUtility-friendly fields (float/int/bool/enum/Vector3/Quaternion).
/// </summary>
public static class RCCP_RecordIO {

    private const string SubFolder = "Replays";

    /// <summary>Replay directory under persistentDataPath; created on first access.</summary>
    private static string Dir {

        get {

            string d = Path.Combine(Application.persistentDataPath, SubFolder);

            if (!Directory.Exists(d))
                Directory.CreateDirectory(d);

            return d;

        }

    }

    /// <summary>
    /// Writes a single recorded clip to disk as JSON (filename derived from recordName). Overwrites a clip with
    /// the same name. Failures are warned, never thrown — saving a replay must not break gameplay.
    /// </summary>
    public static void Save(RCCP_Recorder.RecordedClip clip) {

        if (clip == null)
            return;

        try {

            string path = Path.Combine(Dir, MakeSafeFileName(clip.recordName) + ".json");
            File.WriteAllText(path, JsonUtility.ToJson(clip));

        } catch (System.Exception e) {

            Debug.LogWarning("RCCP_RecordIO: failed to save replay '" + clip.recordName + "': " + e.Message);

        }

    }

    /// <summary>
    /// Reads every persisted clip back from disk. Returns an empty list if the folder is absent or unreadable.
    /// </summary>
    public static List<RCCP_Recorder.RecordedClip> LoadAll() {

        List<RCCP_Recorder.RecordedClip> list = new List<RCCP_Recorder.RecordedClip>();

        try {

            string d = Path.Combine(Application.persistentDataPath, SubFolder);

            if (!Directory.Exists(d))
                return list;

            foreach (string file in Directory.GetFiles(d, "*.json")) {

                try {

                    RCCP_Recorder.RecordedClip clip = JsonUtility.FromJson<RCCP_Recorder.RecordedClip>(File.ReadAllText(file));

                    if (clip != null)
                        list.Add(clip);

                } catch {

                    //  Skip a single corrupt file rather than aborting the whole load.

                }

            }

        } catch (System.Exception e) {

            Debug.LogWarning("RCCP_RecordIO: failed to load replays: " + e.Message);

        }

        return list;

    }

    /// <summary>Strips characters that are illegal in file names.</summary>
    private static string MakeSafeFileName(string name) {

        if (string.IsNullOrEmpty(name))
            name = "record";

        foreach (char c in Path.GetInvalidFileNameChars())
            name = name.Replace(c, '_');

        return name;

    }

}
