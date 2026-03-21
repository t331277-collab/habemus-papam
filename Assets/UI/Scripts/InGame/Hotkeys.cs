using UnityEngine;
using UnityEngine.SceneManagement;

public class Hotkeys : MonoBehaviour
{
    public void OnClickMainMenu()
    {
        Time.timeScale = 1f;
        if (SaveManager.Instance != null)
        {
            SaveManager.Instance.GoToMainMenu();
            return;
        }

        SceneManager.LoadScene("MainScene");
    }

    public void OnClickExit()
    {
        Time.timeScale = 1f;
        Application.Quit();

#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#endif
    }
    
}
