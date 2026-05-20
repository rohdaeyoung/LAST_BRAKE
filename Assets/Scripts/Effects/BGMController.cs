using UnityEngine;
using UnityEngine.Audio;
using System.Collections;

public class BGMController : MonoBehaviour
{
    public static BGMController Instance { get; private set; }

    [SerializeField] private AudioSource bgmSource;
    [SerializeField] private AudioMixer  audioMixer;       // "BGM_Pitch", "BGM_Distortion" 파라미터
    [SerializeField] private AudioClip   clipNormal;
    [SerializeField] private AudioClip   clipClub;
    [SerializeField] private AudioClip   clipDistorted;
    [SerializeField] private float       fadeDuration = 1.5f;

    private Coroutine fadeCoroutine;

    private void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
        DontDestroyOnLoad(gameObject);

        // AudioSource 없으면 자동 추가
        if (bgmSource == null) bgmSource = GetComponent<AudioSource>();
        if (bgmSource == null) bgmSource = gameObject.AddComponent<AudioSource>();
        bgmSource.loop = true;
    }

    private void OnDestroy()
    {
        if (Instance == this) Instance = null;
    }

    public void PlayNormal()   => CrossfadeTo(clipNormal,    1f);
    public void PlayClub()     => CrossfadeTo(clipClub,      1f);

    // RISK >= 60 시 BGM 변조
    public void SetDistorted(bool on)
    {
        if (audioMixer == null) return;
        if (on)
        {
            audioMixer.SetFloat("BGM_Pitch", 0.85f);
            audioMixer.SetFloat("BGM_Distortion", 0.6f);
        }
        else
        {
            audioMixer.SetFloat("BGM_Pitch", 1f);
            audioMixer.SetFloat("BGM_Distortion", 0f);
        }
    }

    public void FadeOut(float duration)
    {
        if (bgmSource == null) return;
        StartCoroutine(FadeVolumeRoutine(bgmSource.volume, 0f, duration));
    }

    private void CrossfadeTo(AudioClip clip, float targetVolume)
    {
        if (clip == null) return;
        if (bgmSource == null) return;
        if (fadeCoroutine != null) StopCoroutine(fadeCoroutine);
        fadeCoroutine = StartCoroutine(CrossfadeRoutine(clip, targetVolume));
    }

    private IEnumerator CrossfadeRoutine(AudioClip clip, float targetVolume)
    {
        if (bgmSource == null) yield break;
        float half = fadeDuration * 0.5f;
        yield return FadeVolumeRoutine(bgmSource.volume, 0f, half);
        if (bgmSource == null) yield break;
        bgmSource.clip = clip;
        bgmSource.Play();
        yield return FadeVolumeRoutine(0f, targetVolume, half);
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
