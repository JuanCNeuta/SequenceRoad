using LottiePlugin.UI;
using UnityEngine;
using LottiePlugin;

public class WinnerPanelController : MonoBehaviour
{
    public AnimatedImage animacionConfeti;

    // Esta función se llama automáticamente cuando el GameObject se activa.
    void OnEnable()
    {
        // Asegúrate de que la animación esté asignada para evitar errores
        if (animacionConfeti != null)
        {
            // Reproduce la animación desde el principio
            animacionConfeti.Play();
        }
    }

    public void ReproducirAnimacion()
    {
        if (animacionConfeti != null)
        {
            animacionConfeti.Play();
        }
    }
}
