using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneLoader : MonoBehaviour
{
    public void LoadOpening()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene("Opening");
    }

    public void LoadHow()      => SceneManager.LoadScene("How");      // 방법 설명 씬 로드
    public void LoadMain()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene("Main");
    }

    public void LoadSetting()
    {
        SceneManager.LoadScene("Setting");
    }
}