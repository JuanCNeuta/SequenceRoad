using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// StartMenu adaptado para mostrar siempre panelConsent al pulsar Iniciar,
/// usando ConsentPanelAnimator para animar las letras.
/// </summary>
public class StartMenu : MonoBehaviour
{
    [Header("Paneles UI")]
    [SerializeField] private CanvasGroup panelStart;
    [SerializeField] private CanvasGroup panelConsent;
    [SerializeField] private CanvasGroup panelProfile;
    [SerializeField] private CanvasGroup panelSeleccion;
    [SerializeField] private CanvasGroup panelLevel;

    [Header("Profile Inputs")]
    [SerializeField] private TMP_InputField nameInput;
    [SerializeField] private Button skipNameButton;
    [SerializeField] private float profileTimeout = 10f;
    [SerializeField] private string defaultName = "Jugador 1";

    [Header("Consent UI")]
    [SerializeField] private TMP_Text consentText;
    [SerializeField] private Button acceptConsentButton;     // botón Aceptar
    [SerializeField] private Button cancelConsentButton;     // botón Cancelar (regresa a inicio)
    [SerializeField] private ConsentPanel consentAnimator; // nuevo componente

    [Header("Sounds")]
    [SerializeField] private AudioClip clickSound;
    [SerializeField] private AudioClip soundBack;

    [Header("Intro Audio")]
    [SerializeField] private AudioClip consentAudio;
    [SerializeField] private AudioClip profileAudio;
    [SerializeField] private AudioClip selectionAudio;

    private Coroutine profileTimer;

    public static bool OpenLevelOnLoad = false;

    #region Unity Callbacks

    private void Awake()
    {
        if (panelStart == null) Debug.LogError("StartMenu: panelStart no asignado.");
        if (panelConsent == null) Debug.LogError("StartMenu: panelConsent no asignado.");
        if (panelProfile == null) Debug.LogError("StartMenu: panelProfile no asignado.");
        if (panelSeleccion == null) Debug.LogError("StartMenu: panelSeleccion no asignado.");
        if (panelLevel == null) Debug.LogError("StartMenu: panelLevel no asignado.");
        if (nameInput == null) Debug.LogError("StartMenu: nameInput no asignado.");
        if (skipNameButton == null) Debug.LogError("StartMenu: skipNameButton no asignado.");
        if (acceptConsentButton == null) Debug.LogError("StartMenu: acceptConsentButton no asignado.");
        if (cancelConsentButton == null) Debug.LogError("StartMenu: cancelConsentButton no asignado.");
        if (consentAnimator == null && consentText == null) Debug.LogWarning("StartMenu: ni consentAnimator ni consentText asignados (se necesita al menos uno).");
    }

    private void Start()
    {
        // Conectar eventos
        skipNameButton.onClick.AddListener(OnSkipName);
        acceptConsentButton.onClick.AddListener(OnAcceptConsent);
        cancelConsentButton.onClick.AddListener(OnCancelConsent);

        // Si venimos desde un nivel y queremos abrir el selector de niveles directo
        if (OpenLevelOnLoad)
        {
            panelStart.alpha = 0f; panelStart.gameObject.SetActive(false);
            panelProfile.alpha = 0f; panelProfile.gameObject.SetActive(false);
            panelSeleccion.alpha = 0f; panelSeleccion.gameObject.SetActive(false);
            panelConsent.alpha = 0f; panelConsent.gameObject.SetActive(false);
            panelLevel.alpha = 1f; panelLevel.gameObject.SetActive(true);
            OpenLevelOnLoad = false;
            return;
        }

        // Inicializar paneles: solo panelStart visible
        panelStart.alpha = 1f; panelStart.gameObject.SetActive(true);
        panelProfile.alpha = 0f; panelProfile.gameObject.SetActive(false);
        panelSeleccion.alpha = 0f; panelSeleccion.gameObject.SetActive(false);
        panelLevel.alpha = 0f; panelLevel.gameObject.SetActive(false);
        panelConsent.alpha = 0f; panelConsent.gameObject.SetActive(false);

        // Si no hay animator, y consentText está vacío, ponemos un texto breve por defecto
        if (consentAnimator == null && consentText != null && string.IsNullOrWhiteSpace(consentText.text))
        {
            consentText.text = "Usa los bloques para crear una ruta y guiar \n"
                + "a tu personaje hasta el final del laberinto. \n\n"
                + "¡Prepárate para la aventura! \n\n"
                + "Pulsa Aceptar para seguir.";
        }
    }

    #endregion

    #region Panel Transitions

    /// <summary>
    /// Siempre muestra el panelConsent al pulsar Iniciar.
    /// </summary>
    public void OnIniciarButton()
    {
        PlayClick();

        // Iniciar transición con fade
        StartCoroutine(FadePanels.Instance.TransitionPanels(panelStart, panelConsent));

        AudioManager.Instance.PlaySound(consentAudio);

        // Iniciar animacion si existe el animator
        if (consentAnimator != null)
        {
            consentAnimator.StartAnimation();
        }
    }

    /// <summary>
    /// Aceptar: revela el texto completo (por si la animación sigue) y avanza a perfil.
    /// </summary>
    public void OnAcceptConsent()
    {
        AudioManager.Instance.StopSFX();

        PlayClick();

        // Revelar texto inmediatamente
        if (consentAnimator != null) consentAnimator.RevealFullText();

        if (profileTimer != null) StopCoroutine(profileTimer);

        // Iniciar transición con fade
        StartCoroutine(FadePanels.Instance.TransitionPanels(panelConsent, panelProfile));

        AudioManager.Instance.PlaySound(profileAudio);

        profileTimer = StartCoroutine(AutoSkipProfile());
    }

    /// <summary>
    /// Cancelar: revela texto completo y regresa al inicio.
    /// </summary>
    public void OnCancelConsent()
    {
        // Revelar texto por si la animacion estaba en curso
        if (consentAnimator != null) consentAnimator.RevealFullText();

        AudioManager.Instance.StopSFX();

        PlayBack();

        // Detener animacion (para no dejar corriendo)
        if (consentAnimator != null) consentAnimator.StopAnimation();

        // Iniciar transición con fade
        StartCoroutine(FadePanels.Instance.TransitionPanels(panelConsent, panelStart));
    }

    public void OnProfileNext()
    {
        AudioManager.Instance.StopSFX();

        PlayClick();

        if (profileTimer != null) StopCoroutine(profileTimer);
        string playerName = nameInput.text.Trim();
        if (string.IsNullOrEmpty(playerName)) playerName = defaultName;

        GameSessionManager.Instance.StartNewSession(playerName);

        // Iniciar transición con fade
        StartCoroutine(FadePanels.Instance.TransitionPanels(panelProfile, panelSeleccion));

        AudioManager.Instance.PlaySound(selectionAudio);
    }

    public void RegresarDeProfileAStart()
    {
        AudioManager.Instance.StopSFX();
        PlayBack();
        // Iniciar transición con fade
        StartCoroutine(FadePanels.Instance.TransitionPanels(panelProfile, panelStart));
    }

    public void RegresarDeSeleccionAProfile()
    {
        AudioManager.Instance.StopSFX();
        PlayBack();
        // Iniciar transición con fade
        StartCoroutine(FadePanels.Instance.TransitionPanels(panelSeleccion, panelProfile));
        AudioManager.Instance.PlaySound(profileAudio);
    }

    public void RegresarDeLevelASeleccion()
    {
        AudioManager.Instance.StopSFX();
        PlayBack();
        // Iniciar transición con fade
        StartCoroutine(FadePanels.Instance.TransitionPanels(panelLevel, panelSeleccion));
        AudioManager.Instance.PlaySound(selectionAudio);
    }

    public void ExitDesktop()
    {
        Application.Quit();
    }

    #endregion

    #region Profile Omission

    private IEnumerator AutoSkipProfile()
    {
        yield return new WaitForSeconds(profileTimeout);
        OnSkipName();
    }

    public void OnSkipName()
    {
        if (profileTimer != null) StopCoroutine(profileTimer);
        nameInput.text = defaultName;
        OnProfileNext();
    }

    #endregion

    #region Audio Methods
    private void PlayClick()
    {
        if (clickSound == null) return;
        if (AudioManager.Instance != null) AudioManager.Instance.PlaySound(clickSound);
        else
        {
            Vector3 pos = (Camera.main != null) ? Camera.main.transform.position : Vector3.zero;
            AudioSource.PlayClipAtPoint(clickSound, pos);
        }
    }

    private void PlayBack()
    {
        if (soundBack == null) return;
        if (AudioManager.Instance != null) AudioManager.Instance.PlaySound(soundBack);
        else
        {
            Vector3 pos = (Camera.main != null) ? Camera.main.transform.position : Vector3.zero;
            AudioSource.PlayClipAtPoint(soundBack, pos);
        }
    }

    #endregion
}
