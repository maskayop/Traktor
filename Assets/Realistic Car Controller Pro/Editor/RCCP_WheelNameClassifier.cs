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
using System.Text;
using UnityEngine;

/// <summary>
/// Shared wheel-mesh name classifier used by RCCP_ModelColliderAudit and
/// RCCP_BodyCollidersWizard. Token-aware matching so legitimate body meshes
/// like "door_trim" / "InteriorTrim" no longer false-positive against the
/// "rim" pattern (V2.45.1 audit regression — naive Contains() matched any
/// substring including "rim" inside "trim", and downstream Auto-Fix then
/// destroyed valid body colliders).
///
/// Matching pipeline:
///   1. If the lowered name contains "steering", return false (steering
///      wheel meshes are not road wheels).
///   2. If the lowered name contains an unambiguous COMPOUND substring
///      (front_left, frontwheel, etc.), return true.
///   3. Tokenize on separator chars, camelCase boundaries, and letter↔digit
///      boundaries. Return true if any token equals an EXACT-TOKEN entry.
/// </summary>
public static class RCCP_WheelNameClassifier {

    // Whole-token matches. Each pattern must equal one full token after
    // tokenization — guards against "rim" matching inside "trim" / "InteriorTrim".
    private static readonly string[] ExactTokens = {
        "wheel", "tire", "tyre", "rim", "whl",
        "fl", "fr", "rl", "rr",
        "lf", "rf", "lr"
    };

    // Unambiguous compound substrings (raw Contains against the lowered name).
    // These have no risk of false-positive — every substring here only appears
    // in wheel-related names. The no-separator forms (frontwheel / rearwheel /
    // backwheel) preserve the old `Contains("wheel")` behavior for names that
    // don't split via separators or camelCase, so we don't regress vs. the
    // previous matcher.
    private static readonly string[] CompoundSubstrings = {
        "front_left", "front_right", "rear_left", "rear_right",
        "frontleft", "frontright", "rearleft", "rearright",
        "leftfront", "rightfront", "leftrear", "rightrear",
        "frontwheel", "rearwheel", "backwheel"
    };

    /// <summary>True if the name looks like a wheel mesh (case-insensitive).</summary>
    public static bool IsWheel(string name) {
        if (string.IsNullOrEmpty(name)) return false;

        string lower = name.ToLowerInvariant();

        // Steering-wheel guard must run before any wheel test. Catches
        // "steeringwheel", "steering_wheel", "SteeringWheel" — the lowered
        // form always contains "wheel", which would otherwise match below.
        if (lower.Contains("steering")) return false;

        // Compound substrings — unambiguous fast path.
        for (int i = 0; i < CompoundSubstrings.Length; i++) {
            if (lower.Contains(CompoundSubstrings[i])) return true;
        }

        // Token-equality path.
        List<string> tokens = Tokenize(name);
        for (int i = 0; i < tokens.Count; i++) {
            for (int j = 0; j < ExactTokens.Length; j++) {
                if (tokens[i] == ExactTokens[j]) return true;
            }
        }

        return false;
    }

    /// <summary>True if <paramref name="t"/>'s name or any ancestor up to depth 4 looks like a wheel mesh.</summary>
    public static bool IsWheel(Transform t) {
        if (t == null) return false;

        Transform current = t;
        int depth = 0;
        while (current != null && depth < 4) {
            if (IsWheel(current.name)) return true;
            current = current.parent;
            depth++;
        }
        return false;
    }

    /// <summary>
    /// Splits a name into lowercase tokens. Boundaries: any non-alphanumeric
    /// char (treats `_`, `-`, `.`, space, parens etc. as separators); camelCase
    /// boundary (lowerCase→Upper); letter↔digit boundary (Wheel1 → wheel/1).
    /// </summary>
    private static List<string> Tokenize(string name) {
        var tokens = new List<string>();
        var buffer = new StringBuilder();
        char prevOriginal = '\0';

        for (int i = 0; i < name.Length; i++) {
            char c = name[i];

            // Non-alphanumeric → separator → flush.
            if (!char.IsLetterOrDigit(c)) {
                FlushBuffer(buffer, tokens);
                prevOriginal = '\0';
                continue;
            }

            // Boundary check uses ORIGINAL char types (camelCase intent is
            // preserved by the original casing, not the lowered casing).
            if (buffer.Length > 0 && prevOriginal != '\0') {
                bool prevLower = char.IsLower(prevOriginal);
                bool prevUpper = char.IsUpper(prevOriginal);
                bool prevDigit = char.IsDigit(prevOriginal);
                bool currUpper = char.IsUpper(c);
                bool currDigit = char.IsDigit(c);
                bool currLetter = char.IsLetter(c);

                if ((prevLower && currUpper) ||
                    (prevLower && currDigit) ||
                    (prevUpper && currDigit) ||
                    (prevDigit && currLetter)) {
                    FlushBuffer(buffer, tokens);
                }
            }

            buffer.Append(char.ToLowerInvariant(c));
            prevOriginal = c;
        }
        FlushBuffer(buffer, tokens);

        return tokens;
    }

    private static void FlushBuffer(StringBuilder buffer, List<string> tokens) {
        if (buffer.Length == 0) return;
        tokens.Add(buffer.ToString());
        buffer.Length = 0;
    }
}
#endif
