using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SceneryManager : MonoBehaviour
{
    #region Public Variables
    // Acesso ao Fall Manager
    public FallManager fallManagerScript;

    // Acesso ao Path Generator
    public PathGenerator pathGeneratorScript;

    // Número de objetos
    public int sceneryObjectsNumber;

    // Distância de corte
    public float clippingDistance;

    // Distância lateral máxima
    public float x_pos_MaxVal;

    // Espaçamento vertical
    public float y_Spacing;

    // Posição de instância
    public Vector2 instancePosition;

    // Unidades por pixel
    public float unitsPerPixel;

    // Referência do maior tamanho dos sprites, usado para calcular o fator de velocidade
    public float biggestSpriteReferenceSize;

    // Correção do valor máximo para o fator de velocidade do objeto
    public float sceneryObjectsSpeedFactor_MaxValCorrection;

    // Valor mínimo para o fator de velocidade do objeto
    public float sceneryObjectsSpeedFactor_MinVal;

    // Correção do valor mínimo para o fator de velocidade do objeto
    public float sceneryObjectsSpeedFactor_MinValCorrection;

    // Cores dos níveis
    public Color[] levelColors;

    // Renderizador do plano de fundo
    public SpriteRenderer backgroundRenderer;

    // Plataformas
    public Sprite[] platforms;

    // Gramas
    public Sprite[] grasses;

    // Sprites dos objetos
    public Sprite[] sprites;
    #endregion

    #region Private Variables
    // Nível
    private int level;

    // Ponto de mudança de nível
    private float levelUpdatePoint;

    // Objeto do cenário
    private Object pSceneryObject;

    // Objetos do cenário
    private GameObject[] sceneryObjects;

    // Fator de velocidade de cada objeto
    private float[] sceneryObjectsSpeedFactor;

    // Tamanhos dos sprites (Na lateral)
    private List<float> spriteSizes;

    // Fila de índices dos objetos do gerador contínuo
    private int[] sceneryGeneratorIndexes;

    // Coroutine do Scenery Generator
    private Coroutine coroutine_SG;
    #endregion

    #region Unity Methods
    private void Start()
    {
        // Inicializa as variáveis
        level = 0;
        levelUpdatePoint = fallManagerScript.paceStep[fallManagerScript.paceStep.Length - 1] / levelColors.Length;

        // Carrega os objetos e cria suas arrays
        pSceneryObject = Resources.Load("Scenery Object", typeof(GameObject));
        sceneryObjects = new GameObject[sceneryObjectsNumber];
        sceneryObjectsSpeedFactor = new float[sceneryObjectsNumber];

        // Cria uma lista para os tamanhos dos sprites
        spriteSizes = new List<float>();

        // Preenche a lista com os tamanhos
        for (int i = 0; i < sprites.Length; ++i)
        {
            if (!spriteSizes.Contains(sprites[i].rect.width))
            {
                spriteSizes.Add(sprites[i].rect.width);
            }
        }

        // Odena a lista por ordem crescente
        spriteSizes.Sort();

        // Define a sorting order do plano de fundo para que ele seja o primeiro objeto a ser renderizado
        backgroundRenderer.sortingOrder = -(spriteSizes.Count + 2);

        // Cria os objetos e define o comportamento deles
        for (int i = 0; i < sceneryObjectsNumber; ++i)
        {
            sceneryObjects[i] = Instantiate(pSceneryObject, instancePosition, Quaternion.identity) as GameObject;

            // Se houverem sprites
            if (sprites.Length > 0)
            {
                // Define aleatóriamente qual sprite será usado
                sceneryObjects[i].GetComponent<SpriteRenderer>().sprite = sprites[Random.Range(0, sprites.Length)];

                // Define a sorting order do objeto
                sceneryObjects[i].GetComponent<SpriteRenderer>().sortingOrder = -(spriteSizes.IndexOf(sceneryObjects[i].GetComponent<SpriteRenderer>().sprite.rect.width) + 2);
            }

            // Define o fator de velocidade dos objetos 
            if (sceneryObjects[i].GetComponent<SpriteRenderer>().sprite.rect.width / biggestSpriteReferenceSize >= 1F)
            {
                // Caso esse objeto seja o maior de todos então seu fator será limitado para um máximo
                sceneryObjectsSpeedFactor[i] = sceneryObjectsSpeedFactor_MaxValCorrection;
            }
            else if (sceneryObjects[i].GetComponent<SpriteRenderer>().sprite.rect.width / biggestSpriteReferenceSize <= sceneryObjectsSpeedFactor_MinVal)
            {
                // Caso esse objeto seja o menor de todos então seu fator será limitado para um mínimo
                sceneryObjectsSpeedFactor[i] = sceneryObjectsSpeedFactor_MinValCorrection;
            }
            else
            {
                // Em casos gerais o fator será o a largura do objeto dividida pela largura do maior objeto
                sceneryObjectsSpeedFactor[i] = sceneryObjects[i].GetComponent<SpriteRenderer>().sprite.rect.width / biggestSpriteReferenceSize;
            }
        }

        // Cria a array que contém os índices dos objetos em ordem de geração
        sceneryGeneratorIndexes = new int[sceneryObjectsNumber];

        // Preenche a array
        for (int i = 0; i < sceneryGeneratorIndexes.Length; ++i)
        {
            sceneryGeneratorIndexes[i] = -1;
        }

        // Inicia a coroutine do Scenery Generator
        if (sceneryObjectsNumber > 0)
        {
            coroutine_SG = StartCoroutine(SceneryGen());
        }
    }

    private void FixedUpdate()
    {
        // Movimenta os objetos do cenário
        for (int i = 0; i < sceneryObjects.Length; ++i)
        {
            sceneryObjects[i].transform.Translate(fallManagerScript.finalDirection * sceneryObjectsSpeedFactor[i]);
        }

        // Atualiza o nível
        if (fallManagerScript.paceFactor >= levelUpdatePoint * (level + 1) && levelUpdatePoint * (level + 1) < fallManagerScript.paceStep[fallManagerScript.paceStep.Length - 1])
        {
            ++level;
        }

        // Atualiza a cor do plano de fundo
        UpdateBackgroundColor();
    }
    #endregion

    #region Color Update
    private void UpdateBackgroundColor()
    {
        if (level > 0 && level < levelColors.Length)
        {
            backgroundRenderer.color = Color.Lerp(levelColors[level - 1], levelColors[level], (fallManagerScript.paceFactor - (levelUpdatePoint * level)) / ((levelUpdatePoint * (level + 1)) - (levelUpdatePoint * level)));
        }
    }
    #endregion

    #region Scenery Generator
    private IEnumerator SceneryGen()
    {
        // Define que o gerador será atualizado em uma taxa fixa de aproximadamente 20 ms (50 Hz)
        WaitForFixedUpdate fixedWait = new WaitForFixedUpdate();

        // Define a condição de primeiro passe
        bool firstPass = true;

        // Altura inicial
        float height = 20F;

        // Indica se o objeto foi gerado
        bool generationComplete = false;

        // Geração contínua
        for (int i = 0; true; ++i)
        {
            // Reseta o loop
            if (i == sceneryObjectsNumber)
            {
                i = 0;
            }

            // Se for o primeiro passe
            if (firstPass)
            {
                // Define a posição do objeto
                sceneryObjects[i].transform.position = new Vector2(Random.Range(-x_pos_MaxVal, x_pos_MaxVal), height);

                // Registra o índice do objeto
                RegisterObjectIndexOn_SGI(i);

                firstPass = false;
            }

            // Checa se o objeto passou da altura máxima
            if ((sceneryObjects[i].transform.position.y - ((sceneryObjects[i].GetComponent<SpriteRenderer>().sprite.rect.height / (2F * unitsPerPixel)))) - fallManagerScript.transform.position.y > clippingDistance)
            {
                // Checa se há um objeto com a mesma sorting order que esse gerado
                for (int j = 0; j < sceneryGeneratorIndexes.Length && sceneryGeneratorIndexes[j] != -1; ++j)
                {
                    // O objeto não deve ser ele mesmo e deve ter a mesma sorting order
                    if (i != sceneryGeneratorIndexes[j] && sceneryObjects[i].GetComponent<SpriteRenderer>().sortingOrder == sceneryObjects[sceneryGeneratorIndexes[j]].GetComponent<SpriteRenderer>().sortingOrder)
                    {
                        // Altura: (Altura do objeto j / 2) - Espaçamento - (Altura do objeto i / 2)
                        height = (sceneryObjects[sceneryGeneratorIndexes[j]].transform.position.y - ((sceneryObjects[sceneryGeneratorIndexes[j]].GetComponent<SpriteRenderer>().sprite.rect.height / (2F * unitsPerPixel)))) - y_Spacing - (sceneryObjects[i].GetComponent<SpriteRenderer>().sprite.rect.height / (2F * unitsPerPixel));

                        // Posiciona o objeto
                        sceneryObjects[i].transform.position = new Vector2(Random.Range(-x_pos_MaxVal, x_pos_MaxVal), height);

                        // Registra o índice do objeto
                        RegisterObjectIndexOn_SGI(i);

                        // Declara a geração como completa e quebra o loop
                        generationComplete = true;
                        break;
                    }
                }

                // Se a geração ainda não foi completa
                if (!generationComplete)
                {
                    // Altura: (Y do fall manager) - Espaçamento - (Altura do objeto i / 2)
                    height = fallManagerScript.transform.position.y - y_Spacing - (sceneryObjects[i].GetComponent<SpriteRenderer>().sprite.rect.height / (2F * unitsPerPixel));

                    // Posiciona o objeto
                    sceneryObjects[i].transform.position = new Vector2(Random.Range(-x_pos_MaxVal, x_pos_MaxVal), height);

                    // Registra o índice do objeto
                    RegisterObjectIndexOn_SGI(i);
                }

                // Reseta o estado da geração
                generationComplete = false;
            }

            yield return fixedWait;
        }
    }

    private void RegisterObjectIndexOn_SGI(int index)
    {
        // Registra o índice em ordem, a cada novo elemento todos os outros são deslocados para o fim da array
        for (int i = sceneryGeneratorIndexes.Length - 1; i >= 1; --i)
        {
            sceneryGeneratorIndexes[i] = sceneryGeneratorIndexes[i - 1];
        }

        sceneryGeneratorIndexes[0] = index;
    }
    #endregion
}