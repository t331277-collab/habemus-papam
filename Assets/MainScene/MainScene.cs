using UnityEngine;
using UnityEngine.SceneManagement;
public class MainScene : MonoBehaviour
{
    public void OnClickStartGame()
    {
        SceneManager.LoadScene("GameScene");
    }

    public void OnClickLoad()
    {

    }
}
