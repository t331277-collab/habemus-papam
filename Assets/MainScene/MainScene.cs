using UnityEngine;

public class MainScene : MonoBehaviour
{
    public void OnClickStartGame()
    {
        if (SaveManager.Instance != null)
        {
            SaveManager.Instance.StartNewGame();
        }
    }

    public void OnClickLoad()
    {
        if (SaveManager.Instance != null)
        {
            SaveManager.Instance.LoadGame();
        }
    }
}
