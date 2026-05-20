using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using TMPro;

/// <summary>
/// NotoSansKR-Regular SDF를 TMP 기본 폰트로 설정하고
/// 전체 씬의 TextMeshProUGUI를 Regular로 교체합니다.
///
/// 수정 내역:
/// - Regular SDF를 Resources/Fonts & Materials/ 로 복사 (TMP 기본 탐색 경로)
/// - VariableFont SDF를 Resources에서 삭제
/// - File.Exists 상대경로 버그 수정 → AssetDatabase.LoadAssetAtPath 사용
/// - VariableFont 교체 로직 실제 동작하도록 구현
/// </summary>
public class FontSetupFixer : EditorWindow
{
    // 원본 Regular SDF 위치
    const string SRC_REGULAR = "Assets/TextMesh Pro/Fonts/NotoSansKR-Regular SDF.asset";

    // TMP가 기본으로 탐색하는 Resources 경로
    const string DST_REGULAR = "Assets/TextMesh Pro/Resources/Fonts & Materials/NotoSansKR-Regular SDF.asset";

    // 삭제할 VariableFont (Resources 안에 있는 것)
    const string DST_VARIABLE = "Assets/TextMesh Pro/Resources/Fonts & Materials/NotoSansKR-VariableFont_wght SDF.asset";

    [MenuItem("LAST BRAKE/13. 한글 폰트 완전 교체 (Regular로)")]
    public static void FixFontToRegular()
    {
        // ── 1) 원본 Regular SDF 에셋 확인 ──────────────────────────────
        var srcFont = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>(SRC_REGULAR);
        if (srcFont == null)
        {
            // GUID로 폴백 탐색
            string[] guids = AssetDatabase.FindAssets("NotoSansKR-Regular SDF t:TMP_FontAsset");
            if (guids.Length == 0)
            {
                EditorUtility.DisplayDialog("오류",
                    "NotoSansKR-Regular SDF 에셋을 찾을 수 없습니다.\n" +
                    "Assets/TextMesh Pro/Fonts/ 폴더를 확인하세요.", "확인");
                return;
            }
            SRC_REGULAR_PATH_OVERRIDE = AssetDatabase.GUIDToAssetPath(guids[0]);
            srcFont = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>(SRC_REGULAR_PATH_OVERRIDE);
            Debug.Log($"[FontSetupFixer] 폴백 경로로 발견: {SRC_REGULAR_PATH_OVERRIDE}");
        }

        string srcPath = string.IsNullOrEmpty(SRC_REGULAR_PATH_OVERRIDE) ? SRC_REGULAR : SRC_REGULAR_PATH_OVERRIDE;

        // ── 2) Resources/Fonts & Materials/ 에 Regular SDF 복사 ────────
        TMP_FontAsset regularFont;
        var existing = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>(DST_REGULAR);
        if (existing == null)
        {
            bool copied = AssetDatabase.CopyAsset(srcPath, DST_REGULAR);
            if (!copied)
            {
                EditorUtility.DisplayDialog("오류",
                    $"Regular SDF 복사 실패:\n{srcPath} → {DST_REGULAR}", "확인");
                return;
            }
            AssetDatabase.Refresh();
            regularFont = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>(DST_REGULAR);
            Debug.Log($"[FontSetupFixer] Regular SDF → Resources 복사 완료");
        }
        else
        {
            regularFont = existing;
            Debug.Log($"[FontSetupFixer] Resources에 Regular SDF 이미 존재, 재사용");
        }

        // ── 3) Dynamic 모드 설정 (모든 한글 자동 렌더링) ────────────────
        regularFont.atlasPopulationMode = AtlasPopulationMode.Dynamic;
        EditorUtility.SetDirty(regularFont);
        Debug.Log("[FontSetupFixer] Dynamic 모드 설정 완료");

        // 원본 srcFont도 Dynamic으로 설정
        srcFont.atlasPopulationMode = AtlasPopulationMode.Dynamic;
        EditorUtility.SetDirty(srcFont);

        // ── 4) TMP_Settings 기본 폰트 교체 ──────────────────────────────
        var settings = TMP_Settings.instance;
        if (settings != null)
        {
            var field = typeof(TMP_Settings).GetField("m_defaultFontAsset",
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            if (field != null)
            {
                field.SetValue(settings, regularFont);
                EditorUtility.SetDirty(settings);
                Debug.Log("[FontSetupFixer] TMP_Settings 기본 폰트 → NotoSansKR-Regular SDF 변경 완료");
            }
            else
            {
                Debug.LogWarning("[FontSetupFixer] TMP_Settings.m_defaultFontAsset 필드를 찾을 수 없음 (TMP 버전 확인 필요)");
            }
        }
        else
        {
            Debug.LogWarning("[FontSetupFixer] TMP_Settings.instance가 null입니다.");
        }

        // ── 5) Resources의 VariableFont SDF 삭제 ────────────────────────
        if (AssetDatabase.LoadAssetAtPath<TMP_FontAsset>(DST_VARIABLE) != null)
        {
            bool deleted = AssetDatabase.DeleteAsset(DST_VARIABLE);
            Debug.Log(deleted
                ? "[FontSetupFixer] VariableFont SDF (Resources) 삭제 완료"
                : "[FontSetupFixer] VariableFont SDF 삭제 실패 (수동 삭제 필요)");
        }

        // ── 6) 전체 씬 TextMeshProUGUI 교체 ─────────────────────────────
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
        int scenesProcessed = 0;

        // 현재 열린 씬 기록 (작업 후 복원)
        string originalScene = UnityEditor.SceneManagement.EditorSceneManager
            .GetActiveScene().path;

        foreach (string scenePath in scenePaths)
        {
            // ▶ 버그 수정: File.Exists(상대경로) 대신 AssetDatabase로 씬 존재 확인
            var sceneAsset = AssetDatabase.LoadAssetAtPath<SceneAsset>(scenePath);
            if (sceneAsset == null)
            {
                Debug.LogWarning($"[FontSetupFixer] 씬 없음: {scenePath}");
                continue;
            }

            var scene = EditorSceneManager.OpenScene(scenePath, OpenSceneMode.Single);
            scenesProcessed++;

            // 비활성 오브젝트(DialoguePanel 등)까지 포함
            var allTMP = Object.FindObjectsByType<TextMeshProUGUI>(
                FindObjectsInactive.Include, FindObjectsSortMode.None);

            foreach (var tmp in allTMP)
            {
                if (tmp.font != regularFont)
                {
                    tmp.font = regularFont;
                    EditorUtility.SetDirty(tmp);
                    totalFixed++;
                }
            }

            EditorSceneManager.SaveScene(scene);
        }

        // ── 7) 저장 및 갱신 ─────────────────────────────────────────────
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

        // 원래 씬 복원 (또는 MainMenu)
        string restoreScene = string.IsNullOrEmpty(originalScene)
            ? "Assets/Scenes/00_MainMenu.unity"
            : originalScene;
        if (AssetDatabase.LoadAssetAtPath<SceneAsset>(restoreScene) != null)
            EditorSceneManager.OpenScene(restoreScene, OpenSceneMode.Single);

        EditorUtility.DisplayDialog("완료",
            $"NotoSansKR-Regular SDF 교체 완료!\n\n" +
            $"· Regular SDF → Resources/Fonts & Materials/ 복사\n" +
            $"· Dynamic 모드 → 모든 한글 자동 렌더링\n" +
            $"· TMP 기본 폰트 → Regular로 변경\n" +
            $"· VariableFont (Resources) 삭제\n" +
            $"· 씬 {scenesProcessed}개 처리 / TMP {totalFixed}개 교체\n\n" +
            "▶ Play 눌러서 한글이 잘 나오는지 확인하세요!", "확인");
    }

    // 폴백 경로 임시 저장용
    static string SRC_REGULAR_PATH_OVERRIDE = "";
}
