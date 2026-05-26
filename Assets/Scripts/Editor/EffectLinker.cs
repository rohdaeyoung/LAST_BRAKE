using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine.SceneManagement;
using System.Reflection;

/// <summary>
/// LAST BRAKE - FX / CG / OBJ / StatHUD 스프라이트 전체 자동 연결 (PDF 스펙 기반)
/// 메뉴 20: 효과 스프라이트 전체 연결
/// </summary>
public class EffectLinker
{
    // ─────────────────────────────────────────────────────────────
    //  경로 상수
    // ─────────────────────────────────────────────────────────────
    const string FX_PATH  = "Assets/Sprites/경진대회 이미지/Overlay_image/";
    const string CG_PATH  = "Assets/Sprites/경진대회 이미지/CG_image/";
    const string OBJ_PATH = "Assets/Sprites/경진대회 이미지/Object_image/";
    const string UI_PATH  = "Assets/Sprites/경진대회 이미지/UI_image/";

    // ─────────────────────────────────────────────────────────────
    //  연결 대상 씬
    // ─────────────────────────────────────────────────────────────
    static readonly string[] AllScenes =
    {
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
        "Assets/Scenes/10_TrueEnd.unity",
    };

    // ═════════════════════════════════════════════════════════════
    //  메뉴 20: 효과 전체 연결
    // ═════════════════════════════════════════════════════════════
    [MenuItem("LAST BRAKE/20. 효과(FX·CG·OBJ·HUD) 전체 연결 (경진대회)")]
    public static void LinkAllEffects()
    {
        if (!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo()) return;

        // ── 스프라이트 로드 ─────────────────────────────────
        // FX
        var sprRedWarning = Load<Sprite>(FX_PATH,  "FX_RedWarningOverlay");
        var sprGlitch     = Load<Sprite>(FX_PATH,  "FX_GlitchOverlay");
        var sprVignette   = Load<Sprite>(FX_PATH,  "FX_DarkVignette");
        var sprBlurPulse  = Load<Sprite>(FX_PATH,  "FX_BlurPulse");

        // CG
        var cgClubOffer    = Load<Sprite>(CG_PATH, "CG_Club_Offer");
        var cgSeoyeon      = Load<Sprite>(CG_PATH, "CG_Seoyeon_Confrontation");
        var cgCollapse     = Load<Sprite>(CG_PATH, "CG_Collapse");
        var cgHospital     = Load<Sprite>(CG_PATH, "CG_Hospital_Recovery");
        var cgBadEnd       = Load<Sprite>(CG_PATH, "CG_BadEnd_Arrest");

        // OBJ
        var objSmartphone  = Load<Sprite>(OBJ_PATH, "OBJ_Smartphone");
        var objClock       = Load<Sprite>(OBJ_PATH, "OBJ_Clock0730");
        var objPill        = Load<Sprite>(OBJ_PATH, "OBJ_PillBottle");
        var objChat        = Load<Sprite>(OBJ_PATH, "OBJ_ChatWindow");
        var objReport      = Load<Sprite>(OBJ_PATH, "OBJ_ReportPaper");

        // UI 스탯 아이콘
        var iconReason       = Load<Sprite>(UI_PATH,  "UI_StatIcon_Reason");
        var iconMental       = Load<Sprite>(UI_PATH,  "UI_StatIcon_Mental");
        var iconDependency   = Load<Sprite>(UI_PATH,  "UI_StatIcon_Dependency");
        var iconRelationship = Load<Sprite>(UI_PATH,  "UI_StatIcon_Relationship");
        var iconReality      = Load<Sprite>(UI_PATH,  "UI_StatIcon_Reality");

        // FourthWallBreak 얼굴 스프라이트 (TrueEnd 응시 연출용)
        const string CHAR_PATH = "Assets/Sprites/경진대회 이미지/Character_image/";
        var faceNormal    = Load<Sprite>(CHAR_PATH, "Doyun_Face_Normal");
        var faceAnxious   = Load<Sprite>(CHAR_PATH, "Doyun_Face_Anxious");
        var faceSurprised = Load<Sprite>(CHAR_PATH, "Doyun_Face_Surprised");
        var faceRegret    = Load<Sprite>(CHAR_PATH, "Doyun_Face_Regret");

        var log   = new System.Text.StringBuilder();
        int total = 0;

        // ── 씬별 연결 ──────────────────────────────────────
        foreach (var scenePath in AllScenes)
        {
            var scene = EditorSceneManager.OpenScene(scenePath, OpenSceneMode.Single);
            string sFile = System.IO.Path.GetFileName(scenePath);
            int n = 0;

            // ── FXManager ──────────────────────────────────
            n += LinkComponent<FXManager>(scene, sFile, "FXManager", fxm =>
            {
                SetField(fxm, "sprRedWarning", sprRedWarning);
                SetField(fxm, "sprGlitch",     sprGlitch);
                SetField(fxm, "sprVignette",   sprVignette);
                SetField(fxm, "sprBlurPulse",  sprBlurPulse);
            }, log);

            // ── CGViewer ───────────────────────────────────
            n += LinkComponent<CGViewer>(scene, sFile, "CGViewer", cgv =>
            {
                SetField(cgv, "cgClubOffer",           cgClubOffer);
                SetField(cgv, "cgSeoyeonConfrontation", cgSeoyeon);
                SetField(cgv, "cgCollapse",            cgCollapse);
                SetField(cgv, "cgHospitalRecovery",    cgHospital);
                SetField(cgv, "cgBadEndArrest",        cgBadEnd);
            }, log);

            // ── ObjectCutIn ────────────────────────────────
            n += LinkComponent<ObjectCutIn>(scene, sFile, "ObjectCutIn", obj =>
            {
                SetField(obj, "objSmartphone",  objSmartphone);
                SetField(obj, "objClock0730",   objClock);
                SetField(obj, "objPillBottle",  objPill);
                SetField(obj, "objChatWindow",  objChat);
                SetField(obj, "objReportPaper", objReport);
            }, log);

            // ── StatHUD ────────────────────────────────────
            n += LinkComponent<StatHUD>(scene, sFile, "StatHUD", hud =>
            {
                SetField(hud, "iconReason",       iconReason);
                SetField(hud, "iconMental",       iconMental);
                SetField(hud, "iconDependency",   iconDependency);
                SetField(hud, "iconRelationship", iconRelationship);
                SetField(hud, "iconReality",      iconReality);
            }, log);

            // ── FourthWallBreak (엔딩 씬 전용) ─────────────
            bool isEndingScene = scenePath.Contains("End") || scenePath.Contains("True");
            if (isEndingScene)
            {
                FourthWallBreak fwb = null;
                foreach (var root in scene.GetRootGameObjects())
                {
                    fwb = root.GetComponentInChildren<FourthWallBreak>(true);
                    if (fwb != null) break;
                }
                if (fwb != null)
                {
                    // 얼굴 스프라이트 (TrueEnd 응시 연출용)
                    SetField(fwb, "faceNormal",    faceNormal);
                    SetField(fwb, "faceAnxious",   faceAnxious);
                    SetField(fwb, "faceSurprised", faceSurprised);
                    SetField(fwb, "faceRegret",    faceRegret);
                    EditorUtility.SetDirty(fwb);
                    log.AppendLine($"  [FWB Face OK] {sFile} ← 얼굴 스프라이트 4종");
                    n++;
                }
            }

            total += n;
            EditorSceneManager.SaveScene(scene);
        }

        EditorSceneManager.OpenScene("Assets/Scenes/00_MainMenu.unity");
        AssetDatabase.Refresh();

        // ── 누락 스프라이트 보고 ───────────────────────────
        var missing = new System.Text.StringBuilder();
        CheckMissing(missing, sprRedWarning,    "FX_RedWarningOverlay");
        CheckMissing(missing, sprGlitch,        "FX_GlitchOverlay");
        CheckMissing(missing, sprVignette,      "FX_DarkVignette");
        CheckMissing(missing, sprBlurPulse,     "FX_BlurPulse");
        CheckMissing(missing, cgClubOffer,      "CG_Club_Offer");
        CheckMissing(missing, cgSeoyeon,        "CG_Seoyeon_Confrontation");
        CheckMissing(missing, cgCollapse,       "CG_Collapse");
        CheckMissing(missing, cgHospital,       "CG_Hospital_Recovery");
        CheckMissing(missing, cgBadEnd,         "CG_BadEnd_Arrest");
        CheckMissing(missing, objSmartphone,    "OBJ_Smartphone");
        CheckMissing(missing, objClock,         "OBJ_Clock0730");
        CheckMissing(missing, objPill,          "OBJ_PillBottle");
        CheckMissing(missing, objChat,          "OBJ_ChatWindow");
        CheckMissing(missing, objReport,        "OBJ_ReportPaper");
        CheckMissing(missing, iconReason,       "UI_StatIcon_Reason");
        CheckMissing(missing, iconMental,       "UI_StatIcon_Mental");
        CheckMissing(missing, iconDependency,   "UI_StatIcon_Dependency");
        CheckMissing(missing, iconRelationship, "UI_StatIcon_Relationship");
        CheckMissing(missing, iconReality,      "UI_StatIcon_Reality");
        CheckMissing(missing, faceNormal,       "Doyun_Face_Normal");
        CheckMissing(missing, faceAnxious,      "Doyun_Face_Anxious");
        CheckMissing(missing, faceSurprised,    "Doyun_Face_Surprised");
        CheckMissing(missing, faceRegret,       "Doyun_Face_Regret");

        string missStr = missing.Length > 0
            ? "\n\n⚠️ 누락 스프라이트:\n" + missing
            : "\n\n✅ 모든 스프라이트 연결 성공";

        EditorUtility.DisplayDialog("✅ 효과 연결 완료",
            $"총 {total}개 컴포넌트 연결\n{log}{missStr}", "확인");
        Debug.Log("[EffectLinker]\n" + log);
    }

    // ═════════════════════════════════════════════════════════════
    //  유틸리티
    // ═════════════════════════════════════════════════════════════

    /// <summary>씬에서 컴포넌트를 찾거나 새 GameObject에 추가 후 필드 설정</summary>
    static int LinkComponent<T>(
        UnityEngine.SceneManagement.Scene scene,
        string sFile,
        string goName,
        System.Action<T> setup,
        System.Text.StringBuilder log)
        where T : MonoBehaviour
    {
        T comp = null;
        foreach (var root in scene.GetRootGameObjects())
        {
            comp = root.GetComponentInChildren<T>(true);
            if (comp != null) break;
        }

        if (comp == null)
        {
            // 새 오브젝트 생성
            var go = new GameObject(goName);
            SceneManager.MoveGameObjectToScene(go, scene);
            comp = go.AddComponent<T>();
        }

        setup(comp);
        EditorUtility.SetDirty(comp);
        log.AppendLine($"  [{typeof(T).Name} OK] {sFile}");
        return 1;
    }

    static T Load<T>(string folder, string file) where T : Object
        => AssetDatabase.LoadAssetAtPath<T>(folder + file + ".png");

    static void SetField(object target, string fieldName, object value)
    {
        if (target == null) return;
        var f = target.GetType().GetField(fieldName,
            BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance);
        f?.SetValue(target, value);
    }

    static void CheckMissing(System.Text.StringBuilder sb, Object spr, string name)
    {
        if (spr == null) sb.AppendLine($"  [MISS] {name}.png");
    }
}
