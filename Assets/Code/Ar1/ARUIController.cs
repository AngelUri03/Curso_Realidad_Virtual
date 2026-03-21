using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UIElements;
using UnityEngine.TextCore.Text;
using Vuforia;

public class ARUIController : MonoBehaviour
{
    [Header("Modelos")]
    [SerializeField] private GameObject rick;
    [SerializeField] private GameObject morty;

    [Header("Animadores")]
    [SerializeField] private Animator rickAnimator;
    [SerializeField] private Animator mortyAnimator;

    [Header("Parámetros Animator")]
    [SerializeField] private string rickWalkParameter = "IsWalking";
    [SerializeField] private string mortyAnimationStateName = "Slow Run";

    [Header("Objetos extra")]
    [SerializeField] private GameObject gun;
    [SerializeField] private GameObject portalGun;
    [SerializeField] private GameObject robot;

    [Header("Movimiento entre marcadores")]
    [SerializeField] private ObserverBehaviour[] imageTargets;
    [SerializeField] private float moveSpeed = 0.6f;
    [SerializeField] private float rotationSpeed = 8f;
    [SerializeField] private float arriveDistance = 0.01f;

    [Header("Ajuste de orientación de Rick")]
    [SerializeField] private float rickRotationOffsetY = 0f;

    [Header("Materiales de Rick")]
    [SerializeField] private Material coatMaterial;
    [SerializeField] private Material hairMaterial;
    [SerializeField] private Material trousersMaterial;

    [Header("Fuente de botones (opcional, TextCore Font Asset)")]
    [SerializeField] private FontAsset buttonFont;

    [Header("Escena anterior")]
    [SerializeField] private string previousSceneName = "MainMenu";

    private UIDocument uiDocument;

    private VisualElement sidebarTrack;

    private Button toggleSidebarButton;
    private Button colorButton;
    private Button mortyButton;
    private Button extraButton;
    private Button moveButton;
    private Button backButton;

    private bool isSidebarExpanded = false;

    // 0 = bata, 1 = cabello, 2 = pantalón
    private int colorCycleIndex = 0;

    // Índice del extra activo
    private int currentExtraIndex = -1;
    private GameObject[] extraObjects;

    // Movimiento
    private bool isMoving = false;
    private int currentTargetIndex = -1;

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
        extraButton = root.Q<Button>("extra-button");
        moveButton = root.Q<Button>("move-button");
        backButton = root.Q<Button>("back-button");

        extraObjects = new GameObject[] { gun, portalGun, robot };

        ApplyButtonFont();
        RegisterButtonAnimations();

        if (toggleSidebarButton != null)
            toggleSidebarButton.clicked += ToggleSidebar;

        if (colorButton != null)
            colorButton.clicked += ChangeRickColor;

        if (mortyButton != null)
            mortyButton.clicked += ToggleMorty;

        if (extraButton != null)
            extraButton.clicked += ShowRandomExtraObject;

        if (moveButton != null)
            moveButton.clicked += MoveRickToNextMarker;

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

        if (extraButton != null)
            extraButton.clicked -= ShowRandomExtraObject;

        if (moveButton != null)
            moveButton.clicked -= MoveRickToNextMarker;

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

        HideAllExtraObjects();
        currentExtraIndex = -1;
        SetRickWalking(false);
    }

    private void HideAllExtraObjects()
    {
        if (extraObjects == null)
            return;

        foreach (GameObject obj in extraObjects)
        {
            if (obj != null)
                obj.SetActive(false);
        }
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

        if (extraButton != null)
            extraButton.style.unityFontDefinition = fontDefinition;

        if (moveButton != null)
            moveButton.style.unityFontDefinition = fontDefinition;

        if (backButton != null)
            backButton.style.unityFontDefinition = fontDefinition;
    }

    private void RegisterButtonAnimations()
    {
        RegisterPressAnimation(toggleSidebarButton);
        RegisterPressAnimation(colorButton);
        RegisterPressAnimation(mortyButton);
        RegisterPressAnimation(extraButton);
        RegisterPressAnimation(moveButton);
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
                    coatMaterial.color = randomColor;
                else
                    Debug.LogError("No se asignó coatMaterial.");
                break;

            case 1:
                if (hairMaterial != null)
                    hairMaterial.color = randomColor;
                else
                    Debug.LogError("No se asignó hairMaterial.");
                break;

            case 2:
                if (trousersMaterial != null)
                    trousersMaterial.color = randomColor;
                else
                    Debug.LogError("No se asignó trousersMaterial.");
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
        if (morty == null)
        {
            Debug.LogError("Morty no está asignado.");
            return;
        }

        bool newState = !morty.activeSelf;
        morty.SetActive(newState);

        if (newState)
        {
            Animator animator = mortyAnimator != null ? mortyAnimator : morty.GetComponent<Animator>();

            if (animator != null)
            {
                animator.Rebind();
                animator.Update(0f);
                animator.Play(mortyAnimationStateName, 0, 0f);
            }
            else
            {
                Debug.LogWarning("Morty no tiene Animator.");
            }
        }

        EnsureRickAlwaysVisible();
    }

    private void ShowRandomExtraObject()
    {
        EnsureRickAlwaysVisible();

        if (extraObjects == null || extraObjects.Length == 0)
        {
            Debug.LogError("No hay objetos extra configurados.");
            return;
        }

        int validCount = 0;
        foreach (GameObject obj in extraObjects)
        {
            if (obj != null)
                validCount++;
        }

        if (validCount == 0)
        {
            Debug.LogError("Todos los objetos extra están vacíos en el Inspector.");
            return;
        }

        if (currentExtraIndex >= 0 &&
            currentExtraIndex < extraObjects.Length &&
            extraObjects[currentExtraIndex] != null)
        {
            extraObjects[currentExtraIndex].SetActive(false);
        }

        int nextIndex = GetRandomExtraIndexWithoutRepeat();

        if (nextIndex == -1)
        {
            Debug.LogError("No se pudo seleccionar un objeto extra válido.");
            return;
        }

        extraObjects[nextIndex].SetActive(true);
        currentExtraIndex = nextIndex;

        if (extraButton != null)
            extraButton.text = "Extra: " + extraObjects[nextIndex].name;
    }

    private int GetRandomExtraIndexWithoutRepeat()
    {
        if (extraObjects == null || extraObjects.Length == 0)
            return -1;

        List<int> availableIndexes = new List<int>();

        for (int i = 0; i < extraObjects.Length; i++)
        {
            if (extraObjects[i] != null && i != currentExtraIndex)
                availableIndexes.Add(i);
        }

        if (availableIndexes.Count == 0)
        {
            if (currentExtraIndex >= 0 &&
                currentExtraIndex < extraObjects.Length &&
                extraObjects[currentExtraIndex] != null)
            {
                return currentExtraIndex;
            }

            return -1;
        }

        int randomListIndex = Random.Range(0, availableIndexes.Count);
        return availableIndexes[randomListIndex];
    }

    private void MoveRickToNextMarker()
    {
        EnsureRickAlwaysVisible();

        if (isMoving)
            return;

        if (rick == null)
        {
            Debug.LogError("Rick no está asignado.");
            return;
        }

        StartCoroutine(MoveRickCoroutine());
    }

    private IEnumerator MoveRickCoroutine()
    {
        isMoving = true;

        ObserverBehaviour target = GetNextDetectedTarget();

        if (target == null)
        {
            Debug.LogWarning("No hay marcadores detectados para mover a Rick.");
            SetRickWalking(false);
            isMoving = false;
            yield break;
        }

        Vector3 endPosition = target.transform.position;
        endPosition.y = rick.transform.position.y;

        Vector3 direction = endPosition - rick.transform.position;
        direction.y = 0f;

        if (direction.sqrMagnitude < 0.0001f)
        {
            SetRickWalking(false);
            isMoving = false;
            yield break;
        }

        yield return StartCoroutine(RotateRickTowards(endPosition));

        SetRickWalking(true);

        while (Vector3.Distance(rick.transform.position, endPosition) > arriveDistance)
        {
            Vector3 moveDirection = endPosition - rick.transform.position;
            moveDirection.y = 0f;

            if (moveDirection.sqrMagnitude > 0.0001f)
            {
                Quaternion targetRotation = Quaternion.LookRotation(moveDirection.normalized, Vector3.up);
                targetRotation *= Quaternion.Euler(0f, rickRotationOffsetY, 0f);

                rick.transform.rotation = Quaternion.Slerp(
                    rick.transform.rotation,
                    targetRotation,
                    Time.deltaTime * rotationSpeed
                );
            }

            rick.transform.position = Vector3.MoveTowards(
                rick.transform.position,
                endPosition,
                moveSpeed * Time.deltaTime
            );

            yield return null;
        }

        rick.transform.position = endPosition;
        SetRickWalking(false);
        isMoving = false;
    }

    private IEnumerator RotateRickTowards(Vector3 targetPosition)
    {
        if (rick == null)
            yield break;

        Vector3 direction = targetPosition - rick.transform.position;
        direction.y = 0f;

        if (direction.sqrMagnitude < 0.0001f)
            yield break;

        Quaternion targetRotation = Quaternion.LookRotation(direction.normalized, Vector3.up);
        targetRotation *= Quaternion.Euler(0f, rickRotationOffsetY, 0f);

        while (Quaternion.Angle(rick.transform.rotation, targetRotation) > 1f)
        {
            rick.transform.rotation = Quaternion.Slerp(
                rick.transform.rotation,
                targetRotation,
                Time.deltaTime * rotationSpeed
            );

            yield return null;
        }

        rick.transform.rotation = targetRotation;
    }

    private void SetRickWalking(bool walking)
    {
        if (rickAnimator != null)
            rickAnimator.SetBool(rickWalkParameter, walking);
    }

    private ObserverBehaviour GetNextDetectedTarget()
    {
        if (imageTargets == null || imageTargets.Length == 0)
            return null;

        int totalTargets = imageTargets.Length;

        for (int i = 1; i <= totalTargets; i++)
        {
            int index = (currentTargetIndex + i) % totalTargets;
            ObserverBehaviour target = imageTargets[index];

            if (target != null &&
                (target.TargetStatus.Status == Status.TRACKED ||
                 target.TargetStatus.Status == Status.EXTENDED_TRACKED))
            {
                currentTargetIndex = index;
                return target;
            }
        }

        return null;
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