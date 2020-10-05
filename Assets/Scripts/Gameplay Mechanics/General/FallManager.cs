using UnityEngine;

public class FallManager : MonoBehaviour
{
    #region Public Variables
    #region Editor Acessible
    // Objetos controlados pelo FallManager
    public Transform[] objectsTransform;

    // Passo inicial
    public float initialPace;

    // Fator de aumento de passo
    public float paceFactorIncrease;

    // Pontos de passo constante
    public float[] paceStep;

    // Tempo em cada ponto de passo constante
    public float[] paceStepTimes;
    #endregion

    // Fator de passo
    [System.NonSerialized]
    public float paceFactor;

    // Direção calculada
    [System.NonSerialized]
    public Vector2 finalDirection;
    #endregion

    #region Private Variables
    // Transform do FallManager
    private Transform fallManagerTransform;

    // Direção padrão
    private Vector2 direction;

    // Estado do ponto de passo constante
    private int paceStepIndex;

    // Contagem do tempo do ponto do passo
    private float paceStepInitialTime;

    // Determina se o passo deve aumentar
    private bool paceIsIncreasing;

    // Determina se o passo máximo foi alcaçado
    private bool maximumPaceWasReached;
    #endregion

    #region Unity Methods
    private void Start()
    {
        // Acessa o transform do FallManager
        fallManagerTransform = gameObject.transform;

        // Define a direção de queda com o passo e o define o fator de passo
        direction = new Vector2(0F, -initialPace);
        paceFactor = 1F;

        // Define a condição inicial em que o passo deve aumentar
        paceIsIncreasing = true;

        // Define a condição inicial de que o passo máximo não foi alcançado
        maximumPaceWasReached = false;

        // Índice inicial da array dos pontos de passo constante
        paceStepIndex = 0;

        // Estado inicial do tempo do ponto de passo constante
        paceStepInitialTime = 0;
    }

    private void FixedUpdate()
    {
        // Calcula a direção final
        finalDirection = direction * paceFactor;

        // Atualiza o passo
        UpdatePace();

        // Move toda a estrutura
        fallManagerTransform.Translate(finalDirection);

        for (int i = 0; i < objectsTransform.Length; ++i)
        {
            objectsTransform[i].Translate(finalDirection);
        }
    }
    #endregion

    #region UpdadePace
    // Atualiza o passo
    private void UpdatePace()
    {
        // Aumenta o passo e checa se o ponto de passo constante já foi atingido
        if (paceIsIncreasing && !maximumPaceWasReached)
        {
            paceFactor += paceFactorIncrease;

            if (paceFactor >= paceStep[paceStepIndex])
            {
                paceFactor = paceStep[paceStepIndex];
                paceIsIncreasing = false;

                // Marca o tempo inicial do tempo do passo constante
                paceStepInitialTime = Time.time;
            }
        }
        // Checa se o tempo de passo constante já foi atingido
        else if (!maximumPaceWasReached)
        {
            if (Time.time - paceStepInitialTime >= paceStepTimes[paceStepIndex])
            {
                ++paceStepIndex;

                if (paceStepIndex == paceStep.Length)
                {
                    paceIsIncreasing = false;
                    maximumPaceWasReached = true;
                }
                else
                {
                    paceIsIncreasing = true;
                }
            }
        }
    }
    #endregion
}