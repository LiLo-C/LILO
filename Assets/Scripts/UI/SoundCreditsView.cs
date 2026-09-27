using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Lilo.UI
{
    /// <summary>
    /// A separate, scrollable page for audio acknowledgements in the main menu.
    /// The existing team credits remain laid out in the scene.
    /// </summary>
    public sealed class SoundCreditsView : MonoBehaviour
    {
        private readonly struct SoundCredit
        {
            public readonly string Title;
            public readonly string Creator;
            public readonly string Url;

            public SoundCredit(string title, string creator, string url)
            {
                Title = title;
                Creator = creator;
                Url = url;
            }
        }

        private static readonly SoundCredit[] Credits =
        {
            new SoundCredit("Basement", "deranged (Freesound)", "https://pixabay.com/sound-effects/film-special-effects-basement-75082/"),
            new SoundCredit("Creepy Vocal Ambience", "deleted_user_4772965 (Freesound)", "https://pixabay.com/sound-effects/horror-creepy-vocal-ambience-6074/"),
            new SoundCredit("Flickering Neon", "Kave_msri", "https://pixabay.com/sound-effects/film-special-effects-flickering-neon-316717/"),
            new SoundCredit("What a Real-Life Dementor Would Sound Like", "Mazellmi", "https://www.youtube.com/watch?v=karlcsjzz5E"),
            new SoundCredit("Office Ambience", "freesound_community", "https://pixabay.com/sound-effects/city-office-ambience-6322/"),
            new SoundCredit("typing on laptop keyboard 1.wav", "Sorinious_Genious (Freesound)", "https://pixabay.com/sound-effects/technology-typing-on-laptop-keyboard-1wav-14885/"),
            new SoundCredit("Light Switch Flip", "Homemade_SFX", "https://pixabay.com/sound-effects/household-light-switch-flip-272436/"),
            new SoundCredit("Drawer Open", "cMilan (Freesound)", "https://pixabay.com/sound-effects/household-drawer-open-98801/"),
            new SoundCredit("Ambient Clarity", "Grumpynora", "https://pixabay.com/sound-effects/musical-ambient-clarity-459927/"),
            new SoundCredit("Horror Background Atmosphere", "Universfield", "https://pixabay.com/sound-effects/horror-horror-background-atmosphere-156462/"),
            new SoundCredit("Sound Effect 69166", "freesound_community", "https://pixabay.com/sound-effects/69166/"),
            new SoundCredit("Sound Effect 64282", "freesound_community", "https://pixabay.com/sound-effects/64282/"),
            new SoundCredit("Ambient Empty Room Noise Sound Effect", "XomXomski", "https://pixabay.com/sound-effects/ambient-empty-room-noise-sound-effect-429845/"),
            new SoundCredit("Typing Keyboard ASMR", "DRAGON-STUDIO", "https://pixabay.com/sound-effects/typing-keyboard-asmr-356116/"),
            new SoundCredit("Sound Effect 66807", "freesound_community", "https://pixabay.com/sound-effects/66807/"),
            new SoundCredit("Sound Effect 478011", "Vicki Hamilton", "https://pixabay.com/sound-effects/478011/"),
            new SoundCredit("Sound Effect 6335", "freesound_community", "https://pixabay.com/sound-effects/6335/"),
            new SoundCredit("Sound Effect 26531", "freesound_community", "https://pixabay.com/sound-effects/26531/"),
            new SoundCredit("Sound Effect 345814", "Universfield", "https://pixabay.com/sound-effects/345814/"),
            new SoundCredit("Scream Horror SFX", "Jusatti890", "https://pixabay.com/sound-effects/490916/"),
            new SoundCredit("Sound Effect 490910", "Jusatti890", "https://pixabay.com/sound-effects/490910/"),
            new SoundCredit("Scary Horror Atmosphere", "Universfield", "https://pixabay.com/sound-effects/scary-horror-atmosphere-176754/"),
            new SoundCredit("Sound Effect 33867", "freesound_community", "https://pixabay.com/sound-effects/33867/"),
            new SoundCredit("Sound Effect 183748", "Roland Horvers", "https://pixabay.com/sound-effects/183748/"),
            new SoundCredit("Key Collect SFX", "litupsubway", "https://pixabay.com/sound-effects/key-collect-sfx-522219/")
        };

        private static readonly Color32 MainText = new Color32(223, 227, 218, 255);
        private static readonly Color32 SecondaryText = new Color32(173, 190, 194, 255);
        private static readonly Color32 CardColor = new Color32(35, 49, 62, 240);

        private GameObject _sourcesPage;
        private bool _built;

        public void ShowTeamCredits()
        {
            if (!_built) Build();
            _sourcesPage.SetActive(false);
        }

        private void Build()
        {
            _built = true;
            RectTransform root = (RectTransform)transform;
            TextMeshProUGUI existingText = GetComponentInChildren<TextMeshProUGUI>(true);
            TMP_FontAsset font = existingText != null ? existingText.font : TMP_Settings.defaultFontAsset;

            Button openButton = CreateButton("OpenSoundCredits", root, new Color32(46, 62, 74, 255));
            SetRect((RectTransform)openButton.transform, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f),
                new Vector2(-400f, -390f), new Vector2(500f, 168f));
            CreateText("Label", openButton.transform, font, "SOUND CREDITS  >", 38f,
                MainText, TextAlignmentOptions.Center, Vector2.zero, new Vector2(480f, 150f));

            _sourcesPage = new GameObject("SoundCreditsPage", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
            _sourcesPage.transform.SetParent(root, false);
            RectTransform page = (RectTransform)_sourcesPage.transform;
            SetRect(page, Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero);
            Image pageBackground = _sourcesPage.GetComponent<Image>();
            pageBackground.color = new Color32(10, 16, 24, 250);

            CreateText("Heading", page, font, "SOUND CREDITS", 58f, MainText,
                TextAlignmentOptions.Center, new Vector2(250f, 335f), new Vector2(1100f, 92f));
            CreateText("Hint", page, font, "Tap a sound to view its source  |  Swipe to see more", 25f,
                SecondaryText, TextAlignmentOptions.Center, new Vector2(250f, 275f), new Vector2(1300f, 44f));

            GameObject scrollObject = new GameObject("SoundCreditsScroll", typeof(RectTransform),
                typeof(CanvasRenderer), typeof(Image), typeof(ScrollRect));
            scrollObject.transform.SetParent(page, false);
            RectTransform scrollRect = (RectTransform)scrollObject.transform;
            SetRect(scrollRect, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f),
                new Vector2(250f, -18f), new Vector2(1500f, 540f));
            Image scrollBackground = scrollObject.GetComponent<Image>();
            scrollBackground.color = new Color32(16, 26, 37, 200);

            GameObject viewportObject = new GameObject("Viewport", typeof(RectTransform),
                typeof(RectMask2D));
            viewportObject.transform.SetParent(scrollRect, false);
            RectTransform viewport = (RectTransform)viewportObject.transform;
            SetRect(viewport, Vector2.zero, Vector2.one, Vector2.zero, new Vector2(-32f, 0f));

            GameObject contentObject = new GameObject("Content", typeof(RectTransform));
            contentObject.transform.SetParent(viewport, false);
            RectTransform content = (RectTransform)contentObject.transform;
            float contentHeight = 24f + Credits.Length * 112f;
            SetRect(content, new Vector2(0f, 1f), new Vector2(1f, 1f),
                Vector2.zero, new Vector2(0f, contentHeight), new Vector2(0.5f, 1f));

            for (int i = 0; i < Credits.Length; i++)
                AddCredit(content, font, Credits[i], i);

            ScrollRect scrolling = scrollObject.GetComponent<ScrollRect>();
            scrolling.content = content;
            scrolling.viewport = viewport;
            scrolling.horizontal = false;
            scrolling.vertical = true;
            scrolling.movementType = ScrollRect.MovementType.Clamped;
            scrolling.scrollSensitivity = 40f;

            Button backButton = CreateButton("BackToTeamCredits", page, new Color32(46, 62, 74, 255));
            SetRect((RectTransform)backButton.transform, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f),
                new Vector2(250f, -390f), new Vector2(500f, 168f));
            CreateText("Label", backButton.transform, font, "BACK TO TEAM", 38f,
                MainText, TextAlignmentOptions.Center, Vector2.zero, new Vector2(480f, 150f));

            openButton.onClick.AddListener(() =>
            {
                scrolling.verticalNormalizedPosition = 1f;
                _sourcesPage.SetActive(true);
            });
            backButton.onClick.AddListener(() => _sourcesPage.SetActive(false));
            _sourcesPage.SetActive(false);
        }

        private static void AddCredit(RectTransform content, TMP_FontAsset font, SoundCredit credit, int index)
        {
            Button row = CreateButton("SoundCredit_" + (index + 1), content, CardColor);
            SetRect((RectTransform)row.transform, new Vector2(0f, 1f), new Vector2(1f, 1f),
                new Vector2(0f, -12f - index * 112f), new Vector2(-24f, 100f), new Vector2(0.5f, 1f));

            TextMeshProUGUI title = CreateText("Title", row.transform, font, credit.Title, 36f,
                MainText, TextAlignmentOptions.Left, new Vector2(28f, 18f), new Vector2(1000f, 45f));
            StretchTextHorizontally(title.rectTransform, 28f, 205f);
            title.enableAutoSizing = true;
            title.fontSizeMin = 30f;
            title.fontSizeMax = 36f;

            TextMeshProUGUI creator = CreateText("Creator", row.transform, font, credit.Creator, 30f,
                SecondaryText, TextAlignmentOptions.Left, new Vector2(28f, -22f), new Vector2(1000f, 38f));
            StretchTextHorizontally(creator.rectTransform, 28f, 205f);
            creator.enableAutoSizing = true;
            creator.fontSizeMin = 26f;
            creator.fontSizeMax = 30f;

            TextMeshProUGUI source = CreateText("SourceLink", row.transform, font, "SOURCE  >", 28f,
                SecondaryText, TextAlignmentOptions.Center, new Vector2(-115f, 0f), new Vector2(190f, 60f));
            RectTransform sourceRect = source.rectTransform;
            sourceRect.anchorMin = new Vector2(1f, 0.5f);
            sourceRect.anchorMax = new Vector2(1f, 0.5f);

            string url = credit.Url;
            row.onClick.AddListener(() => Application.OpenURL(url));
        }

        private static void StretchTextHorizontally(RectTransform rect, float left, float right)
        {
            rect.anchorMin = new Vector2(0f, 0.5f);
            rect.anchorMax = new Vector2(1f, 0.5f);
            rect.offsetMin = new Vector2(left, rect.offsetMin.y);
            rect.offsetMax = new Vector2(-right, rect.offsetMax.y);
        }

        private static Button CreateButton(string name, Transform parent, Color32 color)
        {
            GameObject buttonObject = new GameObject(name, typeof(RectTransform), typeof(CanvasRenderer),
                typeof(Image), typeof(Button));
            buttonObject.transform.SetParent(parent, false);
            Image image = buttonObject.GetComponent<Image>();
            image.color = color;
            Button button = buttonObject.GetComponent<Button>();
            button.targetGraphic = image;
            return button;
        }

        private static TextMeshProUGUI CreateText(string name, Transform parent, TMP_FontAsset font,
            string value, float size, Color color, TextAlignmentOptions alignment,
            Vector2 position, Vector2 dimensions)
        {
            GameObject textObject = new GameObject(name, typeof(RectTransform), typeof(TextMeshProUGUI));
            textObject.transform.SetParent(parent, false);
            RectTransform rect = (RectTransform)textObject.transform;
            SetRect(rect, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), position, dimensions);
            TextMeshProUGUI label = textObject.GetComponent<TextMeshProUGUI>();
            label.font = font;
            label.text = value;
            label.fontSize = size;
            label.color = color;
            label.alignment = alignment;
            label.raycastTarget = false;
            label.overflowMode = TextOverflowModes.Ellipsis;
            return label;
        }

        private static void SetRect(RectTransform rect, Vector2 anchorMin, Vector2 anchorMax,
            Vector2 position, Vector2 size, Vector2? pivot = null)
        {
            rect.anchorMin = anchorMin;
            rect.anchorMax = anchorMax;
            rect.pivot = pivot ?? new Vector2(0.5f, 0.5f);
            rect.anchoredPosition = position;
            rect.sizeDelta = size;
        }
    }
}
