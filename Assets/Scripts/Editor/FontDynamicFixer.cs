using UnityEngine;
using UnityEditor;
using TMPro;

public class FontDynamicFixer : EditorWindow
{
    [MenuItem("LAST BRAKE/10. 폰트 Dynamic 모드로 전환")]
    public static void SetFontDynamic()
    {
        // 프로젝트 내 모든 TMP_FontAsset 찾기
        string[] guids = AssetDatabase.FindAssets("t:TMP_FontAsset");
        int count = 0;

        foreach (string guid in guids)
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);

            // LiberationSans 기본 폰트는 건드리지 않음
            if (path.Contains("LiberationSans")) continue;

            TMP_FontAsset font = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>(path);
            if (font == null) continue;

            // Dynamic 모드로 전환 (없는 글자 자동 추가)
            font.atlasPopulationMode = AtlasPopulationMode.Dynamic;
            EditorUtility.SetDirty(font);
            count++;
            Debug.Log($"Dynamic 전환: {path}");
        }

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

        EditorUtility.DisplayDialog("완료",
            $"{count}개 폰트 에셋 → Dynamic 모드 전환 완료!\n\n" +
            "이제 사각형 없이 모든 글자가 자동으로 표시됩니다.\n\n" +
            "▶ 버튼으로 확인해보세요.", "확인");
    }
}
