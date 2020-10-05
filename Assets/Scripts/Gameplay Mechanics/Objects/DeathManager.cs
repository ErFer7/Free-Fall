using System;
using System.Linq;
using UnityEngine;

public class DeathManager : MonoBehaviour
{
    #region Public Variables
    // Acesso ao jogador
    public GameObject player;

    // Acesso ao Fall Manager
    public Transform fallManager;

    // Acesso ao Path Generator
    public PathGenerator pathGenerator;

    // Nova posição do jogador
    [NonSerialized]
    public Vector2 newPlayerPosition;
    #endregion

    #region Private Variables
    // Distâncias entre as plataformas e o fall manager
    private float[] distances;

    // Menor distância entre uma plataforma e o fall manager
    private float smallestDistance;

    // Índice da plataforma mais próxima
    private int closestPlatformIndex;
    #endregion;

    #region Unity Methods
    private void Start()
    {
        // Cria a array das distâncias
        distances = new float[pathGenerator.platformNumber];
    }
    
    private void OnTriggerEnter2D(Collider2D collision)
    {
        // Se o jogador está colidindo
        if (collision.gameObject == player.gameObject)
        {
            // Reduz a vida do jogador
            --player.GetComponent<PlayerControls>().lives;

            // Calcula as distâncias
            for (int i = 0; i < pathGenerator.platformNumber; ++i)
            {
                distances[i] = Vector2.Distance(fallManager.position, pathGenerator.platforms[i].transform.position);
            }

            // Determina a menor distância e o índice da plataforma a quem esta distância pertence
            smallestDistance = distances.Min();
            closestPlatformIndex = Array.IndexOf(distances, smallestDistance);

            // Define a nova posição do jogador na plataforma mais próxima do fall manager (centro da tela)
            newPlayerPosition = new Vector2(pathGenerator.platforms[closestPlatformIndex].transform.position.x, pathGenerator.platforms[closestPlatformIndex].transform.position.y + 1.5F);
            player.transform.position = newPlayerPosition;

            // Anula a velocidade do jogador
            player.GetComponent<Rigidbody2D>().velocity = Vector2.zero;

            // Aciona a animação de morte do jogador
            player.GetComponent<AnimationManager>().AnimationTrigger(gameObject);
        }
    }
    #endregion
}