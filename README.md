# LAST BRAKE: 끝나지 않는 밤

> 약물 중독을 주제로 한 선택형 비주얼 노벨 게임  
> Unity 6 (6000.0.73f1) · URP · TextMeshPro · C#

---

## 📖 프로젝트 개요

**LAST BRAKE**는 클럽 문화와 약물 중독을 소재로, 플레이어의 선택에 따라 주인공 도윤의 운명이 달라지는 비주얼 노벨입니다.  
마감 기한: **2026년 6월 5일**

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
| `10_TrueEnd` | 트루 엔딩 (세 엔딩 모두 클리어 후 새 게임 시 해금) |

---

## 📊 수치 시스템

게임 내 3가지 수치가 플레이어의 선택에 따라 변화하며 엔딩을 결정합니다.

| 수치 | 초기값 | 의미 |
|------|--------|------|
| **INT (판단력)** | 100 | 높을수록 이성적 판단 가능 |
| **RISK (위험도)** | 0 | 높을수록 위험한 상황에 노출 |
| **ADDICT (의존도)** | 0 | 높을수록 약물에 의존 |

### 엔딩 조건

| 엔딩 | 조건 |
|------|------|
| **굿 엔딩** | INT ≥ 75 AND ADDICT ≤ 40 |
| **배드 엔딩** | ADDICT ≥ 80 |
| **노멀 엔딩** | INT ≤ 50 AND RISK ≥ 60 (또는 기타) |
| **트루 엔딩** | 세 엔딩 모두 클리어 후 새 게임 시작 |

---

## 🎮 선택지 수치 변화표

### Step 1 — 프롤로그
| 선택지 | INT | RISK | ADDICT |
|--------|-----|------|--------|
| 집에 갈게요 (이성적 선택) | +10 | -5 | 0 |
| 가볼까? (클럽 동행) | -10 | +15 | +5 |

### Step 2 — 클럽
| 선택지 | INT | RISK | ADDICT |
|--------|-----|------|--------|
| 거절한다 | +15 | -5 | 0 |
| 한 번만 해본다 | 0 | 0 | +5 |
| 적극적으로 즐긴다 | -20 | +10 | +20 |

### Step 3 — 다음날 아침
| 선택지 | INT | RISK | ADDICT |
|--------|-----|------|--------|
| 다시 연락한다 | -5 | +5 | +10 |
| 더 구한다 | -10 | +10 | +20 |
| 끊겠다고 결심 | +15 | 0 | -5 |

### Step 4 — 파티
| 선택지 | INT | RISK | ADDICT |
|--------|-----|------|--------|
| 함께한다 | -10 | +10 | +10 |
| 더 강한 걸 요구 | -20 | +15 | +25 |
| 민재 말을 따른다 | -15 | +10 | +30 |

### Step 5 — 무너짐 (엔딩 결정)
| 선택지 | 조건 | INT | RISK | ADDICT |
|--------|------|-----|------|--------|
| 경찰에 신고한다 | INT ≥ 70 필요 | +25 | -20 | -20 |
| 다 끝났어... (포기) | 없음 | -15 | +10 | +5 |
| 형 시키는 거 다 할게 | 없음 (강제 배드) | -25 | +20 | +30 |

---

## 🏗️ 핵심 스크립트 구조

```
Assets/Scripts/
├── Core/
│   ├── GameManager.cs         # 싱글턴 — 씬 전환, 엔딩 분기, 클리어 기록
│   └── StatManager.cs         # 싱글턴 — INT/RISK/ADDICT 관리, 엔딩 계산
├── Dialogue/
│   ├── DialogueManager.cs     # 대화 시퀀스 재생, 선택지 분기 처리
│   ├── DialogueBootstrap.cs   # DialogueUI 런타임 생성 및 초기화
│   └── DialogueData.cs        # ScriptableObject — 대화 라인 + 선택지 데이터
├── Characters/
│   └── FourthWallBreak.cs     # 4번째 벽 깨기 — 2단계 엔딩 시퀀스
├── Ending/
│   ├── GoodEndSequence.cs     # 굿 엔딩 씬 컨트롤러
│   ├── NormalEndSequence.cs   # 노멀 엔딩 씬 컨트롤러
│   ├── BadEndSequence.cs      # 배드 엔딩 씬 컨트롤러
│   └── TrueEndSequence.cs     # 트루 엔딩 씬 컨트롤러
├── UI/
│   ├── DialogueUI.cs          # 대화창 UI 제어
│   ├── ChoiceUI.cs            # 선택지 버튼 UI
│   ├── MainMenuUI.cs          # 메인메뉴 버튼 처리
│   └── StatReportUI.cs        # 수치 바 애니메이션 (판단력/위험도/의존도)
└── Effects/
    ├── BGMController.cs        # BGM 페이드인/아웃, 크로스페이드
    ├── PostProcessingController.cs  # 흑백화, 탈색 효과
    └── ScreenEffects.cs        # 화면 페이드인/아웃, 노이즈 효과
```

---

## ✨ FourthWallBreak 시스템

모든 엔딩의 마지막에 공통으로 실행되는 **2단계 메타 시퀀스**입니다.

### Phase 1 — 메시지 화면
1. BGM 페이드아웃 (2초)
2. 도윤 응시 애니메이션
3. 화면 흑백화 (2.5초)
4. 핵심 메시지 페이드인: *"중독은 특별한 사람이 아니라, 반복된 선택에서 시작됩니다."*
5. 암전 전환 (1초)

### Phase 2 — 결말 화면 (런타임 생성)
- **왼쪽**: 도윤 캐릭터 이미지 패널 (Inspector에서 Sprite 할당 가능)
- **오른쪽 상단**: 게임 결과 — 판단력 / 위험도 / 의존도 수치 바 애니메이션
- **하단 대화창**: 도윤이 말을 건넴 *"이래도 계속 하겠습니까?"*
- **선택지 3개**:
  - ① 다시 시작한다 → Step1으로 재시작
  - ② 누군가에게 연락한다 → 정신건강위기상담전화 1393 연결
  - ③ 게임 종료

---

## 🐛 주요 수정 이력

### 1. 엔딩 씬 내용 미표시 문제
- **원인**: `DialogueBootstrap`만 생성하고 `DialogueManager`는 생성하지 않아, 대화 완료 콜백이 호출되지 않음
- **수정**: `EnsureDialogueSystem()`에서 두 컴포넌트 모두 런타임 생성

### 2. RectTransform 중복 추가 오류
- **원인**: `Image` 컴포넌트 추가 시 `RectTransform`이 자동 생성되는데, 그 위에 다시 `AddComponent<RectTransform>()` 호출
- **수정**: `GetOrAddRT()` 헬퍼 메서드로 안전하게 처리

### 3. FourthWallBreak UI 참조 실패
- **원인**: 씬 계층구조에서 FourthWallBreak 오브젝트에 자식이 없음 (YAML 확인) — `GetComponentsInChildren` 반환값 없음
- **수정**: `FindObjectsByType<Transform>(FindObjectsInactive.Include)` 씬 전체 탐색으로 교체

### 4. 메인메뉴에서 TrueEnd로 바로 점프
- **원인**: `MainMenuUI.OnStart()`에서 `IsTrueEndUnlocked()` true 시 TrueEnd로 직행하는 코드 존재
- **수정**: 해당 분기 제거 — TrueEnd 분기는 `GoToEnding()`에서만 처리

### 5. NormalEndSequence UnassignedReferenceException
- **원인**: `darkRoomOverlay` SerializeField가 Inspector 미할당 상태에서 Unity fake-null로 인해 null 체크를 통과하고 예외 발생
- **수정**: `darkRoomOverlay` 필드 제거, `ScreenEffects.FadeOut()`으로 대체

### 6. Step5 선택지 수치 모두 0
- **원인**: `Step5_Collapse.asset`의 세 선택지 intDelta/riskDelta/addictDelta가 모두 0으로 설정되어 어떤 선택을 해도 같은 엔딩
- **수정**: 경찰 신고(+25/-20/-20), 포기(-15/+10/+5), 복종(-25/+20/+30)으로 설정

### 7. BGMController / PostProcessingController DontDestroyOnLoad 이후 fake-null
- **원인**: 씬 전환 시 기존 인스턴스가 파괴되면서 Instance가 dangling reference 상태
- **수정**: `OnDestroy()`에서 `if (Instance == this) Instance = null` 추가

---

## 🔧 개발 환경

- **엔진**: Unity 6 (6000.0.73f1)
- **렌더 파이프라인**: URP (Universal Render Pipeline)
- **UI**: TextMeshPro (NotoSansKR 한글 폰트)
- **언어**: C# 9.0
- **플랫폼 대상**: Windows / Mac / Linux

---

## 📁 주요 에셋 경로

```
Assets/
├── Resources/
│   └── Dialogues/            # 대화 ScriptableObject 에셋 (11개)
├── Scripts/                  # 모든 C# 스크립트
├── Sprites/
│   ├── Backgrounds/          # 배경 스프라이트 (씬별)
│   └── Characters/           # 캐릭터 스프라이트 (4인 × 8감정)
├── Audio/                    # BGM / SFX
└── TextMesh Pro/
    └── Fonts/                # NotoSansKR 한글 폰트 에셋
```

---

## 🚀 실행 방법

1. Unity 6 (6000.0.73f1) 이상에서 프로젝트 열기
2. `Assets/Scenes/00_MainMenu` 씬 열기
3. Play 버튼 또는 **File > Build Settings > Build** 로 빌드

---

## 📞 위기상담 안내

이 게임은 약물 중독의 심각성을 알리기 위해 제작되었습니다.  
도움이 필요하다면 언제든지 연락하세요.

> **정신건강위기상담전화: ☎ 1393** (24시간 무료)

---

*Made with Unity 6 · © 2026 rohdaeyoung*
