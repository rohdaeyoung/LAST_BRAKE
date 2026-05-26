using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine.UI;
using System.Reflection;

/// <summary>
/// LAST BRAKE - UI / FX / CG 스프라이트 자동 연결  (PDF 스펙 기반)
/// 메뉴 19: UI+FX 전체 연결
/// </summary>
public class UIAssetLinker
{
    const string UI_PATH      = "Assets/Sprites/경진대회 이미지/UI_image/";
    const string FX_PATH      = "Assets/Sprites/경진대회 이미지/Overlay_image/";
    const string OBJ_PATH     = "Assets/Sprites/경진대회 이미지/Object_image/";

    // ─────────────────────────────────────────────────────────
    //  UI 오브젝트 이름 → 스프라이트 파일명
    // ─────────────────────────────────────────────────────────
    static readonly (string goName, string file, string folder)[] UIMap =
    {
        // 대화창 패널
        ("DialoguePanel",  "UI_DialogueBox",          UI_PATH),
        // 화자 이름표 배경
        ("SpeakerName",    "UI_NameTag",               UI_PATH),
        // 선택지 버튼
        ("ChoiceButton_0", "UI_ChoiceButton_Normal",   UI_PATH),
        ("ChoiceButton_1", "UI_ChoiceButton_Normal",   UI_PATH),
        ("ChoiceButton_2", "UI_ChoiceButton_Normal",   UI_PATH),
        // FX 오버레이
        ("NoiseOverlay",   "FX_GlitchOverlay",         FX_PATH),
        ("FadePanel",      "FX_DarkVignette",           FX_PATH),
    };

    // ─────────────────────────────────────────────────────────
    //  메뉴 19: UI + FX 전체 씬 연결
    // ─────────────────────────────────────────────────────────
    [MenuItem("LAST BRAKE/19. UI+FX 스프라이트 연결 (경진대회)")]
    public static void LinkUIAll()
    {
        if (!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo()) return;

        string[] stepScenes =
        {
            "Assets/Scenes/01_Step1_Prologue.unity",
            "Assets/Scenes/02_Step2_Club.unity",
            "Assets/Scenes/03_Step3_Morning.unity",
            "Assets/Scenes/04_Step4_Party.unity",
            "Assets/Scenes/05_Step5_Collapse.unity",
        };

        var log   = new System.Text.StringBuilder();
        int total = 0;

        foreach (var scenePath in stepScenes)
        {
            var    scene = EditorSceneManager.OpenScene(scenePath, OpenSceneMode.Single);
            string sFile = System.IO.Path.GetFileName(scenePath);
            int    n     = LinkUIInScene(scene, sFile, log);
            total += n;
            EditorSceneManager.SaveScene(scene);
        }

        EditorSceneManager.OpenScene("Assets/Scenes/00_MainMenu.unity");
        AssetDatabase.Refresh();

        EditorUtility.DisplayDialog("✅ UI+FX 연결 완료",
            $"총 {total}개 항목 연결\n\n{log}", "확인");
        Debug.Log("[UIAssetLinker]\n" + log);
    }

    static int LinkUIInScene(UnityEngine.SceneManagement.Scene scene,
                             string sFile, System.Text.StringBuilder log)
    {
        int count = 0;
        foreach (var (goName, file, folder) in UIMap)
        {
            var sprite = AssetDatabase.LoadAssetAtPath<Sprite>(folder + file + ".png");
            if (sprite == null) { log.AppendLine($"  [MISS] {file}.png"); continue; }

            foreach (var root in scene.GetRootGameObjects())
            {
                var tf = FindInChildren(root.transform, goName);
                if (tf == null) continue;

                // Image 컴포넌트에 연결
                var img = tf.GetComponent<Image>();
                if (img != null)
                {
                    // FadePanel / NoiseOverlay 는 color 유지, sprite만 교체
                    bool isFX = (goName == "FadePanel" || goName == "NoiseOverlay");
                    img.sprite = sprite;
                    if (!isFX) img.color = Color.white;
                    img.type   = Image.Type.Sliced; // 대화창은 9-slice가 자연스러움
                    EditorUtility.SetDirty(img);
                    log.AppendLine($"  [OK] {sFile} {goName} ← {file}");
                    count++;
                    break;
                }
            }
        }
        return count;
    }

    static Transform FindInChildren(Transform t, string name)
    {
        if (t.name == name) return t;
        foreach (Transform c in t)
        {
            var r = FindInChildren(c, name);
            if (r != null) return r;
        }
        return null;
    }
}
