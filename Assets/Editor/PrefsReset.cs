#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;

// Este metodo aparece en el menu “Tools/Reset PlayerPrefs”
public static class PrefsReset
{
    [MenuItem("Tools/Reset PlayerPrefs %#r")]  // Ctrl/Cmd + Shift + R
    public static void DeletePlayerPrefs()
    {
        PlayerPrefs.DeleteKey("UnlockedLevel");
        PlayerPrefs.DeleteAll();  // si quieres borrar todo
        PlayerPrefs.Save();
        Debug.Log(" PlayerPrefs reseteados.");
    }
}
#endif
