using UnityEngine;
using UnityEngine.UI;

public class LifeManager : MonoBehaviour
{
    #region Public Variables
    // Acesso ao jogador
    public GameObject player;

    // Acesso ao Path Generator
    public PathGenerator pathGenerator;
    #endregion

    #region Unity Methods
    private void OnTriggerEnter2D(Collider2D collision)
    {
        // Se o jogador está colidindo
        if (collision.gameObject == player.gameObject)
        {
            // Aumenta a vida do jogador
            ++player.GetComponent<PlayerControls>().lives;

            // Inicia a animação da vida extra no jogador
            player.GetComponent<AnimationManager>().AnimationTrigger(gameObject);

            // Move a vida extra para uma posição de espera e marca ela para esperar
            gameObject.transform.position = pathGenerator.instancePosition;
            pathGenerator.extraLifeIsWaiting = true;
        }
    }
    #endregion
}