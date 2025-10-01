// GameSessionManager.cs
using System.Collections.Generic;
using UnityEngine;

public class GameSessionManager : MonoBehaviour
{
    public static GameSessionManager Instance { get; private set; }
    public int SelectedCharacterIndex { get; set; }

    public string PlayerName { get; private set; }
    // Ahora un diccionario por nombre de nivel
    private Dictionary<string, float> _levelScores;
    public float TotalScore { get; private set; }

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);

        _levelScores = new Dictionary<string, float>();
        TotalScore = 0f;
    }

    public void StartNewSession(string playerName)
    {
        PlayerName = playerName;
        _levelScores.Clear();
        TotalScore = 0f;
    }

    /// <summary>
    /// Si el nivel ya existía, resta primero su antiguo valor.
    /// Luego guarda (o sobreescribe) y ajusta TotalScore.
    /// </summary>
    public void AddOrUpdateLevelScore(string levelName, float levelScore)
    {
        if (_levelScores.ContainsKey(levelName))
        {
            TotalScore -= _levelScores[levelName];
        }
        
        _levelScores[levelName] = levelScore;
        TotalScore += levelScore;

        Debug.Log($"[Session] Nivel «{levelName}» = {levelScore}. Total ahora: {TotalScore}");
    }

    public void UndoLevelScore(string levelName)
    {
        if (_levelScores.TryGetValue(levelName, out float old))
        {
            TotalScore -= old;
            _levelScores.Remove(levelName);
            Debug.Log($"[Session] Quitado nivel «{levelName}» ({old}). Total ahora: {TotalScore}");
        }
    }
}

