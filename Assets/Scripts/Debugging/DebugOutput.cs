using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Profiling;

public class DebugOutput : MonoBehaviour
{
    #region Public Variables
    #region Editor Acessible
    // Output de preferência
    public DebugMode debugMode;

    // Tempo de atualização do sistema
    public float updateTime;

    // Acesso ao Fall Manager
    public FallManager fallManager;

    // Acesso ao jogador
    public PlayerControls player;
    #endregion

    // Numerador do modo de output
    public enum DebugMode
    {
        Fps,
        PaceFactor,
        Lives,
        TimeScale,
        Memory_MonoUsedSize,
        Memory_TotalAllocated
    }
    #endregion

    #region Private Variables
    // Armazenamento da coroutine do sistema
    private Coroutine coroutine;

    // Textos de output
    private Text text;

    // Tempo de espera (mesmo que o tempo de atualização)
    public WaitForSecondsRealtime waitTime;
    #endregion

    #region Unity Methods
    void Start()
    {
        // Acesso ao text e inicialização da coroutine
        text = gameObject.GetComponent<Text>();
        coroutine = StartCoroutine(Debugger());

        // Definição do tempo de espera
        waitTime = new WaitForSecondsRealtime(updateTime);
    }
    #endregion

    #region FrameRateUpdate
    // Output
    IEnumerator Debugger()
    {
        while (true)
        {
            switch(debugMode)
            {
                case DebugMode.Fps:
                    text.text = Mathf.Floor(1F / Time.unscaledDeltaTime) + " FPS";
                    break;
                case DebugMode.PaceFactor:
                    text.text = "PF: " + fallManager.paceFactor.ToString("#.000");
                    break;
                case DebugMode.Lives:
                    text.text = "Vidas: " + player.lives;
                    break;
                case DebugMode.TimeScale:
                    text.text = "TS: " + Time.timeScale.ToString("#.000");
                    break;
                case DebugMode.Memory_MonoUsedSize:
                    text.text = "M_MUS: " + (Profiler.GetMonoUsedSizeLong() / 1000) + " KB";
                    break;
                case DebugMode.Memory_TotalAllocated:
                    text.text = "M_TAM: " + (Profiler.GetTotalAllocatedMemoryLong() / 1000000) + " MB";
                    break;
            }

            yield return waitTime;
        }
    }
    #endregion
}
