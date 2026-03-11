using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UIElements;
using UnityEngine.TextCore.Text;

public class MainMenuController : MonoBehaviour
{
    [Header("Assets")]
    [SerializeField] private Texture2D heroTexture;

    [Header("Optional Fonts (TextCore Font Assets)")]
    [SerializeField] private FontAsset titleFont;
    [SerializeField] private FontAsset bodyFont;

    private UIDocument uiDocument;

    private VisualElement root;
    private VisualElement mainCard;
    private VisualElement bgAura1;
    private VisualElement bgAura2;

    private VisualElement heroImage;
    private VisualElement heroGlowFront;
    private VisualElement heroGlowBack;

    private Label titleLabel;
    private Label eyebrowLabel;
    private Label subtitleLabel;

    private Button startButton;
    private Button exitButton;

    private IVisualElementScheduledItem startPulseItem;
    private IVisualElementScheduledItem auraItem;
    private IVisualElementScheduledItem heroItem;

    private bool startAlt = false;
    private bool auraAlt = false;
    private bool heroAlt = false;

    private void OnEnable()
    {
        uiDocument = GetComponent<UIDocument>();

        if (uiDocument == null)
        {
            Debug.LogError("No se encontró UIDocument.");
            return;
        }

        root = uiDocument.rootVisualElement;

        if (root == null)
        {
            Debug.LogError("rootVisualElement es null.");
            return;
        }

        QueryElements();
        ApplyAssets();
        RegisterButtonEvents();
        RegisterHeroFrameHover();

        PlayCardEntryAnimation();
        StartButtonPulse();
        StartAuraAnimation();
        StartHeroAnimation();
    }

    private void OnDisable()
    {
        UnregisterButtonEvents();
        PauseAnimations();
    }

    private void QueryElements()
    {
        mainCard = root.Q<VisualElement>("main-card");
        bgAura1 = root.Q<VisualElement>("bg-portal-aura");
        bgAura2 = root.Q<VisualElement>("bg-portal-aura-2");

        heroImage = root.Q<VisualElement>("hero-image");
        heroGlowFront = root.Q<VisualElement>("hero-image-glow-front");
        heroGlowBack = root.Q<VisualElement>("hero-image-glow-back");

        titleLabel = root.Q<Label>("app-title");
        eyebrowLabel = root.Q<Label>("eyebrow");
        subtitleLabel = root.Q<Label>("app-subtitle");

        startButton = root.Q<Button>("start-ar-button");
        exitButton = root.Q<Button>("exit-button");
    }

    private void ApplyAssets()
    {
        ApplyHeroImage();
        ApplyFonts();
    }

    private void ApplyHeroImage()
    {
        if (heroImage == null)
            return;

        if (heroTexture != null)
            heroImage.style.backgroundImage = new StyleBackground(heroTexture);
        else
            Debug.LogWarning("No se asignó heroTexture en el inspector.");
    }

    private void ApplyFonts()
    {
        if (titleFont != null && titleLabel != null)
        {
            titleLabel.style.unityFontDefinition = new FontDefinition
            {
                fontAsset = titleFont
            };
        }
        else
        {
            if (titleFont == null)
                Debug.LogWarning("No se asignó titleFont (FontAsset) en el inspector.");
            if (titleLabel == null)
                Debug.LogWarning("No se encontró app-title en el UXML.");
        }

        if (bodyFont != null)
        {
            FontDefinition bodyFontDefinition = new FontDefinition
            {
                fontAsset = bodyFont
            };

            if (eyebrowLabel != null)
                eyebrowLabel.style.unityFontDefinition = bodyFontDefinition;

            if (subtitleLabel != null)
                subtitleLabel.style.unityFontDefinition = bodyFontDefinition;

            if (startButton != null)
                startButton.style.unityFontDefinition = bodyFontDefinition;

            if (exitButton != null)
                exitButton.style.unityFontDefinition = bodyFontDefinition;
        }
        else
        {
            Debug.LogWarning("No se asignó bodyFont (FontAsset) en el inspector.");
        }
    }

    private void RegisterButtonEvents()
    {
        if (startButton != null)
        {
            startButton.clicked += OnStartARClicked;
            RegisterPressAnimation(startButton);
        }

        if (exitButton != null)
        {
            exitButton.clicked += OnExitClicked;
            RegisterPressAnimation(exitButton);
        }
    }

    private void UnregisterButtonEvents()
    {
        if (startButton != null)
            startButton.clicked -= OnStartARClicked;

        if (exitButton != null)
            exitButton.clicked -= OnExitClicked;
    }

    private void RegisterHeroFrameHover()
    {
        VisualElement heroFrame = root.Q<VisualElement>("hero-image-frame");

        if (heroFrame == null)
            return;

        heroFrame.RegisterCallback<PointerEnterEvent>(_ =>
        {
            heroFrame.style.scale = new Scale(new Vector3(1.015f, 1.015f, 1f));
        });

        heroFrame.RegisterCallback<PointerLeaveEvent>(_ =>
        {
            heroFrame.style.scale = new Scale(new Vector3(1f, 1f, 1f));
        });
    }

    private void PauseAnimations()
    {
        if (startPulseItem != null)
            startPulseItem.Pause();

        if (auraItem != null)
            auraItem.Pause();

        if (heroItem != null)
            heroItem.Pause();
    }

    private void PlayCardEntryAnimation()
    {
        if (mainCard == null)
            return;

        mainCard.AddToClassList("card-enter");

        mainCard.schedule.Execute(() =>
        {
            mainCard.AddToClassList("card-enter-active");
            mainCard.RemoveFromClassList("card-enter");
        }).StartingIn(50);
    }

    private void StartButtonPulse()
    {
        if (startButton == null)
            return;

        startButton.AddToClassList("start-idle-a");

        startPulseItem = startButton.schedule.Execute(() =>
        {
            if (startAlt)
            {
                startButton.RemoveFromClassList("start-idle-b");
                startButton.AddToClassList("start-idle-a");
            }
            else
            {
                startButton.RemoveFromClassList("start-idle-a");
                startButton.AddToClassList("start-idle-b");
            }

            startAlt = !startAlt;
        }).Every(1200);
    }

    private void StartAuraAnimation()
    {
        if (bgAura1 != null)
            bgAura1.AddToClassList("aurora-a");

        if (bgAura2 != null)
            bgAura2.AddToClassList("aurora-c");

        auraItem = root.schedule.Execute(() =>
        {
            if (bgAura1 != null)
            {
                bgAura1.RemoveFromClassList(auraAlt ? "aurora-b" : "aurora-a");
                bgAura1.AddToClassList(auraAlt ? "aurora-a" : "aurora-b");
            }

            if (bgAura2 != null)
            {
                bgAura2.RemoveFromClassList(auraAlt ? "aurora-a" : "aurora-c");
                bgAura2.AddToClassList(auraAlt ? "aurora-c" : "aurora-a");
            }

            auraAlt = !auraAlt;
        }).Every(1500);
    }

    private void StartHeroAnimation()
    {
        if (heroImage != null)
            heroImage.AddToClassList("hero-visual-a");

        if (heroGlowFront != null)
            heroGlowFront.AddToClassList("hero-front-a");

        if (heroGlowBack != null)
            heroGlowBack.AddToClassList("hero-back-a");

        heroItem = root.schedule.Execute(() =>
        {
            if (heroImage != null)
            {
                heroImage.RemoveFromClassList(heroAlt ? "hero-visual-b" : "hero-visual-a");
                heroImage.AddToClassList(heroAlt ? "hero-visual-a" : "hero-visual-b");
            }

            if (heroGlowFront != null)
            {
                heroGlowFront.RemoveFromClassList(heroAlt ? "hero-front-b" : "hero-front-a");
                heroGlowFront.AddToClassList(heroAlt ? "hero-front-a" : "hero-front-b");
            }

            if (heroGlowBack != null)
            {
                heroGlowBack.RemoveFromClassList(heroAlt ? "hero-back-b" : "hero-back-a");
                heroGlowBack.AddToClassList(heroAlt ? "hero-back-a" : "hero-back-b");
            }

            heroAlt = !heroAlt;
        }).Every(1450);
    }

    private void RegisterPressAnimation(Button button)
    {
        button.RegisterCallback<PointerDownEvent>(_ =>
        {
            button.style.scale = new Scale(new Vector3(0.965f, 0.965f, 1f));
        });

        button.RegisterCallback<PointerUpEvent>(_ =>
        {
            button.style.scale = new Scale(new Vector3(1f, 1f, 1f));
        });

        button.RegisterCallback<PointerLeaveEvent>(_ =>
        {
            button.style.scale = new Scale(new Vector3(1f, 1f, 1f));
        });
    }

    private void OnStartARClicked()
    {
        Debug.Log("Click en Iniciar experiencia");

        if (Application.CanStreamedLevelBeLoaded("Ar1"))
            SceneManager.LoadScene("Ar1");
        else
            Debug.LogError("La escena 'Ar1' no está en Build Settings.");
    }

    private void OnExitClicked()
    {
        Debug.Log("Click en Salir");

#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }
}