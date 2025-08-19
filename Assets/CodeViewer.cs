using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class CodeViewer : MonoBehaviour
{
    [System.Serializable]
    public class CodeGroup
    {
        [Header("UI Elements")]
        public GameObject panel;            // Panel that contains the scroll view
        public TMP_Text textComponent;      // TextMeshProUGUI for display-only code
        public TextAsset codeFile;          // Optional: source file
        [TextArea(6, 20)]
        public string rawCode;              // Unformatted code, used for copy/save

        [Header("Scroll View references")]
        public RectTransform viewport;      // ScrollRect viewport
        public RectTransform content;       // ScrollRect content (parent of text)

        [Header("Layout options")]
        public bool wrapText = false;       // true = vertical scrolling only; false = also horizontal scroll
        public float padding = 16f;         // extra pixels added to measured size
    }

    [Header("Code Groups (3)")]
    public CodeGroup group1;
    public CodeGroup group2;
    public CodeGroup group3;

    [Header("Styling (hex colors)")]
    public string keywordColor = "#569CD6";  // blue
    public string typeColor    = "#4EC9B0";  // teal/number
    public string stringColor  = "#D69D85";  // string color
    public string commentColor = "#6A9955";  // green
    public string preprocColor = "#C586C0";  // purple

    [Header("UI tuning")]
    public int fontSize = 18;

    void Start()
    {
        SetupGroup(group1);
        SetupGroup(group2);
        SetupGroup(group3);
    }

    void SetupGroup(CodeGroup group)
    {
        if (group == null) return;

        if (group.codeFile != null)
            group.rawCode = group.codeFile.text;

        if (group.textComponent == null || group.content == null || group.viewport == null)
        {
            Debug.LogWarning("CodeViewer: Missing Text/Content/Viewport reference in a group.");
            return;
        }

        SetupTMPText(group.textComponent);

        // Set highlighted text
        group.textComponent.text = HighlightCPP(group.rawCode ?? "");
        group.textComponent.ForceMeshUpdate();

        // Size the content based on text length (deferred to ensure layout has valid sizes)
        if (group.panel != null) group.panel.SetActive(true); // temporarily show to get correct viewport size
        StartCoroutine(DeferredResize(group));

        // Start hidden if you like
        if (group.panel != null) group.panel.SetActive(false);
    }

    IEnumerator DeferredResize(CodeGroup group)
    {
        // Wait a couple of frames so the Canvas/Layouts settle
        yield return null;
        yield return null;
        ResizeContentToText(group);
    }

    void SetupTMPText(TMP_Text tmp)
    {
        tmp.richText = true;
        tmp.fontSize = fontSize;
        tmp.alignment = TextAlignmentOptions.TopLeft;
        tmp.textWrappingMode = TextWrappingModes.NoWrap;    // default; per-group override in ResizeContentToText
        tmp.overflowMode = TextOverflowModes.Overflow;
    }

    void ResizeContentToText(CodeGroup group)
    {
        if (group == null || group.textComponent == null || group.content == null || group.viewport == null) return;

        var text = group.textComponent;
        var content = group.content;
        var textRT = text.rectTransform;
        var vp = group.viewport;

        // Ensure top-left growth is predictable
        content.pivot = new Vector2(0f, 1f);
        content.anchorMin = new Vector2(0f, 1f);
        content.anchorMax = new Vector2(0f, 1f);
        textRT.pivot = new Vector2(0f, 1f);
        textRT.anchorMin = new Vector2(0f, 1f);
        textRT.anchorMax = new Vector2(0f, 1f);

        if (group.wrapText)
        {
            // Vertical scroll only: fix width to viewport, compute needed height
            text.textWrappingMode = TextWrappingModes.Normal;
            text.overflowMode = TextOverflowModes.Overflow;

            float targetWidth = Mathf.Max(0.0f, vp.rect.width);
            // Set content (and text) width to viewport width
            SetWidth(content, targetWidth);
            SetWidth(textRT, targetWidth);

            // Measure height for that width
            text.ForceMeshUpdate();
            Vector2 pref = text.GetPreferredValues(text.text, targetWidth, Mathf.Infinity);

            float targetHeight = Mathf.Ceil(pref.y) + group.padding;
            SetHeight(content, targetHeight);
            SetHeight(textRT, targetHeight);
        }
        else
        {
            // Horizontal + vertical scroll: no wrapping; measure required width and height
            text.textWrappingMode = TextWrappingModes.NoWrap;
            text.overflowMode = TextOverflowModes.Overflow;

            text.ForceMeshUpdate();
            Vector2 pref = text.GetPreferredValues(text.text);

            float targetWidth  = Mathf.Ceil(pref.x) + group.padding;
            float targetHeight = Mathf.Ceil(pref.y) + group.padding;

            SetWidth(content, targetWidth);
            SetHeight(content, targetHeight);
            SetWidth(textRT, targetWidth);
            SetHeight(textRT, targetHeight);
        }

        // Optional: snap to top-left so you see the start of the code
        var scroll = group.viewport.GetComponentInParent<ScrollRect>();
        if (scroll != null)
        {
            // Content anchored top-left; normalized position (1,1) is top-left
            scroll.normalizedPosition = new Vector2(0f, 1f);
        }
    }

    void SetWidth(RectTransform rt, float w)
    {
        rt.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, w);
    }

    void SetHeight(RectTransform rt, float h)
    {
        rt.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, h);
    }

    // --- Panel control ---
    public void ShowPanel1() => ShowPanel(group1);
    public void ShowPanel2() => ShowPanel(group2);
    public void ShowPanel3() => ShowPanel(group3);

    public void HidePanel1() => HidePanel(group1);
    public void HidePanel2() => HidePanel(group2);
    public void HidePanel3() => HidePanel(group3);

    void ShowPanel(CodeGroup group)
    {
        if (group?.panel != null)
        {
            group.panel.SetActive(true);
            // Recalculate in case viewport size is different when shown
            StartCoroutine(DeferredResize(group));
        }
    }

    void HidePanel(CodeGroup group)
    {
        if (group?.panel != null)
            group.panel.SetActive(false);
    }

    // --- Copy ---
    public void CopyGroup1() => CopyRawCode(group1);
    public void CopyGroup2() => CopyRawCode(group2);
    public void CopyGroup3() => CopyRawCode(group3);

    void CopyRawCode(CodeGroup group)
    {
        if (group == null) return;
        GUIUtility.systemCopyBuffer = group.rawCode ?? "";
        Debug.Log("Copied raw code to clipboard.");
    }

    // --- Save ---
    public void SaveGroup1() => SaveRawCode(group1, "code1.txt");
    public void SaveGroup2() => SaveRawCode(group2, "code2.txt");
    public void SaveGroup3() => SaveRawCode(group3, "code3.txt");

    void SaveRawCode(CodeGroup group, string filename)
    {
        if (group == null) return;

        string tmpPath = Path.Combine(Application.temporaryCachePath, filename);
        File.WriteAllText(tmpPath, group.rawCode ?? "");

        NativeFilePicker.ExportFile(tmpPath, (bool success) =>
        {
            Debug.Log($"NativeFilePicker Export finished: {success}");
        });
    }

    // --- Simple C++ syntax highlighter ---
    string HighlightCPP(string code)
    {
        if (string.IsNullOrEmpty(code)) return "";

        var placeholders = new List<string>();
        var coloredRepls = new List<string>();
        int idx = 0;

        string Store(Match m, string color)
        {
            string original = m.Value;
            string colored = $"<color={color}><noparse>{original}</noparse></color>";
            string token = $"__PH_{idx}__";
            placeholders.Add(token);
            coloredRepls.Add(colored);
            idx++;
            return token;
        }

        code = Regex.Replace(code, @"/\*[\s\S]*?\*/", m => Store(m, commentColor));
        code = Regex.Replace(code, @"//.*?$", m => Store(m, commentColor), RegexOptions.Multiline);
        code = Regex.Replace(code, @"""(?:\\.|[^""\\])*""", m => Store(m, stringColor));
        code = Regex.Replace(code, @"'(?:\\.|[^'\\])+'", m => Store(m, stringColor));
        code = Regex.Replace(code, @"^\s*#.*$", m => Store(m, preprocColor), RegexOptions.Multiline);

        string[] keywords = new string[] {
            "int","float","double","if","else","for","while","return","class","struct","public","private","protected",
            "virtual","override","const","constexpr","static","template","typename","using","namespace","std","new","delete",
            "switch","case","break","continue","bool","void","unsigned","signed","long","short","sizeof","this","operator",
            "try","catch","throw","auto"
        };

        foreach (var kw in keywords)
            code = Regex.Replace(code, $@"\b{Regex.Escape(kw)}\b", $"<color={keywordColor}>{kw}</color>");

        code = Regex.Replace(code, @"\b\d+(\.\d+)?\b", m => $"<color={typeColor}>{m.Value}</color>");

        for (int i = 0; i < placeholders.Count; i++)
            code = code.Replace(placeholders[i], coloredRepls[i]);

        return code;
    }

        // --- Open Website Buttons ---
    public void OpenWebsite1()
    {
        Application.OpenURL("https://github.com/xhelaledin/RobotArmControl");
    }

    public void OpenWebsite2()
    {
        Application.OpenURL("https://github.com/rweather/arduinolibs");
    }

    public void OpenWebsite3()
    {
        Application.OpenURL("https://github.com/Octoate/ArduinoDES");
    }

    // Optional: call this if the viewport size changes at runtime (e.g., orientation, window resize)
    public void RefreshSizes()
    {
        ResizeContentToText(group1);
        ResizeContentToText(group2);
        ResizeContentToText(group3);
    }
}
