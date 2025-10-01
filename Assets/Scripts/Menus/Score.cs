// Score.cs
using UnityEngine;
using TMPro;
using UnityEngine.SceneManagement;

public class Score : MonoBehaviour
{
    private float _score;
    private TextMeshProUGUI _textMesh;
    private bool _finished = false;

    public float CurrentScore => _score;
    public bool HasFinished => _finished;

    void Start()
    {
        _textMesh = GetComponent<TextMeshProUGUI>();
    }

    void Update()
    {
        _textMesh.text = _score.ToString("0");
    }

    public void AddPoints(float inScore)
    {
        _score += inScore;
    }

    public void FinishLevel()
    {
        if (_finished) return;
        _finished = true;

        // Uso del nombre de la escena para el diccionario
        string scene = SceneManager.GetActiveScene().name;
        GameSessionManager.Instance.AddOrUpdateLevelScore(scene, _score);
    }
}

