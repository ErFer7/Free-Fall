using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PathGenerator : MonoBehaviour
{
    #region Public Variables
    #region Editor Acessible
    // Acesso ao Fall Manager
    public GameObject fallManager;

    // Acesso ao jogador
    public GameObject player;

    // Acesso ao Scenery Manager
    public SceneryManager sceneryManager;

    // Número de plataformas
    public int platformNumber;

    // Possibilidade de geração de uma plataforma mortal
    [Range(0F, 100F)]
    public float deathPlatform_GenChance;

    // Intensidade do aumento de chance de geração de uma plataforma mortal
    public float deathPlatform_GenChance_Factor;

    // Chance de geração de uma vida extra
    [Range(0F, 100F)]
    public float extraLife_GenChance;

    // Intensidade do aumento de chance de geração de uma vida extra
    public float extraLife_GenChance_Factor;

    // Chance de geração de um item de slow motion
    [Range(0F, 100F)]
    public float slowMotion_GenChance;

    // Intensidade do aumento de chance de geração de um item de slow motion
    public float slowMotion_GenChance_Factor;

    // Local de instanciamento de objetos
    public Vector2 instancePosition;

    // Distancia do FallManager em que a plataforma retorna a geração
    public float clippingDistance;

    // Valor máximo que uma plataforma pode ser gerada na horizontal
    public float x_pos_MaxVal;

    // Frequência das posições no eixo x
    public float x_pos_Stepping;

    // Espaçamento vertical
    public float y_spacing;

    // Offset da posição inicial da vida extra
    public float extraLifePositionOffset;

    // Offset da posição inicial do slow motion
    public float slowMotionPositionOffset;
    #endregion

    // Plataformas
    [System.NonSerialized]
    public GameObject[] platforms;

    // Inibidor de geração de vida extra enquanto em área de tela
    [System.NonSerialized]
    public bool extraLifeIsWaiting;

    // Inibidor de geração de Slow Motion enquanto em área de tela
    [System.NonSerialized]
    public bool slowMotionIsWaiting;
    #endregion

    #region Private Variables
    // Platafoma
    private Object pPlatform;

    // Plataforma mortal
    private Object pDeathPlatform;

    // Vida extra
    private Object pExtraLife;
    private GameObject extraLife;

    // Índice da plataforma associada a vida extra
    private int extraLifeBasePlatformIndex;

    // Slow Motion
    private Object pSlowMotion;
    private GameObject slowMotion;

    // Índice da plataforma associada com o slow motion
    private int slowMotionBasePlatformIndex;

    // Acesso ao FallManager
    private FallManager fallManagerScript;

    // Coroutine do PathGen
    private Coroutine coroutine;

    // Definições dimensionais e vetoriais
    private Vector2 firstPlatformPosition;
    private List<float> x_pos;

    // Plataformas mortais
    private GameObject[] deathPlatforms;

    // Metade do tamanho da plataforma
    private float halfOfPlatformLenght;
    #endregion

    #region Unity Methods
    private void Start()
    {
        // Inicializa o gerador de números com o tempo como seed
        Random.InitState((int)System.DateTime.Now.Ticks);

        // Acessa o FallManager
        fallManagerScript = fallManager.GetComponent<FallManager>();

        // Carrega as plataformas e cria uma array para recebê-las
        pPlatform = Resources.Load("Platform", typeof(GameObject));
        platforms = new GameObject[platformNumber];

        // Carrega as plataformas mortais e cria uma array para recebê-las
        pDeathPlatform = Resources.Load("Death Platform Variant", typeof(GameObject));
        deathPlatforms = new GameObject[platformNumber];

        // Carrega a vida extra
        pExtraLife = Resources.Load("Extra Life", typeof(GameObject));

        // Carrega o slow motion
        pSlowMotion = Resources.Load("Slow Motion", typeof(GameObject));

        // Define a posição da primeira plataforma e as posições permitidas no eixo X
        firstPlatformPosition = new Vector2(0F, -1F);
        x_pos = new List<float>();

        // Preenche a array das posições do eixo X
        for (float i = x_pos_MaxVal; i > 0; i -= x_pos_Stepping)
        {
            // Checa se a posição não irá prender o jogador
            if (i < x_pos_MaxVal - 2 * player.GetComponent<CircleCollider2D>().radius || i == x_pos_MaxVal)
            {
                x_pos.Add(i);
                x_pos.Add(-i);
            }
        }

        // Posição Central no eixo X
        x_pos.Add(0F);

        // Cria plataformas e associa as suas variáveis
        for (int i = 0; i < platformNumber; ++i)
        {
            platforms[i] = Instantiate(pPlatform, instancePosition, Quaternion.identity) as GameObject;

            deathPlatforms[i] = Instantiate(pDeathPlatform, instancePosition, Quaternion.identity) as GameObject;
            deathPlatforms[i].GetComponent<DeathManager>().player = player;
            deathPlatforms[i].GetComponent<DeathManager>().fallManager = fallManager.transform;
            deathPlatforms[i].GetComponent<DeathManager>().pathGenerator = gameObject.GetComponent<PathGenerator>();
        }

        // Define a metade do comprimento da plataforma
        halfOfPlatformLenght = platforms[0].GetComponent<BoxCollider2D>().size.x / 2F;

        // Cria a vida extra e associa as suas variáveis
        extraLife = Instantiate(pExtraLife, instancePosition, Quaternion.identity) as GameObject;
        extraLife.GetComponent<LifeManager>().player = player;
        extraLife.GetComponent<LifeManager>().pathGenerator = gameObject.GetComponent<PathGenerator>();

        // A vida extra pode ser gerada
        extraLifeIsWaiting = true;

        // Definição inicial para o índice
        extraLifeBasePlatformIndex = 0;

        // Cria o slow motion e associa as suas variáveis
        slowMotion = Instantiate(pSlowMotion, instancePosition, Quaternion.identity) as GameObject;
        slowMotion.GetComponent<SlowMotionManager>().player = player;
        slowMotion.GetComponent<SlowMotionManager>().pathGenerator = gameObject.GetComponent<PathGenerator>();

        // O slow motion pode ser gerado
        slowMotionIsWaiting = true;

        // Definição inicial para o índice
        slowMotionBasePlatformIndex = 0;

        // Inicia o gerador de caminho
        coroutine = StartCoroutine(PathGen());
    }
    #endregion

    #region Path Generator
    IEnumerator PathGen()
    {
        // Define que o gerador será atualizado em uma taxa fixa de aproximadamente 20 ms (50 Hz)
        WaitForFixedUpdate fixedWait = new WaitForFixedUpdate();

        // Altura inicial
        float height = -1F;

        // Indice de posição horizontal
        int x_pos_Index;

        // Indice de posição horizontal da plataforma mortal
        int x_pos_Index_DP = 0;

        // Determina o primeiro passe
        bool firstPass = true;

        // Geração contínua
        for (int i = 0; true ; ++i)
        {
            // Reseta o loop
            if ( i == platformNumber)
            {
                i = 0;
            }

            // Gera o índice da plataforma
            if (firstPass)
            {
                // Índice da primeira plataforma (posição 0)
                x_pos_Index = x_pos.Count - 1;
            }
            else
            {
                // Índice contínuo
                x_pos_Index = Random.Range(0, x_pos.Count);
            }

            // Determina o índice da plataforma mortal
            if (x_pos[x_pos_Index] > 0F)
            {
                x_pos_Index_DP = x_pos_Index + 1;
            }
            else if (x_pos[x_pos_Index] < 0F)
            {
                x_pos_Index_DP = x_pos_Index - 1;
            }
            else if (Mathf.Approximately(x_pos[x_pos_Index], 0F))
            {
                x_pos_Index_DP = x_pos_Index;
            }

            // Detecta se as plataformas já passaram da altura de corte
            if (platforms[i].transform.position.y - fallManager.transform.position.y > clippingDistance)
            {
                // Move a plataforma i para a nova posição
                platforms[i].transform.position = new Vector2(x_pos[x_pos_Index], height);

                // Redefine a textura da plataforma
                platforms[i].GetComponent<SpriteRenderer>().sprite = sceneryManager.platforms[Random.Range(0, sceneryManager.platforms.Length)];

                // Redefine a grama da plataforma
                platforms[i].transform.GetChild(0).GetComponent<SpriteRenderer>().sprite = sceneryManager.grasses[Random.Range(0, sceneryManager.grasses.Length)];

                // A vida extra pode ser gerada
                if (i == extraLifeBasePlatformIndex)
                {
                    extraLifeIsWaiting = true;
                    extraLifeBasePlatformIndex = -1;
                }

                // O slow motion pode ser gerado
                if (i == slowMotionBasePlatformIndex)
                {
                    slowMotionIsWaiting = true;
                    slowMotionBasePlatformIndex = -1;
                }

                // Se as condições permitirem move a vida extra para uma posição acima de alguma plataforma
                if (Random.Range(0F, 100F) < extraLife_GenChance + fallManagerScript.paceFactor * extraLife_GenChance_Factor && extraLifeIsWaiting && i != slowMotionBasePlatformIndex && !firstPass)
                {
                    extraLife.transform.position = new Vector2(x_pos[x_pos_Index], height + extraLifePositionOffset);

                    // Determina o novo índice da plataforma associada
                    extraLifeBasePlatformIndex = i;

                    // A vida extra não pode ser gerada
                    extraLifeIsWaiting = false;
                }

                // Se as condições permitirem move o slow motion para uma nova posição acima de alguma plataforma
                if (Random.Range(0F, 100F) < slowMotion_GenChance + fallManagerScript.paceFactor * slowMotion_GenChance_Factor && slowMotionIsWaiting && i != extraLifeBasePlatformIndex && !firstPass)
                {
                    slowMotion.transform.position = new Vector2(x_pos[x_pos_Index], height + slowMotionPositionOffset);

                    // Determina o novo índice da plataforma associada
                    slowMotionBasePlatformIndex = i;

                    // O slow motion não pode ser gerado
                    slowMotionIsWaiting = false;
                }

                // Determina a altura da próxima plataforma
                height -= y_spacing + (Mathf.FloorToInt(fallManagerScript.paceFactor - 1F));
            }

            // Detecta se as plataformas mortais já passaram da altura de corte
            if (deathPlatforms[i].transform.position.y - fallManager.transform.position.y > clippingDistance)
            {
                // Se as condições permitirem move a plataforma mortal para uma posição simétrica a outra plataforma em relação a Y
                if (Random.Range(0F, 100F) < deathPlatform_GenChance + fallManagerScript.paceFactor * deathPlatform_GenChance_Factor && Mathf.Abs(x_pos[x_pos_Index]) > halfOfPlatformLenght)
                {
                    deathPlatforms[i].transform.position = new Vector2(x_pos[x_pos_Index_DP], platforms[i].transform.position.y);
                }
                // Move a plataforma mortal para uma posição de espera
                else
                {
                    deathPlatforms[i].transform.position = new Vector2(x_pos_MaxVal + 10F, platforms[i].transform.position.y);
                }
            }

            // Determina o fim do primeiro passe
            if (firstPass)
            {
                firstPass = false;
            }

            yield return fixedWait;
        }
    }
    #endregion
}