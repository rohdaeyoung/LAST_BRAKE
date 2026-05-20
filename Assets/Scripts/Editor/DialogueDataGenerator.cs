using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using TMPro;
using UnityEngine.UI;

public class DialogueDataGenerator : EditorWindow
{
    [MenuItem("LAST BRAKE/11. 전체 대화 데이터 자동 생성")]
    public static void GenerateAllDialogueData()
    {
        // Resources/Dialogues 폴더 생성
        if (!AssetDatabase.IsValidFolder("Assets/Resources"))
            AssetDatabase.CreateFolder("Assets", "Resources");
        if (!AssetDatabase.IsValidFolder("Assets/Resources/Dialogues"))
            AssetDatabase.CreateFolder("Assets/Resources", "Dialogues");
        AssetDatabase.Refresh();

        CreateStep1();
        CreateStep2();
        CreateStep3();
        CreateStep4();
        CreateStep5();
        CreateGoodEndDialogue();
        CreateNormalEndDialogue();
        CreateBadEndDialogue();

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

        EditorUtility.DisplayDialog("완료",
            "STEP 1~5 + 엔딩 대화 데이터 생성 완료!\n\n" +
            "Assets/Resources/Dialogues/ 폴더에\n" +
            "Step1_Prologue.asset 등 8개 파일이 생겼는지 확인!\n\n" +
            "다음: ▶ 버튼으로 바로 테스트 가능!",
            "확인");
    }

    // ── STEP 1: 프롤로그 ─────────────────────────
    static void CreateStep1()
    {
        var d = Create("Assets/Resources/Dialogues/Step1_Prologue.asset");
        d.lines = new DialogueLine[]
        {
            L("하준",  CharacterEmotion.Happy,   false, "야, 강도윤! 너 아까 과 주점에서 연예인 올 때만 해도 날아다니더니 왜 벌써 눈이 풀렸냐? 오늘 축제 마지막 날이라고 이대로 집 가면 병나, 임마!"),
            L("도윤",  CharacterEmotion.Drunk,   false, "아... 머리 진짜 깨질 것 같아. 아까 소주랑 맥주랑 막 섞어 마셔서 그런가... 속도 안 좋네."),
            L("서아",  CharacterEmotion.Worried, false, "박하준, 너 도윤이 괴롭히지 마. 도윤아, 너 상태 진짜 안 좋아 보여. 그냥 지금이라도 내가 택시 잡아줄 테니까 들어갈래?"),
            L("민재",  CharacterEmotion.Smug,    false, "서아 넌 매번 분위기를 그렇게 끊더라. 도윤아, 형이 기가 막힌 곳 아는데 거기 갈래? 거기 노래 한 곡만 들어도 피곤한 거 싹 가시고 세상이 다 네 거처럼 보일걸?"),
            L("도윤",  CharacterEmotion.Neutral, false, "피곤한 게 가신다고? 그런 게 어딨어..."),
            L("민재",  CharacterEmotion.Smug,    false, "형 믿어봐. 인생 짧아. 오늘 그냥 미쳐보는 거야. 자, 가자!"),
        };
        d.choices = new ChoiceData[]
        {
            C("아, 머리 아프니까 그냥 집에 갈게요.",  10, -5,  0,  "02_Step2_Club", false, false, 0),
            C("그래... 잠깐만 가볼까?",             -10, 15,  5,  "02_Step2_Club", false, false, 0),
        };
        Save(d);
    }

    // ── STEP 2: 클럽 ─────────────────────────────
    static void CreateStep2()
    {
        var d = Create("Assets/Resources/Dialogues/Step2_Club.asset");
        d.lines = new DialogueLine[]
        {
            L("민재",  CharacterEmotion.Smug,    false, "도윤아, 이거 원샷해봐. 이게 여기 시그니처거든. 한 방에 정신이 번쩍 들 거다."),
            L("도윤",  CharacterEmotion.Shocked, true,  "갑자기 왜 이러지? 심장이 갈비뼈를 뚫고 나올 것 같아. 손끝이 전기에 감전된 것처럼 덜덜 떨리는데... 근데 기분은 왜 이렇게 좋아지는 거지?"),
            L("서아",  CharacterEmotion.Worried, false, "강도윤! 너 정신 차려봐! 너 지금 식은땀 장난 아니야. 눈도 완전히 풀렸어! 민재 오빠, 도윤이한테 뭐 먹인 거야?"),
            L("민재",  CharacterEmotion.Smug,    false, "아, 서아 좀! 애 지금 기분 최고조인데 왜 찬물을 끼얹고 그래. 도윤아, 지금 개쩔지? 그냥 이 리듬에 몸을 맡겨봐!"),
        };
        d.choices = new ChoiceData[]
        {
            C("민재야, 나 진짜 이상해. 일단 화장실 가서 세수 좀 하고 올게.",          15, -5,  0,  "03_Step3_Morning", false, false, 0),
            C("아, 갑자기 왜 이러지? 하준아, 너도 그래? 나만 유난 떠는 거 아니지?",   0,   0,  5,  "03_Step3_Morning", false, false, 0),
            C("대박... 야, 이거 뭐야? 세상이 막 돌아가는데 기분 정말 이상해. 한 잔 더 없어?", -20, 10, 20, "03_Step3_Morning", true,  false, 0),
        };
        Save(d);
    }

    // ── STEP 3: 다음 날 아침 ──────────────────────
    static void CreateStep3()
    {
        var d = Create("Assets/Resources/Dialogues/Step3_Morning.asset");
        d.lines = new DialogueLine[]
        {
            L("민재(카톡)", CharacterEmotion.Smug,    false, "다들 걱정 마라~ 형이 안전하게 집 침대까지 모셔다드림. 도윤아, 너 몸 막 떨리던 거 그거 숙취 아니고 몸살 기운 있어서 그런 거야."),
            L("민재(카톡)", CharacterEmotion.Smug,    false, "내가 식탁 위에 상비약 놔두고 왔거든. 그거 센 거니까 한 알만 딱 먹고 다시 자라. 그럼 바로 멀쩡해질 거다."),
            L("도윤",       CharacterEmotion.Pain,    true,  "...으윽, 진짜 죽을 것 같아. 이게 그냥 술 때문이라고...?"),
            L("도윤",       CharacterEmotion.Happy,   true,  "와... 진짜 살 것 같다. 손 떨리는 게 순식간에 멈췄네. 역시 민재 형이 준 약이 효과 하나는 끝내주는구나. 그냥 심한 숙취였나 봐."),
        };
        d.choices = new ChoiceData[]
        {
            C("어제 기억이 하나도 안 나요... 민재 형 진짜 감사합니다. 약 먹으니까 몸이 날아갈 것 같아요!", -5,  5,  10, "04_Step4_Party", false, false, 0),
            C("형, 이 약 진짜 대박이네요. 이름이 뭐예요? 나중에 또 몸 안 좋으면 제가 따로 사 먹으려고요.", -10, 10, 20, "04_Step4_Party", true,  false, 0),
            C("민재 형, 근데 이거 그냥 상비약 맞아요? 먹자마자 통증이 아예 사라져서 좀 신기해서요.",      15,  0, -5, "04_Step4_Party", false, false, 0),
        };
        Save(d);
    }

    // ── STEP 4: 방심의 파티 ──────────────────────
    static void CreateStep4()
    {
        var d = Create("Assets/Resources/Dialogues/Step4_Party.asset");
        d.lines = new DialogueLine[]
        {
            L("하준",  CharacterEmotion.Happy,   false, "와, 강도윤! 아까 낮까지만 해도 시체 같더니만, 지금은 얼굴에서 광이 난다? 그 약 정체가 뭐야? 나도 시험 기간에 좀 먹게!"),
            L("서아",  CharacterEmotion.Worried, false, "도윤아, 너 진짜 괜찮은 거 맞지? 어제는 너 숨소리도 이상하고 손도 엄청 떨어서 나... 진짜 응급실 불러야 하나 고민했단 말이야."),
            L("민재",  CharacterEmotion.Smug,    false, "도윤아, 오늘 형이 제대로 된 곳 한 군데 더 뚫어놨어. 어제보다 분위기 훨씬 깔끔하고 물 좋은 데니까, 딱 오늘까지만 화끈하게 달리고 내일부터 갓생 사는 거다. 어때?"),
            L("도윤",  CharacterEmotion.Happy,   true,  "사실 몸이 좀 이상할 정도로 가볍긴 한데... 어제의 그 끔찍한 기분이 사라지니까 뭐든 할 수 있을 것 같은 자신감이 생겨."),
        };
        d.choices = new ChoiceData[]
        {
            C("민재 형 말이 맞아요. 어제는 그냥 운이 안 좋았던 거고, 오늘은 제가 분위기 제대로 띄워볼게요! 다들 나가자!", -10, 10, 10, "05_Step5_Collapse", false, false, 0),
            C("형이 준 약 덕분에 몸 상태 완전 최상이에요. 오늘 술 엄청 마셔도 이 약만 있으면 무서울 게 없네요. 가요!", -20, 15, 25, "05_Step5_Collapse", true,  false, 0),
            C("어제 그 이상한 고양감이 다시 느껴질까 무서웠지만... 사실 그런 기분은 처음이었어. 한 번만 더 느껴보고 싶어.", -15, 10, 30, "05_Step5_Collapse", true,  false, 0),
        };
        Save(d);
    }

    // ── STEP 5: 무너진 브레이크 ──────────────────
    static void CreateStep5()
    {
        var d = Create("Assets/Resources/Dialogues/Step5_Collapse.asset");
        d.lines = new DialogueLine[]
        {
            L("민재",  CharacterEmotion.Smug,    false, "도윤아, 오늘 축제 피날레다. 이거 어제 그 약 강화 버전이야. 오늘 이거면 넌 진짜 신이 된 기분일걸?"),
            L("서아",  CharacterEmotion.Worried, false, "박민재 오빠, 그만해! 도윤아, 너 저거 먹으면 진짜 끝이야. 제발 나랑 같이 나가자, 응?"),
            L("도윤",  CharacterEmotion.Drunk,   false, "아, 진짜... 분위기 다 망치네! 민재 형이 나 생각해서 준 건데 네가 왜 난리야? 형, 줘요. 내가 다 마셔버릴 거니까."),
            L("도윤",  CharacterEmotion.Pain,    false, "으... 으윽... 아악! 몸이... 누가 전기로 지지는 것 같아... 민재... 민재 형... 약... 제발..."),
            L("민재",  CharacterEmotion.Smug,    false, "와, 우리 도윤이 약빨 받는 속도가 거의 예술인데. 벌써부터 이렇게 개처럼 기어와서 찾으면 어떡하냐. 이거 아무나 못 구하는 귀한 거야."),
            L("민재",  CharacterEmotion.Smug,    false, "망치긴 누가 망쳐. 선택은 네가 했지. 이제 결정해. 나랑 같이 비즈니스 하면서 이 기분 유지할래? 아니면 여기서 내가 다 너 주변사람들한테 다 말하고 혼자 온몸이 뒤틀려 죽을래?"),
        };
        d.choices = new ChoiceData[]
        {
            C("지옥에 갈 거면 너랑 같이 가줄게. 당장 경찰에 신고한다.", 0, 0, 0, "", false, true, 70),
            C("나 같은 놈이 무슨... 다 끝났어. 그냥 아무도 나 찾지 못하게 해줘...", 0, 0, 0, "", false, false, 0),
            C("알았어... 형 시키는 거 다 할게. 그러니까 제발 그거 한 알만... 한 알만 줘.", 0, 0, 0, "", true, false, 0),
        };
        Save(d);
    }

    // ── 엔딩 대화들 ──────────────────────────────
    static void CreateGoodEndDialogue()
    {
        var d = Create("Assets/Resources/Dialogues/GoodEnd_Dialogue.asset");
        d.lines = new DialogueLine[]
        {
            L("도윤",  CharacterEmotion.Determined, false, "적응...? 최민재, 너 지금 그걸 말이라고 해? 네가 나한테 준 거 평범한 약 아니잖아. 마약이잖아!"),
            L("민재",  CharacterEmotion.Smug,       false, "야, 강도윤, 너도 즐겼으면서 왜 이제 와서 선비질이야? 그냥 조용히 가자."),
            L("도윤",  CharacterEmotion.Determined, false, "닥쳐. 다시는 내 앞에 나타나지 마. 내 손으로 널 경찰에 넘기기 전에."),
            L("서아",  CharacterEmotion.Worried,    false, "도윤아, 나... 정말 무서워. 나 다시 예전으로 돌아갈 수 있을까?"),
            L("서아",  CharacterEmotion.Happy,      false, "응, 돌아갈 수 있어. 내가 옆에 있을게. 우리 같이 이겨내자."),
        };
        d.choices = new ChoiceData[] { };
        Save(d);
    }

    static void CreateNormalEndDialogue()
    {
        var d = Create("Assets/Resources/Dialogues/NormalEnd_Dialogue.asset");
        d.lines = new DialogueLine[]
        {
            L("도윤",  CharacterEmotion.Shocked,    false, "마약... 내가 마약을 했다고. 내 인생이 어떻게 이렇게 한순간에..."),
            L("민재",  CharacterEmotion.Smug,       false, "정신 차려, 도윤아. 이미 늦었어. 너도 이제 공범이야. 그냥 조용히 지내자고."),
            L("도윤",  CharacterEmotion.Shocked,    false, "아니야, 그럴 리 없어... 다 저리 가! 나한테 연락하지 마!"),
            L("서아",  CharacterEmotion.Worried,    false, "도윤아! 제발 문 좀 열어봐! 같이 방법을 찾아보자!"),
            L("도윤",  CharacterEmotion.Pain,       true,  "다 끝났어. 서아 얼굴을 어떻게 봐. 다 나를 쓰레기처럼 보겠지."),
        };
        d.choices = new ChoiceData[] { };
        Save(d);
    }

    static void CreateBadEndDialogue()
    {
        var d = Create("Assets/Resources/Dialogues/BadEnd_Dialogue.asset");
        d.lines = new DialogueLine[]
        {
            L("도윤",  CharacterEmotion.Pain,  false, "너... 죽여버릴 거야... 헉, 헉... 그런데 민재야... 나 몸이 너무 이상해. 제발... 그 약 하나만 더 주면 안 돼?"),
            L("민재",  CharacterEmotion.Smug,  false, "거봐, 결국 찾을 거면서. 자, 이거 먹으면 다시 행복해질 수 있어. 대신 공짜는 없는 거 알지?"),
            L("도윤",  CharacterEmotion.Pain,  false, "알았어, 뭐든 할게. 제발... 제발 좀 줘!"),
            L("도윤",  CharacterEmotion.Smug,  false, "야, 너 힘들지? 이거 한 알이면 스트레스 싹 날아간다. 한번 먹어볼래?"),
        };
        d.choices = new ChoiceData[] { };
        Save(d);
    }

    // ── 유틸리티 ──────────────────────────────────
    static DialogueLine L(string speaker, CharacterEmotion emotion, bool mono, string text)
        => new DialogueLine { speaker = speaker, emotion = emotion, isMonologue = mono, text = text };

    static ChoiceData C(string label, int i, int r, int a, string next,
                        bool forced, bool reqInt, int minInt)
        => new ChoiceData {
            label = label, intDelta = i, riskDelta = r, addictDelta = a,
            nextScene = next, isForcedBad = forced,
            requiresMinINT = reqInt, minINTValue = minInt };

    static DialogueData Create(string path)
    {
        // 기존 에셋 삭제
        var existing = AssetDatabase.LoadAssetAtPath<DialogueData>(path);
        if (existing != null) AssetDatabase.DeleteAsset(path);

        var d = ScriptableObject.CreateInstance<DialogueData>();
        AssetDatabase.CreateAsset(d, path);
        return d;
    }

    static void Save(DialogueData d)
    {
        EditorUtility.SetDirty(d);
        // 즉시 저장 (배열 데이터가 날아가지 않도록)
        AssetDatabase.SaveAssetIfDirty(d);
    }

    // ── Step 씬에 대화 데이터 연결 ────────────────
    [MenuItem("LAST BRAKE/12. 씬에 대화 데이터 연결")]
    public static void LinkDialogueDataToScenes()
    {
        if (!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo()) return;

        LinkScene("Assets/Scenes/01_Step1_Prologue.unity", "Assets/ScriptableObjects/Step1_Prologue.asset", "02_Step2_Club");
        LinkScene("Assets/Scenes/02_Step2_Club.unity",     "Assets/ScriptableObjects/Step2_Club.asset",     "03_Step3_Morning");
        LinkScene("Assets/Scenes/03_Step3_Morning.unity",  "Assets/ScriptableObjects/Step3_Morning.asset",  "04_Step4_Party");
        LinkScene("Assets/Scenes/04_Step4_Party.unity",    "Assets/ScriptableObjects/Step4_Party.asset",    "05_Step5_Collapse");
        LinkScene("Assets/Scenes/05_Step5_Collapse.unity", "Assets/ScriptableObjects/Step5_Collapse.asset", "");

        LinkEndingDialogue("Assets/Scenes/07_GoodEnd.unity",   "Assets/ScriptableObjects/GoodEnd_Dialogue.asset");
        LinkEndingDialogue("Assets/Scenes/08_NormalEnd.unity", "Assets/ScriptableObjects/NormalEnd_Dialogue.asset");
        LinkEndingDialogue("Assets/Scenes/09_BadEnd.unity",    "Assets/ScriptableObjects/BadEnd_Dialogue.asset");

        AssetDatabase.Refresh();
        EditorSceneManager.OpenScene("Assets/Scenes/00_MainMenu.unity");

        EditorUtility.DisplayDialog("완료",
            "모든 씬에 대화 데이터 연결 완료!\n\n" +
            "이제 게임 시작 버튼을 눌러\n실제 스토리를 플레이할 수 있습니다!\n\n" +
            "▶ 버튼 눌러서 테스트해보세요.",
            "확인");
    }

    static void LinkScene(string scenePath, string dataPath, string nextScene)
    {
        EditorSceneManager.OpenScene(scenePath);

        var sc = Object.FindFirstObjectByType<StepController>();
        var data = AssetDatabase.LoadAssetAtPath<DialogueData>(dataPath);
        if (sc != null && data != null)
        {
            var field = typeof(StepController).GetField("dialogueSequence",
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            if (field != null) field.SetValue(sc, new DialogueData[] { data });

            var nextField = typeof(StepController).GetField("nextScene",
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            if (nextField != null) nextField.SetValue(sc, nextScene);

            EditorUtility.SetDirty(sc);
        }

        // DialogueManager에 DialogueUI 연결
        var dm = Object.FindFirstObjectByType<DialogueManager>();
        var duiGO = GameObject.Find("DialogueManager");
        var dui = duiGO?.GetComponent<DialogueUI>() ?? duiGO?.AddComponent<DialogueUI>();
        if (dm != null && dui != null)
        {
            var f = typeof(DialogueManager).GetField("dialogueUI",
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            if (f != null) f.SetValue(dm, dui);
            EditorUtility.SetDirty(dm);
        }

        EditorSceneManager.SaveScene(EditorSceneManager.GetActiveScene());
    }

    static void LinkEndingDialogue(string scenePath, string dataPath)
    {
        EditorSceneManager.OpenScene(scenePath);
        var data = AssetDatabase.LoadAssetAtPath<DialogueData>(dataPath);

        var goodEnd   = Object.FindFirstObjectByType<GoodEndSequence>();
        var normalEnd = Object.FindFirstObjectByType<NormalEndSequence>();
        var badEnd    = Object.FindFirstObjectByType<BadEndSequence>();

        System.Action<object, string, object> setField = (target, fieldName, value) => {
            if (target == null || value == null) return;
            var f = target.GetType().GetField(fieldName,
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            f?.SetValue(target, value);
            if (target is Object o) EditorUtility.SetDirty(o);
        };

        if (goodEnd   != null) setField(goodEnd,   "endingDialogue", new DialogueData[] { data });
        if (normalEnd != null) setField(normalEnd, "endingDialogue", new DialogueData[] { data });
        if (badEnd    != null) setField(badEnd,    "endingDialogue", new DialogueData[] { data });

        EditorSceneManager.SaveScene(EditorSceneManager.GetActiveScene());
    }
}
