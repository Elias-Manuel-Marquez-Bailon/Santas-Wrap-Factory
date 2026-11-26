using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// Menú principal: Jugar (nivel 1) y Salir.
/// </summary>
public class MenuUI : MonoBehaviour
{
    public string gameSceneName = "Factory";

    public void Play()
    {
        GameManager.SelectedLevel = 0;
        SceneManager.LoadScene(gameSceneName);
    }

    public void Quit()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }
}
