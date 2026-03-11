using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UIElements;
using UnityEngine.TextCore.Text;

public class ARUIController : MonoBehaviour
{
    [Header("Modelos")]
    [SerializeField] private GameObject rick;
    [SerializeField] private GameObject morty;
    [SerializeField] private GameObject portalGun;
    [SerializeField] private GameObject nave;

    [Header("Materiales de Rick")]
    [SerializeField] private Material coatMaterial;      // Bata
    [SerializeField] private Material hairMaterial;      // Cabello
    [SerializeField] private Material trousersMaterial;  // Pantalón

    [Header("Fuente de botones (opcional, TextCore Font Asset)")]
    [SerializeField] private FontAsset buttonFont;

    [Header("Escena anterior")]
    [SerializeField] private string previousSceneName = "MainMenu";

    private UIDocument uiDocument;

    private VisualElement sidebarTrack;

    private Button toggleSidebarButton;
    private Button colorButton;
    private Button mortyButton;
    private Button portalGunButton;
    private Button naveButton;
    private Button backButton;

    private bool isSidebarExpanded = false;

    // 0 = bata, 1 = cabello, 2 = pantalón
    private int colorCycleIndex = 0;

    private void OnEnable()
    {
        uiDocument = GetComponent<UIDocument>();

        if (uiDocument == null)
        {
            Debug.LogError("No se encontró UIDocument en este GameObject.");
            return;
        }

        VisualElement root = uiDocument.rootVisualElement;

        sidebarTrack = root.Q<VisualElement>("sidebar-track");

        toggleSidebarButton = root.Q<Button>("toggle-sidebar-button");
        colorButton = root.Q<Button>("color-button");
        mortyButton = root.Q<Button>("morty-button");
        portalGunButton = root.Q<Button>("portalgun-button");
        naveButton = root.Q<Button>("nave-button");
        backButton = root.Q<Button>("back-button");

        ApplyButtonFont();
        RegisterButtonAnimations();

        if (toggleSidebarButton != null)
            toggleSidebarButton.clicked += ToggleSidebar;

        if (colorButton != null)
            colorButton.clicked += ChangeRickColor;

        if (mortyButton != null)
            mortyButton.clicked += ToggleMorty;

        if (portalGunButton != null)
            portalGunButton.clicked += TogglePortalGun;

        if (naveButton != null)
            naveButton.clicked += ToggleNave;

        if (backButton != null)
            backButton.clicked += GoBackToMenu;
    }

    private void Start()
    {
        InitializeObjects();
        StartCoroutine(EnsureRickVisibleNextFrame());

        isSidebarExpanded = false;
        UpdateSidebarVisualState();
    }

    private void OnDisable()
    {
        if (toggleSidebarButton != null)
            toggleSidebarButton.clicked -= ToggleSidebar;

        if (colorButton != null)
            colorButton.clicked -= ChangeRickColor;

        if (mortyButton != null)
            mortyButton.clicked -= ToggleMorty;

        if (portalGunButton != null)
            portalGunButton.clicked -= TogglePortalGun;

        if (naveButton != null)
            naveButton.clicked -= ToggleNave;

        if (backButton != null)
            backButton.clicked -= GoBackToMenu;
    }

    private IEnumerator EnsureRickVisibleNextFrame()
    {
        yield return null;

        if (rick != null)
            rick.SetActive(true);
    }

    private void InitializeObjects()
    {
        if (rick != null)
            rick.SetActive(true);

        if (morty != null)
            morty.SetActive(false);

        if (portalGun != null)
            portalGun.SetActive(false);

        if (nave != null)
            nave.SetActive(false);
    }

    private void ApplyButtonFont()
    {
        if (buttonFont == null)
            return;

        FontDefinition fontDefinition = new FontDefinition
        {
            fontAsset = buttonFont
        };

        if (toggleSidebarButton != null)
            toggleSidebarButton.style.unityFontDefinition = fontDefinition;

        if (colorButton != null)
            colorButton.style.unityFontDefinition = fontDefinition;

        if (mortyButton != null)
            mortyButton.style.unityFontDefinition = fontDefinition;

        if (portalGunButton != null)
            portalGunButton.style.unityFontDefinition = fontDefinition;

        if (naveButton != null)
            naveButton.style.unityFontDefinition = fontDefinition;

        if (backButton != null)
            backButton.style.unityFontDefinition = fontDefinition;
    }

    private void RegisterButtonAnimations()
    {
        RegisterPressAnimation(toggleSidebarButton);
        RegisterPressAnimation(colorButton);
        RegisterPressAnimation(mortyButton);
        RegisterPressAnimation(portalGunButton);
        RegisterPressAnimation(naveButton);
        RegisterPressAnimation(backButton);
    }

    private void RegisterPressAnimation(Button button)
    {
        if (button == null)
            return;

        button.RegisterCallback<PointerDownEvent>(_ =>
        {
            button.style.scale = new Scale(new Vector3(0.88f, 0.88f, 1f));
            button.style.translate = new Translate(new Length(0), new Length(2));
            button.style.opacity = 0.82f;
        });

        button.RegisterCallback<PointerUpEvent>(_ =>
        {
            button.style.scale = new Scale(new Vector3(1.03f, 1.03f, 1f));
            button.style.translate = new Translate(new Length(0), new Length(0));
            button.style.opacity = 1f;

            button.schedule.Execute(() =>
            {
                button.style.scale = new Scale(new Vector3(1f, 1f, 1f));
            }).StartingIn(70);
        });

        button.RegisterCallback<PointerLeaveEvent>(_ =>
        {
            button.style.scale = new Scale(new Vector3(1f, 1f, 1f));
            button.style.translate = new Translate(new Length(0), new Length(0));
            button.style.opacity = 1f;
        });
    }

    private void ToggleSidebar()
    {
        isSidebarExpanded = !isSidebarExpanded;
        UpdateSidebarVisualState();
    }

    private void UpdateSidebarVisualState()
    {
        if (sidebarTrack == null || toggleSidebarButton == null)
            return;

        sidebarTrack.RemoveFromClassList("sidebar-expanded");
        sidebarTrack.RemoveFromClassList("sidebar-collapsed");

        if (isSidebarExpanded)
        {
            sidebarTrack.AddToClassList("sidebar-expanded");
            toggleSidebarButton.text = "<";
        }
        else
        {
            sidebarTrack.AddToClassList("sidebar-collapsed");
            toggleSidebarButton.text = ">";
        }
    }

    private void ChangeRickColor()
    {
        EnsureRickAlwaysVisible();

        Color randomColor = GeneratePleasantRandomColor();

        switch (colorCycleIndex)
        {
            case 0:
                if (coatMaterial != null)
                {
                    coatMaterial.color = randomColor;
                    Debug.Log($"Bata cambiada a: {randomColor}");
                }
                else
                {
                    Debug.LogError("No se asignó coatMaterial.");
                }
                break;

            case 1:
                if (hairMaterial != null)
                {
                    hairMaterial.color = randomColor;
                    Debug.Log($"Cabello cambiado a: {randomColor}");
                }
                else
                {
                    Debug.LogError("No se asignó hairMaterial.");
                }
                break;

            case 2:
                if (trousersMaterial != null)
                {
                    trousersMaterial.color = randomColor;
                    Debug.Log($"Pantalón cambiado a: {randomColor}");
                }
                else
                {
                    Debug.LogError("No se asignó trousersMaterial.");
                }
                break;
        }

        colorCycleIndex++;

        if (colorCycleIndex > 2)
            colorCycleIndex = 0;
    }

    private Color GeneratePleasantRandomColor()
    {
        float hue = Random.Range(0f, 1f);
        float saturation = Random.Range(0.55f, 0.9f);
        float value = Random.Range(0.75f, 1f);

        return Color.HSVToRGB(hue, saturation, value);
    }

    private void ToggleMorty()
    {
        ToggleObject(morty);
        EnsureRickAlwaysVisible();
    }

    private void TogglePortalGun()
    {
        ToggleObject(portalGun);
        EnsureRickAlwaysVisible();
    }

    private void ToggleNave()
    {
        ToggleObject(nave);
        EnsureRickAlwaysVisible();
    }

    private void ToggleObject(GameObject obj)
    {
        if (obj == null)
        {
            Debug.LogError("Hay un objeto no asignado en el inspector.");
            return;
        }

        obj.SetActive(!obj.activeSelf);
    }

    private void EnsureRickAlwaysVisible()
    {
        if (rick != null && !rick.activeSelf)
            rick.SetActive(true);
    }

    private void GoBackToMenu()
    {
        if (Application.CanStreamedLevelBeLoaded(previousSceneName))
        {
            SceneManager.LoadScene(previousSceneName);
        }
        else
        {
            Debug.LogError($"La escena '{previousSceneName}' no está en Build Settings.");
        }
    }
}