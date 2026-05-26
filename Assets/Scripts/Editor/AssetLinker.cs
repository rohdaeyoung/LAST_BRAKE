using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.Reflection;
using System.Collections.Generic;

/// <summary>
/// LAST BRAKE - 배경 + 캐릭터 전체 자동 연결 / 해제
/// 메뉴 17 : 전체 연결
/// 메뉴 18 : 전체 해제
/// </summary>
public class AssetLinker
{
    // ─────────────────────────────────────────────────
    //  경로
    // ─────────────────────────────────────────────────
    const string BG_PATH   = "Assets/Sprites/경진대회 이미지/Background_image/";
    const string CHAR_PATH = "Assets/Sprites/경진대회 이미지/Character_image/";

    // ─────────────────────────────────────────────────
    //  씬 → 배경 파일명 매핑  (PDF 스펙 기준)
    // ─────────────────────────────────────────────────
    static readonly (string scene, string bg)[] SceneBgMap =
    {
        // 00 메인메뉴 — 대학생 자취방 밤
        ("Assets/Scenes/00_MainMenu.unity",       "BG_Room_Night"),
        // 01 프롤로그 — 클럽 내부 (PDF: STEP 1 시작 장면용)
        ("Assets/Scenes/01_Step1_Prologue.unity", "BG_Club_Main"),
        // 02 클럽 — 클럽 입구 (PDF: 도입부 또는 장면 전환용)
        ("Assets/Scenes/02_Step2_Club.unity",     "BG_Club_Entrance_Night"),
        // 03 다음날 아침 — 새벽 자취방 (PDF: 밤샘, 피로, 수면 부족)
        ("Assets/Scenes/03_Step3_Morning.unity",  "BG_Room_Dawn_Exhausted"),
        // 04 파티 — 보라빛 환각 (PDF: 환각 심화 장면용)
        ("Assets/Scenes/04_Step4_Party.unity",    "BG_Hallucination_Purple"),
        // 05 붕괴 — 글리치 붕괴 (PDF: 심리적 붕괴, 위험 단계)
        ("Assets/Scenes/05_Step5_Collapse.unity", "BG_Collapse_Glitch"),
        // 06 엔딩 리포트 — 상담실 (PDF: 회복 과정, 도움받는 장면)
        ("Assets/Scenes/06_EndingReport.unity",   "BG_Counseling_Room"),
        // 07 굿엔딩 — 병실 회복 (PDF: GOOD END)
        ("Assets/Scenes/07_GoodEnd.unity",        "BG_Hospital_Recovery"),
        // 08 노말엔딩 — 고립된 방 (PDF: NORMAL END)
        ("Assets/Scenes/08_NormalEnd.unity",      "BG_Room_Isolation_Night"),
        // 09 배드엔딩 — 유치장 (PDF: BAD END 후반)
        ("Assets/Scenes/09_BadEnd.unity",         "BG_Cell_BadEnd"),
        // 10 트루엔딩 — 메타룸 (PDF: TRUE END)
        ("Assets/Scenes/10_TrueEnd.unity",        "BG_TrueEnd_MetaRoom"),
    };

    // ─────────────────────────────────────────────────
    //  캐릭터 정보
    // ─────────────────────────────────────────────────
    struct CharInfo { public string dialogueName, goName, spriteName; }

    static readonly CharInfo[] Characters =
    {
        new CharInfo { dialogueName="도윤", goName="Char_Doyun",   spriteName="Doyun"   },
        new CharInfo { dialogueName="민재", goName="Char_Minjae",  spriteName="Minjae"  },
        new CharInfo { dialogueName="하준", goName="Char_Hajun",   spriteName="Junho"   },
        new CharInfo { dialogueName="서아", goName="Char_Seoa",    spriteName="Seoyeon" },
        new CharInfo { dialogueName="태성", goName="Char_Taesung", spriteName="Taesung" },
        new CharInfo { dialogueName="현우", goName="Char_Hyunwoo", spriteName="Hyunwoo" },
    };

    // CharacterEmotion → 스프라이트 파일 감정 이름 매핑
    static readonly Dictionary<CharacterEmotion, string> EmotionFile =
        new Dictionary<CharacterEmotion, string>
    {
        { CharacterEmotion.Neutral,    "Normal"    },
        { CharacterEmotion.Happy,      "Normal"    },
        { CharacterEmotion.Worried,    "Anxious"   },
        { CharacterEmotion.Drunk,      "Anxious"   },
        { CharacterEmotion.Shocked,    "Surprised" },
        { CharacterEmotion.Pain,       "Regret"    },
        { CharacterEmotion.Smug,       "Normal"    },
        { CharacterEmotion.Determined, "Normal"    },
    };

    // ─────────────────────────────────────────────────
    //  엔딩 씬 추가 정보 (FourthWallBreak portrait)
    // ─────────────────────────────────────────────────
    static readonly (string scene, string portrait)[] EndingPortraitMap =
    {
        ("Assets/Scenes/07_GoodEnd.unity",  "Doyun_Normal"),
        ("Assets/Scenes/08_NormalEnd.unity","Doyun_Anxious"),
        ("Assets/Scenes/09_BadEnd.unity",   "Doyun_Regret"),
        ("Assets/Scenes/10_TrueEnd.unity",  "Doyun_Surprised"),
    };

    // ═══════════════════════════════════════════════════
    //  메뉴 17 : 전체 연결
    // ═══════════════════════════════════════════════════
    [MenuItem("LAST BRAKE/17. 배경+캐릭터 전체 연결 (경진대회)")]
    public static void LinkAll()
    {
        if (!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo()) return;

        var log = new System.Text.StringBuilder();
        int bgOk = 0, charOk = 0, fail = 0;

        foreach (var (scenePath, bgName) in SceneBgMap)
        {
            var    scene = EditorSceneManager.OpenScene(scenePath, OpenSceneMode.Single);
            string sFile = System.IO.Path.GetFileName(scenePath);

            // ── 배경 스프라이트 로드 ──────────────────
            var bgSprite = AssetDatabase.LoadAssetAtPath<Sprite>(BG_PATH + bgName + ".png");
            if (bgSprite == null)
            {
                log.AppendLine($"[BG MISS] {sFile} ← {bgName}.png 파일 없음");
                fail++; goto SAVE;
            }

            // ── Step 씬 (01~05) : SpriteRenderer Background ──
            if (scenePath.Contains("Step"))
            {
                bool bgLinked = LinkSpriteRendererBG(scene, bgSprite);
                if (bgLinked) { log.AppendLine($"[BG OK] {sFile} ← {bgName}"); bgOk++; }
                else          { log.AppendLine($"[BG FAIL] {sFile} ← Background SR 없음"); fail++; }

                int n = LinkStepCharacters(scene, sFile, log);
                if (n > 0) charOk++;
            }
            // ── 엔딩 씬 (07~10) ──────────────────────
            else if (scenePath.Contains("End") || scenePath.Contains("True"))
            {
                bool bgLinked = LinkEndingBackground(scene, bgSprite, sFile, log);
                if (bgLinked) { bgOk++; }
                else          { fail++; }

                LinkEndingCharacter(scene, scenePath, bgSprite, sFile, log);
                charOk++;
            }
            // ── MainMenu, EndingReport ────────────────
            else
            {
                bool bgLinked = LinkSpriteRendererBG(scene, bgSprite)
                             || LinkImageBG(scene, bgSprite);
                if (bgLinked) { log.AppendLine($"[BG OK] {sFile} ← {bgName}"); bgOk++; }
                else          { log.AppendLine($"[BG FAIL] {sFile} ← Background 없음"); fail++; }
            }

            SAVE:
            EditorSceneManager.SaveScene(scene);
        }

        EditorSceneManager.OpenScene("Assets/Scenes/00_MainMenu.unity");
        AssetDatabase.Refresh();

        EditorUtility.DisplayDialog("✅ 연결 완료",
            $"배경: {bgOk}개  캐릭터 씬: {charOk}개  실패: {fail}개\n\n{log}", "확인");
        Debug.Log("[AssetLinker]\n" + log);
    }

    // ─────────────────────────────────────────────────
    //  Step 씬 배경 (SpriteRenderer "Background" 오브젝트)
    // ─────────────────────────────────────────────────
    static bool LinkSpriteRendererBG(Scene scene, Sprite sprite)
    {
        foreach (var root in scene.GetRootGameObjects())
        {
            var result = FindSpriteRendererBG(root.transform, sprite);
            if (result) return true;
        }
        return false;
    }

    static bool FindSpriteRendererBG(Transform t, Sprite sprite)
    {
        if (t.name == "Background")
        {
            var sr = t.GetComponent<SpriteRenderer>();
            if (sr != null) { sr.sprite = sprite; EditorUtility.SetDirty(sr); return true; }
        }
        foreach (Transform c in t)
            if (FindSpriteRendererBG(c, sprite)) return true;
        return false;
    }

    // ─────────────────────────────────────────────────
    //  Canvas 안 Image "Background" 연결
    // ─────────────────────────────────────────────────
    static bool LinkImageBG(Scene scene, Sprite sprite)
    {
        foreach (var root in scene.GetRootGameObjects())
        {
            var result = FindImageBG(root.transform, sprite);
            if (result) return true;
        }
        return false;
    }

    static bool FindImageBG(Transform t, Sprite sprite)
    {
        if (t.name == "Background")
        {
            var img = t.GetComponent<Image>();
            if (img != null)
            {
                img.sprite = sprite; img.color = Color.white; img.preserveAspect = false;
                EditorUtility.SetDirty(img); return true;
            }
        }
        foreach (Transform c in t)
            if (FindImageBG(c, sprite)) return true;
        return false;
    }

    // ─────────────────────────────────────────────────
    //  엔딩 씬 배경 — Canvas 첫 번째 자식에 Image 생성/연결
    // ─────────────────────────────────────────────────
    static bool LinkEndingBackground(Scene scene, Sprite bgSprite,
                                     string sFile, System.Text.StringBuilder log)
    {
        // Canvas 찾기
        Canvas canvas = null;
        foreach (var root in scene.GetRootGameObjects())
        {
            canvas = root.GetComponentInChildren<Canvas>(true);
            if (canvas != null) break;
        }
        if (canvas == null) { log.AppendLine($"[BG FAIL] {sFile} ← Canvas 없음"); return false; }

        // 기존 "Background" Image 탐색
        Transform bgTf = canvas.transform.Find("Background");
        Image bgImg;

        if (bgTf == null)
        {
            // 없으면 새로 생성 (Canvas의 첫 번째 자식으로)
            var bgGo = new GameObject("Background");
            bgGo.transform.SetParent(canvas.transform, false);
            bgGo.transform.SetAsFirstSibling();           // 맨 뒤에 깔리도록
            bgImg = bgGo.AddComponent<Image>();
            var rt = bgGo.GetComponent<RectTransform>();
            rt.anchorMin = Vector2.zero;
            rt.anchorMax = Vector2.one;
            rt.offsetMin = Vector2.zero;
            rt.offsetMax = Vector2.zero;
        }
        else
        {
            bgImg = bgTf.GetComponent<Image>();
            if (bgImg == null) bgImg = bgTf.gameObject.AddComponent<Image>();
            bgTf.SetAsFirstSibling();
        }

        bgImg.sprite = bgSprite;
        bgImg.color  = Color.white;
        bgImg.preserveAspect = false;
        EditorUtility.SetDirty(bgImg);

        log.AppendLine($"[BG OK] {sFile} ← {bgSprite.name}");
        return true;
    }

    // ─────────────────────────────────────────────────
    //  엔딩 씬 캐릭터 — Doyun_Character SR + FourthWallBreak 연결
    // ─────────────────────────────────────────────────
    static void LinkEndingCharacter(Scene scene, string scenePath,
                                    Sprite bgSprite,
                                    string sFile, System.Text.StringBuilder log)
    {
        // portrait 결정
        string portraitFile = "Doyun_Normal";
        foreach (var (sp, pf) in EndingPortraitMap)
            if (sp == scenePath) { portraitFile = pf; break; }

        var portraitSprite = AssetDatabase.LoadAssetAtPath<Sprite>(CHAR_PATH + portraitFile + ".png");
        var normalSprite   = AssetDatabase.LoadAssetAtPath<Sprite>(CHAR_PATH + "Doyun_Normal.png");

        // Doyun_Character SpriteRenderer 연결
        foreach (var root in scene.GetRootGameObjects())
        {
            var tf = FindInChildren(root.transform, "Doyun_Character");
            if (tf != null)
            {
                var sr = tf.GetComponent<SpriteRenderer>();
                if (sr == null) sr = tf.gameObject.AddComponent<SpriteRenderer>();
                sr.sprite = normalSprite ?? portraitSprite;
                EditorUtility.SetDirty(sr);
                log.AppendLine($"[CHAR OK] {sFile} ← Doyun_Character SR 연결");
                break;
            }
        }

        // FourthWallBreak 필드 연결 (reflection)
        FourthWallBreak fwb = null;
        foreach (var root in scene.GetRootGameObjects())
        {
            fwb = root.GetComponentInChildren<FourthWallBreak>(true);
            if (fwb != null) break;
        }
        if (fwb == null) { log.AppendLine($"[FWB MISS] {sFile} ← FourthWallBreak 없음"); return; }

        SetField(fwb, "doyunPortrait",        portraitSprite);
        SetField(fwb, "finalScreenBackground", bgSprite);

        // stareFrames — Doyun 4감정 스프라이트 배열
        var frames = new List<Sprite>();
        foreach (var emo in new[]{"Normal","Anxious","Surprised","Regret"})
        {
            var s = AssetDatabase.LoadAssetAtPath<Sprite>(CHAR_PATH + "Doyun_" + emo + ".png");
            if (s != null) frames.Add(s);
        }
        if (frames.Count > 0)
            SetField(fwb, "stareFrames", frames.ToArray());

        EditorUtility.SetDirty(fwb);
        log.AppendLine($"[FWB OK] {sFile} ← portrait={portraitFile}, bg={bgSprite.name}, frames={frames.Count}");
    }

    // ─────────────────────────────────────────────────
    //  Step 씬 캐릭터 — CharacterAnimator 슬롯 구성
    //  카메라 orthographicSize=5 기준 (화면 높이 10유닛)
    //  PPU=100 (PDF 스펙) → 1254px/100 = 12.54유닛 → scale=0.55 적용 → 약 6.9유닛
    //  캐릭터 발이 화면 하단 근처에 위치: y = -1.8
    // ─────────────────────────────────────────────────

    // 캐릭터별 화면 X 위치 (화면 너비 약 17.8유닛 기준)
    static readonly Dictionary<string, float> CharPosX = new Dictionary<string, float>
    {
        { "Char_Doyun",   0f    },   // 주인공 — 중앙
        { "Char_Minjae",  3.5f  },   // 오른쪽
        { "Char_Hajun",  -3.5f  },   // 왼쪽
        { "Char_Seoa",    3.5f  },   // 오른쪽 (민재와 교대)
        { "Char_Taesung",-3.5f  },   // 왼쪽
        { "Char_Hyunwoo", 3.5f  },   // 오른쪽
    };

    static int LinkStepCharacters(Scene scene, string sFile, System.Text.StringBuilder log)
    {
        // CharacterAnimator 찾기 또는 생성
        CharacterAnimator ca = null;
        foreach (var root in scene.GetRootGameObjects())
        {
            ca = root.GetComponentInChildren<CharacterAnimator>(true);
            if (ca != null) break;
        }
        if (ca == null)
        {
            var go = new GameObject("CharacterAnimator");
            SceneManager.MoveGameObjectToScene(go, scene);
            ca = go.AddComponent<CharacterAnimator>();
        }

        var slots = new List<CharacterSlot>();

        foreach (var ch in Characters)
        {
            // Char_XXX GameObject SpriteRenderer 탐색
            SpriteRenderer sr = null;
            GameObject charGo = null;
            foreach (var root in scene.GetRootGameObjects())
            {
                var tf = FindInChildren(root.transform, ch.goName);
                if (tf != null) { sr = tf.GetComponent<SpriteRenderer>(); charGo = tf.gameObject; break; }
            }
            if (sr == null)
            {
                charGo = new GameObject(ch.goName);
                SceneManager.MoveGameObjectToScene(charGo, scene);
                sr = charGo.AddComponent<SpriteRenderer>();
            }

            // 위치·스케일 설정
            // PPU=100 → 1254px = 12.54유닛. scale=0.55 → 약 6.9유닛 (화면 69%)
            float posX = CharPosX.ContainsKey(ch.goName) ? CharPosX[ch.goName] : 0f;
            charGo.transform.position   = new Vector3(posX, -1.8f, 0f);
            charGo.transform.localScale = new Vector3(0.55f, 0.55f, 1f);
            sr.sortingOrder = 5;   // 배경(−10)보다 앞, UI Canvas보다 뒤

            // 처음엔 숨김 (대화 재생 시 화자만 표시됨)
            charGo.SetActive(false);

            // 감정 스프라이트 로드 (중복 없이)
            var emotionList   = new List<EmotionSprite>();
            var addedEmotions = new HashSet<CharacterEmotion>();
            foreach (var kv in EmotionFile)
            {
                if (addedEmotions.Contains(kv.Key)) continue;
                string path = CHAR_PATH + ch.spriteName + "_" + kv.Value + ".png";
                var    spr  = AssetDatabase.LoadAssetAtPath<Sprite>(path);
                if (spr == null) continue;
                emotionList.Add(new EmotionSprite { emotion = kv.Key, sprite = spr });
                addedEmotions.Add(kv.Key);
            }

            if (emotionList.Count == 0)
            {
                log.AppendLine($"  [CHAR MISS] {ch.spriteName} 스프라이트 없음");
                continue;
            }

            // Normal 스프라이트 기본 설정
            var normalSpr = AssetDatabase.LoadAssetAtPath<Sprite>(
                CHAR_PATH + ch.spriteName + "_Normal.png");
            if (normalSpr != null) sr.sprite = normalSpr;
            EditorUtility.SetDirty(sr);
            EditorUtility.SetDirty(charGo);

            slots.Add(new CharacterSlot
            {
                characterName  = ch.dialogueName,
                spriteRenderer = sr,
                emotionSprites = emotionList.ToArray()
            });
            log.AppendLine($"  [CHAR OK] {ch.dialogueName}({ch.spriteName}) x={posX} {emotionList.Count}감정");
        }

        // slots 주입
        var field = typeof(CharacterAnimator).GetField("slots",
            BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance);
        if (field != null)
        {
            field.SetValue(ca, slots.ToArray());
            EditorUtility.SetDirty(ca);
        }
        return slots.Count;
    }

    // ═══════════════════════════════════════════════════
    //  메뉴 18 : 전체 해제
    // ═══════════════════════════════════════════════════
    [MenuItem("LAST BRAKE/18. 배경+캐릭터 연결 전체 해제")]
    public static void UnlinkAll()
    {
        if (!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo()) return;

        int cleared = 0;
        var log = new System.Text.StringBuilder();

        foreach (var (scenePath, _) in SceneBgMap)
        {
            var    scene = EditorSceneManager.OpenScene(scenePath, OpenSceneMode.Single);
            string sFile = System.IO.Path.GetFileName(scenePath);

            foreach (var root in scene.GetRootGameObjects())
                cleared += ClearRecursive(root.transform, sFile, log);

            EditorSceneManager.SaveScene(scene);
        }

        EditorSceneManager.OpenScene("Assets/Scenes/00_MainMenu.unity");
        AssetDatabase.Refresh();

        EditorUtility.DisplayDialog("✅ 연결 해제 완료",
            $"총 {cleared}개 항목 해제\n\n{log}", "확인");
        Debug.Log("[AssetLinker Unlink]\n" + log);
    }

    static int ClearRecursive(Transform t, string sFile, System.Text.StringBuilder log)
    {
        int count = 0;

        if (t.name == "Background")
        {
            var sr = t.GetComponent<SpriteRenderer>();
            if (sr != null && sr.sprite != null)
            { sr.sprite = null; EditorUtility.SetDirty(sr); count++; log.AppendLine($"[BG SR] {sFile}"); }
            var img = t.GetComponent<Image>();
            if (img != null && img.sprite != null)
            { img.sprite = null; img.color = Color.black; EditorUtility.SetDirty(img); count++; log.AppendLine($"[BG IMG] {sFile}"); }
        }

        if (t.name.StartsWith("Char_") || t.name == "Doyun_Character")
        {
            var sr = t.GetComponent<SpriteRenderer>();
            if (sr != null && sr.sprite != null)
            { sr.sprite = null; EditorUtility.SetDirty(sr); count++; log.AppendLine($"[CHAR] {t.name}"); }
        }

        var ca = t.GetComponent<CharacterAnimator>();
        if (ca != null)
        {
            var f = typeof(CharacterAnimator).GetField("slots",
                BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance);
            if (f != null)
            {
                var slots = f.GetValue(ca) as CharacterSlot[];
                if (slots != null)
                    foreach (var s in slots)
                        if (s?.spriteRenderer != null) s.spriteRenderer.sprite = null;
                f.SetValue(ca, new CharacterSlot[0]);
                EditorUtility.SetDirty(ca); count++;
                log.AppendLine($"[CA] {sFile}");
            }
        }

        var fwb = t.GetComponent<FourthWallBreak>();
        if (fwb != null)
        {
            SetField(fwb, "doyunPortrait",         (Sprite)null);
            SetField(fwb, "finalScreenBackground", (Sprite)null);
            SetField(fwb, "stareFrames",           new Sprite[0]);
            EditorUtility.SetDirty(fwb); count++;
            log.AppendLine($"[FWB] {sFile}");
        }

        foreach (Transform child in t)
            count += ClearRecursive(child, sFile, log);
        return count;
    }

    // ─────────────────────────────────────────────────
    //  유틸리티
    // ─────────────────────────────────────────────────
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

    static void SetField(object target, string fieldName, object value)
    {
        if (target == null) return;
        var f = target.GetType().GetField(fieldName,
            BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance);
        f?.SetValue(target, value);
    }
}
