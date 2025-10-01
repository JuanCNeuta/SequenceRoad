using UnityEngine;

public class InicioJugador : MonoBehaviour
{
    void Start()
    {
        int indexJugador = PlayerPrefs.GetInt("JugadorIn");
        Instantiate(GameManagerMenu.instance.personajes[indexJugador].personajeJugable, transform.position, Quaternion.identity);
    }

}
