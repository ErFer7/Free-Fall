using System.Collections;
using UnityEngine;

public class SlowMotionManager : MonoBehaviour
{
    #region Public Variables
    // Acesso ao jogador
    public GameObject player;

    // Acesso ao Path Generator
    public PathGenerator pathGenerator;

    // Escala de tempo objetiva
    public float slowMotionTimeScale;

    // Tempo em slow motion
    public float slowMotionTime;

    // Intensidade da transição
    public float transitionIntensity;
    #endregion

    #region Private Variables
    // Coroutine do SlowDownEffect()
    private Coroutine coroutine_SDE;

    // Coroutine do TimeControl()
    private Coroutine coroutine_TC;

    // Estado da coroutine do TimeControl()
    private bool isCoroutine_TC_Running;
    #endregion

    #region Unity Methods
    private void Start()
    {
        // Estado inicial
        isCoroutine_TC_Running = false;
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        // Se o jogador está colidindo com este objeto
        if (collision.gameObject == player.gameObject)
        {
            // Inicia o efeito de slow motion
            coroutine_SDE = StartCoroutine(SlowDownEffect(slowMotionTimeScale, slowMotionTime, transitionIntensity));

            // Inicia a animação de slow motion no jogador
            player.GetComponent<AnimationManager>().AnimationTrigger(gameObject);

            // Move este objeto para a posição de espera
            gameObject.transform.position = pathGenerator.instancePosition;
        }
    }
    #endregion

    #region Slow Motion
    private IEnumerator SlowDownEffect(float slowMotionTimeScale, float slowMotionTime, float transitionIntensity)
    {
        // Inicializa a variável de tempo local
        float initialTime = -1F;

        // Inicia a desaceleração do tempo
        coroutine_TC = StartCoroutine(TimeControl(slowMotionTimeScale, transitionIntensity, 0.05F));

        /* Estados:
         * isCoroutine_TC_Running == true && initialTime < 0F: Desacelerando o tempo
         * isCoroutine_TC_Running == false && initialTime < 0F: O tempo acabou de ser desacelerado
         * isCoroutine_TC_Running == false && initialTime > 0F: O tempo de espera começou
         * isCoroutine_TC_Running == false && (Tempo) - initialTime >= (Tempo máximo): O tempo de espera acabou
         * isCoroutine_TC_Running == true && initialTime > 0F: Acelerando o tempo
         */

        while (true)
        {
            // Checa se o tempo já foi desacelerado
            if (!isCoroutine_TC_Running && initialTime < 0F)
            {
                // Marca o tempo inicial
                initialTime = Time.unscaledTime;
            }

            // Checa se o tempo de espera acabou
            if (initialTime > 0F && Time.unscaledTime - initialTime >= slowMotionTime)
            {
                // Inicia a aceleração do tempo
                coroutine_TC = StartCoroutine(TimeControl(1F, transitionIntensity, 0.05F));

                // Sai do loop
                break;
            }

            yield return null;
        }

        // Marca que um novo item de slow motion pode ser gerado
        pathGenerator.slowMotionIsWaiting = true;

        yield return null;
    }

    // AVISO: O tempo no input não corresponde ao tempo real
    private IEnumerator TimeControl(float timeScale, float transitionIntensity, float convergenceValue = 0.001F)
    {
        // Define que a coroutine está sendo executada
        isCoroutine_TC_Running = true;

        // Tempo de espera fixo
        WaitForFixedUpdate fixedWait = new WaitForFixedUpdate();

        // Loop de controle da interpolação linear da escala do tempo
        for (float i = 0; i <= 1F; i += (Time.fixedDeltaTime / (transitionIntensity + (convergenceValue * 100F))))
        {
            // Faz a interpolação linear da escala do tempo
            Time.timeScale = Mathf.Lerp(Time.timeScale, timeScale, i);

            // Se a escala do tempo convergir
            if (Mathf.Abs(timeScale - Time.timeScale) <= convergenceValue)
            {
                Time.timeScale = timeScale;
                break;
            }

            yield return fixedWait;
        }

        // Define que a coroutine não está mais sendo executada
        isCoroutine_TC_Running = false;

        yield return fixedWait;
    }
    #endregion
}