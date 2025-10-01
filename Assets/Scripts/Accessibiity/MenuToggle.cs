using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class MenuToggle : MonoBehaviour
{
    bool currentState;

    [SerializeField] UnityEvent turnedOn;
    [SerializeField] UnityEvent turnedOff;

    [SerializeField] float autoHideDelay = 5f; // segundos para autoocultar

    Coroutine hideCoroutine;

    public void ToggleState()
    {
        currentState = !currentState;

        if (currentState)
            TurnOn();
        else
            TurnOff();
    }

    public void TurnOn()
    {
        currentState = true;
        turnedOn.Invoke();

        // Inicia autocierre
        if (hideCoroutine != null)
            StopCoroutine(hideCoroutine);

        hideCoroutine = StartCoroutine(AutoHide());
    }

    public void TurnOff()
    {
        currentState = false;
        turnedOff.Invoke();

        // Detiene autocierre si se cierra manualmente
        if (hideCoroutine != null)
            StopCoroutine(hideCoroutine);
    }

    IEnumerator AutoHide()
    {
        yield return new WaitForSeconds(autoHideDelay);
        TurnOff();
    }
}