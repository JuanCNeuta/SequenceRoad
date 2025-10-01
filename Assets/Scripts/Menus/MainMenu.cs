using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenu : MonoBehaviour
{
    public void EscenaJuego()
    {
        SceneManager.LoadScene("Selection_Menu");
    }

    public void Salir()
    {
        Application.Quit();
    }
}
