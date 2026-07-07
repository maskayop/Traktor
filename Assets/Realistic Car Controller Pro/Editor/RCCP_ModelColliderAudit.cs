//----------------------------------------------
//        Realistic Car Controller Pro
//
// Copyright 2014 - 2026 BoneCracker Games
// https://www.bonecrackergames.com
// Ekrem Bugra Ozdoganlar
//
//----------------------------------------------

#if UNITY_EDITOR
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

/// <summary>
/// Pre-flight audit for pre-existing colliders on a vehicle model. Surfaces unsafe
/// configurations the Setup Wizard / AI Vehicle Builder would otherwise inherit
/// silently (triggers on body, non-convex on body, colliders on wheel meshes).
/// Includes disabled GameObjects + components in the scan but only flags them.
/// </summary>
public static class RCCP_ModelColliderAudit {

    /// <summary>Summary of one pre-existing collider that the audit considers unsafe.</summary>
    public struct Finding {
        public Collider collider;
        public string reason;
    }

    /// <summary>Aggregated report. Pass to <see cref="AutoFix"/> to apply the fixable categories.</summary>
    public class Report {
        public List<Finding> triggers   = new List<Finding>();   // strip
        public List<Finding> nonConvex  = new List<Finding>();   // set convex = true
        public List<Finding> onWheel    = new List<Finding>();   // strip
        public List<Finding> disabled   = new List<Finding>();   // flag only — never auto-strip
        public GameObject vehicle;

        public int FixableCount => triggers.Count + nonConvex.Count + onWheel.Count;
        public int FlagOnlyCount => disabled.Count;
        public bool HasFixable => FixableCount > 0;
        public bool HasFindings => FixableCount > 0 || FlagOnlyCount > 0;
    }

    /// <summary>Walks the vehicle hierarchy (including disabled GOs/components) and classifies every Collider.</summary>
    public static Report Analyze(GameObject vehicle) {
        Report r = new Report { vehicle = vehicle };
        if (vehicle == null) return r;

        // includeInactive: true catches disabled GameObjects AND disabled components.
        Collider[] colliders = vehicle.GetComponentsInChildren<Collider>(true);

        foreach (Collider c in colliders) {
            if (c == null) continue;
            if (c is WheelCollider) continue;          // owned by RCCP, handled elsewhere

            bool onWheel = IsOnWheelMesh(c.transform);
            bool isDisabled = !c.enabled || !c.gameObject.activeInHierarchy;

            // Disabled colliders are flag-only (could be intentional LOD swaps).
            if (isDisabled) {
                r.disabled.Add(new Finding { collider = c, reason = (onWheel ? "disabled, on wheel mesh" : "disabled") });
                continue;
            }

            if (onWheel) {
                r.onWheel.Add(new Finding { collider = c, reason = $"{c.GetType().Name} on wheel mesh" });
                continue;   // a wheel-mesh collider is unsafe regardless of trigger/convex
            }

            if (c.isTrigger) {
                r.triggers.Add(new Finding { collider = c, reason = $"{c.GetType().Name} (trigger)" });
                continue;
            }

            // Non-convex MeshColliders break Rigidbody attachment in Unity.
            if (c is MeshCollider mc && !mc.convex) {
                r.nonConvex.Add(new Finding { collider = mc, reason = "MeshCollider not convex" });
            }
        }

        return r;
    }

    /// <summary>Applies every fixable finding inside a single Undo group. Returns the number of fixes applied.</summary>
    public static int AutoFix(Report report) {
        if (report == null || !report.HasFixable) return 0;

        Undo.IncrementCurrentGroup();
        Undo.SetCurrentGroupName("RCCP Auto-Fix Model Colliders");
        int undoGroup = Undo.GetCurrentGroup();

        int fixes = 0;

        foreach (Finding f in report.triggers) {
            if (f.collider != null) { Undo.DestroyObjectImmediate(f.collider); fixes++; }
        }
        foreach (Finding f in report.onWheel) {
            if (f.collider != null) { Undo.DestroyObjectImmediate(f.collider); fixes++; }
        }
        foreach (Finding f in report.nonConvex) {
            if (f.collider is MeshCollider mc && mc != null) {
                Undo.RecordObject(mc, "Set MeshCollider Convex");
                mc.convex = true;
                fixes++;
            }
        }

        Undo.CollapseUndoOperations(undoGroup);
        return fixes;
    }

    /// <summary>
    /// Shows a 3-button dialog summarising the report. Returns true if the caller should continue
    /// (with or without auto-fix), false if the user wants to abort.
    /// </summary>
    public static bool PromptUserIfNeeded(Report report) {
        if (report == null || !report.HasFindings) return true;

        string body = BuildDialogBody(report);

        // No fixable issues — informational only, single OK.
        if (!report.HasFixable) {
            EditorUtility.DisplayDialog("Model Collider Audit", body, "Continue");
            return true;
        }

        int reply = EditorUtility.DisplayDialogComplex(
            "Model Collider Audit",
            body,
            "Fix & Continue",       // 0
            "Continue As-Is",       // 1
            "Cancel");              // 2

        if (reply == 2) return false;
        if (reply == 0) AutoFix(report);
        return true;
    }

    private static string BuildDialogBody(Report report) {
        System.Text.StringBuilder sb = new System.Text.StringBuilder();
        string vehicleName = report.vehicle != null ? report.vehicle.name : "(null)";
        sb.AppendLine($"Found pre-existing collider issues on '{vehicleName}':");
        sb.AppendLine();
        if (report.triggers.Count > 0)  sb.AppendLine($"  • {report.triggers.Count} trigger collider(s) on body  → will be removed");
        if (report.nonConvex.Count > 0) sb.AppendLine($"  • {report.nonConvex.Count} non-convex MeshCollider(s) on body  → will be set to convex");
        if (report.onWheel.Count > 0)   sb.AppendLine($"  • {report.onWheel.Count} collider(s) on wheel meshes  → will be removed");
        if (report.disabled.Count > 0) {
            sb.AppendLine();
            sb.AppendLine($"  {report.disabled.Count} disabled collider(s) found — left as-is (could be intentional).");
        }
        if (report.HasFixable) {
            sb.AppendLine();
            sb.AppendLine("Auto-fix the issues above?");
        }
        return sb.ToString();
    }

    private static bool IsOnWheelMesh(Transform t) {
        // Depth-4 parent walk lives inside RCCP_WheelNameClassifier.IsWheel(Transform).
        return RCCP_WheelNameClassifier.IsWheel(t);
    }
}
#endif
