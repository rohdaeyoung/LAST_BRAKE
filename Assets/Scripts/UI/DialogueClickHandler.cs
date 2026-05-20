using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// 대화창 위에 올려두는 투명 버튼 핸들러.
/// Input System 종류(Old/New 모두)에 무관하게
/// UI Button 클릭으로 대화를 진행합니다.
/// </summary>
[RequireComponent(typeof(Button))]
public class DialogueClickHandler : MonoBehaviour
{
    private void Start()
    {
        var btn = GetComponent<Button>();
        btn.onClick.RemoveAllListeners();
        btn.onClick.AddListener(OnClick);
        Debug.Log("[DialogueClickHandler] 클릭 핸들러 등록 완료");
    }

    private void OnClick()
    {
        Debug.Log("[DialogueClickHandler] 대화창 클릭 → OnScreenTapped");
        DialogueManager.Instance?.OnScreenTapped();
    }
}
