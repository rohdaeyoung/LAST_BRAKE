using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine.UI;

public class BackgroundLinker
{
    private static readonly string[] AllScenes = new[]
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

    // ─── 연결 해제 ───────────────────────────────────────────
    [MenuItem("LAST BRAKE/16. 배경·캐릭터 연결 전체 해제")]
    public static void UnlinkAll()
    {
        if (!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo()) return;

        int cleared = 0;
        var log = new System.Text.StringBuilder();

        foreach (var scenePath in AllScenes)
        {
            var scene = EditorSceneManager.OpenScene(scenePath, OpenSceneMode.Single);

            foreach (var go in Resources.FindObjectsOfTypeAll<GameObject>())
            {
                if (go.scene != scene) continue;

                // Background SpriteRenderer
                if (go.name == "Background")
                {
                    var sr = go.GetComponent<SpriteRenderer>();
                    if (sr != null && sr.sprite != null)
                    {
                        sr.sprite = null;
                        EditorUtility.SetDirty(sr);
                        log.AppendLine($"[BG cleared] {System.IO.Path.GetFileName(scenePath)}");
                        cleared++;
                    }
                    var img = go.GetComponent<Image>();
                    if (img != null && img.sprite != null)
                    {
                        img.sprite = null;
                        img.color = Color.black;
                        EditorUtility.SetDirty(img);
                        log.AppendLine($"[BG-Image cleared] {System.IO.Path.GetFileName(scenePath)}");
                        cleared++;
                    }
                }

                // Char_ 로 시작하는 캐릭터 오브젝트
                if (go.name.StartsWith("Char_"))
                {
                    var sr = go.GetComponent<SpriteRenderer>();
                    if (sr != null && sr.sprite != null)
                    {
                        sr.sprite = null;
                        EditorUtility.SetDirty(sr);
                        log.AppendLine($"[Char cleared] {go.name}  ({System.IO.Path.GetFileName(scenePath)})");
                        cleared++;
                    }
                }

                // CharacterAnimator 슬롯 전체 해제
                var ca = go.GetComponent<CharacterAnimator>();
                if (ca != null)
                {
                    var slotsField = typeof(CharacterAnimator).GetField("slots",
                        System.Reflection.BindingFlags.NonPublic |
                        System.Reflection.BindingFlags.Public |
                        System.Reflection.BindingFlags.Instance);
                    if (slotsField != null)
                    {
                        var slots = slotsField.GetValue(ca) as CharacterSlot[];
                        if (slots != null)
                        {
                            foreach (var slot in slots)
                            {
                                if (slot.spriteRenderer != null)
                                    slot.spriteRenderer.sprite = null;
                            }
                            EditorUtility.SetDirty(ca);
                            log.AppendLine($"[CA cleared] {System.IO.Path.GetFileName(scenePath)}");
                            cleared++;
                        }
                    }
                }
            }

            EditorSceneManager.SaveScene(scene);
        }

        EditorSceneManager.OpenScene("Assets/Scenes/00_MainMenu.unity");
        AssetDatabase.Refresh();

        EditorUtility.DisplayDialog(
            "연결 해제 완료",
            $"총 {cleared}개 해제\n\n{log}",
            "확인");

        Debug.Log("[BackgroundLinker - Unlink]\n" + log);
    }

    // ─── 배경 연결 ───────────────────────────────────────────
    private static readonly (string scene, string sprite)[] SceneMap = new[]
    {
        ("Assets/Scenes/00_MainMenu.unity",          "BG_MainMenu"),
        ("Assets/Scenes/01_Step1_Prologue.unity",    "BG_Step1_Prologue"),
        ("Assets/Scenes/02_Step2_Club.unity",        "BG_Step2_Club"),
        ("Assets/Scenes/03_Step3_Morning.unity",     "BG_Step3_Morning"),
        ("Assets/Scenes/04_Step4_Party.unity",       "BG_Step4_Party"),
        ("Assets/Scenes/05_Step5_Collapse.unity",    "BG_Step5_Collapse"),
        ("Assets/Scenes/06_EndingReport.unity",      "BG_Ending"),
        ("Assets/Scenes/07_GoodEnd.unity",           "BG_GoodEnd"),
        ("Assets/Scenes/08_NormalEnd.unity",         "BG_NormalEnd"),
        ("Assets/Scenes/09_BadEnd.unity",            "BG_BadEnd"),
        ("Assets/Scenes/10_TrueEnd.unity",           "BG_Ending"),
    };

    [MenuItem("LAST BRAKE/15. 배경 스프라이트 씬 자동 연결")]
    public static void LinkAllBackgrounds()
    {
        if (!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo()) return;

        int success = 0, fail = 0;
        var log = new System.Text.StringBuilder();

        foreach (var (scenePath, spriteName) in SceneMap)
        {
            string assetPath = $"Assets/Sprites/Backgrounds/{spriteName}.png";
            var sprite = AssetDatabase.LoadAssetAtPath<Sprite>(assetPath);
            if (sprite == null)
            {
                log.AppendLine($"[SKIP] {spriteName}.png 없음");
                fail++; continue;
            }

            var scene = EditorSceneManager.OpenScene(scenePath, OpenSceneMode.Single);
            bool linked = false;

            foreach (var go in Resources.FindObjectsOfTypeAll<GameObject>())
            {
                if (go.scene != scene) continue;
                if (go.name != "Background") continue;

                var sr = go.GetComponent<SpriteRenderer>();
                if (sr != null)
                {
                    sr.sprite = sprite;
                    EditorUtility.SetDirty(sr);
                    linked = true; break;
                }
                var img = go.GetComponent<Image>();
                if (img != null)
                {
                    img.sprite = sprite;
                    img.color = Color.white;
                    img.preserveAspect = false;
                    EditorUtility.SetDirty(img);
                    linked = true; break;
                }
            }

            if (linked) { success++; log.AppendLine($"[OK] {System.IO.Path.GetFileName(scenePath)} → {spriteName}"); }
            else        { fail++;    log.AppendLine($"[FAIL] {System.IO.Path.GetFileName(scenePath)} — Background 없음"); }

            EditorSceneManager.SaveScene(scene);
        }

        EditorSceneManager.OpenScene("Assets/Scenes/00_MainMenu.unity");
        AssetDatabase.Refresh();

        EditorUtility.DisplayDialog("배경 연결 완료",
            $"성공: {success} / 실패: {fail}\n\n{log}", "확인");
    }
}
