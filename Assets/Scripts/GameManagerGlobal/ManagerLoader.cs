using UnityEngine;

public class NewMonoBehaviourScript : MonoBehaviour
{
    public GameObject globalManagerPrefab;

    void Awake()
    {
        if (GameObject.Find("GameManagerGlobal") == null)
        {
            GameObject global = Instantiate(globalManagerPrefab);
            global.name = "GameManagerGlobal";
            DontDestroyOnLoad(global);
        }
    }
}
