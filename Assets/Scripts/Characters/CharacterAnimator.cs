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

    public void SetEmotion(string speakerName, CharacterEmotion emotion)
    {
        foreach (var slot in slots)
        {
            if (slot.characterName == speakerName)
            {
                slot.ApplyEmotion(emotion);
                return;
            }
        }
    }

    public void ShowCharacter(string name, bool show)
    {
        foreach (var slot in slots)
            if (slot.characterName == name) slot.gameObject.SetActive(show);
    }
}

[System.Serializable]
public class CharacterSlot
{
    public string        characterName;
    public SpriteRenderer spriteRenderer;
    public EmotionSprite[] emotionSprites;
    public GameObject    gameObject => spriteRenderer.gameObject;

    public void ApplyEmotion(CharacterEmotion emotion)
    {
        foreach (var es in emotionSprites)
        {
            if (es.emotion == emotion)
            {
                spriteRenderer.sprite = es.sprite;
                return;
            }
        }
    }
}

[System.Serializable]
public struct EmotionSprite
{
    public CharacterEmotion emotion;
    public Sprite           sprite;
}
