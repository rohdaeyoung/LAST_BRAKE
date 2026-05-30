using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.SceneManagement;
using System.Collections;

/// <summary>
/// LAST BRAKE — BGM 중앙 관리자
///
/// ▸ 씬 로드 시 씬 이름에 맞는 BGM 자동 선택
/// ▸ 수치 상태에 따라 BGM 동적 전환
///
/// BGM 매핑
/// ┌──────────────────┬──────────────────────────────┐
/// │ 씬               │ BGM                          │
/// ├──────────────────┼──────────────────────────────┤
/// │ 00_MainMenu      │ clipNormal  (다크 앰비언트)    │
/// │ 01_Prologue      │ clipNormal                   │
/// │ 02_Club          │ clipClub    (먹먹한 클럽음)    │
/// │ 03_Morning       │ clipNormal                   │
/// │ 04_Party         │ clipClub                     │
/// │ 05_Collapse      │ clipEerie   (으스스한 베드음)  │
/// │ 07~10 Ending     │ clipEerie  (FourthWallBreak  │
/// │                  │  시작 시 FadeOut으로 끊김)     │
/// └──────────────────┴──────────────────────────────┘
///
/// RISK ≥ 60: SetDistorted(true)로 현재 트랙에 피치/왜곡 효과
/// ADDICT ≥ 70: PlayEerie() — 으스스한 분위기로 전환
/// </summary>
public class BGMController : MonoBehaviour
{
    public static BGMController Instance { get; private set; }

    // ── Inspector 클립 (AudioLinker 메뉴 30 자동 연결) ────────────────
    [Header("BGM 클립 (AudioLinker 메뉴 30 자동 연결)")]
    [SerializeField] private AudioClip clipNormal;     // BGM_Normal_DarkAmbient_Loop
    [SerializeField] private AudioClip clipClub;       // BGM_Club_Muffled_Loop
    [SerializeField] private AudioClip clipDistorted;  // BGM_Distorted_Risk_Loop
    [SerializeField] private AudioClip clipEerie;      // BGM_LastBrake_EerieBed_Loop  ← 신규

    [Header("설정")]
    [SerializeField] private float fadeDuration = 1.5f;
    [SerializeField] [Range(0f,1f)] private float bgmVolume = 0.7f;

    // ── 런타임 ──────────────────────────────────────────────────────────
    private AudioSource  bgmSource;
    [SerializeField] private AudioMixer audioMixer;  // BGM_Pitch, BGM_Distortion 파라미터
    private Coroutine    fadeCoroutine;
    private bool         _distorted;

    // ─────────────────────────────────────────────────────────────────────
    private void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
        DontDestroyOnLoad(gameObject);

        bgmSource            = GetComponent<AudioSource>() ?? gameObject.AddComponent<AudioSource>();
        bgmSource.loop       = true;
        bgmSource.volume     = bgmVolume;
        bgmSource.playOnAwake = false;

        // 씬 전환마다 자동 BGM 선택
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnDestroy()
    {
        if (Instance == this)
        {
            SceneManager.sceneLoaded -= OnSceneLoaded;
            Instance = null;
        }
    }

    // ── 씬 로드 시 자동 BGM ──────────────────────────────────────────────
    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        // 왜곡 효과 초기화
        _distorted = false;
        SetDistorted(false);

        PlayForScene(scene.name);
    }

    /// <summary>씬 이름으로 BGM 자동 선택. 외부에서도 직접 호출 가능.</summary>
    public void PlayForScene(string sceneName)
    {
        AudioClip target = sceneName switch
        {
            var s when s.Contains("Club")     => clipClub,    // 02_Step2_Club
            var s when s.Contains("Party")    => clipClub,    // 04_Step4_Party
            var s when s.Contains("Collapse") => clipEerie,   // 05_Step5_Collapse
            var s when s.Contains("End")      => clipEerie,   // 07~10 Ending 씬
            _                                 => clipNormal,  // 나머지 (MainMenu, Prologue, Morning)
        };

        if (target == null) return;
        // 이미 같은 클립이 재생 중이면 스킵
        if (bgmSource.clip == target && bgmSource.isPlaying) return;
        CrossfadeTo(target, bgmVolume);
    }

    // ══════════════════════════════════════════════════════════════════════
    //  퍼블릭 API
    // ══════════════════════════════════════════════════════════════════════

    public void PlayNormal()     => CrossfadeTo(clipNormal,    bgmVolume);
    public void PlayClub()       => CrossfadeTo(clipClub,      bgmVolume);
    public void PlayDistorted()  => CrossfadeTo(clipDistorted, bgmVolume);
    public void PlayEerie()      => CrossfadeTo(clipEerie,     bgmVolume);

    /// <summary>RISK ≥ 60: 현재 트랙에 피치·왜곡 AudioMixer 효과 적용</summary>
    public void SetDistorted(bool on)
    {
        _distorted = on;
        if (audioMixer == null) return;
        if (on)
        {
            audioMixer.SetFloat("BGM_Pitch",      0.85f);
            audioMixer.SetFloat("BGM_Distortion", 0.6f);
        }
        else
        {
            audioMixer.SetFloat("BGM_Pitch",      1f);
            audioMixer.SetFloat("BGM_Distortion", 0f);
        }
    }

    public void FadeOut(float duration)
    {
        if (bgmSource == null) return;
        if (fadeCoroutine != null) StopCoroutine(fadeCoroutine);
        fadeCoroutine = StartCoroutine(FadeVolumeRoutine(bgmSource.volume, 0f, duration));
    }

    public void FadeIn(float duration)
    {
        if (bgmSource == null) return;
        if (!bgmSource.isPlaying) bgmSource.Play();
        if (fadeCoroutine != null) StopCoroutine(fadeCoroutine);
        fadeCoroutine = StartCoroutine(FadeVolumeRoutine(bgmSource.volume, bgmVolume, duration));
    }

    // ── 내부 ─────────────────────────────────────────────────────────────
    private void CrossfadeTo(AudioClip clip, float targetVolume)
    {
        if (clip == null || bgmSource == null) return;
        if (fadeCoroutine != null) StopCoroutine(fadeCoroutine);
        fadeCoroutine = StartCoroutine(CrossfadeRoutine(clip, targetVolume));
    }

    private IEnumerator CrossfadeRoutine(AudioClip clip, float targetVolume)
    {
        float half = fadeDuration * 0.5f;
        if (bgmSource.isPlaying)
            yield return FadeVolumeRoutine(bgmSource.volume, 0f, half);
        if (bgmSource == null) yield break;
        bgmSource.clip = clip;
        bgmSource.Play();
        yield return FadeVolumeRoutine(0f, targetVolume, half);
        fadeCoroutine = null;
    }

    private IEnumerator FadeVolumeRoutine(float from, float to, float duration)
    {
        if (bgmSource == null) yield break;
        float elapsed = 0f;
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            if (bgmSource == null) yield break;
            bgmSource.volume = Mathf.Lerp(from, to, elapsed / duration);
            yield return null;
        }
        if (bgmSource != null) bgmSource.volume = to;
    }
}
