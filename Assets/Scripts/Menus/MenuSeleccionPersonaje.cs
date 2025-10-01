using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement;

/// <summary>
/// Maneja la selección de personaje, con opción de auto-seleccionar tras timeout.
/// </summary>
public class MenuSeleccionPersonaje : MonoBehaviour
{
    [Header("Configuración")]
    [Tooltip("Tiempo en segundos para auto-seleccionar el primer personaje si no hay interacción.")]
    public float autoSelectTimeout = 10f;
    [SerializeField] private AudioClip clickSound;
    [SerializeField] private AudioClip SelectSound;

    private int index;

    [Header("Paneles UI")]
    [SerializeField] private CanvasGroup panelLevel;     // CanvasGroup del panel de nivel
    [SerializeField] private CanvasGroup panelSeleccion; // CanvasGroup del panel de selección

    [Header("Componentes UI")]
    [SerializeField] private Image image;
    [SerializeField] private TextMeshProUGUI nombre;

    [Header("Intro Audio")]
    [SerializeField] private AudioClip levelMenuAudio;

    private GameManagerMenu gameManager;
    private Coroutine autoSelectCoroutine;

    void Start()
    {
        gameManager = GameManagerMenu.instance;
        if (gameManager == null)
        {
            Debug.LogError("GameManagerMenu.instance es null.");
            return;
        }

        index = PlayerPrefs.GetInt("JugadorIndex", 0);
        index = Mathf.Clamp(index, 0, gameManager.personajes.Count - 1);
        ChangeScreen();

        // Iniciar contador para auto-selección
        autoSelectCoroutine = StartCoroutine(AutoSelectAfterTimeout());
    }

    /// <summary>
    /// Cambia la UI al personaje actual.
    /// </summary>
    private void ChangeScreen()
    {
        PlayerPrefs.SetInt("JugadorIndex", index);
        image.sprite = gameManager.personajes[index].imagen;
        nombre.text = gameManager.personajes[index].nombre;
    }

    /// <summary>
    /// Coroutine que espera y luego selecciona automáticamente el primer personaje.
    /// </summary>
    private IEnumerator AutoSelectAfterTimeout()
    {
        yield return new WaitForSeconds(autoSelectTimeout);
        // Auto-selecciona el personaje actual (índice 0 si nunca cambiaron)
        index = 0;
        ChangeScreen();
        SelectCharacter();
    }

    /// <summary>
    /// Avanza al siguiente personaje y reinicia timeout.
    /// </summary>
    public void NextCharacter()
    {
        // Reproducir SFX vía AudioManager
        if (clickSound != null)
        {
            if (AudioManager.Instance != null)
            {
                AudioManager.Instance.PlaySound(clickSound);
            }
            else
            {
                // Fallback: si no hay AudioManager, usamos PlayClipAtPoint
                Vector3 pos = (Camera.main != null) ? Camera.main.transform.position : Vector3.zero;
                AudioSource.PlayClipAtPoint(clickSound, pos);
            }
        }

        StopAutoSelect();
        index = (index + 1) % gameManager.personajes.Count;
        ChangeScreen();
        autoSelectCoroutine = StartCoroutine(AutoSelectAfterTimeout());
    }

    /// <summary>
    /// Regresa al personaje anterior y reinicia timeout.
    /// </summary>
    public void PreviousCharacter()
    {
        // Reproducir SFX vía AudioManager
        if (clickSound != null)
        {
            if (AudioManager.Instance != null)
            {
                AudioManager.Instance.PlaySound(clickSound);
            }
            else
            {
                // Fallback: si no hay AudioManager, usamos PlayClipAtPoint
                Vector3 pos = (Camera.main != null) ? Camera.main.transform.position : Vector3.zero;
                AudioSource.PlayClipAtPoint(clickSound, pos);
            }
        }

        StopAutoSelect();
        index = (index - 1 + gameManager.personajes.Count) % gameManager.personajes.Count;
        ChangeScreen();
        autoSelectCoroutine = StartCoroutine(AutoSelectAfterTimeout());
    }

    /// <summary>
    /// Selecciona el personaje y guarda en sesión, luego realiza la transición con fade a panelLevel.
    /// </summary>
    public void SelectCharacter()
    {
        AudioManager.Instance.StopSFX();
        // Reproducir SFX vía AudioManager
        if (SelectSound != null)
        {
            if (AudioManager.Instance != null)
            {
                AudioManager.Instance.PlaySound(SelectSound);
            }
            else
            {
                // Fallback: si no hay AudioManager, usamos PlayClipAtPoint
                Vector3 pos = (Camera.main != null) ? Camera.main.transform.position : Vector3.zero;
                AudioSource.PlayClipAtPoint(SelectSound, pos);
            }
        }

        StopAutoSelect();
        GameSessionManager.Instance.SelectedCharacterIndex = index;

        if (FadePanels.Instance != null)
        {
            // Iniciar transición con fade de panelSeleccion a panelLevel
            StartCoroutine(FadePanels.Instance.TransitionPanels(panelSeleccion, panelLevel));
            AudioManager.Instance.PlaySound(levelMenuAudio);
        }
        else
        {
            Debug.LogError("FadePanels.Instance es nulo. Asegúrate de que el script FadePanels esté en la escena.");
            // Fallback: activar/desactivar sin fade
            panelSeleccion.gameObject.SetActive(false);
            panelLevel.gameObject.SetActive(true);
        }
    }

    /// <summary>
    /// Detiene la coroutine de auto-selección si existe.
    /// </summary>
    private void StopAutoSelect()
    {
        if (autoSelectCoroutine != null)
            StopCoroutine(autoSelectCoroutine);
    }
}
