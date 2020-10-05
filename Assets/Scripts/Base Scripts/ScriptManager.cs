using UnityEngine;

public class ScriptManager : MonoBehaviour
{
    #region Public Variables
    // Configurações gerais
    [Header("Settings")]
    public int targetFramerate;
    #endregion

    #region Unity Methods
    private void Awake()
    {
        // Impede que duas intâncias da classe existam ao mesmo tempo (Singleton)
        if (GameObject.FindGameObjectsWithTag("Script Manager").Length == 1)
        {
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void Start()
    {
        // Define o framerate alvo em cada plataforma
        #if UNITY_EDITOR
        Application.targetFrameRate = 0;
        #elif UNITY_ANDROID
        Application.targetFrameRate = targetFramerate;
        #endif
    }
    #endregion
}