using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using TMPro;
using UnityEngine.UI;

public class LayoutFixer : EditorWindow
{
    [MenuItem("LAST BRAKE/10. 메인메뉴 레이아웃 수정")]
    public static void FixLayout()
    {
        if (!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo()) return;
        EditorSceneManager.OpenScene("Assets/Scenes/00_MainMenu.unity");

        var allTMP = Object.FindObjectsByType<TextMeshProUGUI>(FindObjectsSortMode.None);

        foreach (var tmp in allTMP)
        {
            var rt = tmp.GetComponent<RectTransform>();
            string name = tmp.gameObject.name;

            switch (name)
            {
                case "TitleText":
                    // 제목 위로 올리기
                    rt.anchoredPosition = new Vector2(0, 200);
                    rt.sizeDelta        = new Vector2(900, 220);
                    tmp.fontSize        = 72;
                    tmp.fontStyle       = FontStyles.Bold;
                    tmp.color           = Color.white;
                    break;

                case "SubText":
                    // 부제목 크게 + 잘 보이게
                    rt.anchoredPosition = new Vector2(0, 60);
                    rt.sizeDelta        = new Vector2(820, 100);
                    tmp.fontSize        = 26;
                    tmp.fontStyle       = FontStyles.Normal;
                    tmp.color           = new Color(0.85f, 0.85f, 0.85f);
                    tmp.outlineWidth    = 0.08f;
                    tmp.outlineColor    = new Color32(0, 0, 0, 200);
                    break;

                case "Label":
                    // 버튼 텍스트
                    tmp.fontSize  = 28;
                    tmp.fontStyle = FontStyles.Bold;
                    tmp.color     = Color.white;
                    break;
            }

            EditorUtility.SetDirty(tmp);
        }

        // 버튼 위치도 조정 (제목이 올라간 만큼 버튼도 올림)
        var buttons = Object.FindObjectsByType<Button>(FindObjectsSortMode.None);
        foreach (var btn in buttons)
        {
            var rt = btn.GetComponent<RectTransform>();
            if (btn.gameObject.name == "StartButton")
                rt.anchoredPosition = new Vector2(0, -80);
            else if (btn.gameObject.name == "QuitButton")
                rt.anchoredPosition = new Vector2(0, -180);

            EditorUtility.SetDirty(btn);
        }

        EditorSceneManager.SaveScene(EditorSceneManager.GetActiveScene());
        AssetDatabase.Refresh();

        EditorUtility.DisplayDialog("완료", "레이아웃 수정 완료!\n▶ 버튼으로 확인해보세요.", "확인");
    }
}
