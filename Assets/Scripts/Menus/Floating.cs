using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(RectTransform))]
public class Floating : MonoBehaviour
{
    public float floatHeight = 20f;
    public float floatSpeed = 2f;
    public float rotateSpeed = 20f;
    private Vector2 startPos;

    void Start()
    {
        startPos = GetComponent<RectTransform>().anchoredPosition;
    }

    void Update()
    {
        float t = Mathf.PingPong(Time.time * floatSpeed, 1f);
        var rt = GetComponent<RectTransform>();
        rt.anchoredPosition = startPos + Vector2.up * (t * floatHeight);
        rt.Rotate(0, 0, rotateSpeed * Time.deltaTime);
    }
}

