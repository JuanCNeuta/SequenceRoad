using UnityEngine;
using UnityEngine.SceneManagement;

public class ExitToLevelSelect : MonoBehaviour
{
    [SerializeField] private string levelSelectSceneName = "StartMenu";
    [SerializeField] private AudioClip clickSound;

    public void OnExitToLevelSelect()
    {
        // Reproducir SFX vía AudioManager
        if (clickSound != null)
        {
            if (AudioManager.Instance != null)
                AudioManager.Instance.PlaySound(clickSound);
            else
            {
                Vector3 pos = (Camera.main != null) ? Camera.main.transform.position : Vector3.zero;
                AudioSource.PlayClipAtPoint(clickSound, pos);
            }
        }

        StartMenu.OpenLevelOnLoad = true;

        if (AccessibilityManager.Instance != null)
        {
            Debug.Log("[ExitToLevelSelect] Destruyendo AccessibilityManager para reiniciar accesibilidad.");
            Destroy(AccessibilityManager.Instance.gameObject);
        }

        SceneManager.LoadScene(levelSelectSceneName);
    }
}

