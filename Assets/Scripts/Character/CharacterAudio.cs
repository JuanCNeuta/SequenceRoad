using UnityEngine;

public class CharacterAudio : MonoBehaviour
{
    [Header("Sonidos de personaje")]
    public AudioClip touchObstacleClip;
    public AudioClip HappyClip;

    public void PlayTouchObstacleSound()
    {
        if (touchObstacleClip != null)
        {
            AudioManager.Instance?.PlaySound(touchObstacleClip);
        }
        else
        {
            Debug.LogWarning("No hay sonido asignado para colisión en " + gameObject.name);
        }
    }

    public void PlaySoundHappy()
    {
        if (touchObstacleClip != null)
        {
            AudioManager.Instance?.PlaySound(HappyClip);
        }
        else
        {
            Debug.LogWarning("No hay sonido asignado para Character Happy en " + gameObject.name);
        }
    }

}
