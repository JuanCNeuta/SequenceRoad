using UnityEngine;

public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance;

    //Se usa para controlar si se para o no la musica
    private bool isMusicOn = true;

    // Se usa para controlar que ningun sonido se reproduzca
    private bool isAudioEnabled = true;

    //Se usa para controlar el audio de las voces tanto del narrador, como del personaje
    private bool isSFXEnabled = true;

    private AudioSource musicSource;
    private AudioSource sfxSource;

    [Header("Música de fondo")]
    public AudioClip backgroundMusic;
    [Range(0f, 1f)]
    public float musicVolume = 0.5f;


    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);

            musicSource = gameObject.AddComponent<AudioSource>();
            sfxSource = gameObject.AddComponent<AudioSource>();

            musicSource.loop = true;
            musicSource.playOnAwake = false;
            musicSource.volume = musicVolume;

            sfxSource.playOnAwake = false;
            sfxSource.loop = false;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void Start()
    {
        if (backgroundMusic != null)
        {
            PlaySound(backgroundMusic, true); // reproducir con loop en la música
        }
    }

    public void PlaySound(AudioClip clip, bool loop = false)
    {
        if (clip != null && isAudioEnabled)
        {
            if (clip == backgroundMusic)
            {
                musicSource.clip = clip;
                musicSource.loop = loop;
                musicSource.volume = musicVolume;
                musicSource.Play();
            }
            else
            {
                sfxSource.volume = 0.8f;
                sfxSource.PlayOneShot(clip);
            }
        }
    }

    public void ToggleMusic()
    {
        if (isMusicOn)
        {
            musicSource.Pause();
        }
        else
        {
            musicSource.UnPause();
        }

        isMusicOn = !isMusicOn;
    }

    public void ToggleAllAudio()
    {
        isAudioEnabled = !isAudioEnabled;

        // Música (pausar o reanudar, pero no reiniciar)
        if (isAudioEnabled)
        {
            musicSource.UnPause();
        }
        else
        {
            musicSource.Pause();
        }

        // Silenciar ambos canales
        musicSource.mute = !isAudioEnabled;
        sfxSource.mute = !isAudioEnabled;
    }

    public void ToggleSFX()
    {
        isSFXEnabled = !isSFXEnabled;
        sfxSource.mute = !isSFXEnabled;

        // Si se desactivan los SFX, detener cualquier audio reproduciéndose
        if (!isSFXEnabled)
        {
            StopSFX();
        }
    }

    public bool GetMusicState() => isMusicOn;
    public bool GetSFXState() => isSFXEnabled;
    public bool GetAllAudioState() => isAudioEnabled;

    public void SetMusicState(bool on)
    {
        if (musicSource == null) return;
        if (on)
        {
            // reanudar o reproducir
            if (musicSource.clip != null && !musicSource.isPlaying) musicSource.Play();
            musicSource.UnPause();
        }
        else
        {
            musicSource.Pause();
        }
        isMusicOn = on;
    }

    public void SetSFXState(bool enabled)
    {
        isSFXEnabled = enabled;
        if (sfxSource != null)
        {
            sfxSource.mute = !isSFXEnabled;
            // Si se desactivan, detener audio actual
            if (!isSFXEnabled) StopSFX();
        }
    }

    public void SetAllAudioState(bool enabled)
    {
        isAudioEnabled = enabled;
        if (musicSource != null)
        {
            if (enabled) musicSource.UnPause(); else musicSource.Pause();
            musicSource.mute = !enabled;
        }
        if (sfxSource != null) sfxSource.mute = !enabled;
    }

    /// <summary>
    /// Detiene todos los efectos de sonido.
    /// </summary>
    public void StopSFX()
    {
        if (sfxSource != null)
        {
            sfxSource.Stop();
        }
    }

    /// <summary>
    /// Verifica si hay algún SFX reproduciéndose.
    /// </summary>
    public bool IsSFXPlaying()
    {
        return sfxSource != null && sfxSource.isPlaying;
    }
}