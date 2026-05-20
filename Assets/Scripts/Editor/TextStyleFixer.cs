using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using TMPro;

public class TextStyleFixer : EditorWindow
{
    [MenuItem("LAST BRAKE/9. 메인메뉴 텍스트 진하게 수정")]
    public static void FixTextStyle()
    {
        if (!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo()) return;

        EditorSceneManager.OpenScene("Assets/Scenes/00_MainMenu.unity");

        var allTMP = Object.FindObjectsByType<TextMeshProUGUI>(FindObjectsSortMode.None);

        foreach (var tmp in allTMP)
        {
            string name = tmp.gameObject.name;

            if (name == "TitleText")
            {
                tmp.fontSize     = 72;
                tmp.fontStyle    = FontStyles.Bold;
                tmp.color        = Color.white;
                tmp.characterSpacing = 5f;
            }
            else if (name == "SubText")
            {
                tmp.fontSize  = 24;
                tmp.fontStyle = FontStyles.Normal;
                tmp.color     = new Color(0.9f, 0.9f, 0.9f);
            }
            else if (name == "Label") // 버튼 텍스트
            {
                tmp.fontSize  = 28;
                tmp.fontStyle = FontStyles.Bold;
                tmp.color     = Color.white;
            }

            // 공통: outline 효과로 더 진하게
            tmp.outlineWidth = 0.1f;
            tmp.outlineColor = new Color32(0, 0, 0, 180);

            EditorUtility.SetDirty(tmp);
        }

        EditorSceneManager.SaveScene(EditorSceneManager.GetActiveScene());
        AssetDatabase.Refresh();

        EditorUtility.DisplayDialog("완료",
            "텍스트 스타일 수정 완료!\n▶ 버튼으로 확인해보세요.", "확인");
    }
}
