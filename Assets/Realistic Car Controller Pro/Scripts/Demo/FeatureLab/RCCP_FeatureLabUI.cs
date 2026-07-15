//----------------------------------------------
//        Realistic Car Controller Pro
//
// Copyright © 2014 - 2026 BoneCracker Games
// https://www.bonecrackergames.com
// Ekrem Bugra Ozdoganlar
//
//----------------------------------------------

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using TMPro;
using F = RCCP_FeatureLabUIFactory;

/// <summary>
/// Self-assembling Feature Lab panel. Builds every element from code at Start —
/// the canvas prefab ships near-empty. Two-way bound: widgets write through entry
/// setters; the manager's 10 Hz tick refreshes widgets from entry getters so the UI
/// always shows what IS (cruise self-cancel, preset clobber, replay overrides).
/// </summary>
[RequireComponent(typeof(RCCP_FeatureLab))]
public class RCCP_FeatureLabUI : MonoBehaviour {

    private class Row {

        public RCCP_FeatureLabEntry entry;
        public RectTransform root;
        public CanvasGroup group;
        public TextMeshProUGUI valueLabel;
        public TextMeshProUGUI statusLabel;
        public Slider slider;
        public Toggle toggle;
        public TextMeshProUGUI enumLabel;
        public RectTransform card;
        public TextMeshProUGUI cardReason;
        public TextMeshProUGUI cardReadout;
        public bool suppress;   //  true while Refresh writes widget values

    }

    [Header("Behaviour")]
    public bool startOpen = true;

    public RectTransform SelectorBarRoot { get; private set; }

    private RCCP_FeatureLab lab;
    private RectTransform panel;
    private RectTransform listContent;
    private TextMeshProUGUI footerCount;
    private TMP_InputField search;
    private Button searchClear;
    private CanvasGroup panelGroup;
    private CanvasGroup emptyState;
    private TextMeshProUGUI toast;
    private Coroutine toastRoutine;
    private Button featuresButton;

    private readonly List<Row> rows = new List<Row>(140);
    private readonly List<Button> railButtons = new List<Button>(10);
    private RCCP_FeatureLabCategory currentCategory = RCCP_FeatureLabCategory.Assists;
    private Row expanded;
    private bool panelVisible;

    private static readonly string[] CategoryLabels = new string[] {
        "Assists", "Engine", "Drivetrain", "Physics & Aero", "Wheels",
        "Camera", "Lights", "Audio & VFX", "Damage", "Systems" };

    private void Start() {

        lab = GetComponent<RCCP_FeatureLab>();

        EnsureEventSystem();
        BuildChrome();
        BuildRows();

        lab.OnRebound += RefreshAll;
        lab.OnTick += RefreshVisible;
        RCCP_InputManager.OnFeatureLab += TogglePanel;

        SelectCategory(RCCP_FeatureLabCategory.Assists);
        SetPanelVisible(startOpen);
        RefreshAll();

    }

    private void OnDestroy() {

        if (lab != null) {

            lab.OnRebound -= RefreshAll;
            lab.OnTick -= RefreshVisible;

        }

        RCCP_InputManager.OnFeatureLab -= TogglePanel;

    }

    /// <summary>
    /// Toggle requested through the "Feature Lab" input action (RCCP_InputActions > Optional map, F by default).
    /// </summary>
    private void TogglePanel() {

        SetPanelVisible(!panelVisible);

    }

    private void Update() {

        //  Photo mode owns the screen — hide everything, restore after.
        bool photo = RCCP_PhotoMode.IsActive;
        panelGroup.alpha = photo ? 0f : (panelVisible ? 1f : 0f);
        panelGroup.blocksRaycasts = !photo && panelVisible;
        featuresButton.gameObject.SetActive(!photo && !panelVisible);

    }

    private void EnsureEventSystem() {

        if (EventSystem.current != null)
            return;

        GameObject es = new GameObject("EventSystem", typeof(EventSystem));

#if ENABLE_INPUT_SYSTEM && !ENABLE_LEGACY_INPUT_MANAGER
        es.AddComponent<UnityEngine.InputSystem.UI.InputSystemUIInputModule>();
#else
        es.AddComponent<StandaloneInputModule>();
#endif

    }

    //  ---------------------------------------------------------------- chrome

    private void BuildChrome() {

        //  Docked panel, left side, full height with margins.
        Image panelImg = F.Panel("Panel", transform, F.PanelBg);
        panel = panelImg.rectTransform;
        F.SetAnchors(panel, new Vector2(0f, 0f), new Vector2(0f, 1f), new Vector2(16f, 16f), new Vector2(16f + 480f, -16f));
        panelGroup = panel.gameObject.AddComponent<CanvasGroup>();

        //  Header: accent bar + title + close.
        Image header = F.Panel("Header", panel, new Color(0f, 0f, 0f, .25f));
        F.SetAnchors(header.rectTransform, new Vector2(0f, 1f), new Vector2(1f, 1f), new Vector2(0f, -56f), new Vector2(0f, 0f));

        Image accent = F.Panel("AccentBar", header.transform, F.Accent);
        F.SetAnchors(accent.rectTransform, new Vector2(0f, 0f), new Vector2(0f, 1f), new Vector2(14f, 12f), new Vector2(20f, -12f));

        TextMeshProUGUI title = F.Text("Title", header.transform, "FEATURE LAB", 22f, F.TextMain, TextAlignmentOptions.Left, FontStyles.Bold);
        F.SetAnchors(title.rectTransform, new Vector2(0f, 0f), new Vector2(1f, 1f), new Vector2(32f, 0f), new Vector2(-56f, 0f));

        Button close = F.TextButton("Close", header.transform, "X", new Color(1f, 1f, 1f, .06f), F.TextMain, 18f);
        F.SetAnchors(((RectTransform)close.transform), new Vector2(1f, .5f), new Vector2(1f, .5f), Vector2.zero, Vector2.zero);
        ((RectTransform)close.transform).sizeDelta = new Vector2(36f, 36f);
        ((RectTransform)close.transform).anchoredPosition = new Vector2(-28f, 0f);
        close.onClick.AddListener(() => SetPanelVisible(false));

        //  Search.
        search = F.Field("Search", panel, "Search all features...");
        F.SetAnchors(((RectTransform)search.transform), new Vector2(0f, 1f), new Vector2(1f, 1f), new Vector2(14f, -100f), new Vector2(-14f, -64f));
        search.onValueChanged.AddListener(_ => ApplyFilter());

        //  Right padding so typed text never runs under the clear button.
        search.textViewport.offsetMax = new Vector2(-40f, search.textViewport.offsetMax.y);

        //  Clear button ('X') — visible only while a query is active; clears the
        //  text, releases keyboard focus, and drops back to the category view.
        searchClear = F.TextButton("Clear", search.transform, "X", new Color(1f, 1f, 1f, .06f), F.TextDim, 14f);
        RectTransform clearRect = (RectTransform)searchClear.transform;
        F.SetAnchors(clearRect, new Vector2(1f, .5f), new Vector2(1f, .5f), Vector2.zero, Vector2.zero);
        clearRect.sizeDelta = new Vector2(28f, 28f);
        clearRect.anchoredPosition = new Vector2(-18f, 0f);
        searchClear.onClick.AddListener(ClearSearch);
        searchClear.gameObject.SetActive(false);

        //  Category rail (left) + feature list (right).
        RectTransform rail = F.Rect("Rail", panel);
        F.SetAnchors(rail, new Vector2(0f, 0f), new Vector2(0f, 1f), new Vector2(8f, 56f), new Vector2(8f + 118f, -108f));
        F.VLayout(rail, 4f, 4, true);

        for (int i = 0; i < CategoryLabels.Length; i++) {

            int index = i;
            Button b = F.TextButton("Cat_" + CategoryLabels[i], rail, CategoryLabels[i], F.RowBg, F.TextDim, 13f);
            b.gameObject.AddComponent<LayoutElement>().preferredHeight = 44f;
            b.onClick.AddListener(() => { search.SetTextWithoutNotify(""); SelectCategory((RCCP_FeatureLabCategory)index); });
            railButtons.Add(b);

        }

        ScrollRect scroll = F.Scroll("List", panel, out listContent);
        F.SetAnchors(((RectTransform)scroll.transform), new Vector2(0f, 0f), new Vector2(1f, 1f), new Vector2(134f, 56f), new Vector2(-8f, -108f));
        F.VLayout(listContent, 4f, 6, true);

        //  Footer.
        Button resetCat = F.TextButton("ResetCategory", panel, "RESET CATEGORY", F.RowBg, F.TextMain, 13f);
        F.SetAnchors(((RectTransform)resetCat.transform), new Vector2(0f, 0f), new Vector2(.5f, 0f), new Vector2(14f, 12f), new Vector2(-4f, 44f));
        resetCat.onClick.AddListener(() => { lab.ResetCategory(currentCategory); ShowToast("Category reset"); });

        Button resetAll = F.TextButton("ResetAll", panel, "RESET ALL", new Color(1f, .478f, 0f, .18f), F.Accent, 13f);
        F.SetAnchors(((RectTransform)resetAll.transform), new Vector2(.5f, 0f), new Vector2(1f, 0f), new Vector2(4f, 12f), new Vector2(-60f, 44f));
        resetAll.onClick.AddListener(() => { lab.ResetAll(); ShowToast("All features reset"); });

        footerCount = F.Text("Count", panel, "", 11f, F.TextDim, TextAlignmentOptions.Right);
        F.SetAnchors(footerCount.rectTransform, new Vector2(1f, 0f), new Vector2(1f, 0f), new Vector2(-56f, 12f), new Vector2(-8f, 44f));

        //  Empty state (covers the list area while no vehicle).
        Image empty = F.Panel("EmptyState", panel, new Color(0f, 0f, 0f, .5f));
        F.SetAnchors(empty.rectTransform, new Vector2(0f, 0f), new Vector2(1f, 1f), new Vector2(134f, 56f), new Vector2(-8f, -108f));
        emptyState = empty.gameObject.AddComponent<CanvasGroup>();
        TextMeshProUGUI emptyText = F.Text("Text", empty.transform, "Spawn a vehicle to begin", 18f, F.TextMain, TextAlignmentOptions.Center, FontStyles.Bold);
        F.Fill(emptyText.rectTransform);

        //  FEATURES toggle button (outside the panel, top-right of screen).
        featuresButton = F.TextButton("FeaturesButton", transform, "FEATURES  [TAB]", F.PanelBg, F.Accent, 14f);
        RectTransform fb = (RectTransform)featuresButton.transform;
        F.SetAnchors(fb, new Vector2(1f, 1f), new Vector2(1f, 1f), Vector2.zero, Vector2.zero);
        fb.sizeDelta = new Vector2(150f, 40f);
        fb.anchoredPosition = new Vector2(-92f, -36f);
        featuresButton.onClick.AddListener(() => SetPanelVisible(true));

        //  Toast (bottom center).
        Image toastBg = F.Panel("Toast", transform, F.PanelBg);
        F.SetAnchors(toastBg.rectTransform, new Vector2(.5f, 0f), new Vector2(.5f, 0f), Vector2.zero, Vector2.zero);
        toastBg.rectTransform.sizeDelta = new Vector2(420f, 44f);
        toastBg.rectTransform.anchoredPosition = new Vector2(0f, 48f);
        toast = F.Text("Text", toastBg.transform, "", 14f, F.TextMain, TextAlignmentOptions.Center);
        F.Fill(toast.rectTransform);
        toastBg.gameObject.AddComponent<CanvasGroup>().alpha = 0f;

        //  Bottom-center selector bar container — Task 16 fills it.
        SelectorBarRoot = F.Rect("SelectorBar", transform);
        F.SetAnchors(SelectorBarRoot, new Vector2(.5f, 0f), new Vector2(.5f, 0f), Vector2.zero, Vector2.zero);
        SelectorBarRoot.sizeDelta = new Vector2(900f, 96f);
        SelectorBarRoot.anchoredPosition = new Vector2(0f, 110f);

    }

    //  ---------------------------------------------------------------- rows

    private void BuildRows() {

        for (int i = 0; i < lab.Entries.Count; i++)
            rows.Add(BuildRow(lab.Entries[i]));

    }

    private Row BuildRow(RCCP_FeatureLabEntry entry) {

        Row row = new Row();
        row.entry = entry;

        Image rowBg = F.Panel("Row_" + entry.id, listContent, F.RowBg);
        row.root = rowBg.rectTransform;
        row.group = rowBg.gameObject.AddComponent<CanvasGroup>();
        F.VLayout(row.root, 2f, 6, true);

        //  Header line: name + inline control/value + expand button behavior.
        RectTransform head = F.Rect("Head", row.root);
        head.gameObject.AddComponent<LayoutElement>().preferredHeight = 34f;

        TextMeshProUGUI name = F.Text("Name", head, entry.name, 15f, F.TextMain);
        name.enableAutoSizing = true;
        name.fontSizeMin = 11f;
        name.fontSizeMax = 15f;
        name.textWrappingMode = TextWrappingModes.NoWrap;
        F.SetAnchors(name.rectTransform, new Vector2(0f, 0f), new Vector2(.62f, 1f), new Vector2(6f, 0f), Vector2.zero);

        Button expandBtn = F.NoNavigation(head.gameObject.AddComponent<Button>());
        expandBtn.onClick.AddListener(() => ToggleCard(row));

        //  Kind-specific inline control on the right side of the head line.
        RectTransform ctrl = F.Rect("Ctrl", head);
        F.SetAnchors(ctrl, new Vector2(.62f, 0f), new Vector2(1f, 1f), Vector2.zero, new Vector2(-6f, 0f));

        switch (entry) {

            case RCCP_FeatureLabToggle t: {

                row.toggle = F.Pill("Toggle", ctrl);
                RectTransform tr = (RectTransform)row.toggle.transform;
                F.SetAnchors(tr, new Vector2(1f, .5f), new Vector2(1f, .5f), Vector2.zero, Vector2.zero);
                tr.sizeDelta = new Vector2(56f, 26f);   //  SetAnchors zeroes the offsets — with point anchors that IS the size; restore the pill rect or the track renders 0×0.
                tr.anchoredPosition = new Vector2(-30f, 0f);
                row.toggle.onValueChanged.AddListener(x => { if (!row.suppress && lab.HasVehicle) { t.set(lab.Context, x); RefreshRow(row); } });
                break;

            }

            case RCCP_FeatureLabSlider s: {

                row.valueLabel = F.Text("Value", ctrl, "", 14f, F.Accent, TextAlignmentOptions.Right);
                row.valueLabel.enableAutoSizing = true;
                row.valueLabel.fontSizeMin = 8f;
                row.valueLabel.fontSizeMax = 14f;
                row.valueLabel.textWrappingMode = TextWrappingModes.NoWrap;
                F.Fill(row.valueLabel.rectTransform);
                row.slider = F.HSlider("Slider", row.root);
                row.slider.gameObject.AddComponent<LayoutElement>().preferredHeight = 22f;
                row.slider.minValue = s.min;
                row.slider.maxValue = s.max;
                row.slider.onValueChanged.AddListener(x => { if (!row.suppress && lab.HasVehicle) { s.set(lab.Context, x); UpdateSliderLabel(row, s, x); } });
                break;

            }

            case RCCP_FeatureLabEnum e: {

                Button prev = F.TextButton("Prev", ctrl, "<", F.RowBg, F.TextMain, 14f);
                F.SetAnchors(((RectTransform)prev.transform), new Vector2(0f, .5f), new Vector2(0f, .5f), Vector2.zero, Vector2.zero);
                ((RectTransform)prev.transform).sizeDelta = new Vector2(26f, 26f);
                ((RectTransform)prev.transform).anchoredPosition = new Vector2(13f, 0f);

                row.enumLabel = F.Text("EnumValue", ctrl, "", 14f, F.TextMain, TextAlignmentOptions.Center, FontStyles.Bold);
                row.enumLabel.enableAutoSizing = true;
                row.enumLabel.fontSizeMin = 8f;
                row.enumLabel.fontSizeMax = 14f;
                row.enumLabel.textWrappingMode = TextWrappingModes.NoWrap;
                F.SetAnchors(row.enumLabel.rectTransform, new Vector2(0f, 0f), new Vector2(1f, 1f), new Vector2(28f, 0f), new Vector2(-28f, 0f));

                Button next = F.TextButton("Next", ctrl, ">", F.RowBg, F.TextMain, 14f);
                F.SetAnchors(((RectTransform)next.transform), new Vector2(1f, .5f), new Vector2(1f, .5f), Vector2.zero, Vector2.zero);
                ((RectTransform)next.transform).sizeDelta = new Vector2(26f, 26f);
                ((RectTransform)next.transform).anchoredPosition = new Vector2(-13f, 0f);

                prev.onClick.AddListener(() => CycleEnum(row, e, -1));
                next.onClick.AddListener(() => CycleEnum(row, e, +1));
                break;

            }

            case RCCP_FeatureLabAction a: {

                bool hasStatus = a.status != null;
                if (hasStatus) {

                    row.statusLabel = F.Text("Status", ctrl, "", 12f, F.TextDim, TextAlignmentOptions.Left);
                    row.statusLabel.enableAutoSizing = true;
                    row.statusLabel.fontSizeMin = 8f;
                    row.statusLabel.fontSizeMax = 12f;
                    row.statusLabel.textWrappingMode = TextWrappingModes.NoWrap;
                    F.SetAnchors(row.statusLabel.rectTransform, new Vector2(0f, 0f), new Vector2(.45f, 1f), Vector2.zero, Vector2.zero);

                }

                Button run = F.TextButton("Run", ctrl, a.buttonLabel, new Color(1f, .478f, 0f, .18f), F.Accent, 13f);
                F.SetAnchors(((RectTransform)run.transform), new Vector2(hasStatus ? .45f : 0f, .1f), new Vector2(1f, .9f), Vector2.zero, new Vector2(-4f, 0f));
                run.onClick.AddListener(() => {

                    if (!lab.HasVehicle)
                        return;

                    try {

                        a.invoke(lab.Context);
                        ShowToast(entry.name + " — done");

                    } catch (System.Exception ex) {

                        ShowToast(entry.name + " failed: " + ex.Message);

                    }

                    RefreshRow(row);

                });
                break;

            }

            case RCCP_FeatureLabReadout _: {

                row.valueLabel = F.Text("Value", ctrl, "", 14f, F.TextDim, TextAlignmentOptions.Right);
                row.valueLabel.enableAutoSizing = true;
                row.valueLabel.fontSizeMin = 8f;
                row.valueLabel.fontSizeMax = 14f;
                row.valueLabel.textWrappingMode = TextWrappingModes.NoWrap;
                F.Fill(row.valueLabel.rectTransform);
                break;

            }

        }

        //  Expandable card: description + reason + hint + reset.
        Image card = F.Panel("Card", row.root, F.CardBg);
        row.card = card.rectTransform;
        F.VLayout(row.card, 4f, 8, true);

        TextMeshProUGUI desc = F.Text("Desc", row.card, entry.description, 13f, F.TextDim);
        desc.gameObject.AddComponent<LayoutElement>().minHeight = 30f;

        if (entry is RCCP_FeatureLabReadout) {

            row.cardReadout = F.Text("ReadoutValue", row.card, "", 13f, F.TextMain);
            row.cardReadout.gameObject.AddComponent<LayoutElement>().minHeight = 20f;
            row.cardReadout.gameObject.SetActive(false);

        }

        row.cardReason = F.Text("Reason", row.card, "", 12f, new Color(1f, .6f, .3f, 1f));
        row.cardReason.gameObject.AddComponent<LayoutElement>().minHeight = 16f;

        if (!string.IsNullOrEmpty(entry.vehicleHint)) {

            TextMeshProUGUI hint = F.Text("Hint", row.card, "BEST TRIED WITH: " + entry.vehicleHint, 11f, F.Accent, TextAlignmentOptions.Left, FontStyles.Bold);
            hint.gameObject.AddComponent<LayoutElement>().minHeight = 16f;

        }

        Button reset = F.TextButton("Reset", row.card, "RESET", F.RowBg, F.TextMain, 12f);
        reset.gameObject.AddComponent<LayoutElement>().preferredHeight = 26f;
        reset.onClick.AddListener(() => { if (lab.ResetEntry(entry)) { ShowToast(entry.name + " reset"); RefreshRow(row); } });
        if (entry is RCCP_FeatureLabReadout)
            reset.gameObject.SetActive(false);

        row.card.gameObject.SetActive(false);
        return row;

    }

    //  ---------------------------------------------------------------- behaviour

    public void SetPanelVisible(bool visible) {

        panelVisible = visible;

        //  A hidden panel must not hold keyboard focus — an invisible focused search
        //  field would keep eating WASD as text.
        if (!visible && search != null && search.isFocused) {

            search.DeactivateInputField();

            if (EventSystem.current != null && EventSystem.current.currentSelectedGameObject == search.gameObject)
                EventSystem.current.SetSelectedGameObject(null);

        }

    }

    private void ClearSearch() {

        search.SetTextWithoutNotify("");
        search.DeactivateInputField();

        if (EventSystem.current != null && EventSystem.current.currentSelectedGameObject == search.gameObject)
            EventSystem.current.SetSelectedGameObject(null);

        ApplyFilter();

    }

    private void SelectCategory(RCCP_FeatureLabCategory category) {

        currentCategory = category;

        if (expanded != null)
            expanded.card.gameObject.SetActive(false);

        expanded = null;

        for (int i = 0; i < railButtons.Count; i++) {

            Image img = railButtons[i].targetGraphic as Image;
            img.color = i == (int)category ? new Color(1f, .478f, 0f, .22f) : F.RowBg;

        }

        ApplyFilter();

    }

    private void ApplyFilter() {

        string q = search.text == null ? "" : search.text.Trim().ToLowerInvariant();
        bool searching = q.Length > 0;
        int visible = 0;

        //  Covers every text-change path, including SetTextWithoutNotify + SelectCategory.
        if (searchClear != null)
            searchClear.gameObject.SetActive(search.text.Length > 0);

        for (int i = 0; i < rows.Count; i++) {

            Row row = rows[i];
            bool show = searching
                ? row.entry.name.ToLowerInvariant().Contains(q) || row.entry.id.Contains(q) || row.entry.description.ToLowerInvariant().Contains(q)
                : row.entry.category == currentCategory;

            row.root.gameObject.SetActive(show);

            if (show)
                visible++;

            if (!show && row.card.gameObject.activeSelf) {

                row.card.gameObject.SetActive(false);

                if (expanded == row)
                    expanded = null;

            }

        }

        footerCount.text = visible + " / " + rows.Count;
        RefreshVisible();

    }

    private void ToggleCard(Row row) {

        bool opening = !row.card.gameObject.activeSelf;

        if (expanded != null && expanded != row)
            expanded.card.gameObject.SetActive(false);

        row.card.gameObject.SetActive(opening);
        expanded = opening ? row : null;
        RefreshRow(row);

    }

    private void CycleEnum(Row row, RCCP_FeatureLabEnum e, int dir) {

        if (!lab.HasVehicle)
            return;

        int count = e.labels.Length;
        int next = (e.get(lab.Context) + dir + count) % count;
        e.set(lab.Context, next);
        RefreshRow(row);

    }

    private void UpdateSliderLabel(Row row, RCCP_FeatureLabSlider s, float value) {

        row.valueLabel.text = value.ToString(s.format) + (s.unit.Length > 0 ? " " + s.unit : "");

    }

    //  ---------------------------------------------------------------- refresh

    private void RefreshAll() {

        emptyState.alpha = lab.HasVehicle ? 0f : 1f;
        emptyState.blocksRaycasts = !lab.HasVehicle;
        RefreshVisible();

    }

    private void RefreshVisible() {

        for (int i = 0; i < rows.Count; i++) {

            if (rows[i].root.gameObject.activeSelf)
                RefreshRow(rows[i]);

        }

    }

    private void RefreshRow(Row row) {

        bool available = lab.HasVehicle && row.entry.IsAvailable(lab.Context);
        row.group.alpha = available ? 1f : .45f;
        row.group.interactable = available;
        row.cardReason.text = available ? "" : row.entry.availabilityReason;

        if (!available)
            return;

        row.suppress = true;

        try {

            switch (row.entry) {

                case RCCP_FeatureLabToggle t:
                    row.toggle.SetIsOnWithoutNotify(t.get(lab.Context));
                    break;

                case RCCP_FeatureLabSlider s:
                    float v = s.get(lab.Context);
                    row.slider.SetValueWithoutNotify(v);
                    UpdateSliderLabel(row, s, v);
                    break;

                case RCCP_FeatureLabEnum e:
                    int index = Mathf.Clamp(e.get(lab.Context), 0, e.labels.Length - 1);
                    row.enumLabel.text = e.labels[index];
                    break;

                case RCCP_FeatureLabAction a:
                    if (a.status != null && row.statusLabel != null)
                        row.statusLabel.text = a.status(lab.Context);
                    break;

                case RCCP_FeatureLabReadout ro:
                    string val = ro.read(lab.Context);
                    if (val.Length > 24) {

                        row.valueLabel.text = "Click to read >";
                        if (row.cardReadout != null) {

                            row.cardReadout.text = val;
                            row.cardReadout.gameObject.SetActive(true);

                        }

                    } else {

                        row.valueLabel.text = val;
                        if (row.cardReadout != null) {

                            row.cardReadout.text = "";
                            row.cardReadout.gameObject.SetActive(false);

                        }

                    }
                    break;

            }

        } catch (System.Exception ex) {

            row.cardReason.text = "binding error: " + ex.Message;

        }

        row.suppress = false;

    }

    //  ---------------------------------------------------------------- toast

    public void ShowToast(string message) {

        if (toast == null)
            return;

        if (toastRoutine != null)
            StopCoroutine(toastRoutine);

        toastRoutine = StartCoroutine(ToastRoutine(message));

    }

    private IEnumerator ToastRoutine(string message) {

        toast.text = message;
        CanvasGroup g = toast.transform.parent.GetComponent<CanvasGroup>();
        g.alpha = 1f;
        //  Unscaled — toasts must work at any timeScale (V2.57 TS-05 convention).
        yield return new WaitForSecondsRealtime(2.2f);

        while (g.alpha > 0f) {

            g.alpha -= Time.unscaledDeltaTime * 2f;
            yield return null;

        }

    }

}
