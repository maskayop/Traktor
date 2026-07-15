//----------------------------------------------
//        Realistic Car Controller Pro
//
// Copyright © 2014 - 2026 BoneCracker Games
// https://www.bonecrackergames.com
// Ekrem Bugra Ozdoganlar
//
//----------------------------------------------

using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// Code-only uGUI/TMP element factory for the Feature Lab. Everything the panel shows
/// is built through these helpers so the canvas prefab ships near-empty and the whole
/// UI restyles from the color constants below.
/// </summary>
public static class RCCP_FeatureLabUIFactory {

    public static readonly Color Accent = new Color(1f, .478f, 0f, 1f);            //  #FF7A00
    public static readonly Color PanelBg = new Color(.04f, .045f, .06f, .9f);
    public static readonly Color RowBg = new Color(1f, 1f, 1f, .035f);
    public static readonly Color CardBg = new Color(0f, 0f, 0f, .38f);
    public static readonly Color TextMain = new Color(.92f, .92f, .95f, 1f);
    public static readonly Color TextDim = new Color(.62f, .64f, .7f, 1f);

    /// <summary>
    /// Kills keyboard/gamepad UI navigation. WASD/arrows must drive the vehicle — with default
    /// Automatic navigation, pressing W after clicking any control walks selection up the panel
    /// and lands on (and activates) the search field.
    /// </summary>
    public static T NoNavigation<T>(T selectable) where T : Selectable {

        Navigation nav = selectable.navigation;
        nav.mode = Navigation.Mode.None;
        selectable.navigation = nav;
        return selectable;

    }

    public static RectTransform Rect(string name, Transform parent) {

        GameObject go = new GameObject(name, typeof(RectTransform));
        go.layer = 5;   //  UI layer
        RectTransform rt = (RectTransform)go.transform;
        rt.SetParent(parent, false);
        return rt;

    }

    public static void Fill(RectTransform rt) {

        rt.anchorMin = Vector2.zero;
        rt.anchorMax = Vector2.one;
        rt.offsetMin = Vector2.zero;
        rt.offsetMax = Vector2.zero;

    }

    public static void SetAnchors(RectTransform rt, Vector2 min, Vector2 max, Vector2 offsetMin, Vector2 offsetMax) {

        rt.anchorMin = min;
        rt.anchorMax = max;
        rt.offsetMin = offsetMin;
        rt.offsetMax = offsetMax;

    }

    public static VerticalLayoutGroup VLayout(RectTransform rt, float spacing, int padding, bool childControlHeight) {

        VerticalLayoutGroup l = rt.gameObject.AddComponent<VerticalLayoutGroup>();
        l.spacing = spacing;
        l.padding = new RectOffset(padding, padding, padding, padding);
        l.childControlWidth = true;
        l.childControlHeight = childControlHeight;
        l.childForceExpandWidth = true;
        l.childForceExpandHeight = false;
        return l;

    }

    public static Image Panel(string name, Transform parent, Color color) {

        RectTransform rt = Rect(name, parent);
        Image img = rt.gameObject.AddComponent<Image>();
        img.color = color;
        return img;

    }

    public static TextMeshProUGUI Text(string name, Transform parent, string text, float size, Color color,
        TextAlignmentOptions align = TextAlignmentOptions.Left, FontStyles style = FontStyles.Normal) {

        RectTransform rt = Rect(name, parent);
        TextMeshProUGUI t = rt.gameObject.AddComponent<TextMeshProUGUI>();
        t.text = text;
        t.fontSize = size;
        t.color = color;
        t.alignment = align;
        t.fontStyle = style;
        t.raycastTarget = false;
        t.textWrappingMode = TextWrappingModes.Normal;
        return t;

    }

    public static Button TextButton(string name, Transform parent, string label, Color bg, Color textColor, float textSize = 16f) {

        Image img = Panel(name, parent, bg);
        Button b = img.gameObject.AddComponent<Button>();
        b.targetGraphic = img;

        ColorBlock colors = b.colors;
        colors.highlightedColor = new Color(1.15f, 1.15f, 1.15f, 1f);
        colors.pressedColor = new Color(.85f, .85f, .85f, 1f);
        b.colors = colors;

        TextMeshProUGUI t = Text("Label", img.transform, label, textSize, textColor, TextAlignmentOptions.Center);
        t.enableAutoSizing = true;
        t.fontSizeMin = 8f;
        t.fontSizeMax = textSize;
        t.textWrappingMode = TextWrappingModes.NoWrap;
        Fill(t.rectTransform);
        return NoNavigation(b);

    }

    public static Slider HSlider(string name, Transform parent) {

        RectTransform root = Rect(name, parent);
        Slider s = root.gameObject.AddComponent<Slider>();

        Image bg = Panel("Background", root, new Color(1f, 1f, 1f, .08f));
        SetAnchors(bg.rectTransform, new Vector2(0f, .5f), new Vector2(1f, .5f), new Vector2(0f, -3f), new Vector2(0f, 3f));

        RectTransform fillArea = Rect("Fill Area", root);
        SetAnchors(fillArea, new Vector2(0f, .5f), new Vector2(1f, .5f), new Vector2(0f, -3f), new Vector2(0f, 3f));

        Image fill = Panel("Fill", fillArea, Accent);
        Fill(fill.rectTransform);

        RectTransform handleArea = Rect("Handle Slide Area", root);
        Fill(handleArea);

        Image handle = Panel("Handle", handleArea, Color.white);
        handle.rectTransform.sizeDelta = new Vector2(18f, 0f);   //  y=0: Slider.UpdateVisuals stretches the non-drag axis — a nonzero y overflows the row.

        s.targetGraphic = handle;
        s.fillRect = fill.rectTransform;
        s.handleRect = handle.rectTransform;
        s.direction = Slider.Direction.LeftToRight;
        return NoNavigation(s);

    }

    public static Toggle Pill(string name, Transform parent) {

        RectTransform root = Rect(name, parent);
        root.sizeDelta = new Vector2(56f, 26f);
        Toggle t = root.gameObject.AddComponent<Toggle>();

        Image track = Panel("Track", root, new Color(1f, 1f, 1f, .12f));
        Fill(track.rectTransform);

        Image knob = Panel("Knob", root, Accent);
        SetAnchors(knob.rectTransform, new Vector2(1f, .5f), new Vector2(1f, .5f), Vector2.zero, Vector2.zero);
        knob.rectTransform.sizeDelta = new Vector2(20f, 20f);
        knob.rectTransform.anchoredPosition = new Vector2(-13f, 0f);

        t.targetGraphic = track;
        t.toggleTransition = Toggle.ToggleTransition.None;   //  knob never fades — the pill visual slides/tints it per isOn instead.
        root.gameObject.AddComponent<RCCP_FeatureLabPillVisual>().Bind(t, track, knob);
        return NoNavigation(t);

    }

    public static TMP_InputField Field(string name, Transform parent, string placeholder) {

        Image bg = Panel(name, parent, new Color(1f, 1f, 1f, .06f));
        TMP_InputField f = bg.gameObject.AddComponent<TMP_InputField>();

        RectTransform area = Rect("Text Area", bg.transform);
        Fill(area);
        area.offsetMin = new Vector2(12f, 4f);
        area.offsetMax = new Vector2(-12f, -4f);
        area.gameObject.AddComponent<RectMask2D>();

        TextMeshProUGUI ph = Text("Placeholder", area, placeholder, 15f, TextDim);
        Fill(ph.rectTransform);

        TextMeshProUGUI txt = Text("Text", area, "", 15f, TextMain);
        Fill(txt.rectTransform);

        f.textViewport = area;
        f.textComponent = txt;
        f.placeholder = ph;
        f.targetGraphic = bg;
        return NoNavigation(f);

    }

    public static ScrollRect Scroll(string name, Transform parent, out RectTransform content) {

        RectTransform root = Rect(name, parent);
        ScrollRect sr = root.gameObject.AddComponent<ScrollRect>();

        RectTransform viewport = Rect("Viewport", root);
        Fill(viewport);
        viewport.gameObject.AddComponent<RectMask2D>();
        Image vpImg = viewport.gameObject.AddComponent<Image>();
        vpImg.color = new Color(0f, 0f, 0f, .01f);   //  raycast catcher for drag-scroll

        content = Rect("Content", viewport);
        SetAnchors(content, new Vector2(0f, 1f), new Vector2(1f, 1f), Vector2.zero, Vector2.zero);
        content.pivot = new Vector2(.5f, 1f);
        content.gameObject.AddComponent<ContentSizeFitter>().verticalFit = ContentSizeFitter.FitMode.PreferredSize;

        sr.viewport = viewport;
        sr.content = content;
        sr.horizontal = false;
        sr.vertical = true;
        sr.movementType = ScrollRect.MovementType.Clamped;
        sr.scrollSensitivity = 25f;
        return sr;

    }

}

/// <summary>
/// Runtime visual driver for <see cref="RCCP_FeatureLabUIFactory.Pill"/>: the knob parks left in
/// gray while off and slides right turning accent-orange while on, with the track tinting to
/// match. Polls isOn in LateUpdate because RefreshRow writes state through SetIsOnWithoutNotify
/// (no event fires), and runs on unscaled time so the switch stays live at any timeScale.
/// Early-outs once settled so idle pills never dirty the canvas.
/// </summary>
public class RCCP_FeatureLabPillVisual : MonoBehaviour {

    private static readonly Color TrackOff = new Color(1f, 1f, 1f, .12f);
    private static readonly Color TrackOn = new Color(1f, .478f, 0f, .3f);
    private static readonly Color KnobOff = new Color(.55f, .57f, .62f, 1f);

    private const float KnobXOff = -43f;   //  20x20 knob inset 13 px from the pill's left edge (56 wide, right-anchored).
    private const float KnobXOn = -13f;    //  13 px from the right edge — matches the authored ON pose.
    private const float BlendSpeed = 8f;   //  1 / travel seconds.

    private Toggle toggle;
    private Image track;
    private Image knob;

    private float blend = -1f;   //  0 = off pose, 1 = on pose; negative = snap to current state on first frame.

    public void Bind(Toggle targetToggle, Image trackImage, Image knobImage) {

        toggle = targetToggle;
        track = trackImage;
        knob = knobImage;

    }

    private void OnEnable() {

        blend = -1f;   //  Rows (de)activate with category switches and search — snap, don't replay the slide.

    }

    private void LateUpdate() {

        if (toggle == null || track == null || knob == null)
            return;

        float target = toggle.isOn ? 1f : 0f;

        if (blend < 0f)
            blend = target;
        else if (Mathf.Approximately(blend, target))
            return;
        else
            blend = Mathf.MoveTowards(blend, target, Time.unscaledDeltaTime * BlendSpeed);

        float eased = blend * blend * (3f - 2f * blend);   //  smoothstep
        knob.rectTransform.anchoredPosition = new Vector2(Mathf.Lerp(KnobXOff, KnobXOn, eased), 0f);
        knob.color = Color.Lerp(KnobOff, RCCP_FeatureLabUIFactory.Accent, eased);
        track.color = Color.Lerp(TrackOff, TrackOn, eased);

    }

}
