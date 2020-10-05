using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneManager_TestOnly : MonoBehaviour
{
    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (SceneManager.GetActiveScene().buildIndex == 0)
            {
                #if UNITY_EDITOR
                UnityEditor.EditorApplication.isPlaying = false;
                #else
                Application.Quit();   
                #endif
            }
            else
            {
                SceneManager.LoadScene(0);
            }
        }
    }

    public void StartGame()
    {
        SceneManager.LoadScene(1);
    }
}