using System.Collections;
using UnityEngine;

public class CollisionAnimation : MonoBehaviour
{
    #region Public Variables
    // Acesso ao componente de colisão do jogador
    public GameObject player;

    // Configurações
    public float firstPhaseFlashIntensity;
    public float secondPhaseFlashIntensity;
    #endregion

    #region Private Variables
    // Coroutine da animação e sub-animação
    private Coroutine coroutine_F;
    private Coroutine coroutine_LA;

    // Cor do sprite do muro
    private Color color;

    // Determina se a sub-animação terminou
    private bool lerpAlphaIsFinished;

    // Determina se o jogador ainda está colidindo com a parede
    private bool playerStillColliding;
    #endregion

    #region Unity Methods
    private void Start()
    {
        // Define a cor inicial e o estado da sub-animação
        color = new Color(1F, 1F, 1F, 0F);
        lerpAlphaIsFinished = false;
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        // Define que o jogador está colidindo
        playerStillColliding = true;

        // Se a colisão for com o jogador
        if (collision.gameObject == player.gameObject)
        {
            // Inicia a animação, animações em execução serão paradas
            if (coroutine_F != null)
            {
                StopCoroutine(coroutine_F);
                coroutine_F = StartCoroutine(Flash());
            }
            else
            {
                coroutine_F = StartCoroutine(Flash());
            }
        }
    }

    private void OnCollisionExit2D(Collision2D collision)
    {
        // Define que o jogador não está mais colidindo
        if (collision.gameObject == player.gameObject)
        {
            playerStillColliding = false;
        }
    }
    #endregion

    #region Collision Animation
    private IEnumerator Flash()
    {
        // Inicia o primeiro estágio da animação do flash, para qualquer sub-animação que esteja em execução
        if (coroutine_LA != null)
        {
            StopCoroutine(coroutine_LA);
            coroutine_LA = StartCoroutine(LerpAlpha(1F, firstPhaseFlashIntensity, 0.05F));
        }
        else
        {
            coroutine_LA = StartCoroutine(LerpAlpha(1F, firstPhaseFlashIntensity, 0.05F));
        }

        // Aguarda a sub-animação acabar e o jogador parar de colidir
        while(!lerpAlphaIsFinished || playerStillColliding)
        {
            yield return null;
        }

        // Anula a sub-animação anterior e inicia o segundo estágio da animação do flash
        coroutine_LA = null;
        coroutine_LA = StartCoroutine(LerpAlpha(0F, secondPhaseFlashIntensity, 0.05F));

        // Aguarda a sub-animação acabar
        while (!lerpAlphaIsFinished)
        {
            yield return null;
        }

        // Anula a animação e sub-animação
        coroutine_LA = null;
        coroutine_F = null;

        yield return null;
    }

    private IEnumerator LerpAlpha(float value, float intensity, float convergenceValue = 0.001F)
    {
        // Define que a sub-animação não acabou
        lerpAlphaIsFinished = false;

        // Operação de interpolação linear do alfa
        for (float i = 0; i <= 1; i += Time.deltaTime * intensity)
        {
            // Interpola o alfa e muda a cor da parede
            color.a = Mathf.Lerp(color.a, value, i);
            gameObject.GetComponent<SpriteRenderer>().color = color;

            // Condição de convergência
            if (Mathf.Abs(value - color.a) <= convergenceValue)
            {
                color.a = value;
                gameObject.GetComponent<SpriteRenderer>().color = color;
                break;
            }

            yield return null;
        }

        // Define que a sub-animação acabou
        lerpAlphaIsFinished = true;

        yield return null;
    }
    #endregion
}