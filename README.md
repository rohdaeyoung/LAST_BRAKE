# LAST BRAKE: 끝나지 않는 밤

> 약물 중독을 주제로 한 선택형 비주얼 노벨 게임  
> Unity 6 (6000.0.73f1) · URP · TextMeshPro · C#  
> 마감: **2026년 6월 5일**

---

## 📖 프로젝트 개요

**LAST BRAKE**는 클럽 문화와 약물 중독을 소재로, 플레이어의 선택에 따라 주인공 도윤의 운명이 달라지는 비주얼 노벨입니다.  
게임 내 3가지 수치(판단력·위험도·의존도)가 선택에 따라 변화하며 4가지 엔딩으로 분기됩니다.

### 핵심 메시지
> "중독은 특별한 사람이 아니라, 반복된 선택에서 시작됩니다."

---

## 🎭 등장인물

| 이름 | 역할 |
|------|------|
| **도윤** | 주인공. 클럽 문화에 발을 들이게 되는 대학생 |
| **민재** | 도윤을 약물 세계로 끌어들이는 선배 |
| **하준** | 도윤의 친구. 위험을 경고하는 존재 |
| **서아** | 도윤을 걱정하며 함께하려는 인물 |

---

## 🗺️ 씬 구성 (총 11개)

| 씬 파일 | 내용 |
|---------|------|
| `00_MainMenu` | 타이틀 화면 |
| `01_Step1_Prologue` | 프롤로그 — 첫 번째 선택 (클럽 가기 / 집으로) |
| `02_Step2_Club` | 클럽 — 약물 첫 접촉 선택 |
| `03_Step3_Morning` | 다음날 아침 — 현실 직시 또는 회피 |
| `04_Step4_Party` | 파티 — 깊어지는 중독의 갈림길 |
| `05_Step5_Collapse` | 무너짐 — 최후의 선택 (엔딩 분기 결정) |
| `06_EndingReport` | (미사용 — 엔딩 직행 방식으로 변경) |
| `07_GoodEnd` | 굿 엔딩 「새벽의 선택」 |
| `08_NormalEnd` | 노멀 엔딩 「연락처 삭제」 |
| `09_BadEnd` | 배드 엔딩 「끝나지 않는 밤」 |
| `10_TrueEnd` | 트루 엔딩 — 세 엔딩 모두 클리어 후 해금 |

---

## 📊 수치 시스템

게임 내 3가지 수치가 플레이어의 선택에 따라 변화하며 엔딩을 결정합니다.

| 수치 | 초기값 | 의미 |
|------|--------|------|
| **INT (판단력)** | 100 | 높을수록 이성적 판단 가능 |
| **RISK (위험도)** | 0 | 높을수록 위험한 상황에 노출 |
| **ADDICT (의존도)** | 0 | 높을수록 약물에 의존 |

### 엔딩 분기 조건

| 엔딩 | 조건 |
|------|------|
| **굿 엔딩** `07_GoodEnd` | INT ≥ 75 AND ADDICT ≤ 40 |
| **배드 엔딩** `09_BadEnd` | ADDICT ≥ 80 |
| **노멀 엔딩** `08_NormalEnd` | INT ≤ 50 AND RISK ≥ 60 (또는 기타) |
| **트루 엔딩** `10_TrueEnd` | 세 엔딩 모두 클리어 후 새 게임 시작 |

---

## 🎮 선택지 수치 변화표

### Step 1 — 프롤로그
| 선택지 | INT | RISK | ADDICT |
|--------|-----|------|--------|
| 집에 갈게요 (이성적 선택) | +10 | −5 | 0 |
| 가볼까? (클럽 동행) | −10 | +15 | +5 |

### Step 2 — 클럽
| 선택지 | INT | RISK | ADDICT |
|--------|-----|------|--------|
| 거절한다 | +15 | −5 | 0 |
| 한 번만 해본다 | 0 | 0 | +5 |
| 적극적으로 즐긴다 | −20 | +10 | +20 |

### Step 3 — 다음날 아침
| 선택지 | INT | RISK | ADDICT |
|--------|-----|------|--------|
| 다시 연락한다 | −5 | +5 | +10 |
| 더 구한다 | −10 | +10 | +20 |
| 끊겠다고 결심 | +15 | 0 | −5 |

### Step 4 — 파티
| 선택지 | INT | RISK | ADDICT |
|--------|-----|------|--------|
| 함께한다 | −10 | +10 | +10 |
| 더 강한 걸 요구 | −20 | +15 | +25 |
| 민재 말을 따른다 | −15 | +10 | +30 |

### Step 5 — 무너짐 (엔딩 결정)
| 선택지 | 조건 | INT | RISK | ADDICT |
|--------|------|-----|------|--------|
| 경찰에 신고한다 | INT ≥ 70 필요 | +25 | −20 | −20 |
| 다 끝났어... (포기) | 없음 | −15 | +10 | +5 |
| 형 시키는 거 다 할게 | 없음 (강제 배드) | −25 | +20 | +30 |

---

## 🏗️ 전체 스크립트 구조

```
Assets/Scripts/
├── Core/
│   ├── GameManager.cs              # 싱글턴 — 씬 전환, 엔딩 분기, 클리어 기록
│   └── StatManager.cs              # 싱글턴 — INT/RISK/ADDICT 관리, 엔딩 계산
│                                     선택 후 FXManager 자동 연동
├── Dialogue/
│   ├── DialogueManager.cs          # 대화 시퀀스 재생, 선택지 분기, FX/CG/OBJ 트리거
│   ├── DialogueBootstrap.cs        # DialogueUI 런타임 생성 및 초기화
│   └── DialogueData.cs             # ScriptableObject — 대화 라인 + 선택지 + 효과 데이터
│                                     (FXType / CGType / ObjectType 열거형 포함)
├── Characters/
│   └── FourthWallBreak.cs          # 3단계 엔딩 메타 시퀀스
│                                     Phase1(메시지) / PhaseFACE(얼굴 응시, TrueEnd) / Phase2(결과 화면)
├── UI/
│   ├── DialogueUI.cs               # 대화창 UI 제어
│   ├── ChoiceSystem.cs             # 선택지 버튼 UI
│   ├── MainMenuUI.cs               # 메인메뉴 버튼 처리
│   ├── StatHUD.cs                  # 인게임 스탯 HUD (상단 바 — anchorMax.x 방식)
│   └── StatReportUI.cs             # 엔딩 결과 화면 수치 바 애니메이션
├── Effects/
│   ├── FXManager.cs                # FX 오버레이 (RedWarning / Glitch / Vignette / BlurPulse)
│   │                                 약한 배경 정적 + 강한 버스트 모드 분리
│   ├── CGViewer.cs                 # CG 전체화면 이벤트 이미지 (탭하여 닫기)
│   ├── ObjectCutIn.cs              # 소품 컷인 (우측 하단 슬라이드인/아웃)
│   ├── BGMController.cs            # BGM 페이드인/아웃, 크로스페이드
│   ├── PostProcessingController.cs # 흑백화, 탈색 효과
│   └── ScreenEffects.cs            # 화면 페이드인/아웃, DontDestroyOnLoad
└── Editor/
    └── EffectLinker.cs             # 메뉴 20: FX/CG/OBJ/StatHUD 스프라이트 전체 자동 연결
```

---

## ✨ 효과 시스템 (Effects Pipeline)

대화 라인의 `DialogueLine` 구조체에 효과 필드를 직접 지정하면, `DialogueManager`가 라인 출력 시 자동으로 트리거합니다.

### FXType — 화면 오버레이 효과

| 값 | 효과 | 용도 |
|----|------|------|
| `RedWarning` | 붉은 화면 플래시 (0.15초 인/0.45초 아웃) | 위험한 선택 직후 |
| `Glitch` | 약한 배경 정적 (alpha 0.02–0.06, 상시) | 의존도 60+ 이상 분위기 |
| `GlitchOff` | 글리치 종료 | 글리치 구간 끝 |
| `Vignette` | 비네트 강화 (intensity 0.8) | 긴장감 고조 |
| `BlurPulse` | 블러 펄스 3회 | 정신 혼란 묘사 |

> **PlayGlitchBurst(duration)**: 강한 글리치를 duration초 동안만 번쩍이고 자동 종료.  
> 대화 텍스트를 가리지 않도록 선택 결과 등 극적 순간에만 사용.

### CGType — 전체화면 CG 이미지

| 값 | 파일명 | 장면 |
|----|--------|------|
| `ClubOffer` | `CG_Club_Offer.png` | 클럽에서 약물 제안 |
| `SeoyeonConfrontation` | `CG_Seoyeon_Confrontation.png` | 서아와의 대면 |
| `Collapse` | `CG_Collapse.png` | 도윤 붕괴 장면 |
| `HospitalRecovery` | `CG_Hospital_Recovery.png` | 병원 회복 (굿엔딩) |
| `BadEndArrest` | `CG_BadEnd_Arrest.png` | 체포 장면 (배드엔딩) |

### ObjectType — 소품 컷인

| 값 | 파일명 | 장면 |
|----|--------|------|
| `Smartphone` | `OBJ_Smartphone.png` | 문자/연락 장면 |
| `Clock0730` | `OBJ_Clock0730.png` | 다음날 아침 |
| `PillBottle` | `OBJ_PillBottle.png` | 약물 등장 |
| `ChatWindow` | `OBJ_ChatWindow.png` | 채팅 화면 |
| `ReportPaper` | `OBJ_ReportPaper.png` | 경찰 신고서 |

---

## 📊 StatHUD — 인게임 스탯 바

화면 최상단에 항상 표시되는 3개의 수치 바입니다.

- **이성** (초록) — INT 수치, 낮을수록 위험 색
- **위험도** (주황) — RISK 수치, 60 초과 시 빨간색
- **의존도** (보라) — ADDICT 수치, 60 초과 시 빨간색

선택 후 `AnimateChange()`로 0.6초 동안 부드럽게 수치가 변합니다.  
구현 방식: Unity Slider 대신 `fillRT.anchorMax.x` 직접 제어 → 레이아웃 충돌 없음.

---

## 🎬 FourthWallBreak — 엔딩 메타 시퀀스

모든 엔딩 씬에 부착되어 자동 실행되는 **3단계 4번째 벽 깨기** 시스템입니다.

### Phase 1 — 메시지 화면 (전 엔딩 공통)
1. BGM 페이드아웃
2. 도윤 응시 애니메이션 (stareFrames 배열 순서대로 재생)
3. 화면 흑백화
4. 핵심 메시지 페이드인
5. 암전 전환

### Phase FACE — 얼굴 응시 (TrueEnd 전용)
- 도윤의 얼굴이 화면 가득 채움
- 글리치 + 비네트 0.9 강화
- 표정 전환 시퀀스: Surprised → Anxious → Regret (점점 빨라짐)
- 눈 깜빡임 애니메이션 (랜덤 1.5~3.5초 간격)
- 미세 좌우 흔들림
- **탭/클릭 시 Phase 2로 전환**

### Phase 2 — 결말 화면 (전 엔딩 공통)
런타임에 Canvas를 직접 생성하며, 씬 배경을 완전히 가림.

| 영역 | 내용 |
|------|------|
| **좌측 패널** | 엔딩별 도윤 얼굴 스프라이트 + 하단 이름 배지 |
| **우측 상단 카드** | "— 게임 결과 —" + 판단력/위험도/의존도 수치 바 애니메이션 |
| **하단 대화창** | "이래도 계속 하겠습니까?" |
| **선택지 3개** | ① 다시 시작 ② 1393 전화 연결 ③ 게임 종료 |

**엔딩별 얼굴 표정:**
| 엔딩 | 스프라이트 | 의미 |
|------|-----------|------|
| GoodEnd | `Doyun_Face_Normal` | 회복, 차분 |
| NormalEnd | `Doyun_Face_Anxious` | 불안, 혼란 |
| BadEnd | `Doyun_Face_Regret` | 후회, 고통 |
| TrueEnd | `Doyun_Face_Surprised` | 응시, 4th wall |

**Phase 2 배경 효과:**  
글리치 완전 OFF → **호흡하는 비네트** (5초 주기, intensity 0.25↔0.50)  
중독의 무게감을 조용히 표현하며 대화 텍스트 가독성 유지.

---

## 🖼️ 에셋 경로

```
Assets/
├── Resources/
│   └── Dialogues/                      # 대화 ScriptableObject 에셋 (11개)
├── Scripts/                            # 전체 C# 스크립트
├── Sprites/
│   ├── Backgrounds/                    # 씬별 배경 (BG_*.png)
│   ├── Characters/                     # 캐릭터 (4인 × 8감정 = 32개)
│   └── 경진대회 이미지/
│       ├── Overlay_image/              # FX 오버레이 4종
│       │   ├── FX_RedWarningOverlay.png
│       │   ├── FX_GlitchOverlay.png
│       │   ├── FX_DarkVignette.png
│       │   └── FX_BlurPulse.png
│       ├── CG_image/                   # 이벤트 CG 5종
│       ├── Object_image/               # 소품 컷인 5종
│       ├── UI_image/                   # 스탯 아이콘 5종
│       └── Character_image/            # 도윤 얼굴 크롭 4종
│           ├── Doyun_Face_Normal.png
│           ├── Doyun_Face_Anxious.png
│           ├── Doyun_Face_Surprised.png
│           └── Doyun_Face_Regret.png
├── Audio/                              # BGM / SFX
└── TextMesh Pro/
    └── Fonts/                          # NotoSansKR 한글 폰트
```

---

## 🛠️ Editor 도구

Unity 상단 메뉴 **LAST BRAKE** 에서 실행:

| 메뉴 번호 | 이름 | 기능 |
|-----------|------|------|
| `20` | 효과(FX·CG·OBJ·HUD) 전체 연결 | 전체 11개 씬을 순회하며 FXManager·CGViewer·ObjectCutIn·StatHUD·FourthWallBreak 컴포넌트에 스프라이트 자동 연결. 누락 스프라이트 목록 보고. |

---

## 🐛 버그 수정 이력

### 엔딩 시퀀스 관련

| # | 증상 | 원인 | 수정 |
|---|------|------|------|
| 1 | 엔딩 씬 대화 미표시 | DialogueBootstrap만 생성, DialogueManager 누락 | `EnsureDialogueSystem()`에서 두 컴포넌트 모두 생성 |
| 2 | Phase 2 대화창 안 나옴 | `dlgCG.alpha = 0f`로 시작, ShowFinalQuestion 도달 전 코루틴 중단 | `alpha = 1f`로 시작, StatReportUI null 체크 추가 |
| 3 | Phase FACE → Phase 2 전환 후 검은 화면 | FadeOut(0.5f) 호출 후 FadeIn이 늦게 시작돼 검은 화면 유지 | FadeOut 제거, 0.2초 대기 후 즉시 Phase 2 전환 |
| 4 | 엔딩 화면 뒤로 씬 배경 비침 | `FindSceneBackgroundSprite()`가 씬 배경을 60% 투명도로 깔음 | 해당 함수 제거, 단색 불투명 배경(`#0D0D24`) 사용 |

### 수치·UI 관련

| # | 증상 | 원인 | 수정 |
|---|------|------|------|
| 5 | StatHUD 수치 바 레이아웃 깨짐 | Slider + HorizontalLayoutGroup + LayoutElement 충돌 | `fillRT.anchorMax.x` 직접 제어 방식으로 전면 재작성 |
| 6 | Phase 2 스탯 행이 카드 아래로 넘침 | `anchorMin/Max.y = 0.5` 중앙 앵커 기준으로 3개 행 계산 오류 | 상단 앵커(`y=1`) 기준으로 교체, 카드 높이 235→260px |
| 7 | StatReportUI NullReferenceException | `Awake()`에서 null 레이블에 `.text` 직접 접근 | `if (label != null)` 체크 추가 |
| 8 | Step5 어느 선택을 해도 같은 엔딩 | 선택지 delta 값 모두 0 | 경찰(+25/−20/−20), 포기(−15/+10/+5), 복종(−25/+20/+30) 설정 |

### 효과 시스템 관련

| # | 증상 | 원인 | 수정 |
|---|------|------|------|
| 9 | 글리치가 대화 텍스트를 가려 읽기 불가 | `ADDICT ≥ 60` 시 `SetGlitchActive(true)` 무한 지속, alpha 최대 0.45 | 상시 모드 alpha 0.02–0.06으로 약화, `PlayGlitchBurst()`로 강한 버스트 분리 |
| 10 | CG 표시 중 탭하면 대화 스킵됨 | CG 콜백 대기 중 `isTyping = false` 상태 | CG 표시 중 `isTyping = true` 유지, 콜백에서 `false`로 복원 |
| 11 | FXManager 글리치 비대칭 이동 | `rt.offsetMin.x ≠ rt.offsetMax.x` | `rt.offsetMin = rt.offsetMax = new Vector2(x, 0f)` |

### 기타

| # | 증상 | 원인 | 수정 |
|---|------|------|------|
| 12 | 씬 전환 후 BGM/PP 인스턴스 dangling | `DontDestroyOnLoad` 후 씬 재진입 시 fake-null | `OnDestroy()`에서 `if (Instance == this) Instance = null` |
| 13 | RectTransform 중복 추가 오류 | `AddComponent<RectTransform>()` 중복 호출 | `EnsureRT()` 헬퍼로 안전 처리 |
| 14 | 메인메뉴에서 TrueEnd 직행 | `IsTrueEndUnlocked()` true 시 TrueEnd 직행 분기 | 해당 분기 제거, TrueEnd는 `GoToEnding()`에서만 처리 |

---

## 🔧 개발 환경

| 항목 | 버전 / 내용 |
|------|------------|
| **엔진** | Unity 6 (6000.0.73f1) |
| **렌더 파이프라인** | URP (Universal Render Pipeline) |
| **UI 텍스트** | TextMeshPro + NotoSansKR 한글 폰트 |
| **언어** | C# 9.0 |
| **플랫폼 대상** | Windows / Mac / Linux |

---

## 🚀 실행 방법

1. Unity 6 (6000.0.73f1) 이상에서 프로젝트 열기
2. `Assets/Scenes/00_MainMenu` 씬 열기
3. 메뉴에서 **LAST BRAKE > 20. 효과 전체 연결** 실행 (스프라이트 최초 연결 시 1회)
4. Play 버튼 또는 **File > Build Settings > Build** 로 빌드

---

## 📞 위기상담 안내

이 게임은 약물 중독의 심각성을 알리기 위해 제작되었습니다.  
게임 내 선택지 ② "누군가에게 연락한다"는 실제 위기상담 전화로 연결됩니다.

> **정신건강위기상담전화: ☎ 1393** (24시간 무료)

---

*Made with Unity 6 · © 2026 rohdaeyoung*
