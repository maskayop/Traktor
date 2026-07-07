//----------------------------------------------
//        Realistic Car Controller Pro
//
// Copyright 2014 - 2025 BoneCracker Games
// https://www.bonecrackergames.com
// Ekrem Bugra Ozdoganlar
//
//----------------------------------------------

#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
using System;
using System.Collections;
using System.Collections.Generic;

[CustomEditor(typeof(RCCP_AeroDynamics))]
public class RCCP_AeroDynamicsEditor : Editor {

    RCCP_AeroDynamics prop;
    GUISkin skin;

    private void OnEnable() {

        skin = RCCP_DesignSystem.Skin;

    }

    public override void OnInspectorGUI() {

        prop = (RCCP_AeroDynamics)target;
        serializedObject.Update();
        GUI.skin = skin;

        Transform com = prop.COM;

        EditorGUILayout.HelpBox("Manages the dynamics of the vehicle.", MessageType.Info, true);

        if (GUILayout.Button(new GUIContent("COM", "Centre of mass. Must be placed correctly. You can google it for vehicles to see which locations are suitable.")))
            Selection.activeGameObject = prop.COM.gameObject;

        EditorGUILayout.PropertyField(serializedObject.FindProperty("dynamicCOM"), new GUIContent("Dynamic COM"));
        EditorGUILayout.PropertyField(serializedObject.FindProperty("ignoreRigidbodyDragOnAccelerate"), new GUIContent("Ignore Drag On Accelerate"));

        EditorGUILayout.PropertyField(serializedObject.FindProperty("downForce"), new GUIContent("Downforce"));
        EditorGUILayout.PropertyField(serializedObject.FindProperty("airResistance"), new GUIContent("Air Resistance"));
        EditorGUILayout.PropertyField(serializedObject.FindProperty("wheelResistance"), new GUIContent("Wheel Resistance"));

        EditorGUILayout.PropertyField(serializedObject.FindProperty("autoReset"), new GUIContent("Auto Reset"));

        if (prop.autoReset)
            EditorGUILayout.PropertyField(serializedObject.FindProperty("autoResetTime"), new GUIContent("Auto Reset Timer"));

        EditorGUILayout.Space();
        EditorGUILayout.LabelField("Inertia Tensor", EditorStyles.boldLabel);
        EditorGUILayout.HelpBox("Rotational 'heaviness' per axis. OFF (default) = Unity's automatic tensor (stock). " +
            "Multiplier scales the auto tensor per axis and works on any vehicle; Absolute freezes exact kg·m² values.", MessageType.None, true);

        SerializedProperty overrideProp = serializedObject.FindProperty("overrideInertiaTensor");
        SerializedProperty scaleProp = serializedObject.FindProperty("inertiaTensorScale");
        SerializedProperty absProp = serializedObject.FindProperty("inertiaTensorAbsolute");

        EditorGUI.BeginChangeCheck();
        EditorGUILayout.PropertyField(overrideProp, new GUIContent("Override Inertia Tensor"));

        // Coerce legacy/unset (0,0,0) data to sane defaults ONLY when the user just enabled the override
        // (a user-initiated change). Merely inspecting an existing vehicle never dirties it; the runtime
        // RecomputeInertia has the same guard so behavior is safe regardless.
        if (EditorGUI.EndChangeCheck() && overrideProp.boolValue) {
            if (scaleProp.vector3Value == Vector3.zero)
                scaleProp.vector3Value = Vector3.one;
            if (absProp.vector3Value == Vector3.zero)
                absProp.vector3Value = new Vector3(2000f, 2030f, 400f);
        }

        if (overrideProp.boolValue) {

            SerializedProperty modeProp = serializedObject.FindProperty("inertiaTensorMode");
            EditorGUILayout.PropertyField(modeProp, new GUIContent("Mode"));

            if (modeProp.enumValueIndex == (int)RCCP_AeroDynamics.InertiaTensorMode.Multiplier) {

                EditorGUILayout.LabelField("Per-axis multiplier (1 = stock)", EditorStyles.miniLabel);

                Vector3 s = scaleProp.vector3Value;
                if (s == Vector3.zero)
                    s = Vector3.one;   // display fallback only — persisted below only if the user edits

                EditorGUI.BeginChangeCheck();
                s.x = EditorGUILayout.Slider(new GUIContent("Pitch (X)", "Nose-dive under braking / squat under acceleration."), s.x, 0.05f, 5f);
                s.y = EditorGUILayout.Slider(new GUIContent("Yaw (Y)", "How eagerly the car rotates into a turn / how easily it spins. Lower = darty, higher = planted."), s.y, 0.05f, 5f);
                s.z = EditorGUILayout.Slider(new GUIContent("Roll (Z)", "Body lean in corners / rollover resistance."), s.z, 0.05f, 5f);
                bool scaleChanged = EditorGUI.EndChangeCheck();

                if (GUILayout.Button(new GUIContent("Reset Multipliers (1, 1, 1)"))) {
                    s = Vector3.one;
                    scaleChanged = true;
                }

                if (scaleChanged)
                    scaleProp.vector3Value = s;

            } else {

                EditorGUILayout.PropertyField(absProp,
                    new GUIContent("Absolute (kg·m²)", "X = Pitch, Y = Yaw, Z = Roll."));

            }

            // Live readout (Play mode) — shows the auto base vs the applied tensor in real kg·m².
            if (Application.isPlaying && prop.CarController != null && prop.CarController.Rigid != null) {

                EditorGUILayout.Space();
                EditorGUILayout.LabelField("Auto base (kg·m²)", prop.lastAutoInertiaTensor.ToString("F1"));
                EditorGUILayout.LabelField("Applied (kg·m²)", prop.CarController.Rigid.inertiaTensor.ToString("F1"));

                if (GUILayout.Button("Recompute Now"))
                    prop.RecomputeInertia();

            }

        }

        EditorGUILayout.Space();
        RCCP_DesignSystem.DrawSkinSeparator();

        if (!EditorUtility.IsPersistent(prop)) {

            EditorGUILayout.BeginVertical(GUI.skin.box);

            RCCP_DesignSystem.DrawBackButton(prop);

            EditorGUILayout.EndVertical();

        }

        RCCP_DesignSystem.ResetTransform(prop);

        serializedObject.ApplyModifiedProperties();

        if (GUI.changed)
            EditorUtility.SetDirty(prop);

    }

}
#endif
