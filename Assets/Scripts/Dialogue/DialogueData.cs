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
    public string  speaker;
    [TextArea(2, 6)]
    public string  text;
    public Sprite  portrait;
    public CharacterEmotion emotion;
    public bool    isMonologue; // 독백이면 이탤릭 + 다른 색상

    [Header("효과 트리거 (선택)")]
    public FXType     fxEffect;   // 이 라인 시작 시 발동할 FX 오버레이
    public CGType     cgScene;    // 이 라인 시작 시 표시할 CG 이미지
    public ObjectType objCutIn;   // 이 라인 시작 시 표시할 소품 컷인
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

// ── FX 오버레이 종류 ─────────────────────────────────────────────
public enum FXType
{
    None,
    RedWarning,   // FX_RedWarningOverlay — 위험한 선택 시 붉은 경고
    Glitch,       // FX_GlitchOverlay    — 환각/붕괴 글리치
    Vignette,     // FX_DarkVignette     — 어두운 비네트 강조
    BlurPulse,    // FX_BlurPulse        — 초점 흔들림
    GlitchOff,    // 글리치 해제
}

// ── CG 이벤트 종류 ───────────────────────────────────────────────
public enum CGType
{
    None,
    ClubOffer,              // CG_Club_Offer          — 처음 권유받는 장면
    SeoyeonConfrontation,   // CG_Seoyeon_Confrontation — 서연 충돌
    Collapse,               // CG_Collapse             — 심리 붕괴
    HospitalRecovery,       // CG_Hospital_Recovery    — 회복 엔딩
    BadEndArrest,           // CG_BadEnd_Arrest        — BAD END 체포
}

// ── 오브젝트 컷인 종류 ───────────────────────────────────────────
public enum ObjectType
{
    None,
    Smartphone,   // OBJ_Smartphone   — 연락/알림
    Clock0730,    // OBJ_Clock0730    — 07:30 시계
    PillBottle,   // OBJ_PillBottle   — 약통
    ChatWindow,   // OBJ_ChatWindow   — 채팅창
    ReportPaper,  // OBJ_ReportPaper  — 최종 리포트
}
