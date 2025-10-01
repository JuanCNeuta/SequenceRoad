using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class UIAnimation : MonoBehaviour
{

    [SerializeField]
    float duration;
    [SerializeField]
    float delay;

    [SerializeField]
    AnimationCurve animationCurve;
    [SerializeField]
    RectTransform target;

    [SerializeField]
    Vector2 startingPoint;
    [SerializeField]
    Vector2 endingPoint;

    public void FadeIn()
    {
        StopAllCoroutines();
        StartCoroutine(FadeInCourutineMenuDropDown(startingPoint,endingPoint));
    }

    public void FadeOut()
    {
        StopAllCoroutines();
        StartCoroutine(FadeInCourutineMenuDropDown(endingPoint, startingPoint));
    }

    IEnumerator FadeInCourutineMenuDropDown(Vector2 a, Vector2 b)
    {
        Vector2 startingPoint=a;
        Vector2 endingPoint=b;
        float elapsed = 0;
        while (elapsed <= delay)
        {
            elapsed += Time.deltaTime;
            yield return null;
        }

        elapsed = 0;
        while(elapsed <= duration)
        {
            float percentage= elapsed / duration;
            float curvePercentage = animationCurve.Evaluate(percentage);
            elapsed += Time.deltaTime;
            Vector2 currentPosition=Vector2.LerpUnclamped(startingPoint, endingPoint, curvePercentage);
            target.anchoredPosition = currentPosition;
            yield return null;
        }

        target.anchoredPosition = endingPoint;
    }
    
    
}
