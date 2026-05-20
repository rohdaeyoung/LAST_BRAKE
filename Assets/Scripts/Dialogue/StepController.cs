using UnityEngine;
using UnityEngine.SceneManagement;

// 각 Step 씬의 루트 오브젝트에 붙임
public class StepController : MonoBehaviour
{
    public static StepController Instance { get; private set; }

    [SerializeField] private DialogueData[] dialogueSequence;
    [SerializeField] private string nextScene;

    // 씬 이름 → Resources 경로 매핑
    private static readonly System.Collections.Generic.Dictionary<string, string> SceneToAsset
        = new System.Collections.Generic.Dictionary<string, string>
    {
        { "01_Step1_Prologue",  "Dialogues/Step1_Prologue"  },
        { "02_Step2_Club",      "Dialogues/Step2_Club"      },
        { "03_Step3_Morning",   "Dialogues/Step3_Morning"   },
        { "04_Step4_Party",     "Dialogues/Step4_Party"     },
        { "05_Step5_Collapse",  "Dialogues/Step5_Collapse"  },
    };

    private static readonly System.Collections.Generic.Dictionary<string, string> SceneToNext
        = new System.Collections.Generic.Dictionary<string, string>
    {
        { "01_Step1_Prologue",  "02_Step2_Club"      },
        { "02_Step2_Club",      "03_Step3_Morning"   },
        { "03_Step3_Morning",   "04_Step4_Party"     },
        { "04_Step4_Party",     "05_Step5_Collapse"  },
        { "05_Step5_Collapse",  ""                   },
    };

    private void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;

        // DialogueBootstrap — DialogueUI 없으면 런타임 생성
        gameObject.AddComponent<DialogueBootstrap>();
    }

    private void Start()
    {
        // Inspector 연결이 없으면 Resources에서 자동 로드
        string sceneName = SceneManager.GetActiveScene().name;

        if (dialogueSequence == null || dialogueSequence.Length == 0)
        {
            if (SceneToAsset.TryGetValue(sceneName, out string assetPath))
            {
                var data = Resources.Load<DialogueData>(assetPath);
                if (data != null)
                {
                    dialogueSequence = new DialogueData[] { data };
                    Debug.Log($"[StepController] Resources에서 로드: {assetPath}");
                }
                else
                {
                    Debug.LogWarning($"[StepController] Resources/{assetPath} 없음! 메뉴 11 실행 필요");
                }
            }
        }

        if (string.IsNullOrEmpty(nextScene))
        {
            if (SceneToNext.TryGetValue(sceneName, out string next))
                nextScene = next;
        }

        if (DialogueManager.Instance != null)
            DialogueManager.Instance.StartSequence(dialogueSequence);
        else
            Debug.LogWarning("[StepController] DialogueManager.Instance가 없습니다!");
    }

    // ※ 클릭 처리는 DialogueBootstrap이 생성한 DialogueClickArea Button이 담당.
    //   여기서 Input.GetMouseButtonDown을 중복 호출하면 한 번 클릭에
    //   OnScreenTapped()가 두 번 호출되어 대사가 2줄씩 건너뛰는 버그 발생.
    //   → Update() 클릭 감지 제거.

    public void OnDialogueSequenceComplete()
    {
        if (string.IsNullOrEmpty(nextScene))
        {
            if (GameManager.Instance != null) GameManager.Instance.GoToEnding();
            else UnityEngine.SceneManagement.SceneManager.LoadScene("06_EndingReport");
        }
        else
        {
            if (GameManager.Instance != null) GameManager.Instance.LoadScene(nextScene, 0.8f);
            else UnityEngine.SceneManagement.SceneManager.LoadScene(nextScene);
        }
    }
}
