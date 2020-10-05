using UnityEngine;

public class ParticleManager : MonoBehaviour
{
    #region Public Variables
    [Space]
    [Header("Grass Platform")]
    public float grassPlatformStartLifeTime;
    public float grassPlatformGravity;
    public float grassPlatformImpactScaler;
    public float grassPlatformEmissionRateScaler;
    public float grassPlatformMinimumEmissionRate;
    public float grassPlatformMaximumEmissionRate;
    [Space]
    [Header("Walls")]
    public float wallsStartLifeTime;
    public float wallsGravity;
    public float wallsImpactScaler;
    public float wallsEmissionRateScaler;
    public float wallsMinimumEmissionRate;
    public float wallsMaximumEmissionRate;
    [Header("Textures")]
    public Texture2D grassPlatformParticles;
    public Texture2D wallsParticles;
    #endregion

    #region Private Variables
    private Vector2 playerVelocityOnCollision;
    private Vector2 normal;
    private float emissionRate;
    private string otherObjectTag;
    private ParticleSystem particles;
    private ParticleSystem.MainModule particles_Main;
    private ParticleSystem.EmissionModule particles_Emission;
    private ParticleSystem.ShapeModule particles_Shape;
    private ParticleSystem.VelocityOverLifetimeModule particles_VelocityOverLifetime;
    private ParticleSystem.InheritVelocityModule particles_InheritVelocity;
    private ParticleSystem.ColorOverLifetimeModule particles_ColorOverLifetime;
    private ParticleSystem.CollisionModule particles_Collision;
    #endregion

    #region Unity Methods
    private void Start()
    {
        particles = gameObject.GetComponent<ParticleSystem>();

        particles_Main = particles.main;
        particles_Emission = particles.emission;
        particles_Shape = particles.shape;
        particles_VelocityOverLifetime = particles.velocityOverLifetime;
        particles_InheritVelocity = particles.inheritVelocity;
        particles_ColorOverLifetime = particles.colorOverLifetime;
        particles_Collision = particles.collision;
    }

    private void FixedUpdate()
    {
        playerVelocityOnCollision = gameObject.GetComponent<Rigidbody2D>().velocity;
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.contactCount > 0)
        {
            otherObjectTag = collision.gameObject.tag;

            normal = collision.GetContact(0).normal;

            switch (otherObjectTag)
            {
                case "Grass Platform":

                    SetParticleBehaviour(grassPlatformStartLifeTime, grassPlatformGravity, grassPlatformParticles, normal, grassPlatformEmissionRateScaler, grassPlatformMaximumEmissionRate, grassPlatformMinimumEmissionRate, grassPlatformImpactScaler);
                    break;

                case "Walls":

                    SetParticleBehaviour(wallsStartLifeTime, wallsGravity, wallsParticles, normal, wallsEmissionRateScaler, wallsMaximumEmissionRate, wallsMinimumEmissionRate, wallsImpactScaler);
                    break;
                default:
                    break;
            }
        }
    }
    #endregion

    #region Methods
    private void SetParticleBehaviour(float startLifeTime, float gravity, Texture2D texture, Vector2 normal, float emissionRateScaler, float maximumEmissionRate, float minimumEmissionRate, float impactScaler)
    {
        particles_Main.startLifetime = startLifeTime;
        particles_Main.gravityModifier = gravity;
        particles_Shape.texture = texture;

        emissionRate = playerVelocityOnCollision.magnitude * emissionRateScaler;

        if (emissionRate > maximumEmissionRate)
        {
            emissionRate = maximumEmissionRate;
        }

        particles_Emission.rateOverTime = emissionRate;

        particles_VelocityOverLifetime.x = normal.x * playerVelocityOnCollision.magnitude * impactScaler;
        particles_VelocityOverLifetime.y = normal.y * playerVelocityOnCollision.magnitude * impactScaler;

        if (emissionRate >= minimumEmissionRate)
        {
            particles.Play();
        }
    }
    #endregion
}