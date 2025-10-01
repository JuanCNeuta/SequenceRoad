using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameFlowManager : MonoBehaviour
{
    public static GameFlowManager I { get; private set; }

    // Índices de Build Settings (omitimos 0, que es el menú)
    // 1: MazeTutorial1
    // 2: MazeLevel1
    // 3: MazeTutorial2
    // 4: MazeLevel2
    // 5: MazeLevel3
    // 6: MazeLevel4
    private List<int> sceneSequence = new List<int> { 1, 2, 3, 4, 5, 6 };

    // Posición actual en sceneSequence
    private int currentSeqIndex = -1;

    private void Awake()
    {
        if (I == null)
        {
            I = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    /// <summary>
    /// Carga la escena indicada por su índice en la lista sequence.
    /// </summary>
    public void LoadSceneBySequenceIndex(int seqIndex)
    {
        if (seqIndex < 0 || seqIndex >= sceneSequence.Count)
        {
            Debug.LogError($"[GameFlow] seqIndex {seqIndex} fuera de rango");
            return;
        }

        currentSeqIndex = seqIndex;
        SceneManager.LoadScene(sceneSequence[currentSeqIndex]);
    }

    /// <summary>
    /// Avanza automáticamente al siguiente tutorial/nivel. 
    /// Si se acaba la lista, vuelve al menú (buildIndex 0).
    /// </summary>
    public void LoadNextInSequence()
    {
        currentSeqIndex++;
        if (currentSeqIndex >= sceneSequence.Count)
        {
            Debug.Log("[GameFlow] Final de secuencia, volviendo al menú.");
            SceneManager.LoadScene(0);
            currentSeqIndex = -1;
        }
        else
        {
            SceneManager.LoadScene(sceneSequence[currentSeqIndex]);
        }
    }
}

