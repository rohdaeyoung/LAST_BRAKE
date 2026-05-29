using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine.SceneManagement;
using System.Reflection;

/// <summary>
/// LAST BRAKE — 오디오 자동 연결 / 해제 Editor 도구
///
/// 메뉴 30: BGM 클립 연결  — BGMController의 세 클립 자동 연결
/// 메뉴 31: SFX 전체 연결 — SFXManager의 모든 클립 자동 연결
/// 메뉴 32: 오디오 연결 해제 — 모든 AudioClip 참조를 null 처리 (무음 상태로 복원)
/// </summary>
public class AudioLinker
{
    const string BGM_PATH = "Assets/Audio/BGM/";
    const string SFX_PATH = "Assets/Audio/SFX/";

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

    // ════════════════════════════════════════════════════════════════
    //  메뉴 30: BGM 연결
    // ════════════════════════════════════════════════════════════════
    [MenuItem("LAST BRAKE/30. BGM 클립 연결")]
    public static void LinkBGM()
    {
        if (!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo()) return;

        var clipNormal    = Load(BGM_PATH + "BGM_Normal_DarkAmbient_Loop.wav");
        var clipClub      = Load(BGM_PATH + "BGM_Club_Muffled_Loop.wav");
        var clipDistorted = Load(BGM_PATH + "BGM_Distorted_Risk_Loop.wav");

        int total = 0;
        foreach (var scenePath in AllScenes)
        {
            var scene = EditorSceneManager.OpenScene(scenePath, OpenSceneMode.Single);
            int n = LinkComp<BGMController>(scene, "BGMController", bgm =>
            {
                Set(bgm, "clipNormal",    clipNormal);
                Set(bgm, "clipClub",      clipClub);
                Set(bgm, "clipDistorted", clipDistorted);
            });
            total += n;
            EditorSceneManager.SaveScene(scene);
        }

        EditorSceneManager.OpenScene("Assets/Scenes/00_MainMenu.unity");
        AssetDatabase.Refresh();
        EditorUtility.DisplayDialog("✅ BGM 연결 완료", $"{total}개 씬에 BGM 클립 연결", "확인");
    }

    // ════════════════════════════════════════════════════════════════
    //  메뉴 31: SFX 전체 연결
    // ════════════════════════════════════════════════════════════════
    [MenuItem("LAST BRAKE/31. SFX 전체 연결")]
    public static void LinkSFX()
    {
        if (!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo()) return;

        // ── SFX 클립 로드 ──
        // UI
        var uiSelect    = Load(SFX_PATH + "UI/SFX_UI_Select.wav");
        var uiDanger    = Load(SFX_PATH + "UI/SFX_UI_DangerConfirm.wav");
        var uiForced    = Load(SFX_PATH + "UI/SFX_UI_ForcedClick.wav");
        var uiLocked    = Load(SFX_PATH + "UI/SFX_UI_Locked.wav");
        var uiStatReveal= Load(SFX_PATH + "UI/SFX_UI_StatReveal.wav");
        // FX
        var redWarning  = Load(SFX_PATH + "FX/SFX_RedWarning_Hit.wav");
        var glitchBurst = Load(SFX_PATH + "FX/SFX_Glitch_Burst.wav");
        var blurPulse   = Load(SFX_PATH + "FX/SFX_BlurPulse_Whoosh.wav");
        var msgReveal   = Load(SFX_PATH + "FX/SFX_Message_Reveal.wav");
        var desaturate  = Load(SFX_PATH + "FX/SFX_Desaturate_Drop.wav");
        // Body
        var hbRamp      = Load(SFX_PATH + "Body/SFX_Heartbeat_Ramp.wav");
        var hbSlow      = Load(SFX_PATH + "Body/SFX_Heartbeat_Slow.wav");
        var tinnitus    = Load(SFX_PATH + "Body/SFX_Tinnitus_Ring.wav");
        var breathPanic = Load(SFX_PATH + "Body/SFX_Breath_Panic.wav");
        var tremor      = Load(SFX_PATH + "Body/SFX_Electric_Tremor.wav");
        // Objects
        var pillRattle  = Load(SFX_PATH + "Objects/SFX_Pill_Rattle.wav");
        var pillOpen    = Load(SFX_PATH + "Objects/SFX_Pill_BottleOpen.wav");
        var glass       = Load(SFX_PATH + "Objects/SFX_Glass_Clink.wav");
        var doorKnock   = Load(SFX_PATH + "Objects/SFX_Door_Knock.wav");
        var report      = Load(SFX_PATH + "Objects/SFX_ReportPaper.wav");
        // Phone
        var phoneNotify = Load(SFX_PATH + "Phone/SFX_Phone_Notify.wav");
        var phoneSend   = Load(SFX_PATH + "Phone/SFX_Phone_Send.wav");
        var dial1393    = Load(SFX_PATH + "Phone/SFX_Phone_Dial1393.wav");
        // Ambient
        var ambClub     = Load(SFX_PATH + "Ambient/AMB_Club_Muffled_Loop.wav");
        var ambHospital = Load(SFX_PATH + "Ambient/AMB_Hospital_Room_Loop.wav");
        var ambIsolation= Load(SFX_PATH + "Ambient/AMB_Isolation_Room_Loop.wav");
        var ambNight    = Load(SFX_PATH + "Ambient/AMB_Room_Night_Loop.wav");
        // Ending
        var siren       = Load(SFX_PATH + "Ending/SFX_Police_Siren.wav");
        var cuffs       = Load(SFX_PATH + "Ending/SFX_Cuffs_Metal.wav");
        var radio       = Load(SFX_PATH + "Ending/SFX_Radio_Static.wav");
        var trueStare   = Load(SFX_PATH + "Ending/SFX_TrueEnd_StareLoop.wav");
        var trueTap     = Load(SFX_PATH + "Ending/SFX_TrueEnd_TapCut.wav");
        var endRestart  = Load(SFX_PATH + "Ending/SFX_Ending_Choice_Restart.wav");
        var endQuit     = Load(SFX_PATH + "Ending/SFX_Ending_Choice_Quit.wav");

        int total = 0;
        foreach (var scenePath in AllScenes)
        {
            var scene = EditorSceneManager.OpenScene(scenePath, OpenSceneMode.Single);

            int n = LinkComp<SFXManager>(scene, "SFXManager", sfx =>
            {
                // UI
                Set(sfx, "sfxUISelect",          uiSelect);
                Set(sfx, "sfxUIDangerConfirm",   uiDanger);
                Set(sfx, "sfxUIForcedClick",     uiForced);
                Set(sfx, "sfxUILocked",          uiLocked);
                Set(sfx, "sfxUIStatReveal",      uiStatReveal);
                // FX
                Set(sfx, "sfxRedWarningHit",     redWarning);
                Set(sfx, "sfxGlitchBurst",       glitchBurst);
                Set(sfx, "sfxBlurPulseWhoosh",   blurPulse);
                Set(sfx, "sfxMessageReveal",     msgReveal);
                Set(sfx, "sfxDesaturateDrop",    desaturate);
                // Body
                Set(sfx, "sfxHeartbeatRamp",     hbRamp);
                Set(sfx, "sfxHeartbeatSlow",     hbSlow);
                Set(sfx, "sfxTinnitusRing",      tinnitus);
                Set(sfx, "sfxBreathPanic",       breathPanic);
                Set(sfx, "sfxElectricTremor",    tremor);
                // Objects
                Set(sfx, "sfxPillRattle",        pillRattle);
                Set(sfx, "sfxPillBottleOpen",    pillOpen);
                Set(sfx, "sfxGlassClink",        glass);
                Set(sfx, "sfxDoorKnock",         doorKnock);
                Set(sfx, "sfxReportPaper",       report);
                // Phone
                Set(sfx, "sfxPhoneNotify",       phoneNotify);
                Set(sfx, "sfxPhoneSend",         phoneSend);
                Set(sfx, "sfxPhoneDial1393",     dial1393);
                // Ambient
                Set(sfx, "ambClub",              ambClub);
                Set(sfx, "ambHospital",          ambHospital);
                Set(sfx, "ambIsolation",         ambIsolation);
                Set(sfx, "ambRoomNight",         ambNight);
                // Ending
                Set(sfx, "sfxPoliceSiren",              siren);
                Set(sfx, "sfxCuffsMetal",               cuffs);
                Set(sfx, "sfxRadioStatic",              radio);
                Set(sfx, "sfxTrueEndStareLoop",         trueStare);
                Set(sfx, "sfxTrueEndTapCut",            trueTap);
                Set(sfx, "sfxEndingChoiceRestart",      endRestart);
                Set(sfx, "sfxEndingChoiceQuit",         endQuit);
            });

            total += n;
            EditorSceneManager.SaveScene(scene);
        }

        EditorSceneManager.OpenScene("Assets/Scenes/00_MainMenu.unity");
        AssetDatabase.Refresh();
        EditorUtility.DisplayDialog("✅ SFX 전체 연결 완료",
            $"총 {total}개 씬에 SFX 연결됨\n\n연결 해제: LAST BRAKE > 32. 오디오 연결 해제", "확인");
    }

    // ════════════════════════════════════════════════════════════════
    //  메뉴 32: 오디오 연결 해제 (무음 상태로 복원)
    // ════════════════════════════════════════════════════════════════
    [MenuItem("LAST BRAKE/32. 오디오 연결 해제 (무음 복원)")]
    public static void UnlinkAllAudio()
    {
        if (!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo()) return;

        bool confirm = EditorUtility.DisplayDialog(
            "오디오 연결 해제",
            "모든 씬의 SFXManager·BGMController에서 AudioClip 참조를 null로 초기화합니다.\n"
          + "게임 실행 시 오디오가 전혀 재생되지 않습니다.\n\n"
          + "재연결: LAST BRAKE > 31. SFX 전체 연결",
            "해제", "취소");
        if (!confirm) return;

        int total = 0;
        foreach (var scenePath in AllScenes)
        {
            var scene = EditorSceneManager.OpenScene(scenePath, OpenSceneMode.Single);

            // SFXManager 모든 AudioClip 필드 null
            foreach (var root in scene.GetRootGameObjects())
            {
                var sfx = root.GetComponentInChildren<SFXManager>(true);
                if (sfx != null) { NullAllAudioClips(sfx); EditorUtility.SetDirty(sfx); total++; }

                var bgm = root.GetComponentInChildren<BGMController>(true);
                if (bgm != null) { NullAllAudioClips(bgm); EditorUtility.SetDirty(bgm); total++; }
            }

            EditorSceneManager.SaveScene(scene);
        }

        EditorSceneManager.OpenScene("Assets/Scenes/00_MainMenu.unity");
        AssetDatabase.Refresh();
        EditorUtility.DisplayDialog("🔇 오디오 연결 해제 완료",
            $"{total}개 컴포넌트의 AudioClip 참조 해제됨\n재연결: LAST BRAKE > 30·31", "확인");
    }

    // ══════════════════════════════════════════════════════════════
    //  유틸리티
    // ══════════════════════════════════════════════════════════════

    static int LinkComp<T>(Scene scene, string goName, System.Action<T> setup)
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
            var go = new GameObject(goName);
            SceneManager.MoveGameObjectToScene(go, scene);
            comp = go.AddComponent<T>();
        }
        setup(comp);
        EditorUtility.SetDirty(comp);
        return 1;
    }

    static AudioClip Load(string path)
        => AssetDatabase.LoadAssetAtPath<AudioClip>(path);

    static void Set(object target, string field, object value)
    {
        var f = target.GetType().GetField(field,
            BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance);
        f?.SetValue(target, value);
    }

    /// <summary>컴포넌트의 모든 AudioClip SerializeField를 null로 초기화</summary>
    static void NullAllAudioClips(MonoBehaviour comp)
    {
        var fields = comp.GetType().GetFields(
            BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance);
        foreach (var f in fields)
        {
            if (f.FieldType == typeof(AudioClip))
                f.SetValue(comp, null);
        }
    }
}
