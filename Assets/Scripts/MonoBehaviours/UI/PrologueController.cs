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

    bool tapped;
    bool skipping;
    RectTransform frameRect;

    void Awake()
    {
        frameRect = frameImage.GetComponent<RectTransform>();
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
        frameRect.localScale = Vector3.one * f.zoomFrom;
        narrationText.text = f.narration;
        narrationText.maxVisibleCharacters = 0;

        if (f.hardCut) fadeOverlay.alpha = 0f;
        else yield return StartCoroutine(Fade(0f));

        // Gerakan kamera jalan sendiri di belakang, tidak menahan alur.
        StartCoroutine(Zoom(f.zoomFrom, f.zoomTo, f.zoomDuration));

        // Teks ngetik. Tap pertama bikin teks langsung penuh.
        yield return StartCoroutine(Typewriter(f.narration));

        // Tap berikutnya baru pindah frame.
        tapped = false;
        while (!tapped && !skipping) yield return null;
        tapped = false;

        if (!skipping) yield return StartCoroutine(Fade(1f));
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