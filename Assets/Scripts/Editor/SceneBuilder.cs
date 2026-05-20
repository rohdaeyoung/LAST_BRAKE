using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine.UI;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using TMPro;
using System.IO;

public class SceneBuilder : EditorWindow
{
    // ───────────────────────────────────────────
    // 00_MainMenu 자동 세팅
    // ───────────────────────────────────────────
    [MenuItem("LAST BRAKE/3. MainMenu 씬 UI 자동 세팅")]
    public static void BuildMainMenuScene()
    {
        // 저장되지 않은 변경사항 확인
        if (!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo()) return;

        EditorSceneManager.OpenScene("Assets/Scenes/00_MainMenu.unity");

        // 기존 오브젝트 정리 (에디터 스크립트가 만든 것들)
        ClearScene();

        // ── 카메라 ──
        GameObject camObj = new GameObject("Main Camera");
        Camera cam = camObj.AddComponent<Camera>();
        cam.orthographic = true;
        cam.orthographicSize = 5f;
        cam.clearFlags = CameraClearFlags.SolidColor;   // 스카이박스 제거
        cam.backgroundColor = Color.black;
        cam.tag = "MainCamera";
        camObj.AddComponent<AudioListener>();
        camObj.transform.position = new Vector3(0, 0, -10);

        // ── 매니저 오브젝트들 (DontDestroyOnLoad) ──
        CreateManager<GameManager>("GameManager");
        CreateManager<StatManager>("StatManager");
        CreateManager<BGMController>("BGMController");
        CreateManager<PostProcessingController>("PostProcessingController");
        CreateManager<ScreenEffects>("ScreenEffects");

        // ── Global Volume ──
        GameObject volObj = new GameObject("Global Volume");
        var volume = volObj.AddComponent<Volume>();
        volume.isGlobal = true;
        string profilePath = "Assets/ScriptableObjects/VolumeProfile_MainMenu.asset";
        VolumeProfile profile;
        if (File.Exists(profilePath))
            profile = AssetDatabase.LoadAssetAtPath<VolumeProfile>(profilePath);
        else
        {
            profile = ScriptableObject.CreateInstance<VolumeProfile>();
            var vig = profile.Add<Vignette>();
            vig.intensity.value = 0.2f;
            vig.intensity.overrideState = true;
            var col = profile.Add<ColorAdjustments>();
            col.saturation.value = 0f;
            col.saturation.overrideState = true;
            AssetDatabase.CreateAsset(profile, profilePath);
        }
        volume.profile = profile;

        // ── Canvas ──
        GameObject canvasObj = new GameObject("MainMenuCanvas");
        Canvas canvas = canvasObj.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 0;
        CanvasScaler scaler = canvasObj.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1280, 720);
        canvasObj.AddComponent<GraphicRaycaster>();

        // ── 검은 배경 패널 ──
        GameObject bg = CreateUIObject("Background", canvasObj.transform);
        Image bgImg = bg.AddComponent<Image>();
        bgImg.color = Color.black;
        SetFullStretch(bg.GetComponent<RectTransform>());

        // ── 타이틀 텍스트 ──
        GameObject title = CreateUIObject("TitleText", canvasObj.transform);
        TextMeshProUGUI titleTMP = title.AddComponent<TextMeshProUGUI>();
        titleTMP.text = "LAST BRAKE\n끝나지 않는 밤";
        titleTMP.fontSize = 64;
        titleTMP.alignment = TextAlignmentOptions.Center;
        titleTMP.color = Color.white;
        titleTMP.fontStyle = FontStyles.Bold;
        RectTransform titleRT = title.GetComponent<RectTransform>();
        titleRT.anchoredPosition = new Vector2(0, 120);
        titleRT.sizeDelta = new Vector2(900, 200);

        // ── 부제 텍스트 ──
        GameObject sub = CreateUIObject("SubText", canvasObj.transform);
        TextMeshProUGUI subTMP = sub.AddComponent<TextMeshProUGUI>();
        subTMP.text = "중독은 한순간의 사고가 아니라,\n반복된 선택이 만들어낸 비극이다.";
        subTMP.fontSize = 22;
        subTMP.alignment = TextAlignmentOptions.Center;
        subTMP.color = new Color(0.8f, 0.8f, 0.8f);
        RectTransform subRT = sub.GetComponent<RectTransform>();
        subRT.anchoredPosition = new Vector2(0, 20);
        subRT.sizeDelta = new Vector2(800, 80);

        // ── 시작 버튼 ──
        GameObject startBtn = CreateButton("StartButton", canvasObj.transform,
            "게임 시작", new Vector2(0, -100), new Vector2(300, 70));

        // ── 종료 버튼 ──
        GameObject quitBtn = CreateButton("QuitButton", canvasObj.transform,
            "종료", new Vector2(0, -200), new Vector2(300, 70));

        // ── MainMenuUI 스크립트 연결 ──
        MainMenuUI menuUI = canvasObj.AddComponent<MainMenuUI>();
        // 버튼 자동 연결은 SerializeField라 Inspector에서 수동 연결 필요
        // → 안내 다이얼로그에서 설명

        // ── EventSystem ──
        GameObject es = new GameObject("EventSystem");
        es.AddComponent<UnityEngine.EventSystems.EventSystem>();
        es.AddComponent<UnityEngine.EventSystems.StandaloneInputModule>();

        // ── 페이드 패널 (ScreenEffects용) ──
        GameObject fadeObj = CreateUIObject("FadePanel", canvasObj.transform);
        Image fadeImg = fadeObj.AddComponent<Image>();
        fadeImg.color = Color.black;
        fadeObj.SetActive(false);
        SetFullStretch(fadeObj.GetComponent<RectTransform>());

        EditorSceneManager.SaveScene(EditorSceneManager.GetActiveScene());
        AssetDatabase.Refresh();

        EditorUtility.DisplayDialog("완료",
            "MainMenu 씬 UI 세팅 완료!\n\n" +
            "다음 할 일:\n" +
            "Hierarchy에서 MainMenuCanvas 선택 →\n" +
            "Inspector에서 MainMenuUI 컴포넌트의\n" +
            "· Start Button → StartButton 연결\n" +
            "· Quit Button  → QuitButton 연결\n\n" +
            "그 다음 LAST BRAKE → 4. Step1 씬 세팅 클릭",
            "확인");
    }

    // ───────────────────────────────────────────
    // Step 씬 (01~05) 일괄 자동 세팅
    // ───────────────────────────────────────────
    [MenuItem("LAST BRAKE/4. Step 씬 일괄 세팅 (01~05)")]
    public static void BuildAllStepScenes()
    {
        if (!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo()) return;

        string[] stepScenes = {
            "Assets/Scenes/01_Step1_Prologue.unity",
            "Assets/Scenes/02_Step2_Club.unity",
            "Assets/Scenes/03_Step3_Morning.unity",
            "Assets/Scenes/04_Step4_Party.unity",
            "Assets/Scenes/05_Step5_Collapse.unity"
        };

        foreach (string scenePath in stepScenes)
        {
            EditorSceneManager.OpenScene(scenePath);
            ClearScene();
            BuildStepScene();
            EditorSceneManager.SaveScene(EditorSceneManager.GetActiveScene());
            Debug.Log($"Step 씬 세팅 완료: {scenePath}");
        }

        AssetDatabase.Refresh();
        EditorSceneManager.OpenScene("Assets/Scenes/01_Step1_Prologue.unity");

        EditorUtility.DisplayDialog("완료",
            "Step 씬 01~05 세팅 완료!\n\n" +
            "각 씬의 StepController에\n" +
            "Dialogue Sequence 데이터를 연결해주세요.\n\n" +
            "다음: LAST BRAKE → 5. 엔딩 씬 세팅",
            "확인");
    }

    private static void BuildStepScene()
    {
        // 카메라
        GameObject camObj = new GameObject("Main Camera");
        Camera cam = camObj.AddComponent<Camera>();
        cam.orthographic = true;
        cam.orthographicSize = 5f;
        cam.clearFlags = CameraClearFlags.SolidColor;   // 스카이박스 제거
        cam.backgroundColor = Color.black;
        cam.tag = "MainCamera";
        camObj.AddComponent<AudioListener>();
        camObj.transform.position = new Vector3(0, 0, -10);

        // Global Volume
        GameObject volObj = new GameObject("Global Volume");
        var volume = volObj.AddComponent<Volume>();
        volume.isGlobal = true;

        // ScreenEffects (카메라 흔들림, 노이즈)
        GameObject seObj = new GameObject("ScreenEffects");
        seObj.AddComponent<ScreenEffects>();

        // StepController + DialogueManager + ChoiceSystem
        GameObject scObj = new GameObject("StepController");
        scObj.AddComponent<StepController>();

        GameObject dmObj = new GameObject("DialogueManager");
        dmObj.AddComponent<DialogueManager>();

        GameObject csObj = new GameObject("ChoiceSystem");
        csObj.AddComponent<ChoiceSystem>();

        // ── 배경 스프라이트 자리 (빈 오브젝트, 나중에 스프라이트 교체)
        GameObject bg = new GameObject("Background");
        bg.AddComponent<SpriteRenderer>().sortingOrder = -10;

        // ── 캐릭터 슬롯 (도윤, 민재, 하준, 서아)
        string[] chars = { "Doyun", "Minjae", "Hajun", "Seoa" };
        foreach (string c in chars)
        {
            GameObject ch = new GameObject($"Char_{c}");
            ch.AddComponent<SpriteRenderer>().sortingOrder = 0;
            ch.SetActive(false);
        }

        // ── Canvas
        GameObject canvasObj = new GameObject("DialogueCanvas");
        Canvas canvas = canvasObj.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 10;
        CanvasScaler scaler = canvasObj.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1280, 720);
        canvasObj.AddComponent<GraphicRaycaster>();

        // ── 대화창 패널 ──
        GameObject dialoguePanel = CreateUIObject("DialoguePanel", canvasObj.transform);
        Image dlgImg = dialoguePanel.AddComponent<Image>();
        dlgImg.color = new Color(0, 0, 0, 0.85f);
        RectTransform dlgRT = dialoguePanel.GetComponent<RectTransform>();
        dlgRT.anchorMin = new Vector2(0, 0);
        dlgRT.anchorMax = new Vector2(1, 0);
        dlgRT.offsetMin = new Vector2(20, 20);
        dlgRT.offsetMax = new Vector2(-20, 220);

        // 화자 이름
        GameObject speakerObj = CreateUIObject("SpeakerName", dialoguePanel.transform);
        TextMeshProUGUI speakerTMP = speakerObj.AddComponent<TextMeshProUGUI>();
        speakerTMP.fontSize = 24;
        speakerTMP.color = new Color(1f, 0.85f, 0.4f);
        speakerTMP.fontStyle = FontStyles.Bold;
        RectTransform spRT = speakerObj.GetComponent<RectTransform>();
        spRT.anchoredPosition = new Vector2(20, 160);
        spRT.sizeDelta = new Vector2(400, 40);
        spRT.anchorMin = spRT.anchorMax = new Vector2(0, 0);
        spRT.pivot = new Vector2(0, 0);

        // 대화 텍스트
        GameObject dlgTextObj = CreateUIObject("DialogueText", dialoguePanel.transform);
        TextMeshProUGUI dlgTMP = dlgTextObj.AddComponent<TextMeshProUGUI>();
        dlgTMP.fontSize = 22;
        dlgTMP.color = Color.white;
        RectTransform dtRT = dlgTextObj.GetComponent<RectTransform>();
        dtRT.anchorMin = new Vector2(0, 0);
        dtRT.anchorMax = new Vector2(1, 1);
        dtRT.offsetMin = new Vector2(20, 20);
        dtRT.offsetMax = new Vector2(-20, -50);

        // ── 투명 클릭 버튼 (대화 진행용) ──
        // UI Button 방식 → Input System 종류(Old/New)에 무관하게 동작
        GameObject clickAreaObj = CreateUIObject("DialogueClickArea", dialoguePanel.transform);
        Image clickImg = clickAreaObj.AddComponent<Image>();
        clickImg.color = new Color(0f, 0f, 0f, 0f); // 완전 투명
        Button clickBtn = clickAreaObj.AddComponent<Button>();
        clickBtn.transition = Selectable.Transition.None;
        RectTransform clickRT = clickAreaObj.GetComponent<RectTransform>();
        clickRT.anchorMin = Vector2.zero;
        clickRT.anchorMax = Vector2.one;
        clickRT.offsetMin = clickRT.offsetMax = Vector2.zero;
        // 런타임 onClick 연결은 DialogueClickHandler 컴포넌트로 위임
        clickAreaObj.AddComponent<DialogueClickHandler>();

        dialoguePanel.SetActive(false);

        // ── 선택지 패널 ──
        GameObject choicePanel = CreateUIObject("ChoicePanel", canvasObj.transform);
        Image choiceImg = choicePanel.AddComponent<Image>();
        choiceImg.color = new Color(0, 0, 0, 0.7f);
        RectTransform cpRT = choicePanel.GetComponent<RectTransform>();
        cpRT.anchoredPosition = new Vector2(0, -50);
        cpRT.sizeDelta = new Vector2(600, 280);

        // 선택지 버튼 3개
        for (int i = 0; i < 3; i++)
        {
            float yPos = 90f - (i * 95f);
            GameObject btn = CreateButton($"ChoiceButton_{i}", choicePanel.transform,
                $"선택지 {i + 1}", new Vector2(0, yPos), new Vector2(540, 80));
        }
        choicePanel.SetActive(false);

        // ── 페이드 패널 ──
        GameObject fadeObj = CreateUIObject("FadePanel", canvasObj.transform);
        Image fadeImg = fadeObj.AddComponent<Image>();
        fadeImg.color = Color.black;
        fadeObj.SetActive(false);
        SetFullStretch(fadeObj.GetComponent<RectTransform>());

        // ── 노이즈 오버레이 ──
        GameObject noiseObj = CreateUIObject("NoiseOverlay", canvasObj.transform);
        Image noiseImg = noiseObj.AddComponent<Image>();
        noiseImg.color = new Color(0.8f, 0.1f, 0.1f, 0f);
        noiseObj.SetActive(false);
        SetFullStretch(noiseObj.GetComponent<RectTransform>());

        // ── DialogueUI 컴포넌트를 DialoguePanel 패널에 연결 ──
        DialogueUI dui = dialoguePanel.AddComponent<DialogueUI>();
        // 자동 탐색(Awake)에서 SpeakerName / DialogueText를 찾으므로 별도 연결 불필요

        // ── ChoiceUI 컴포넌트를 ChoicePanel에 추가 ──
        choicePanel.AddComponent<ChoiceUI>();

        // EventSystem
        GameObject es = new GameObject("EventSystem");
        es.AddComponent<UnityEngine.EventSystems.EventSystem>();
        es.AddComponent<UnityEngine.EventSystems.StandaloneInputModule>();
    }

    // ───────────────────────────────────────────
    // 엔딩 씬 (06~10) 자동 세팅
    // ───────────────────────────────────────────
    [MenuItem("LAST BRAKE/5. 엔딩 씬 일괄 세팅 (06~10)")]
    public static void BuildEndingScenes()
    {
        if (!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo()) return;

        BuildEndingReport();
        BuildGoodEnd();
        BuildNormalEnd();
        BuildBadEnd();
        BuildTrueEnd();

        AssetDatabase.Refresh();
        EditorSceneManager.OpenScene("Assets/Scenes/06_EndingReport.unity");

        EditorUtility.DisplayDialog("완료",
            "엔딩 씬 06~10 세팅 완료!\n\n" +
            "이제 픽셀 아트 스프라이트를\n" +
            "Assets/Sprites/Characters/ 에 넣으면\n" +
            "게임 기본 구조가 완성됩니다.",
            "확인");
    }

    private static void BuildEndingReport()
    {
        EditorSceneManager.OpenScene("Assets/Scenes/06_EndingReport.unity");
        ClearScene();

        GameObject camObj = new GameObject("Main Camera");
        Camera cam = camObj.AddComponent<Camera>();
        cam.orthographic = true; cam.backgroundColor = Color.black;
        cam.tag = "MainCamera"; camObj.AddComponent<AudioListener>();
        camObj.transform.position = new Vector3(0, 0, -10);

        new GameObject("EndingCalculator").AddComponent<EndingCalculator>();

        GameObject canvasObj = new GameObject("ReportCanvas");
        Canvas canvas = canvasObj.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        CanvasScaler sc = canvasObj.AddComponent<CanvasScaler>();
        sc.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        sc.referenceResolution = new Vector2(1280, 720);
        canvasObj.AddComponent<GraphicRaycaster>();

        // 배경
        GameObject bg = CreateUIObject("Background", canvasObj.transform);
        bg.AddComponent<Image>().color = new Color(0.05f, 0.05f, 0.05f);
        SetFullStretch(bg.GetComponent<RectTransform>());

        // 타이틀
        GameObject title = CreateUIObject("ReportTitle", canvasObj.transform);
        var titleTMP = title.AddComponent<TextMeshProUGUI>();
        titleTMP.text = "— 당신의 선택 기록 —";
        titleTMP.fontSize = 36; titleTMP.alignment = TextAlignmentOptions.Center;
        titleTMP.color = Color.white;
        title.GetComponent<RectTransform>().anchoredPosition = new Vector2(0, 250);
        title.GetComponent<RectTransform>().sizeDelta = new Vector2(700, 60);

        // 수치 슬롯 3개
        string[] statNames = { "판단력  ???", "위험도  ???", "의존도  ???" };
        float[] yPos = { 120f, 0f, -120f };
        for (int i = 0; i < 3; i++)
        {
            GameObject statObj = CreateUIObject($"Stat_{i}", canvasObj.transform);
            var statTMP = statObj.AddComponent<TextMeshProUGUI>();
            statTMP.text = statNames[i];
            statTMP.fontSize = 32; statTMP.alignment = TextAlignmentOptions.Center;
            statTMP.color = Color.gray;
            statObj.GetComponent<RectTransform>().anchoredPosition = new Vector2(-100, yPos[i]);
            statObj.GetComponent<RectTransform>().sizeDelta = new Vector2(400, 50);

            // 슬라이더
            GameObject sliderObj = new GameObject($"StatBar_{i}");
            sliderObj.transform.SetParent(canvasObj.transform, false);
            Slider slider = sliderObj.AddComponent<Slider>();
            sliderObj.GetComponent<RectTransform>().anchoredPosition = new Vector2(200, yPos[i]);
            sliderObj.GetComponent<RectTransform>().sizeDelta = new Vector2(300, 30);
        }

        new GameObject("EventSystem").AddComponent<UnityEngine.EventSystems.EventSystem>();
        EditorSceneManager.SaveScene(EditorSceneManager.GetActiveScene());
    }

    private static void BuildGoodEnd()
    {
        EditorSceneManager.OpenScene("Assets/Scenes/07_GoodEnd.unity");
        ClearScene();
        BuildBasicEndingScene("GoodEndCanvas");
        new GameObject("GoodEndSequence").AddComponent<GoodEndSequence>();
        // FourthWallBreak 오브젝트 (비활성 상태로 대기)
        GameObject fwb = new GameObject("FourthWallBreak");
        fwb.AddComponent<FourthWallBreak>();
        fwb.SetActive(false);
        EditorSceneManager.SaveScene(EditorSceneManager.GetActiveScene());
    }

    private static void BuildNormalEnd()
    {
        EditorSceneManager.OpenScene("Assets/Scenes/08_NormalEnd.unity");
        ClearScene();
        BuildBasicEndingScene("NormalEndCanvas");
        new GameObject("NormalEndSequence").AddComponent<NormalEndSequence>();
        GameObject fwb = new GameObject("FourthWallBreak");
        fwb.AddComponent<FourthWallBreak>();
        fwb.SetActive(false);
        EditorSceneManager.SaveScene(EditorSceneManager.GetActiveScene());
    }

    private static void BuildBadEnd()
    {
        EditorSceneManager.OpenScene("Assets/Scenes/09_BadEnd.unity");
        ClearScene();
        BuildBasicEndingScene("BadEndCanvas");
        new GameObject("BadEndSequence").AddComponent<BadEndSequence>();
        GameObject fwb = new GameObject("FourthWallBreak");
        fwb.AddComponent<FourthWallBreak>();
        fwb.SetActive(false);
        EditorSceneManager.SaveScene(EditorSceneManager.GetActiveScene());
    }

    private static void BuildTrueEnd()
    {
        EditorSceneManager.OpenScene("Assets/Scenes/10_TrueEnd.unity");
        ClearScene();
        BuildBasicEndingScene("TrueEndCanvas");
        new GameObject("TrueEndSequence").AddComponent<TrueEndSequence>();
        GameObject fwb = new GameObject("FourthWallBreak");
        fwb.AddComponent<FourthWallBreak>();
        fwb.SetActive(false);
        EditorSceneManager.SaveScene(EditorSceneManager.GetActiveScene());
    }

    private static void BuildBasicEndingScene(string canvasName)
    {
        GameObject camObj = new GameObject("Main Camera");
        Camera cam = camObj.AddComponent<Camera>();
        cam.orthographic = true; cam.backgroundColor = Color.black;
        cam.tag = "MainCamera"; camObj.AddComponent<AudioListener>();
        camObj.transform.position = new Vector3(0, 0, -10);

        // Global Volume
        GameObject volObj = new GameObject("Global Volume");
        var volume = volObj.AddComponent<Volume>();
        volume.isGlobal = true;

        // 도윤 캐릭터 (메타 엔딩 응시용)
        GameObject doyun = new GameObject("Doyun_Character");
        doyun.AddComponent<SpriteRenderer>().sortingLayerName = "Default";
        doyun.transform.position = new Vector3(0, -1f, 0);

        // Canvas
        GameObject canvasObj = new GameObject(canvasName);
        Canvas canvas = canvasObj.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 10;
        CanvasScaler sc = canvasObj.AddComponent<CanvasScaler>();
        sc.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        sc.referenceResolution = new Vector2(1280, 720);
        canvasObj.AddComponent<GraphicRaycaster>();

        // 핵심 메시지 텍스트 (FourthWallBreak용)
        GameObject msgGroup = new GameObject("MessageGroup");
        msgGroup.transform.SetParent(canvasObj.transform, false);
        CanvasGroup cg = msgGroup.AddComponent<CanvasGroup>();
        cg.alpha = 0f;

        GameObject msgObj = CreateUIObject("CoreMessage", msgGroup.transform);
        var msgTMP = msgObj.AddComponent<TextMeshProUGUI>();
        msgTMP.text = "중독은 특별한 사람이 아니라,\n반복된 선택에서 시작됩니다.";
        msgTMP.fontSize = 36; msgTMP.alignment = TextAlignmentOptions.Center;
        msgTMP.color = Color.white;
        msgObj.GetComponent<RectTransform>().anchoredPosition = new Vector2(0, 80);
        msgObj.GetComponent<RectTransform>().sizeDelta = new Vector2(900, 120);

        // 선택지 (다시 시작 / 종료)
        GameObject choicePanel = CreateUIObject("FinalChoicePanel", canvasObj.transform);
        choicePanel.AddComponent<Image>().color = Color.clear;
        choicePanel.GetComponent<RectTransform>().anchoredPosition = new Vector2(0, -150);
        choicePanel.GetComponent<RectTransform>().sizeDelta = new Vector2(700, 80);

        CreateButton("RestartButton", choicePanel.transform, "다시 시작한다",
            new Vector2(-180, 0), new Vector2(280, 70));
        CreateButton("QuitButton", choicePanel.transform, "게임 종료",
            new Vector2(180, 0), new Vector2(280, 70));
        choicePanel.SetActive(false);

        // 페이드 패널
        GameObject fadeObj = CreateUIObject("FadePanel", canvasObj.transform);
        fadeObj.AddComponent<Image>().color = Color.black;
        fadeObj.SetActive(false);
        SetFullStretch(fadeObj.GetComponent<RectTransform>());

        GameObject es = new GameObject("EventSystem");
        es.AddComponent<UnityEngine.EventSystems.EventSystem>();
        es.AddComponent<UnityEngine.EventSystems.StandaloneInputModule>();
    }

    // ─── 유틸리티 ───────────────────────────────

    private static void ClearScene()
    {
        var roots = UnityEngine.SceneManagement.SceneManager.GetActiveScene().GetRootGameObjects();
        foreach (var go in roots)
            Object.DestroyImmediate(go);
    }

    private static GameObject CreateManager<T>(string name) where T : Component
    {
        GameObject go = new GameObject(name);
        go.AddComponent<T>();
        return go;
    }

    private static GameObject CreateUIObject(string name, Transform parent)
    {
        GameObject go = new GameObject(name);
        go.transform.SetParent(parent, false);
        go.AddComponent<RectTransform>();
        return go;
    }

    private static GameObject CreateButton(string name, Transform parent, string label,
        Vector2 pos, Vector2 size)
    {
        GameObject btnObj = new GameObject(name);
        btnObj.transform.SetParent(parent, false);
        Image img = btnObj.AddComponent<Image>();
        img.color = new Color(0.15f, 0.15f, 0.15f);
        Button btn = btnObj.AddComponent<Button>();
        ColorBlock cb = btn.colors;
        cb.highlightedColor = new Color(0.3f, 0.3f, 0.3f);
        cb.pressedColor = new Color(0.05f, 0.05f, 0.05f);
        btn.colors = cb;
        RectTransform rt = btnObj.GetComponent<RectTransform>();
        rt.anchoredPosition = pos;
        rt.sizeDelta = size;

        GameObject textObj = new GameObject("Label");
        textObj.transform.SetParent(btnObj.transform, false);
        TextMeshProUGUI tmp = textObj.AddComponent<TextMeshProUGUI>();
        tmp.text = label;
        tmp.fontSize = 26;
        tmp.alignment = TextAlignmentOptions.Center;
        tmp.color = Color.white;
        RectTransform textRT = textObj.GetComponent<RectTransform>();
        textRT.anchorMin = Vector2.zero;
        textRT.anchorMax = Vector2.one;
        textRT.offsetMin = textRT.offsetMax = Vector2.zero;

        return btnObj;
    }

    private static void SetFullStretch(RectTransform rt)
    {
        rt.anchorMin = Vector2.zero;
        rt.anchorMax = Vector2.one;
        rt.offsetMin = rt.offsetMax = Vector2.zero;
    }
}
