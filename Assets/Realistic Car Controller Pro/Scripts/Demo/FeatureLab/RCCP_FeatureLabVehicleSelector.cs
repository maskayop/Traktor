//----------------------------------------------
//        Realistic Car Controller Pro
//
// Copyright © 2014 - 2026 BoneCracker Games
// https://www.bonecrackergames.com
// Ekrem Bugra Ozdoganlar
//
//----------------------------------------------

using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using F = RCCP_FeatureLabUIFactory;

/// <summary>
/// Feature Lab vehicle switcher. Uses DIRECT prefab references (never the
/// RCCP_DemoVehicles registry — that asset ships with the Demo Content addon and its
/// Instance is legitimately null without it). First spawn uses spawnPoint; afterwards
/// the new vehicle takes over the current player vehicle's position, yaw, and momentum
/// in place (RCCP_Demo.Spawn convention). Registers as player, destroys the previous
/// vehicle AFTER the new one is registered.
/// </summary>
public class RCCP_FeatureLabVehicleSelector : MonoBehaviour {

    [Tooltip("Vehicle prefabs, in bar order. Wired in the scene — direct references, no registry.")]
    public RCCP_CarController[] vehiclePrefabs = new RCCP_CarController[0];

    [Tooltip("Optional display names parallel to vehiclePrefabs. Empty entries fall back to prefab name.")]
    public string[] displayNames = new string[0];

    [Tooltip("Spawn anchor in the scene. Used only when no active player vehicle exists — subsequent spawns inherit the current vehicle's position, yaw, and velocity.")]
    public Transform spawnPoint;

    public int SelectedIndex { get; private set; } = -1;

    private RCCP_FeatureLabUI ui;
    private readonly List<Button> buttons = new List<Button>(16);
    private readonly List<int> buttonSourceIndices = new List<int>(16);

    private void Start() {

        StartCoroutine(InitializeBar());

        if (vehiclePrefabs.Length > 0 && RCCP_SceneManager.Instance.activePlayerVehicle == null)
            Spawn(0);

    }

    private System.Collections.IEnumerator InitializeBar() {

        //  RCCP_FeatureLabUI assigns SelectorBarRoot in its own Start (chrome build) —
        //  Start order across GameObjects is nondeterministic, so wait for it here rather
        //  than assuming it's already ready. Give up quietly after ~5 seconds (300 frames)
        //  if the UI never appears (e.g. selector used in a scene without the canvas).
        int frames = 0;

        while (ui == null || ui.SelectorBarRoot == null) {

            ui = FindFirstObjectByType<RCCP_FeatureLabUI>();

            if (ui != null && ui.SelectorBarRoot != null)
                break;

            frames++;

            if (frames > 300)
                yield break;

            yield return null;

        }

        BuildBar();

    }

    private void BuildBar() {

        RectTransform root = ui.SelectorBarRoot;

        Image bg = F.Panel("Bg", root, F.PanelBg);
        F.Fill(bg.rectTransform);

        HorizontalLayoutGroup h = bg.gameObject.AddComponent<HorizontalLayoutGroup>();
        h.spacing = 6f;
        h.padding = new RectOffset(8, 8, 8, 8);
        h.childControlWidth = true;
        h.childControlHeight = true;
        h.childForceExpandWidth = true;
        h.childForceExpandHeight = true;

        for (int i = 0; i < vehiclePrefabs.Length; i++) {

            if (vehiclePrefabs[i] == null)
                continue;

            int index = i;
            Button b = F.TextButton("Vehicle_" + i, bg.transform, DisplayName(i), F.RowBg, F.TextMain, 12f);
            b.onClick.AddListener(() => Spawn(index));
            buttons.Add(b);
            buttonSourceIndices.Add(index);

        }

        HighlightSelection();

    }

    private string DisplayName(int index) {

        if (displayNames != null && index < displayNames.Length && !string.IsNullOrEmpty(displayNames[index]))
            return displayNames[index];

        return vehiclePrefabs[index].name;

    }

    public void Spawn(int index) {

        if (index < 0 || index >= vehiclePrefabs.Length || vehiclePrefabs[index] == null)
            return;

        RCCP_CarController previous = RCCP_SceneManager.Instance.activePlayerVehicle;

        //  Spawn point is the FIRST-spawn anchor only. Once a player vehicle exists,
        //  hand over in place — same position/heading and momentum, like RCCP_Demo.Spawn.
        Vector3 position = spawnPoint != null ? spawnPoint.position : Vector3.up;
        Quaternion rotation = spawnPoint != null ? spawnPoint.rotation : Quaternion.identity;
        Vector3 velocity = Vector3.zero;
        Vector3 angularVelocity = Vector3.zero;

        if (previous != null) {

            position = previous.transform.position;
            //  Yaw only — X/Z lean of the old body must not tilt the new one into the ground.
            rotation = Quaternion.Euler(0f, previous.transform.rotation.eulerAngles.y, 0f);
            velocity = previous.Rigid.linearVelocity;
            angularVelocity = previous.Rigid.angularVelocity;

        }

        //  Lift the spawn slightly — pivots differ between vehicles, and spawning flush
        //  with the ground can start the new body intersecting the road mesh.
        position += Vector3.up * .5f;

        //  RCCP prefabs ship ACTIVE — OnEnable auto-registration runs inside Instantiate.
        RCCP_CarController spawned = Instantiate(vehiclePrefabs[index], position, rotation);
        spawned.gameObject.SetActive(true);

        //  Carry the previous vehicle's momentum over so the swap is seamless at speed.
        spawned.Rigid.linearVelocity = velocity;
        spawned.Rigid.angularVelocity = angularVelocity;

        //  Explicit registration is idempotent vs the auto-register path and sets control + engine.
        RCCP_SceneManager.Instance.RegisterPlayer(spawned, true, true);

        //  Demo hygiene: don't let lab tweaks persist to PlayerPrefs via the customizer.
        if (spawned.Customizer != null)
            spawned.Customizer.autoSave = false;

        if (previous != null)
            Destroy(previous.gameObject);

        //  Task 13 migration: the Systems category's Respawn action reads
        //  RCCP_FeatureLab.Instance.spawnAnchor, not a catalog-owned static. Wire it here
        //  unless the scene already assigned one explicitly.
        if (RCCP_FeatureLab.Instance != null && RCCP_FeatureLab.Instance.spawnAnchor == null && spawnPoint != null)
            RCCP_FeatureLab.Instance.spawnAnchor = spawnPoint;

        SelectedIndex = index;
        HighlightSelection();

        if (ui != null)
            ui.ShowToast(DisplayName(index) + " spawned");

    }

    private void HighlightSelection() {

        for (int i = 0; i < buttons.Count; i++) {

            Image img = buttons[i].targetGraphic as Image;
            img.color = buttonSourceIndices[i] == SelectedIndex ? new Color(1f, .478f, 0f, .25f) : F.RowBg;

        }

    }

}
