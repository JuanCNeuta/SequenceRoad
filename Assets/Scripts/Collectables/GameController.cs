using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.Collections;

public class GameController : MonoBehaviour
{
    
    public MazeGenerator mazeGenerator;
    public ItemSlot itemSlot;

    [Header("SFX")]
    [SerializeField] private AudioClip clickSound;
    [SerializeField] private AudioClip nextLevelSound;

    void Start()
    {
    }

    public void ResetGame()
    {
        if (mazeGenerator.character != null)
        {
            mazeGenerator.character.transform.position = new Vector3(0, 2, 0);
        }

        CollectibleManager.Instance.ResetManager();

        mazeGenerator.ClearCollectibles();
        mazeGenerator.ResetCollectibles();

        itemSlot.ClearArrows();
    }

    public void ClearGame()
    {
        // Reproducir SFX(si existe AudioManager y clip asignado)
        if (clickSound != null && AudioManager.Instance != null)
        {
            AudioManager.Instance.PlaySound(clickSound);
        }
        else if (clickSound != null)
        {
            // Fallback si no hay AudioManager
            Vector3 pos = (Camera.main != null) ? Camera.main.transform.position : Vector3.zero;
            AudioSource.PlayClipAtPoint(clickSound, pos);
        }
        itemSlot.ClearArrows();
    }


    public void RestartLevel()
    {
        // Reproducir SFX (si existe AudioManager y clip asignado)
        if (clickSound != null && AudioManager.Instance != null)
        {
            AudioManager.Instance.PlaySound(clickSound);
        }
        else if (clickSound != null)
        {
            // Fallback si no hay AudioManager
            Vector3 pos = (Camera.main != null) ? Camera.main.transform.position : Vector3.zero;
            AudioSource.PlayClipAtPoint(clickSound, pos);
        }

        // Deshacer la puntuación que se añadió al terminar este nivel
        string scene = SceneManager.GetActiveScene().name;
        GameSessionManager.Instance.UndoLevelScore(scene);

        // Reiniciar estado de collectibles y recargar nivel
        CollectibleManager.Instance.ResetManager();
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    public void UnlockNewLevel()
    {
        // Obtiene el nivel actual en índice de escena
        int current = SceneManager.GetActiveScene().buildIndex;
        int unlocked = PlayerPrefs.GetInt("UnlockedLevel", 1);

        // Si acabas un nivel que es igual o mayor al último desbloqueado,
        // desbloquea el siguiente.
        if (current >= unlocked)
        {
            PlayerPrefs.SetInt("UnlockedLevel", unlocked + 1);
            PlayerPrefs.Save();
        }
    }

    public void NextLevel()
    {
        // Reproducir SFX (si existe AudioManager y clip asignado)
        if (nextLevelSound != null && AudioManager.Instance != null)
        {
            AudioManager.Instance.PlaySound(nextLevelSound);
        }
        else
        {
            // Fallback si no hay AudioManager
            Vector3 pos = (Camera.main != null) ? Camera.main.transform.position : Vector3.zero;
            AudioSource.PlayClipAtPoint(nextLevelSound, pos);
        }
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex + 1);
    }
}