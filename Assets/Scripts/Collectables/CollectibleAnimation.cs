using UnityEngine;
using System.Collections;

public class CollectibleAnimation : MonoBehaviour
{
    [SerializeField] private float minScale = 0.8f;
    [SerializeField] private float maxScale = 1.2f;
    [SerializeField] private float duration = 1.5f;

    private Vector3 originalScale;

    void Start()
    {
        originalScale = transform.localScale;
        StartCoroutine(AnimateScale());
    }

    private IEnumerator AnimateScale()
    {
        while (true)
        {
            yield return ScaleObject(minScale, duration / 2);
            yield return ScaleObject(maxScale, duration / 2);
        }
    }

    private IEnumerator ScaleObject(float targetScale, float time)
    {
        Vector3 startScale = transform.localScale;
        Vector3 endScale = originalScale * targetScale;
        float elapsed = 0f;

        while (elapsed < time)
        {
            transform.localScale = Vector3.Lerp(startScale, endScale, elapsed / time);
            elapsed += Time.deltaTime;
            yield return null;
        }

        transform.localScale = endScale;
    }
}
