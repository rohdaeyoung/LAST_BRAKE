using UnityEngine;
using System.Collections;

/// <summary>
/// LAST BRAKE — 전체 SFX 중앙 관리자
/// DontDestroyOnLoad 싱글턴.
/// AudioLinker (메뉴 31)가 Inspector 필드에 클립을 자동 연결.
/// 메뉴 32 "오디오 연결 해제"로 모든 클립을 null 처리해 무음 상태로 되돌릴 수 있음.
/// </summary>
public class SFXManager : MonoBehaviour
{
    public static SFXManager Instance { get; private set; }

    // ── AudioSource ───────────────────────────────────────────────
    private AudioSource _sfxSource;    // 일회성 SFX (PlayOneShot)
    private AudioSource _loopSource;   // 루프 환경음 / 심장박동 등

    // ══════════════════════════════════════════════════════════════
    //  Inspector 클립 — AudioLinker가 자동 연결
    // ══════════════════════════════════════════════════════════════

    [Header("── UI ──────────────────────────────")]
    [SerializeField] public AudioClip sfxUISelect;          // 일반 선택지 클릭
    [SerializeField] public AudioClip sfxUIDangerConfirm;   // 위험한 선택 확인
    [SerializeField] public AudioClip sfxUIForcedClick;     // 강제 선택(배드 전용)
    [SerializeField] public AudioClip sfxUILocked;          // 잠긴 선택지
    [SerializeField] public AudioClip sfxUIStatReveal;      // 수치 공개 애니메이션

    [Header("── FX 오버레이 ─────────────────────")]
    [SerializeField] public AudioClip sfxRedWarningHit;     // 붉은 경고 플래시
    [SerializeField] public AudioClip sfxGlitchBurst;       // 글리치 버스트
    [SerializeField] public AudioClip sfxBlurPulseWhoosh;   // 블러 펄스
    [SerializeField] public AudioClip sfxMessageReveal;     // Phase1 메시지 등장
    [SerializeField] public AudioClip sfxDesaturateDrop;    // 흑백화 연출

    [Header("── 신체 / 분위기 ──────────────────")]
    [SerializeField] public AudioClip sfxHeartbeatRamp;     // 심장박동 가속 (루프)
    [SerializeField] public AudioClip sfxHeartbeatSlow;     // 심장박동 느림 (루프)
    [SerializeField] public AudioClip sfxTinnitusRing;      // 이명
    [SerializeField] public AudioClip sfxBreathPanic;       // 공황 호흡
    [SerializeField] public AudioClip sfxElectricTremor;    // 전기 떨림

    [Header("── 소품 컷인 ──────────────────────")]
    [SerializeField] public AudioClip sfxPillRattle;        // 약병 흔들기
    [SerializeField] public AudioClip sfxPillBottleOpen;    // 약병 열기
    [SerializeField] public AudioClip sfxGlassClink;        // 유리잔
    [SerializeField] public AudioClip sfxDoorKnock;         // 노크
    [SerializeField] public AudioClip sfxReportPaper;       // 서류 펼치기

    [Header("── 전화 ────────────────────────────")]
    [SerializeField] public AudioClip sfxPhoneNotify;       // 알림음
    [SerializeField] public AudioClip sfxPhoneSend;         // 메시지 전송
    [SerializeField] public AudioClip sfxPhoneDial1393;     // 1393 다이얼

    [Header("── 환경음 (루프) ───────────────────")]
    [SerializeField] public AudioClip ambClub;              // 클럽 환경음
    [SerializeField] public AudioClip ambHospital;          // 병원 환경음
    [SerializeField] public AudioClip ambIsolation;         // 격리실 환경음
    [SerializeField] public AudioClip ambRoomNight;         // 밤방 환경음

    [Header("── 엔딩 ────────────────────────────")]
    [SerializeField] public AudioClip sfxPoliceSiren;       // 경찰 사이렌
    [SerializeField] public AudioClip sfxCuffsMetal;        // 수갑
    [SerializeField] public AudioClip sfxRadioStatic;       // 라디오 잡음
    [SerializeField] public AudioClip sfxTrueEndStareLoop;  // TrueEnd 응시 루프
    [SerializeField] public AudioClip sfxTrueEndTapCut;     // TrueEnd 탭 컷
    [SerializeField] public AudioClip sfxEndingChoiceRestart; // 다시 시작 선택
    [SerializeField] public AudioClip sfxEndingChoiceQuit;    // 게임 종료 선택

    [Header("── 볼륨 ────────────────────────────")]
    [Range(0f,1f)] [SerializeField] private float sfxVolume  = 1.0f;
    [Range(0f,1f)] [SerializeField] private float loopVolume = 0.45f;

    // ─────────────────────────────────────────────────────────────
    private void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
        DontDestroyOnLoad(gameObject);

        _sfxSource  = gameObject.AddComponent<AudioSource>();
        _sfxSource.playOnAwake = false;
        _sfxSource.volume      = sfxVolume;

        _loopSource = gameObject.AddComponent<AudioSource>();
        _loopSource.playOnAwake = false;
        _loopSource.loop        = true;
        _loopSource.volume      = loopVolume;
    }

    private void OnDestroy()
    {
        if (Instance == this) Instance = null;
    }

    // ══════════════════════════════════════════════════════════════
    //  퍼블릭 API
    // ══════════════════════════════════════════════════════════════

    /// <summary>일회성 SFX 재생</summary>
    public void Play(AudioClip clip, float volumeScale = 1f)
    {
        if (clip == null || _sfxSource == null) return;
        _sfxSource.PlayOneShot(clip, volumeScale);
    }

    // ── UI ───────────────────────────────────────────────────────
    public void PlayUISelect()        => Play(sfxUISelect);
    public void PlayUIDanger()        => Play(sfxUIDangerConfirm);
    public void PlayUIForced()        => Play(sfxUIForcedClick);
    public void PlayUILocked()        => Play(sfxUILocked);
    public void PlayStatReveal()      => Play(sfxUIStatReveal, 0.8f);

    // ── FX ───────────────────────────────────────────────────────
    public void PlayRedWarning()      => Play(sfxRedWarningHit);
    public void PlayGlitchBurst()     => Play(sfxGlitchBurst, 0.7f);
    public void PlayBlurPulse()       => Play(sfxBlurPulseWhoosh, 0.8f);
    public void PlayMessageReveal()   => Play(sfxMessageReveal);
    public void PlayDesaturate()      => Play(sfxDesaturateDrop);

    // ── 신체 ─────────────────────────────────────────────────────
    public void PlayTinnitus()        => Play(sfxTinnitusRing, 0.6f);
    public void PlayBreathPanic()     => Play(sfxBreathPanic);
    public void PlayElectricTremor()  => Play(sfxElectricTremor, 0.7f);

    /// <summary>심장박동 루프 시작 (ramp=true: 가속, false: 느림)</summary>
    public void StartHeartbeat(bool ramp = true)
    {
        if (_loopSource == null) return;
        var clip = ramp ? sfxHeartbeatRamp : sfxHeartbeatSlow;
        if (clip == null) return;
        if (_loopSource.clip == clip && _loopSource.isPlaying) return;
        _loopSource.clip = clip;
        _loopSource.volume = loopVolume;
        _loopSource.Play();
    }

    public void StopLoop() => _loopSource?.Stop();

    // ── 소품 ─────────────────────────────────────────────────────
    public void PlayPillRattle()      => Play(sfxPillRattle);
    public void PlayPillOpen()        => Play(sfxPillBottleOpen);
    public void PlayGlass()           => Play(sfxGlassClink);
    public void PlayDoorKnock()       => Play(sfxDoorKnock);
    public void PlayReportPaper()     => Play(sfxReportPaper);

    // ── 전화 ─────────────────────────────────────────────────────
    public void PlayPhoneNotify()     => Play(sfxPhoneNotify);
    public void PlayPhoneSend()       => Play(sfxPhoneSend);
    public void PlayDial1393()        => Play(sfxPhoneDial1393);

    // ── 환경음 루프 ───────────────────────────────────────────────
    public void PlayAmbient(AudioClip clip)
    {
        if (_loopSource == null || clip == null) return;
        if (_loopSource.clip == clip && _loopSource.isPlaying) return;
        _loopSource.clip   = clip;
        _loopSource.volume = loopVolume;
        _loopSource.Play();
    }
    public void PlayAmbClub()      => PlayAmbient(ambClub);
    public void PlayAmbHospital()  => PlayAmbient(ambHospital);
    public void PlayAmbIsolation() => PlayAmbient(ambIsolation);
    public void PlayAmbRoomNight() => PlayAmbient(ambRoomNight);

    // ── 엔딩 ─────────────────────────────────────────────────────
    public void PlayPoliceSiren()     => Play(sfxPoliceSiren);
    public void PlayCuffs()           => Play(sfxCuffsMetal);
    public void PlayRadioStatic()     => Play(sfxRadioStatic);
    public void PlayTrueEndTap()      => Play(sfxTrueEndTapCut);

    /// <summary>TrueEnd 응시 루프 시작/종료</summary>
    public void StartTrueEndStare()
    {
        if (_loopSource == null || sfxTrueEndStareLoop == null) return;
        _loopSource.clip   = sfxTrueEndStareLoop;
        _loopSource.volume = loopVolume * 0.7f;
        _loopSource.Play();
    }

    public void PlayEndingRestart()   => Play(sfxEndingChoiceRestart);
    public void PlayEndingQuit()      => Play(sfxEndingChoiceQuit);

    // ── 페이드 아웃 루프 ─────────────────────────────────────────
    public void FadeOutLoop(float duration = 0.8f)
    {
        if (_loopSource != null)
            StartCoroutine(FadeLoopRoutine(duration));
    }

    private IEnumerator FadeLoopRoutine(float duration)
    {
        if (_loopSource == null) yield break;
        float from = _loopSource.volume;
        float t    = 0f;
        while (t < duration)
        {
            t += Time.deltaTime;
            if (_loopSource == null) yield break;
            _loopSource.volume = Mathf.Lerp(from, 0f, t / duration);
            yield return null;
        }
        if (_loopSource != null) { _loopSource.Stop(); _loopSource.volume = loopVolume; }
    }
}
