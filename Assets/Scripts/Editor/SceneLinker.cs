using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// 각 씬의 컴포넌트 참조(Inspector 연결)를 자동으로 처리합니다.
/// </summary>
public class SceneLinker : EditorWindow
{
    [MenuItem("LAST BRAKE/6. 전체 씬 컴포넌트 자동 연결")]
    public static void LinkAllScenes()
    {
        if (!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo()) return;

        LinkMainMenu();
        LinkStepScenes();
        LinkEndingScenes();

        AssetDatabase.Refresh();
        EditorSceneManager.OpenScene("Assets/Scenes/00_MainMenu.unity");

        EditorUtility.DisplayDialog("완료",
            "모든 씬 컴포넌트 연결 완료!\n\n" +
            "이제 ▶ 버튼을 눌러 게임을 실행해보세요.\n\n" +
            "다음 단계:\n" +
            "LAST BRAKE → 7. DialogueData 샘플 생성",
            "확인");
    }

    // ── 00_MainMenu ──────────────────────────────
    private static void LinkMainMenu()
    {
        EditorSceneManager.OpenScene("Assets/Scenes/00_MainMenu.unity");

        var canvas = Find<Canvas>("MainMenuCanvas");
        if (canvas == null) return;

        var menuUI     = canvas.GetComponent<MainMenuUI>();
        var startBtn   = FindInChildren<Button>(canvas.gameObject, "StartButton");
        var quitBtn    = FindInChildren<Button>(canvas.gameObject, "QuitButton");
        var hintText   = FindInChildren<TextMeshProUGUI>(canvas.gameObject, "TrueEndHint");
        var fadePanel  = FindInChildren<Image>(canvas.gameObject, "FadePanel");

        if (menuUI != null)
        {
            SetField(menuUI, "startButton", startBtn);
            SetField(menuUI, "quitButton",  quitBtn);
            if (hintText != null) SetField(menuUI, "trueEndHint", hintText);
        }

        // ScreenEffects → FadePanel 연결
        var se = Find<ScreenEffects>("ScreenEffects");
        if (se != null && fadePanel != null)
            SetField(se, "fadePanel", fadePanel);

        EditorSceneManager.SaveScene(EditorSceneManager.GetActiveScene());
    }

    // ── 01~05 Step 씬들 ──────────────────────────
    private static void LinkStepScenes()
    {
        string[] scenes = {
            "Assets/Scenes/01_Step1_Prologue.unity",
            "Assets/Scenes/02_Step2_Club.unity",
            "Assets/Scenes/03_Step3_Morning.unity",
            "Assets/Scenes/04_Step4_Party.unity",
            "Assets/Scenes/05_Step5_Collapse.unity"
        };

        string[] nextScenes = {
            "02_Step2_Club",
            "03_Step3_Morning",
            "04_Step4_Party",
            "05_Step5_Collapse",
            "" // Step5 완료 → EndingCalculator로
        };

        for (int i = 0; i < scenes.Length; i++)
        {
            EditorSceneManager.OpenScene(scenes[i]);
            LinkStepScene(nextScenes[i]);
            EditorSceneManager.SaveScene(EditorSceneManager.GetActiveScene());
        }
    }

    private static void LinkStepScene(string nextScene)
    {
        var canvas = Find<Canvas>("DialogueCanvas");
        if (canvas == null) return;

        // DialoguePanel 하위 요소
        var dialoguePanel = FindInChildren<Image>(canvas.gameObject, "DialoguePanel");
        var speakerTMP    = FindInChildren<TextMeshProUGUI>(canvas.gameObject, "SpeakerName");
        var dialogueTMP   = FindInChildren<TextMeshProUGUI>(canvas.gameObject, "DialogueText");

        // ChoicePanel 하위 버튼 3개
        var choicePanel = canvas.transform.Find("ChoicePanel");
        Button[] choiceButtons = new Button[3];
        TextMeshProUGUI[] choiceTexts = new TextMeshProUGUI[3];
        if (choicePanel != null)
        {
            for (int i = 0; i < 3; i++)
            {
                var btnTf = choicePanel.Find($"ChoiceButton_{i}");
                if (btnTf != null)
                {
                    choiceButtons[i] = btnTf.GetComponent<Button>();
                    choiceTexts[i]   = btnTf.GetComponentInChildren<TextMeshProUGUI>();
                }
            }
        }

        var fadePanel  = FindInChildren<Image>(canvas.gameObject, "FadePanel");
        var noisePanel = FindInChildren<Image>(canvas.gameObject, "NoiseOverlay");

        // DialogueUI 연결
        var dialogueUIGO = Find<DialogueManager>("DialogueManager");
        var dialogueUI   = dialogueUIGO?.GetComponent<DialogueUI>()
                        ?? dialogueUIGO?.gameObject.AddComponent<DialogueUI>();
        if (dialogueUI != null)
        {
            SetField(dialogueUI, "panel",        dialoguePanel?.gameObject);
            SetField(dialogueUI, "speakerText",  speakerTMP);
            SetField(dialogueUI, "dialogueText", dialogueTMP);
        }

        // ChoiceUI 연결
        var choiceUIGO = Find<ChoiceSystem>("ChoiceSystem");
        var choiceUI   = choiceUIGO?.GetComponent<ChoiceUI>()
                      ?? choiceUIGO?.gameObject.AddComponent<ChoiceUI>();
        if (choiceUI != null && choicePanel != null)
        {
            SetField(choiceUI, "panel",         choicePanel.gameObject);
            SetField(choiceUI, "choiceButtons", choiceButtons);
            SetField(choiceUI, "choiceTexts",   choiceTexts);
        }

        // DialogueManager 내부 참조
        var dm = Find<DialogueManager>("DialogueManager");
        if (dm != null)
        {
            SetField(dm, "dialogueUI",   dialogueUI);
            SetField(dm, "choiceSystem", choiceUIGO?.GetComponent<ChoiceSystem>()
                                      ?? Find<ChoiceSystem>("ChoiceSystem"));
        }

        // ChoiceSystem 내부 참조
        var cs = Find<ChoiceSystem>("ChoiceSystem");
        if (cs != null)
            SetField(cs, "choiceUI", choiceUI);

        // StepController
        var sc = Find<StepController>("StepController");
        if (sc != null)
            SetField(sc, "nextScene", nextScene);

        // ScreenEffects
        var se = Find<ScreenEffects>("ScreenEffects");
        if (se != null)
        {
            SetField(se, "fadePanel",    fadePanel);
            SetField(se, "noiseOverlay", noisePanel);
        }
    }

    // ── 엔딩 씬들 ────────────────────────────────
    private static void LinkEndingScenes()
    {
        // 06_EndingReport
        EditorSceneManager.OpenScene("Assets/Scenes/06_EndingReport.unity");
        var srUI = Find<StatReportUI>("StatReportUI");
        // StatReportUI의 슬롯들은 씬에 있는 TMP와 Slider를 연결
        EditorSceneManager.SaveScene(EditorSceneManager.GetActiveScene());

        // 07~10 엔딩: FourthWallBreak 연결
        string[] endingScenes = {
            "Assets/Scenes/07_GoodEnd.unity",
            "Assets/Scenes/08_NormalEnd.unity",
            "Assets/Scenes/09_BadEnd.unity",
            "Assets/Scenes/10_TrueEnd.unity"
        };

        foreach (var scenePath in endingScenes)
        {
            EditorSceneManager.OpenScene(scenePath);
            LinkEndingScene();
            EditorSceneManager.SaveScene(EditorSceneManager.GetActiveScene());
        }
    }

    private static void LinkEndingScene()
    {
        var canvas = GameObject.FindFirstObjectByType<Canvas>();
        if (canvas == null) return;

        var fwbGO = GameObject.Find("FourthWallBreak");
        var fwb   = fwbGO?.GetComponent<FourthWallBreak>();
        if (fwb == null) return;

        var msgGroup   = FindInChildren<CanvasGroup>(canvas.gameObject, "MessageGroup");
        var msgTMP     = FindInChildren<TextMeshProUGUI>(canvas.gameObject, "CoreMessage");
        var choicePanel = canvas.transform.Find("FinalChoicePanel");
        var restartBtn = choicePanel != null
            ? choicePanel.Find("RestartButton")?.GetComponent<Button>() : null;
        var quitBtn    = choicePanel != null
            ? choicePanel.Find("QuitButton")?.GetComponent<Button>()    : null;
        var doyun      = GameObject.Find("Doyun_Character")?.GetComponent<SpriteRenderer>();
        var fadePanel  = FindInChildren<Image>(canvas.gameObject, "FadePanel");

        SetField(fwb, "doyunRenderer",      doyun);
        SetField(fwb, "messageText",        msgTMP);
        SetField(fwb, "choicePanel",        choicePanel?.gameObject);
        SetField(fwb, "btnRestart",         restartBtn);
        SetField(fwb, "btnQuit",            quitBtn);
        SetField(fwb, "messageCanvasGroup", msgGroup);

        // 각 엔딩 시퀀스에 FourthWallBreak 연결
        var goodEnd   = GameObject.FindFirstObjectByType<GoodEndSequence>();
        var normalEnd = GameObject.FindFirstObjectByType<NormalEndSequence>();
        var badEnd    = GameObject.FindFirstObjectByType<BadEndSequence>();
        var trueEnd   = GameObject.FindFirstObjectByType<TrueEndSequence>();

        if (goodEnd   != null) SetField(goodEnd,   "fourthWallBreak", fwb);
        if (normalEnd != null) SetField(normalEnd, "fourthWallBreak", fwb);
        if (badEnd    != null) SetField(badEnd,    "fourthWallBreak", fwb);
        if (trueEnd   != null) SetField(trueEnd,   "fourthWallBreak", fwb);
    }

    // ── 유틸리티 ─────────────────────────────────

    private static T Find<T>(string name) where T : Component
    {
        var go = GameObject.Find(name);
        return go != null ? go.GetComponent<T>() : null;
    }

    private static T FindInChildren<T>(GameObject root, string name) where T : Component
    {
        var tf = root.transform.Find(name);
        if (tf != null) return tf.GetComponent<T>();

        // 재귀 탐색
        foreach (Transform child in root.transform)
        {
            var result = FindInChildren<T>(child.gameObject, name);
            if (result != null) return result;
        }
        return null;
    }

    private static void SetField(object target, string fieldName, object value)
    {
        if (target == null || value == null) return;
        var field = target.GetType().GetField(fieldName,
            System.Reflection.BindingFlags.NonPublic |
            System.Reflection.BindingFlags.Public    |
            System.Reflection.BindingFlags.Instance);
        if (field != null)
        {
            field.SetValue(target, value);
            EditorUtility.SetDirty(target as Object);
        }
    }

    // ── 샘플 DialogueData 생성 ───────────────────
    [MenuItem("LAST BRAKE/7. DialogueData 샘플 생성 (Step1)")]
    public static void CreateSampleDialogueData()
    {
        string path = "Assets/ScriptableObjects/Step1_Dialogue.asset";
        if (System.IO.File.Exists(path))
        {
            EditorUtility.DisplayDialog("이미 존재", $"{path}\n이미 있습니다.", "확인");
            return;
        }

        var data = ScriptableObject.CreateInstance<DialogueData>();
        data.lines = new DialogueLine[]
        {
            new DialogueLine { speaker = "하준", emotion = CharacterEmotion.Happy, isMonologue = false,
                text = "야, 강도윤! 너 아까 과 주점에서 연예인 올 때만 해도 날아다니더니 왜 벌써 눈이 풀렸냐? 오늘 축제 마지막 날이라고 이대로 집 가면 병나, 임마!" },
            new DialogueLine { speaker = "도윤", emotion = CharacterEmotion.Drunk, isMonologue = false,
                text = "아... 머리 진짜 깨질 것 같아. 아까 소주랑 맥주랑 막 섞어 마셔서 그런가... 속도 안 좋네." },
            new DialogueLine { speaker = "서아", emotion = CharacterEmotion.Worried, isMonologue = false,
                text = "박하준, 너 도윤이 괴롭히지 마. 도윤아, 너 상태 진짜 안 좋아 보여. 그냥 지금이라도 내가 택시 잡아줄 테니까 들어갈래?" },
            new DialogueLine { speaker = "민재", emotion = CharacterEmotion.Smug, isMonologue = false,
                text = "서아 넌 매번 분위기를 그렇게 끊더라. 도윤아, 형이 기가 막힌 곳 아는데 거기 갈래? 거기 노래 한 곡만 들어도 피곤한 거 싹 가시고 세상이 다 네 거처럼 보일걸?" },
            new DialogueLine { speaker = "도윤", emotion = CharacterEmotion.Neutral, isMonologue = false,
                text = "피곤한 게 가신다고? 그런 게 어딨어..." },
            new DialogueLine { speaker = "민재", emotion = CharacterEmotion.Smug, isMonologue = false,
                text = "형 믿어봐. 인생 짧아. 오늘 그냥 미쳐보는 거야. 자, 가자!" },
        };

        data.choices = new ChoiceData[]
        {
            new ChoiceData { label = "아, 머리 아프니까 그냥 집에 갈게요.",
                intDelta = 10, riskDelta = -5, addictDelta = 0,
                nextScene = "02_Step2_Club", isForcedBad = false },
            new ChoiceData { label = "그래... 잠깐만 가볼까?",
                intDelta = -10, riskDelta = 15, addictDelta = 5,
                nextScene = "02_Step2_Club", isForcedBad = false },
        };

        AssetDatabase.CreateAsset(data, path);
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        EditorGUIUtility.PingObject(data);

        EditorUtility.DisplayDialog("완료",
            "Step1_Dialogue.asset 생성 완료!\n\n" +
            "Assets/ScriptableObjects/ 에서 확인하세요.\n\n" +
            "Step1_Prologue 씬을 열고\n" +
            "StepController → Dialogue Sequence [0] 에\n" +
            "Step1_Dialogue.asset 을 드래그하세요.",
            "확인");
    }
}
