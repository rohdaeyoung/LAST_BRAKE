using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine.SceneManagement;
using System.IO;
using System.Collections.Generic;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using UnityEngine.UI;
using TMPro;

public class ProjectSetup : EditorWindow
{
    [MenuItem("LAST BRAKE/1. 씬 전체 생성 + 빌드 등록")]
    public static void CreateAllScenes()
    {
        string[] sceneNames = {
            "00_MainMenu",
            "01_Step1_Prologue",
            "02_Step2_Club",
            "03_Step3_Morning",
            "04_Step4_Party",
            "05_Step5_Collapse",
            "06_EndingReport",
            "07_GoodEnd",
            "08_NormalEnd",
            "09_BadEnd",
            "10_TrueEnd"
        };

        string scenesPath = "Assets/Scenes";
        if (!Directory.Exists(scenesPath))
            Directory.CreateDirectory(scenesPath);

        List<EditorBuildSettingsScene> buildScenes = new List<EditorBuildSettingsScene>();

        foreach (string sceneName in sceneNames)
        {
            string scenePath = $"{scenesPath}/{sceneName}.unity";

            // 새 씬 생성
            var newScene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);

            // 기본 카메라 추가
            GameObject camObj = new GameObject("Main Camera");
            Camera cam = camObj.AddComponent<Camera>();
            cam.orthographic = true;
            cam.orthographicSize = 5f;
            cam.backgroundColor = Color.black;
            cam.tag = "MainCamera";
            camObj.AddComponent<AudioListener>();

            // 씬별 기본 오브젝트 설정
            SetupScene(sceneName, newScene);

            EditorSceneManager.SaveScene(newScene, scenePath);
            buildScenes.Add(new EditorBuildSettingsScene(scenePath, true));

            Debug.Log($"씬 생성 완료: {scenePath}");
        }

        EditorBuildSettings.scenes = buildScenes.ToArray();
        AssetDatabase.Refresh();

        EditorUtility.DisplayDialog(
            "완료",
            $"씬 {sceneNames.Length}개 생성 + Build Settings 등록 완료!\n\nAssets/Scenes/ 폴더를 확인하세요.",
            "확인"
        );
    }

    private static void SetupScene(string sceneName, Scene scene)
    {
        if (sceneName == "00_MainMenu")
        {
            // 메인 메뉴: MainMenuUI 오브젝트
            GameObject managers = new GameObject("--- MANAGERS ---");
            new GameObject("GameManager").AddComponent<GameManager>();

            GameObject canvas = CreateCanvas("MainMenuCanvas");
            Debug.Log("MainMenu 씬 기본 세팅 완료");
        }
        else if (sceneName.StartsWith("01_") || sceneName.StartsWith("02_") ||
                 sceneName.StartsWith("03_") || sceneName.StartsWith("04_") ||
                 sceneName.StartsWith("05_"))
        {
            // Step 씬: StepController + DialogueManager 오브젝트 준비
            new GameObject("StepController").AddComponent<StepController>();
            new GameObject("DialogueManager").AddComponent<DialogueManager>();
            new GameObject("ChoiceSystem").AddComponent<ChoiceSystem>();
            new GameObject("ScreenEffects").AddComponent<ScreenEffects>();

            // Global Volume (Post Processing)
            GameObject vol = new GameObject("Global Volume");
            var volume = vol.AddComponent<Volume>();
            volume.isGlobal = true;
            volume.profile = CreateVolumeProfile(sceneName);

            CreateCanvas("DialogueCanvas");
            Debug.Log($"{sceneName} 씬 기본 세팅 완료");
        }
        else if (sceneName == "06_EndingReport")
        {
            new GameObject("EndingCalculator").AddComponent<EndingCalculator>();
            new GameObject("StatReportUI").AddComponent<StatReportUI>();
            CreateCanvas("ReportCanvas");
        }
        else if (sceneName.Contains("End"))
        {
            // 엔딩 씬
            GameObject vol = new GameObject("Global Volume");
            var volume = vol.AddComponent<Volume>();
            volume.isGlobal = true;
            volume.profile = CreateVolumeProfile(sceneName);
            CreateCanvas("EndingCanvas");
            Debug.Log($"{sceneName} 씬 기본 세팅 완료");
        }
    }

    private static GameObject CreateCanvas(string name)
    {
        GameObject canvasObj = new GameObject(name);
        Canvas canvas = canvasObj.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvasObj.AddComponent<CanvasScaler>();
        canvasObj.AddComponent<GraphicRaycaster>();

        // EventSystem
        if (Object.FindFirstObjectByType<UnityEngine.EventSystems.EventSystem>() == null)
        {
            GameObject es = new GameObject("EventSystem");
            es.AddComponent<UnityEngine.EventSystems.EventSystem>();
            es.AddComponent<UnityEngine.EventSystems.StandaloneInputModule>();
        }
        return canvasObj;
    }

    private static VolumeProfile CreateVolumeProfile(string sceneName)
    {
        string profilePath = $"Assets/ScriptableObjects/VolumeProfile_{sceneName}.asset";

        var profile = ScriptableObject.CreateInstance<VolumeProfile>();

        var vignette = profile.Add<Vignette>();
        vignette.intensity.value = 0.2f;
        vignette.intensity.overrideState = true;

        var chromatic = profile.Add<ChromaticAberration>();
        chromatic.intensity.value = 0f;
        chromatic.intensity.overrideState = true;

        var colorAdj = profile.Add<ColorAdjustments>();
        colorAdj.saturation.value = 0f;
        colorAdj.saturation.overrideState = true;

        AssetDatabase.CreateAsset(profile, profilePath);
        return profile;
    }

    [MenuItem("LAST BRAKE/2. Persistent 매니저 씬에 추가")]
    public static void AddPersistentManagers()
    {
        // 00_MainMenu 씬에 DontDestroyOnLoad 매니저들 추가 안내
        EditorUtility.DisplayDialog(
            "안내",
            "00_MainMenu 씬을 열고\n\nHierarchy에서:\n" +
            "GameManager 오브젝트 → GameManager.cs 컴포넌트 확인\n" +
            "StatManager 오브젝트 추가 → StatManager.cs 연결\n" +
            "BGMController 오브젝트 추가 → BGMController.cs 연결\n" +
            "PostProcessingController 추가\n\n" +
            "이 매니저들은 DontDestroyOnLoad라 씬이 바뀌어도 유지됩니다.",
            "확인"
        );
    }
}
