using NUnit.Framework;
using System.Collections.Generic;
using UnityEngine;

public class GameManagerMenu : MonoBehaviour
{
    public static GameManagerMenu instance;

    public List<Characters> personajes;

    private void Awake()
    {
        if (GameManagerMenu.instance == null)
        {
            GameManagerMenu.instance = this;
            DontDestroyOnLoad(this.gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }
}
