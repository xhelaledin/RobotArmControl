using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
using TMPro;
using UnityEngine;

public class CodeViewer : MonoBehaviour
{
    [System.Serializable]
    public class CodeGroup
    {
        [Header("UI Elements")]
        public GameObject panel;               // <-- New: Panel container for the code
        public TMP_InputField inputField;      // Assign InputField for this group
        public TextAsset codeFile;             // Optional: assign text/cpp file
        [TextArea(6, 20)]
        public string rawCode;                 // Raw code text (no color tags)
    }

    [Header("Code Groups (3)")]
    public CodeGroup group1;
    public CodeGroup group2;
    public CodeGroup group3;

    [Header("Styling (hex colors)")]
    public string keywordColor = "#569CD6";     // blue
    public string typeColor    = "#4EC9B0";     // teal/number
    public string stringColor  = "#D69D85";     // string color
    public string commentColor = "#6A9955";     // green
    public string preprocColor = "#C586C0";     // purple

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

        if (group.inputField == null)
        {
            Debug.LogWarning("CodeViewer: inputField not assigned in a group.");
            return;
        }

        SetupInputField(group.inputField);
        group.inputField.text = HighlightCPP(group.rawCode ?? "");
        group.inputField.textComponent.ForceMeshUpdate();

        if (group.panel != null)
            group.panel.SetActive(false); // start hidden
    }

    void SetupInputField(TMP_InputField inputField)
    {
        inputField.readOnly = true;
        inputField.lineType = TMP_InputField.LineType.MultiLineNewline;
        inputField.contentType = TMP_InputField.ContentType.Standard;
        inputField.caretBlinkRate = 0f;
        inputField.textComponent.richText = true;
        inputField.textComponent.fontSize = fontSize;
        inputField.textComponent.alignment = TextAlignmentOptions.TopLeft;
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
            group.panel.SetActive(true);
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
}
