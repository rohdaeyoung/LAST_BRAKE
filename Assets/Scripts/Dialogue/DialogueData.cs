using UnityEngine;

[CreateAssetMenu(fileName = "NewDialogue", menuName = "LAST BRAKE/Dialogue Data")]
public class DialogueData : ScriptableObject
{
    public DialogueLine[] lines;
    public ChoiceData[]   choices;
}

[System.Serializable]
public struct DialogueLine
{
    public string     speaker;
    [TextArea(2, 6)]
    public string     text;
    public Sprite     portrait;
    public CharacterEmotion emotion;
    public bool       isMonologue; // 독백이면 이탤릭 + 다른 색상
}

[System.Serializable]
public struct ChoiceData
{
    [TextArea(1, 3)]
    public string label;
    public int    intDelta;
    public int    riskDelta;
    public int    addictDelta;
    public string nextScene;      // 비어 있으면 같은 씬의 다음 DialogueData로
    public bool   isForcedBad;    // ADDICT >= 60일 때 이 선택지가 강제 선택됨
    public bool   requiresMinINT; // true면 INT >= minINTValue 필요
    public int    minINTValue;
}

public enum CharacterEmotion
{
    Neutral,
    Happy,
    Worried,
    Drunk,
    Shocked,
    Pain,
    Smug,
    Determined
}
