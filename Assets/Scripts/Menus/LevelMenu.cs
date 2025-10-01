using UnityEngine;
using UnityEngine.UI;

public class LevelMenu : MonoBehaviour
{
    [Tooltip("Lista de botones de nivel (en orden: botón[0] = Nivel 1, etc.)")]
    public Button[] buttons;

    [Header("SFX")]
    [Tooltip("Sonido que se reproduce al seleccionar un nivel")]
    public AudioClip clickSound;

    private void Awake()
    {
        int unlockedLevel = PlayerPrefs.GetInt("UnlockedLevel", 1);
        Debug.Log($"[LevelMenu] Niveles desbloqueados hasta: {unlockedLevel}");

        for (int i = 0; i < buttons.Length; i++)
        {
            var btn = buttons[i];
            var lockGO = btn.transform.Find("LockIcon")?.gameObject;
            if (lockGO == null)
            {
                Debug.LogWarning($"[LevelMenu] {btn.name} no tiene child 'LockIcon'");
                continue;
            }

            bool isUnlocked = (i + 1) <= unlockedLevel;
            lockGO.SetActive(!isUnlocked);
            btn.interactable = isUnlocked;
            btn.onClick.RemoveAllListeners();

            if (isUnlocked)
            {
                int levelId = i + 1;
                btn.onClick.AddListener(() =>
                {
                    // Reproducir SFX (si existe AudioManager y clip asignado)
                    if (clickSound != null && AudioManager.Instance != null)
                    {
                        AudioManager.Instance.PlaySound(clickSound);
                    }
                    else
                    {
                        // Fallback si no hay AudioManager
                        Vector3 pos = (Camera.main != null) ? Camera.main.transform.position : Vector3.zero;
                        AudioSource.PlayClipAtPoint(clickSound, pos);
                    }

                    OpenLevelByFlow(levelId);
                });
            }
        }
    }

    private void OpenLevelByFlow(int levelId)
    {
        int seqIndex;
        switch (levelId)
        {
            case 1:
                seqIndex = 0;  // MazeTutorial1
                break;
            case 2:
                seqIndex = 2;  // MazeTutorial2
                break;
            case 3:
                seqIndex = 4;  // MazeLevel3
                break;
            case 4:
                seqIndex = 5;  // MazeLevel4
                break;
            default:
                Debug.LogError($"[LevelMenu] levelId inválido: {levelId}");
                return;
        }

        Debug.Log($"[LevelMenu] Cargando secuencia índice {seqIndex}");
        GameFlowManager.I.LoadSceneBySequenceIndex(seqIndex);
    }
}

