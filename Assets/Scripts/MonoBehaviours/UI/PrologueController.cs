using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;

public class PrologueController : MonoBehaviour
{
    [System.Serializable]
    public class Frame
    {
        public Sprite image;
        [TextArea(2, 5)] public string narration;

        [Tooltip("Skala awal. Untuk zoom in isi 1.0, untuk zoom out isi 1.1")]
        public float zoomFrom = 1.0f;

        [Tooltip("Skala akhir. Untuk zoom in isi 1.1, untuk zoom out isi 1.0")]
        public float zoomTo = 1.1f;

        [Tooltip("Lama gerakan kamera, detik. 6-10 detik terasa tenang.")]
        public float zoomDuration = 8f;

        [Tooltip("Centang untuk momen lampu mati: potong keras tanpa fade.")]
        public bool hardCut = false;

        [Tooltip("Hening sebelum frame ini muncul, detik. Isi 1 untuk momen lampu mati.")]
        public float silenceBefore = 0f;
    }

    [Header("Isi adegan")]
    public List<Frame> frames = new List<Frame>();
    public string nextSceneName = "Gameplay";

    [Header("Referensi UI")]
    public Image frameImage;
    public TextMeshProUGUI narrationText;
    public CanvasGroup fadeOverlay;
    public Button tapCatcher;
    public Button skipButton;

    [Header("Setelan")]
    [Tooltip("Karakter per detik. 30-40 nyaman dibaca, di bawah 20 terasa lambat.")]
    public float charsPerSecond = 35f;

    [Tooltip("Jeda tambahan setelah koma dan titik, detik.")]
    public float punctuationPause = 0.18f;

    public float fadeDuration = 0.35f;

    [Header("Stabilo narasi")]
    [Tooltip("Blok warna di belakang tiap baris teks, seperti stabilo. Ikut memanjang per huruf.")]
    public bool highlight = true;
    public Color highlightColor = Color.black;
    [Tooltip("Ruang di sekitar teks per baris: x = kiri dan kanan, y = atas dan bawah.")]
    public Vector2 highlightPadding = new Vector2(16f, 6f);

    bool tapped;
    bool skipping;
    RectTransform frameRect;
    AspectRatioFitter frameFitter;
    Coroutine zoomRoutine;
    RectTransform highlightLayer;
    readonly List<Image> highlightBars = new List<Image>();

    void Awake()
    {
        frameRect = frameImage.GetComponent<RectTransform>();
        frameFitter = frameImage.GetComponent<AspectRatioFitter>();
        if (highlight) CreateHighlightLayer();
        if (tapCatcher != null) tapCatcher.onClick.AddListener(() => tapped = true);
        if (skipButton != null) skipButton.onClick.AddListener(Skip);
    }

    void Start()
    {
        fadeOverlay.alpha = 1f;
        narrationText.text = "";
        StartCoroutine(Run());
    }

    IEnumerator Run()
    {
        for (int i = 0; i < frames.Count; i++)
        {
            if (skipping) break;
            yield return StartCoroutine(PlayFrame(frames[i]));
        }
        yield return StartCoroutine(Fade(1f));
        Finish();
    }

    IEnumerator PlayFrame(Frame f)
    {
        // Hening sebelum frame muncul. Dipakai untuk momen lampu mati.
        if (f.silenceBefore > 0f)
        {
            narrationText.text = "";
            yield return new WaitForSeconds(f.silenceBefore);
        }

        frameImage.sprite = f.image;
        // Rasio ikut gambar, supaya panel beda ukuran tidak gepeng.
        if (frameFitter != null && f.image != null)
            frameFitter.aspectRatio = f.image.rect.width / f.image.rect.height;
        // Zoom frame sebelumnya dihentikan dulu supaya tidak rebutan skala.
        if (zoomRoutine != null) StopCoroutine(zoomRoutine);
        frameRect.localScale = Vector3.one * f.zoomFrom;
        narrationText.text = f.narration;
        narrationText.maxVisibleCharacters = 0;

        if (f.hardCut) fadeOverlay.alpha = 0f;
        else yield return StartCoroutine(Fade(0f));

        // Gerakan kamera jalan sendiri di belakang, tidak menahan alur.
        zoomRoutine = StartCoroutine(Zoom(f.zoomFrom, f.zoomTo, f.zoomDuration));

        // Teks ngetik. Tap pertama bikin teks langsung penuh.
        yield return StartCoroutine(Typewriter(f.narration));

        // Tap berikutnya baru pindah frame.
        tapped = false;
        while (!tapped && !skipping) yield return null;
        tapped = false;

        if (!skipping) yield return StartCoroutine(Fade(1f));
    }

    // Stabilo pakai Image sungguhan, bukan tag <mark>. Tag <mark> TMP selalu agak tembus
    // karena warnanya diambil dari glyph garis bawah yang tipis di atlas font.
    void CreateHighlightLayer()
    {
        RectTransform textRect = narrationText.rectTransform;
        var go = new GameObject("NarrationHighlight", typeof(RectTransform));
        highlightLayer = (RectTransform)go.transform;
        highlightLayer.SetParent(textRect.parent, false);
        // Taruh tepat di bawah teks supaya tergambar di belakangnya.
        highlightLayer.SetSiblingIndex(textRect.GetSiblingIndex());
        highlightLayer.anchorMin = textRect.anchorMin;
        highlightLayer.anchorMax = textRect.anchorMax;
        highlightLayer.pivot = textRect.pivot;
        highlightLayer.anchoredPosition = textRect.anchoredPosition;
        highlightLayer.sizeDelta = textRect.sizeDelta;
        highlightLayer.localRotation = textRect.localRotation;
        highlightLayer.localScale = textRect.localScale;
    }

    Image HighlightBar(int index)
    {
        while (highlightBars.Count <= index)
        {
            var go = new GameObject("Bar", typeof(RectTransform), typeof(Image));
            var bar = go.GetComponent<Image>();
            bar.raycastTarget = false;
            RectTransform r = bar.rectTransform;
            r.SetParent(highlightLayer, false);
            // Titik jangkar di pivot induk, sama dengan titik nol koordinat karakter TMP.
            r.anchorMin = r.anchorMax = highlightLayer.pivot;
            r.pivot = Vector2.zero;
            highlightBars.Add(bar);
        }
        return highlightBars[index];
    }

    void LateUpdate()
    {
        if (highlightLayer == null) return;

        narrationText.ForceMeshUpdate();
        TMP_TextInfo info = narrationText.textInfo;
        int visible = narrationText.maxVisibleCharacters;
        int used = 0;

        for (int l = 0; l < info.lineCount; l++)
        {
            TMP_LineInfo line = info.lineInfo[l];
            float minX = float.MaxValue, maxX = float.MinValue;
            for (int c = line.firstCharacterIndex; c <= line.lastCharacterIndex && c < info.characterCount; c++)
            {
                if (c >= visible) break;
                TMP_CharacterInfo ch = info.characterInfo[c];
                if (!ch.isVisible) continue;
                minX = Mathf.Min(minX, ch.bottomLeft.x);
                maxX = Mathf.Max(maxX, ch.topRight.x);
            }
            if (maxX < minX) continue;

            Image bar = HighlightBar(used++);
            bar.color = highlightColor;
            bar.rectTransform.anchoredPosition = new Vector2(minX - highlightPadding.x, line.descender - highlightPadding.y);
            bar.rectTransform.sizeDelta = new Vector2(maxX - minX + highlightPadding.x * 2f,
                line.ascender - line.descender + highlightPadding.y * 2f);
            bar.enabled = true;
        }

        for (int i = used; i < highlightBars.Count; i++) highlightBars[i].enabled = false;
    }

    IEnumerator Typewriter(string full)
    {
        int total = full.Length;
        int shown = 0;
        float perChar = 1f / Mathf.Max(1f, charsPerSecond);
        tapped = false;

        while (shown < total)
        {
            if (tapped || skipping)
            {
                narrationText.maxVisibleCharacters = total;
                tapped = false;
                yield break;
            }

            shown++;
            narrationText.maxVisibleCharacters = shown;

            float wait = perChar;
            char c = full[shown - 1];
            if (c == ',' || c == '.' || c == '?' || c == '!') wait += punctuationPause;

            float t = 0f;
            while (t < wait)
            {
                if (tapped || skipping)
                {
                    narrationText.maxVisibleCharacters = total;
                    tapped = false;
                    yield break;
                }
                t += Time.deltaTime;
                yield return null;
            }
        }
    }

    IEnumerator Zoom(float from, float to, float duration)
    {
        float t = 0f;
        while (t < duration)
        {
            t += Time.deltaTime;
            float k = t / duration;
            frameRect.localScale = Vector3.one * Mathf.Lerp(from, to, k);
            yield return null;
        }
        frameRect.localScale = Vector3.one * to;
    }

    IEnumerator Fade(float target)
    {
        float start = fadeOverlay.alpha;
        float t = 0f;
        while (t < fadeDuration)
        {
            t += Time.deltaTime;
            fadeOverlay.alpha = Mathf.Lerp(start, target, t / fadeDuration);
            yield return null;
        }
        fadeOverlay.alpha = target;
    }

    public void Skip()
    {
        if (skipping) return;
        skipping = true;
        StopAllCoroutines();
        StartCoroutine(SkipRoutine());
    }

    IEnumerator SkipRoutine()
    {
        yield return StartCoroutine(Fade(1f));
        Finish();
    }

    void Finish()
    {
        SceneManager.LoadScene(nextSceneName);
    }
}