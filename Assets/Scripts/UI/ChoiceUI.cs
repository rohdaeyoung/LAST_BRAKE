using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;
using System.Collections;
using System.Collections.Generic;

public class ChoiceUI : MonoBehaviour
{
    [SerializeField] private GameObject        panel;
    [SerializeField] private Button[]          choiceButtons;
    [SerializeField] private TextMeshProUGUI[] choiceTexts;
    [SerializeField] private Color             forcedRedColor          = new Color(0.9f, 0.2f, 0.2f);
    [SerializeField] private float             forcedClickAnimDuration = 0.3f;

    private Action<int> onButtonClicked;

    // Inspector 미연결 시 자동 탐색 (Bootstrap이 만든 오브젝트 포함)
    private void EnsureRefs()
    {
        if (panel == null)
            panel = gameObject; // ChoiceUI는 ChoicePanel 위에 붙어있음

        if (choiceButtons == null || choiceButtons.Length == 0)
        {
            var btns = new List<Button>();
            var txts = new List<TextMeshProUGUI>();

            // 자식 Button 오브젝트 찾기 (비활성 포함)
            foreach (Transform child in transform)
            {
                var btn = child.GetComponent<Button>();
                if (btn != null)
                {
                    btns.Add(btn);
                    // 버튼의 첫 번째 TextMeshProUGUI 자식을 텍스트로 사용
                    var txt = child.GetComponentInChildren<TextMeshProUGUI>(true);
                    txts.Add(txt);
                }
            }

            choiceButtons = btns.ToArray();
            choiceTexts   = txts.ToArray();
        }
    }

    public void Render(ChoiceData[] choices, Action<int> callback, bool isForcedMode)
    {
        EnsureRefs();
        onButtonClicked = callback;

        if (panel != null) panel.SetActive(true);

        // 버튼이 부족하면 동적 생성
        EnsureButtonCount(choices.Length);

        for (int i = 0; i < choiceButtons.Length; i++)
        {
            if (choiceButtons[i] == null) continue;

            bool visible = i < choices.Length;
            choiceButtons[i].gameObject.SetActive(visible);
            if (!visible) continue;

            int idx = i;
            if (choiceTexts[i] != null)
            {
                choiceTexts[i].text  = choices[i].label;
                choiceTexts[i].color = isForcedMode ? forcedRedColor : Color.white;
            }

            choiceButtons[i].interactable = true;
            choiceButtons[i].onClick.RemoveAllListeners();
            choiceButtons[i].onClick.AddListener(() => onButtonClicked?.Invoke(idx));

            // INT 조건 미달 → 회색 잠금
            if (choices[i].requiresMinINT &&
                StatManager.Instance != null &&
                StatManager.Instance.INT < choices[i].minINTValue)
            {
                choiceButtons[i].interactable = false;
                if (choiceTexts[i] != null) choiceTexts[i].color = Color.gray;
            }
        }
    }

    // 버튼이 부족할 경우 동적으로 추가
    private void EnsureButtonCount(int needed)
    {
        if (choiceButtons.Length >= needed) return;

        var btns = new List<Button>(choiceButtons);
        var txts = new List<TextMeshProUGUI>(choiceTexts);

        while (btns.Count < needed)
        {
            int idx = btns.Count;
            float yPos = 90f - idx * 100f;

            var btnGO = new GameObject($"ChoiceButton_{idx}");
            btnGO.transform.SetParent(transform, false);
            btnGO.AddComponent<Image>().color = new Color(0.15f, 0.15f, 0.15f);
            var btn = btnGO.AddComponent<Button>();
            var rt  = btnGO.GetComponent<RectTransform>();
            rt.anchoredPosition = new Vector2(0f, yPos);
            rt.sizeDelta        = new Vector2(640f, 80f);

            var lblGO = new GameObject("Label");
            lblGO.transform.SetParent(btnGO.transform, false);
            var lbl = lblGO.AddComponent<TextMeshProUGUI>();
            lbl.fontSize  = 22;
            lbl.color     = Color.white;
            lbl.alignment = TextAlignmentOptions.Center;
            var lRT = lblGO.GetComponent<RectTransform>();
            lRT.anchorMin = Vector2.zero;
            lRT.anchorMax = Vector2.one;
            lRT.offsetMin = lRT.offsetMax = Vector2.zero;

            btns.Add(btn);
            txts.Add(lbl);
        }

        choiceButtons = btns.ToArray();
        choiceTexts   = txts.ToArray();
    }

    public void DisableGoodChoices()
    {
        EnsureRefs();
        foreach (var btn in choiceButtons)
            if (btn != null) btn.interactable = false;
    }

    public void AnimateForcedClick(int index)
    {
        EnsureRefs();
        StartCoroutine(ForcedClickAnim(index));
    }

    private IEnumerator ForcedClickAnim(int index)
    {
        if (index >= choiceButtons.Length || choiceButtons[index] == null) yield break;

        var rt       = choiceButtons[index].GetComponent<RectTransform>();
        var original = rt.localScale;
        float t      = 0f;

        while (t < forcedClickAnimDuration)
        {
            t += Time.deltaTime;
            float scale = 1f + Mathf.Sin(t / forcedClickAnimDuration * Mathf.PI) * 0.15f;
            rt.localScale = original * scale;
            yield return null;
        }
        rt.localScale = original;
    }

    public void ApplyRedTint()
    {
        EnsureRefs();
        foreach (var txt in choiceTexts)
            if (txt != null) txt.color = forcedRedColor;
    }

    public void Hide()
    {
        if (panel != null) panel.SetActive(false);
    }
}
