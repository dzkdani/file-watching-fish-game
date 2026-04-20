using UnityEngine;

public class TrashController : MonoBehaviour, IPoolable
{
    [Header("Refs")]
    [SerializeField] private MovementController movement;
    [SerializeField] private SeparationBehavior separation;
    [SerializeField] private TrashConfig config;

    private Vector2 direction;
    private float speed;
    private float minSpeed;
    private float maxSpeed;
    private float moveTimer;

    private float separationStrength;
    private float directionChangeInterval;

    private void OnEnable()
    {
        Init();

        ApplyBaseConfig();

        ConfigSystem.Initialize();
        ConfigSystem.ConfigChanged += ApplyRuntimeConfig;

        if (ConfigSystem.Data != null)
            ApplyRuntimeConfig(ConfigSystem.Data);

        PickNewDirectionAndSpeed();
    }

    private void OnDisable()
    {
        ConfigSystem.ConfigChanged -= ApplyRuntimeConfig;
    }

    private void Init()
    {
        movement = GetComponent<MovementController>();
        if (movement == null)
            movement = gameObject.AddComponent<MovementController>();
        separation = GetComponent<SeparationBehavior>();
        if (separation == null)
            separation = gameObject.AddComponent<SeparationBehavior>();
        if (config == null)
            config = ScriptableObject.CreateInstance<TrashConfig>();
    }

    private void Update()
    {
        float dt = Time.deltaTime;

        moveTimer -= dt;
        if (moveTimer <= 0f || direction.sqrMagnitude <= Mathf.Epsilon)
        {
            PickNewDirectionAndSpeed();
        }

        Vector2 sep = separation.Calculate(transform.position, transform) * separationStrength;
        Vector2 moveDir = Blend(direction, sep);

        movement.Move(moveDir, speed, dt);

        if (moveDir.sqrMagnitude > Mathf.Epsilon)
        {
            direction = moveDir;
        }
    }

    // =========================
    // CONFIG
    // =========================

    private void ApplyBaseConfig()
    {
        if (config == null)
        {
            Debug.LogWarning($"{name} missing TrashBehaviorConfig");
            return;
        }

        minSpeed = config.minSpeed;
        maxSpeed = config.maxSpeed;
        directionChangeInterval = config.directionChangeInterval;
        separationStrength = config.separationStrength;

        ValidateSpeed();
    }

    private void ApplyRuntimeConfig(ConfigData runtime)
    {
        if (runtime == null) return;

        float rMin = runtime.trashMinSpd;
        float rMax = runtime.trashMaxSpd;

        if (rMin > 0f) minSpeed = rMin;
        if (rMax > 0f) maxSpeed = rMax;

        ValidateSpeed();

        speed = Mathf.Clamp(speed, minSpeed, maxSpeed);
    }

    private void ValidateSpeed()
    {
        if (maxSpeed < minSpeed)
        {
            float tmp = minSpeed;
            minSpeed = maxSpeed;
            maxSpeed = tmp;
        }
    }

    // =========================
    // MOVEMENT
    // =========================

    private void PickNewDirectionAndSpeed()
    {
        direction = Random.insideUnitCircle.normalized;
        speed = Random.Range(minSpeed, maxSpeed);
        moveTimer = directionChangeInterval;
    }

    private Vector2 Blend(Vector2 baseDir, Vector2 separation)
    {
        if (separation.sqrMagnitude <= Mathf.Epsilon)
            return baseDir;

        return (baseDir + separation).normalized;
    }

    public void ReturnToPool()
    {
        gameObject.ReturnToPool();
    }
}