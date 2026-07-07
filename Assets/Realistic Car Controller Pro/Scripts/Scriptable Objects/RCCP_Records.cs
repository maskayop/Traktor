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

/// <summary>
/// Recorded clips.
/// </summary>
public class RCCP_Records : ScriptableObject {

    #region singleton
    private static RCCP_Records instance;
    private static bool diskRecordsMerged = false;
    /// <summary>Singleton instance of the recordings configuration, loaded from Resources.</summary>
    public static RCCP_Records Instance {
        get {
            if (instance == null)
                instance = Resources.Load("RCCP_Records") as RCCP_Records;

            //  V2.51 (T2-4): in a build, merge disk-persisted replays once so runtime-saved clips survive.
            //  Editor is skipped so the ScriptableObject asset is never polluted with runtime clips.
            if (instance != null && !diskRecordsMerged && !Application.isEditor) {
                diskRecordsMerged = true;
                instance.MergeDiskRecords();
            }

            return instance;
        }
    }
    #endregion

    /// <summary>
    /// All records.
    /// </summary>
    [Tooltip("All saved record clips captured with the RCCP Recorder, available for playback.")]
    public List<RCCP_Recorder.RecordedClip> records = new List<RCCP_Recorder.RecordedClip>();

    /// <summary>
    /// V2.51 (T2-4): appends disk-persisted replays not already present (deduped by recordName; existing in-memory
    /// records win). Runtime/build only — the in-memory list mutates, the asset on disk is never written.
    /// </summary>
    public void MergeDiskRecords() {

        List<RCCP_Recorder.RecordedClip> disk = RCCP_RecordIO.LoadAll();

        if (disk == null || disk.Count == 0)
            return;

        HashSet<string> existing = new HashSet<string>();

        foreach (RCCP_Recorder.RecordedClip r in records) {
            if (r != null && r.recordName != null)
                existing.Add(r.recordName);
        }

        foreach (RCCP_Recorder.RecordedClip d in disk) {

            if (d == null)
                continue;

            if (d.recordName == null || !existing.Contains(d.recordName)) {
                records.Add(d);
                if (d.recordName != null)
                    existing.Add(d.recordName);
            }

        }

    }

}
