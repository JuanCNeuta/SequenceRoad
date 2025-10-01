using UnityEngine;
using UnityEngine.SocialPlatforms.Impl;
using System;

public class CollectableTutorial : MonoBehaviour
{
    // Evento que se disparar cuando el personaje toca el coleccionable
    public static event Action OnCharacterTouchCollectable;
    void Start()
    {
        
    }

    void Update()
    {
        
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Player"))
            return;

        OnCharacterTouchCollectable?.Invoke();
        gameObject.SetActive(false);
    }
}
