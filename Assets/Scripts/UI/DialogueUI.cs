using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class DialogueUI : MonoBehaviour
{
    [SerializeField] private GameObject      panel;
    [SerializeField] private TextMeshProUGUI speakerText;
    [SerializeField] private TextMeshProUGUI dialogueText;
    [SerializeField] private Image           portrait;
    [SerializeField] private Color           monologueColor = new Color(0.85f, 0.85f, 1f);
    [SerializeField] private Color           normalColor    = Color.white;

    private string currentFullText;

    // Awake는 비활성 오브젝트에서 실행 안 됨 → Show()에서 lazy init
    private void EnsureRefs()
    {
        // 비활성 오브젝트도 검색 (Resources.FindObjectsOfTypeAll)
        if (panel == null)
        {
            foreach (var go in Resources.FindObjectsOfTypeAll<GameObject>())
                if (go.scene.IsValid() && go.name == "DialoguePanel") { panel = go; break; }
            // 그래도 없으면 자기 자신(DialogueUI가 DialoguePanel에 붙어있는 경우)
            if (panel == null) panel = gameObject;
        }

        if (speakerText == null)
        {
            foreach (var t in Resources.FindObjectsOfTypeAll<TextMeshProUGUI>())
                if (t.gameObject.scene.IsValid() && t.gameObject.name == "SpeakerName")
                { speakerText = t; break; }
        }

        if (dialogueText == null)
        {
            foreach (var t in Resources.FindObjectsOfTypeAll<TextMeshProUGUI>())
                if (t.gameObject.scene.IsValid() && t.gameObject.name == "DialogueText")
                { dialogueText = t; break; }
        }
    }

    public void Show(DialogueLine line)
    {
        EnsureRefs();

        if (panel != null) panel.SetActive(true);

        if (speakerText != null)
            speakerText.text = line.isMonologue ? string.Empty : line.speaker;

        if (portrait != null)
        {
            portrait.sprite  = line.portrait;
            portrait.enabled = line.portrait != null;
        }

        if (dialogueText != null)
        {
            dialogueText.color = line.isMonologue ? monologueColor : normalColor;
            dialogueText.fontStyle = line.isMonologue ? FontStyles.Italic : FontStyles.Normal;
        }

        currentFullText = line.text;
    }

    public void Hide()
    {
        if (panel != null) panel.SetActive(false);
    }

    public void SetText(string text)
    {
        if (dialogueText != null) dialogueText.text = text;
    }

    public void AppendChar(char c)
    {
        if (dialogueText != null) dialogueText.text += c;
    }

    public void ShowFullText(string text)
    {
        if (dialogueText != null) dialogueText.text = text;
    }
}
