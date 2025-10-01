using UnityEngine;
using TMPro;

public class Timer : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI timerText;

    private float elapsedTime = 0f;

    void Start()
    {
        elapsedTime = 0f; // Iniciar en cero
    }

    void Update()
    {
        elapsedTime += Time.deltaTime;

        int minutes = Mathf.FloorToInt(elapsedTime / 60);
        int seconds = Mathf.FloorToInt(elapsedTime % 60);
        timerText.text = string.Format("{0:00}:{1:00}", minutes, seconds);
    }

    public void StopTimer()
    {
        enabled = false;
    }

    /// <summary>
    /// Tiempo transcurrido (segundos) desde el inicio.
    /// </summary>
    public float ElapsedTime => elapsedTime;

}