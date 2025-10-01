using LottiePlugin.UI;
using UnityEngine;
using LottiePlugin;

public class WinnerPanelController : MonoBehaviour
{
    public AnimatedImage animacionConfeti;

    // Esta funci�n se llama autom�ticamente cuando el GameObject se activa.
    void OnEnable()
    {
        // Aseg�rate de que la animaci�n est� asignada para evitar errores
        if (animacionConfeti != null)
        {
            // Reproduce la animaci�n desde el principio
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
