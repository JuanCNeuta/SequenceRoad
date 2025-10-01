using System.Collections;
using UnityEngine;
using UnityEngine.TextCore.Text;

public class CollectibleKeys : MonoBehaviour
{
    private GameObject character;

    public AudioClip[] keySounds;
    public AudioClip openDoor;
    [SerializeField] private GameObject panelKey;
    private CanvasGroup canvasGroupKeys;

    private GameObject linkedObstacle;

    private void Start()
    {
        if (panelKey == null)
        {
            Debug.LogError("panelKey no está asignado.");
            return;
        }

        canvasGroupKeys = panelKey.GetComponent<CanvasGroup>();
        if (canvasGroupKeys == null)
        {
            canvasGroupKeys = panelKey.AddComponent<CanvasGroup>();
        }

        canvasGroupKeys.alpha = 0f;
        panelKey.SetActive(false);

        if (MazeGenerator.Instance != null)
        {
            if (MazeGenerator.Instance.ObstacleLinkedToCollectible(gameObject, out GameObject obstacle))
            {
                linkedObstacle = obstacle;
            }
        }

        //Codigo para obtener al animator del personaje para poder hacer la animacion de feliz
        character = FindFirstObjectByType<MazeGenerator>()?.character;

        if (character == null)
        {
            Debug.LogError("No se encontró el personaje en la escena.");
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            PlayRandomSound();
            StartCoroutine(HandleKeyPickup());
        }
    }

    private void PlayRandomSound()
    {
        if (keySounds != null && keySounds.Length > 0)
        {
            int randomIndex = Random.Range(0, keySounds.Length);
            AudioManager.Instance?.PlaySound(keySounds[randomIndex]);
        }
    }

    private IEnumerator HandleKeyPickup()
    {
        float fadeDuration = 0.5f;
        float visibleTime = 1.0f;

        CollectibleManager.Instance.SetCharacterCanMove(false);

        panelKey.SetActive(true);
        canvasGroupKeys.alpha = 0f;

        for (float t = 0; t < fadeDuration; t += Time.deltaTime)
        {
            canvasGroupKeys.alpha = Mathf.Lerp(0f, 1f, t / fadeDuration);
            yield return null;
        }
        canvasGroupKeys.alpha = 1f;

        yield return new WaitForSeconds(visibleTime);

        for (float t = 0; t < fadeDuration; t += Time.deltaTime)
        {
            canvasGroupKeys.alpha = Mathf.Lerp(1f, 0f, t / fadeDuration);
            yield return null;
        }
        canvasGroupKeys.alpha = 0f;
        panelKey.SetActive(false);

        // Activar animación de saltar feliz
        CharacterMovement characterMovement = character.GetComponent<CharacterMovement>();
        if (characterMovement != null)
        {
            characterMovement.PlayVictoryAnimation(2f);
        }

        yield return new WaitForSeconds(1.0f);

        if (linkedObstacle != null)
        {
            OpenDoor door = linkedObstacle.GetComponent<OpenDoor>();
            if (door != null)
            {
                if (Mathf.Approximately(door.angle, 0.0f))
                {
                    door.Open(90f);
                }
                else if (Mathf.Approximately(door.angle, 130f))
                {
                    door.Open(180f);
                }
            }
        }

        //Sonido para saber que se abrio una puerta
        AudioManager.Instance?.PlaySound(openDoor);

        CollectibleManager.Instance.SetCharacterCanMove(true);
        //Destroy(gameObject);
        gameObject.SetActive(false);
    }

}