using UnityEngine;

public class PlayerControls : MonoBehaviour
{
    #region Public Variables
    // Força do movimento horizontal
    public float force;

    // Fator de suavização de aceleração
    public float slowingFactor;

    // Fator de correção do aumento da gravidade
    public float gravityIntensityCorrectionFactor;

    // Velocidade lateral máxima
    public float maximumLateralVelocity;

    // Fator de aumento da velocidade lateral máxima
    public float maximumLateralVelocityIncreaseFactor;

    // Magnitude máxima da componente x na colisão (Usada para definir o inibidor de travamento lateral)
    public float normal_X_MaxMagnitude;

    // Modificador da velocidade (Para a redução do momento de inércia)
    public Vector2 velocityModifier;

    // Acesso ao FallManager
    public FallManager fallManager;

    // Vidas
    [SerializeField]
    private int _lives;
    public int lives
    {
        get
        {
            return _lives;
        }
        set
        {
            if (_lives == value)
            {
                return;
            }

            _lives = value;

            OnLifeValueChange?.Invoke(_lives);
        }
    }
    public delegate void OnLifeValueChangeDelegate(int val);
    public event OnLifeValueChangeDelegate OnLifeValueChange;
    #endregion

    #region Private Variables
    // Acesso ao Script Manager
    private ScriptManager scriptManager;

    // Rigidbody do jogador
    private Rigidbody2D player;

    // Direções
    private Vector2 right;
    private Vector2 left;

    // Intensidade das forças
    private float intensity;

    // Meio da tela
    private float screenMiddle;

    // Inibidores de travamento em parede
    private bool leftIsAllowed;
    private bool rightIsAllowed;

    // Define se o jogador está colidindo
    private bool isColliding;

    // Direção atual
    private Directions currentDirection;

    // Direções
    enum Directions
    {
        right,
        left,
        idle
    }
    #endregion

    #region Unity Methods
    void Start()
    {
        // Acessa o rigidbody do jogador
        player = gameObject.GetComponent<Rigidbody2D>();

        // Define a força padrão para cada direção
        right = new Vector2(force, 0F);
        left = new Vector2(-force, 0F);

        // Determina o meio da tela
        screenMiddle = Screen.width / 2;

        // Define o estado inicial da colisão
        isColliding = true;
    }

    private void Update()
    {
        // Input do controle
        if (!Application.isEditor)
        {
            TouchControl();
        }
        else
        {
            EditorControl();
        }
    }

    private void FixedUpdate()
    {
        // Calcula a intensidade
        intensity = ((fallManager.paceFactor - 1F) / slowingFactor) + 1F;

        // Limitador de velocidade
        MaximumLateralVelocity();

        // Controle do jogador
        Control();

        // Gravidade dinâmica
        player.gravityScale = intensity * gravityIntensityCorrectionFactor;
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        // Define que o jogador está colidindo
        isColliding = true;

        // Checa se a normal da colisão tem o módulo da componente X maior do que o limite
        if (Mathf.Abs(collision.GetContact(0).normal.x) > normal_X_MaxMagnitude)
        {
            // Inibe o movimento para a esquerda
            if (collision.GetContact(0).normal.x > 0F)
            {
                leftIsAllowed = false;
                rightIsAllowed = true;
            }
            // Inibe o movimento para a direita
            else if (collision.GetContact(0).normal.x < 0F)
            {
                leftIsAllowed = true;
                rightIsAllowed = false;
            }
        }
        // Permite os movimentos
        else
        {
            rightIsAllowed = true;
            leftIsAllowed = true;
        }
    }

    private void OnCollisionExit2D(Collision2D collision)
    {
        // Define que o jogador não está colidindo
        isColliding = false;

        // Permite os movimentos
        rightIsAllowed = true;
        leftIsAllowed = true;
    }
    #endregion

    #region Controls
    // Input do jogador no editor
    private void EditorControl()
    {
        // Direita
        if (Input.GetKey(KeyCode.RightArrow))
        {
            currentDirection = Directions.right;
        }
        // Esquerda
        else if (Input.GetKey(KeyCode.LeftArrow))
        {
            currentDirection = Directions.left;
        }
        // Nada
        else
        {
            currentDirection = Directions.idle;
        }
    }

    // Input do jogador no Smartphone
    private void TouchControl()
    {
        // Detecta se há um toque na tela
        if (Input.touchCount > 0)
        {
            // Direita
            if (Input.GetTouch(0).position.x > screenMiddle)
            {
                currentDirection = Directions.right;
            }
            // Esquerda
            else if (Input.GetTouch(0).position.x < screenMiddle)
            {
                currentDirection = Directions.left;
            }
        }
        // Nada
        else
        {
            currentDirection = Directions.idle;
        }
    }

    // Controla o jogador
    private void Control()
    {
        switch(currentDirection)
        {
            // Direita
            case Directions.right:

                if (rightIsAllowed)
                {
                    player.AddForce(right * intensity);
                }
                break;
            // Esquerda
            case Directions.left:

                if (leftIsAllowed)
                {
                    player.AddForce(left * intensity);
                }
                break;
            // Nada. Controla o momento de inércia do jogador
            default:

                player.velocity *= velocityModifier;
                break;
        }
    }
    #endregion

    #region lateral Velocity Limiter
    private void MaximumLateralVelocity()
    {
        // Se o jogador atingiu a velocidade máxima para a direita
        if (player.velocity.x >= maximumLateralVelocity + (intensity * maximumLateralVelocityIncreaseFactor))
        {
            rightIsAllowed = false;
            leftIsAllowed = true;
        }
        // Se o jogador atingiu a velocidade máxima para a esquerda
        else if (player.velocity.x <= -(maximumLateralVelocity + (intensity * maximumLateralVelocityIncreaseFactor)))
        {
            leftIsAllowed = false;
            rightIsAllowed = true;
        }
        // O jogador está dentro da velocidade limite e não está colidindo
        else if (!isColliding)
        {
            rightIsAllowed = true;
            leftIsAllowed = true;
        }
    }
    #endregion
}