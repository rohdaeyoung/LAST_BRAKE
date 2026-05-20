using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using TMPro;

public class FontFixer : EditorWindow
{
    [MenuItem("LAST BRAKE/8. 전체 씬 한글 폰트 교체")]
    public static void FixAllFonts()
    {
        // NotoSansKR 폰트 에셋 찾기
        string[] guids = AssetDatabase.FindAssets("NotoSansKR t:TMP_FontAsset");
        if (guids.Length == 0)
        {
            EditorUtility.DisplayDialog("오류",
                "NotoSansKR TMP 폰트 에셋을 찾을 수 없습니다.\n" +
                "Font Asset Creator에서 Save를 먼저 해주세요.", "확인");
            return;
        }

        string fontPath = AssetDatabase.GUIDToAssetPath(guids[0]);
        TMP_FontAsset notoFont = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>(fontPath);
        Debug.Log($"폰트 에셋 찾음: {fontPath}");

        string[] scenePaths = {
            "Assets/Scenes/00_MainMenu.unity",
            "Assets/Scenes/01_Step1_Prologue.unity",
            "Assets/Scenes/02_Step2_Club.unity",
            "Assets/Scenes/03_Step3_Morning.unity",
            "Assets/Scenes/04_Step4_Party.unity",
            "Assets/Scenes/05_Step5_Collapse.unity",
            "Assets/Scenes/06_EndingReport.unity",
            "Assets/Scenes/07_GoodEnd.unity",
            "Assets/Scenes/08_NormalEnd.unity",
            "Assets/Scenes/09_BadEnd.unity",
            "Assets/Scenes/10_TrueEnd.unity"
        };

        int totalFixed = 0;

        foreach (string scenePath in scenePaths)
        {
            var scene = EditorSceneManager.OpenScene(scenePath);
            var allTMP = Object.FindObjectsByType<TextMeshProUGUI>(FindObjectsSortMode.None);

            foreach (var tmp in allTMP)
            {
                tmp.font = notoFont;
                EditorUtility.SetDirty(tmp);
                totalFixed++;
            }

            EditorSceneManager.SaveScene(scene);
        }

        AssetDatabase.Refresh();
        EditorSceneManager.OpenScene("Assets/Scenes/00_MainMenu.unity");

        EditorUtility.DisplayDialog("완료",
            $"총 {totalFixed}개 텍스트 → NotoSansKR 교체 완료!\n\n▶ 버튼으로 다시 실행해보세요.", "확인");
    }
}
