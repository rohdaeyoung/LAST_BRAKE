using UnityEngine;
using UnityEditor;
using System.IO;

/// <summary>
/// 배경/캐릭터 플레이스홀더 스프라이트를 자동 생성합니다.
/// 실제 픽셀아트 제작 전 씬 확인용 단색 이미지를 Assets/Sprites/ 에 저장합니다.
///
/// LAST BRAKE > 14. 플레이스홀더 스프라이트 생성
/// </summary>
public class PlaceholderSpriteGenerator : EditorWindow
{
    const string BG_PATH  = "Assets/Sprites/Backgrounds";
    const string CHR_PATH = "Assets/Sprites/Characters";

    // ── 배경 정의 ─────────────────────────────────────────────────────────
    static readonly (string name, Color color)[] Backgrounds =
    {
        ("BG_MainMenu",       HexColor("#1A1A2E")),   // 진한 남색 – 메인메뉴
        ("BG_Step1_Prologue", HexColor("#2D1B69")),   // 보라빛 밤 – 프롤로그
        ("BG_Step2_Club",     HexColor("#0D0D1A")),   // 칠흑 – 클럽
        ("BG_Step3_Morning",  HexColor("#FFD9A0")),   // 아침 빛 – 다음날 아침
        ("BG_Step4_Party",    HexColor("#1A0A2E")),   // 파티 밤 – 파티
        ("BG_Step5_Collapse", HexColor("#3D0000")),   // 짙은 붉은 – 무너짐
        ("BG_Ending",         HexColor("#0A0A0A")),   // 암흑 – 엔딩 공통
    };

    // ── 캐릭터 정의 ──────────────────────────────────────────────────────
    // 각 캐릭터의 기본 몸통 색상
    static readonly (string id, Color baseColor)[] Characters =
    {
        ("Doyun",  HexColor("#4A90D9")),   // 도윤 – 파랑
        ("Minjae", HexColor("#E8A838")),   // 민재 – 주황
        ("Hajun",  HexColor("#5CB85C")),   // 하준 – 초록
        ("Seoa",   HexColor("#D95B7A")),   // 서아 – 분홍
    };

    // 감정별 색조 오프셋 (HSV Value 조정)
    static readonly (string emotion, float brightness)[] Emotions =
    {
        ("Neutral",     1.00f),
        ("Happy",       1.20f),
        ("Worried",     0.75f),
        ("Drunk",       0.85f),
        ("Shocked",     1.30f),
        ("Pain",        0.55f),
        ("Smug",        1.10f),
        ("Determined",  0.90f),
    };

    // ── 메인 메뉴 항목 ────────────────────────────────────────────────────
    [MenuItem("LAST BRAKE/14. 플레이스홀더 스프라이트 생성")]
    public static void GenerateAll()
    {
        EnsureDirectory(BG_PATH);
        EnsureDirectory(CHR_PATH);

        int bgCount  = GenerateBackgrounds();
        int chrCount = GenerateCharacters();

        AssetDatabase.Refresh();

        // Sprite 임포트 설정 적용
        ApplySpriteImportSettings(BG_PATH,  1920, 1080);
        ApplySpriteImportSettings(CHR_PATH, 512,  768);

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

        EditorUtility.DisplayDialog("완료",
            $"플레이스홀더 스프라이트 생성 완료!\n\n" +
            $"· 배경: {bgCount}개  →  {BG_PATH}/\n" +
            $"· 캐릭터: {chrCount}개  →  {CHR_PATH}/\n\n" +
            "나중에 실제 픽셀아트로 교체하세요.", "확인");
    }

    // ── 배경 생성 ─────────────────────────────────────────────────────────
    static int GenerateBackgrounds()
    {
        int count = 0;
        foreach (var (name, color) in Backgrounds)
        {
            string filePath = $"{BG_PATH}/{name}.png";
            if (File.Exists(Path.Combine(Application.dataPath, "../", filePath)))
            {
                Debug.Log($"[Placeholder] 스킵 (이미 존재): {filePath}");
                continue;
            }

            // 1920×1080 그라디언트 배경 생성
            var tex = CreateGradientTexture(1920, 1080, color, DarkenColor(color, 0.4f));
            SaveTexture(tex, filePath);
            Object.DestroyImmediate(tex);
            count++;
        }
        return count;
    }

    // ── 캐릭터 생성 ──────────────────────────────────────────────────────
    static int GenerateCharacters()
    {
        int count = 0;
        foreach (var (charId, baseColor) in Characters)
        {
            string charDir = $"{CHR_PATH}/{charId}";
            EnsureDirectory(charDir);

            foreach (var (emotion, brightness) in Emotions)
            {
                string filePath = $"{charDir}/{charId}_{emotion}.png";
                if (File.Exists(Path.Combine(Application.dataPath, "../", filePath)))
                {
                    Debug.Log($"[Placeholder] 스킵 (이미 존재): {filePath}");
                    continue;
                }

                Color emotionColor = AdjustBrightness(baseColor, brightness);
                var tex = CreateCharacterTexture(512, 768, emotionColor, charId, emotion);
                SaveTexture(tex, filePath);
                Object.DestroyImmediate(tex);
                count++;
            }
        }
        return count;
    }

    // ── 텍스처 생성 헬퍼 ─────────────────────────────────────────────────

    /// <summary>단순 수직 그라디언트 텍스처 (배경용)</summary>
    static Texture2D CreateGradientTexture(int w, int h, Color top, Color bottom)
    {
        var tex = new Texture2D(w, h, TextureFormat.RGBA32, false);
        Color[] pixels = new Color[w * h];
        for (int y = 0; y < h; y++)
        {
            float t = (float)y / (h - 1);
            Color c = Color.Lerp(bottom, top, t);
            for (int x = 0; x < w; x++)
                pixels[y * w + x] = c;
        }
        tex.SetPixels(pixels);
        tex.Apply();
        return tex;
    }

    /// <summary>캐릭터 실루엣 텍스처 (512×768, 세로형)</summary>
    static Texture2D CreateCharacterTexture(int w, int h, Color bodyColor, string charId, string emotion)
    {
        var tex = new Texture2D(w, h, TextureFormat.RGBA32, false);
        Color[] pixels = new Color[w * h];

        // 배경: 완전 투명
        for (int i = 0; i < pixels.Length; i++)
            pixels[i] = Color.clear;

        // 몸통 영역 (중앙 정렬, 위쪽 80% 채움)
        int bodyX  = w / 4;
        int bodyW  = w / 2;
        int bodyY  = (int)(h * 0.05f);
        int bodyH  = (int)(h * 0.80f);

        // 몸통 채우기
        for (int y = bodyY; y < bodyY + bodyH; y++)
        {
            for (int x = bodyX; x < bodyX + bodyW; x++)
            {
                if (x >= 0 && x < w && y >= 0 && y < h)
                    pixels[y * w + x] = bodyColor;
            }
        }

        // 머리 (타원형 근사)
        int headCX = w / 2;
        int headCY = bodyY + bodyH - (int)(bodyH * 0.1f);
        int headRX = (int)(bodyW * 0.45f);
        int headRY = (int)(bodyW * 0.55f);
        for (int y = headCY - headRY; y <= headCY + headRY; y++)
        {
            for (int x = headCX - headRX; x <= headCX + headRX; x++)
            {
                if (x < 0 || x >= w || y < 0 || y >= h) continue;
                float dx = (float)(x - headCX) / headRX;
                float dy = (float)(y - headCY) / headRY;
                if (dx * dx + dy * dy <= 1.0f)
                    pixels[y * w + x] = LightenColor(bodyColor, 0.15f);
            }
        }

        // 이름 + 감정 라벨 (하단 영역, 밝은 픽셀로 표시)
        // 픽셀 패턴으로 식별용 마커 그리기 (3×3 점)
        DrawMarker(pixels, w, h, w / 2, (int)(h * 0.92f), GetEmotionMarkerColor(emotion));

        tex.SetPixels(pixels);
        tex.Apply();
        return tex;
    }

    /// <summary>감정 식별용 마커 픽셀 (5×5)</summary>
    static void DrawMarker(Color[] pixels, int w, int h, int cx, int cy, Color color)
    {
        for (int dy = -4; dy <= 4; dy++)
        for (int dx = -4; dx <= 4; dx++)
        {
            int x = cx + dx, y = cy + dy;
            if (x >= 0 && x < w && y >= 0 && y < h)
                pixels[y * w + x] = color;
        }
    }

    static Color GetEmotionMarkerColor(string emotion) => emotion switch
    {
        "Happy"       => Color.yellow,
        "Worried"     => new Color(0.6f, 0.6f, 1f),
        "Drunk"       => new Color(1f, 0.4f, 0.8f),
        "Shocked"     => Color.white,
        "Pain"        => Color.red,
        "Smug"        => new Color(1f, 0.8f, 0f),
        "Determined"  => new Color(0f, 1f, 0.8f),
        _             => Color.gray,
    };

    // ── 유틸리티 ─────────────────────────────────────────────────────────

    static void SaveTexture(Texture2D tex, string assetPath)
    {
        byte[] png = tex.EncodeToPNG();
        string absPath = Path.Combine(Application.dataPath, "../", assetPath);
        string dir = Path.GetDirectoryName(absPath);
        if (!Directory.Exists(dir)) Directory.CreateDirectory(dir);
        File.WriteAllBytes(absPath, png);
        AssetDatabase.ImportAsset(assetPath);
        Debug.Log($"[Placeholder] 생성: {assetPath}");
    }

    static void EnsureDirectory(string assetPath)
    {
        string absPath = Path.Combine(Application.dataPath, "../", assetPath);
        if (!Directory.Exists(absPath))
        {
            Directory.CreateDirectory(absPath);
            AssetDatabase.Refresh();
        }
    }

    /// <summary>임포트된 PNG를 Sprite 타입으로 설정</summary>
    static void ApplySpriteImportSettings(string folderPath, int pixelsPerUnit_w, int pixelsPerUnit_h)
    {
        string[] guids = AssetDatabase.FindAssets("t:Texture2D", new[] { folderPath });
        foreach (string guid in guids)
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);
            var importer = AssetImporter.GetAtPath(path) as TextureImporter;
            if (importer == null) continue;

            importer.textureType         = TextureImporterType.Sprite;
            importer.spriteImportMode    = SpriteImportMode.Single;
            importer.spritePivot         = new Vector2(0.5f, 0.5f);
            importer.spritePixelsPerUnit = 100;
            importer.filterMode          = FilterMode.Bilinear;
            importer.mipmapEnabled       = false;
            importer.alphaIsTransparency = true;
            importer.SaveAndReimport();
        }
    }

    static Color HexColor(string hex)
    {
        ColorUtility.TryParseHtmlString(hex, out Color c);
        return c;
    }

    static Color DarkenColor(Color c, float amount)
    {
        Color.RGBToHSV(c, out float h, out float s, out float v);
        return Color.HSVToRGB(h, s, Mathf.Clamp01(v - amount));
    }

    static Color LightenColor(Color c, float amount)
    {
        Color.RGBToHSV(c, out float h, out float s, out float v);
        return Color.HSVToRGB(h, Mathf.Clamp01(s - amount * 0.5f), Mathf.Clamp01(v + amount));
    }

    static Color AdjustBrightness(Color c, float multiplier)
    {
        Color.RGBToHSV(c, out float h, out float s, out float v);
        return Color.HSVToRGB(h, s, Mathf.Clamp01(v * multiplier));
    }
}
