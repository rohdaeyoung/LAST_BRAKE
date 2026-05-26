using UnityEngine;
using System.Collections.Generic;

// 각 씬의 캐릭터 스프라이트를 감정 상태에 따라 전환
public class CharacterAnimator : MonoBehaviour
{
    public static CharacterAnimator Instance { get; private set; }

    [SerializeField] private CharacterSlot[] slots;

    private void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
    }

    // 특정 화자만 표시, 나머지 숨김
    public void ShowOnlySpeaker(string speakerName)
    {
        if (slots == null) return;
        foreach (var slot in slots)
        {
            if (slot == null) continue;
            bool isSpeaker = slot.characterName == speakerName;
            slot.SetActive(isSpeaker);
        }
    }

    // 모든 캐릭터 숨김 (독백/나레이션)
    public void HideAll()
    {
        if (slots == null) return;
        foreach (var slot in slots)
            slot?.SetActive(false);
    }

    // 모든 캐릭터 표시
    public void ShowAll()
    {
        if (slots == null) return;
        foreach (var slot in slots)
            slot?.SetActive(true);
    }

    public void SetEmotion(string speakerName, CharacterEmotion emotion)
    {
        if (slots == null) return;
        foreach (var slot in slots)
        {
            if (slot == null) continue;
            if (slot.characterName == speakerName)
            {
                slot.ApplyEmotion(emotion);
                return;
            }
        }
    }

    public void ShowCharacter(string name, bool show)
    {
        if (slots == null) return;
        foreach (var slot in slots)
        {
            if (slot == null) continue;
            if (slot.characterName == name)
                slot.SetActive(show);
        }
    }
}

[System.Serializable]
public class CharacterSlot
{
    public string          characterName;
    public SpriteRenderer  spriteRenderer;
    public EmotionSprite[] emotionSprites;

    public GameObject gameObject =>
        spriteRenderer != null ? spriteRenderer.gameObject : null;

    public void SetActive(bool show)
    {
        var go = gameObject;
        if (go != null) go.SetActive(show);
    }

    public void ApplyEmotion(CharacterEmotion emotion)
    {
        if (spriteRenderer == null) return;
        if (emotionSprites == null || emotionSprites.Length == 0) return;

        foreach (var es in emotionSprites)
        {
            if (es.emotion == emotion)
            {
                spriteRenderer.sprite = es.sprite;
                return;
            }
        }
        // 매칭 없으면 첫 번째(Normal) fallback
        spriteRenderer.sprite = emotionSprites[0].sprite;
    }
}

[System.Serializable]
public struct EmotionSprite
{
    public CharacterEmotion emotion;
    public Sprite           sprite;
}
