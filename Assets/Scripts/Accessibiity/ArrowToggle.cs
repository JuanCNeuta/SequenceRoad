using UnityEngine;
using System.Collections;

public class ArrowToggle : MonoBehaviour
{
    [SerializeField] private GameObject arrowIndicator;
    [SerializeField] private float animationDuration = 1f;
    [SerializeField] private string stateKey;

    private bool isOff = false;
    private RectTransform arrowRect;
    private Vector2 originalSize;

    public string StateKey => string.IsNullOrEmpty(stateKey) ? gameObject.name : stateKey;

    private void Awake()
    {
        if (arrowIndicator != null)
        {
            arrowRect = arrowIndicator.GetComponent<RectTransform>();
            originalSize = (arrowRect != null) ? arrowRect.sizeDelta : Vector2.zero;
        }

        if (string.IsNullOrEmpty(stateKey))
            stateKey = gameObject.name;
    }

    public void ToggleArrow()
    {
        isOff = !isOff;
        ApplyArrowState(isOff);
        AccessibilityManager.Instance?.SetArrowState(StateKey, isOff);
        Debug.Log($"[ArrowToggle] {StateKey} Toggle -> {isOff}");
    }

    private void ApplyArrowState(bool show)
    {
        if (arrowIndicator == null) return;

        StopAllCoroutines();
        arrowIndicator.SetActive(show);

        if (show)
        {
            // Si el objeto y componente están activos, animamos; si no, ponemos estado final
            if (isActiveAndEnabled && gameObject.activeInHierarchy && arrowRect != null)
            {
                StartCoroutine(AnimateReveal());
            }
            else
            {
                if (arrowRect != null) arrowRect.sizeDelta = originalSize;
            }
        }
        else
        {
            if (arrowRect != null) arrowRect.sizeDelta = new Vector2(0f, originalSize.y);
        }
    }

    private IEnumerator AnimateReveal()
    {
        if (arrowRect == null) yield break;

        float elapsed = 0f;
        Vector2 currentSize = new Vector2(0f, originalSize.y);
        arrowRect.sizeDelta = currentSize;

        while (elapsed < animationDuration)
        {
            float t = elapsed / animationDuration;
            currentSize.x = Mathf.Lerp(0f, originalSize.x, t);
            arrowRect.sizeDelta = currentSize;
            elapsed += Time.deltaTime;
            yield return null;
        }
        arrowRect.sizeDelta = originalSize;
    }

    public void SetArrow(bool show)
    {
        isOff = show;
        ApplyArrowState(show);
    }
}
