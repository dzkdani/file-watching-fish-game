using UnityEngine;
using UnityEngine.UIElements;

public class FishController : MonoBehaviour
{
    private enum State
    {
        Idle,
        Wander,
        SeekFood,
        Fear
    }

    [SerializeField] private FishConfig config;


    [Header("Refs")]
    [SerializeField] private MovementController movement;
    [SerializeField] private SeparationBehavior separation;
    [SerializeField] private FoodSensor foodSensor;

    [Header("Config")]
    [SerializeField] private float wanderInterval = 1.5f;
    [SerializeField] private float fearDuration = 1f;
    [SerializeField] private float fearSpeedMultiplier = 1.5f;
    [SerializeField] private float foodArriveDistance = 0.15f;
    [SerializeField] private State state = State.Wander;

    private Vector2 direction;
    private float speed;
    private float minSpeed;
    private float maxSpeed;

    private float wanderTimer;
    private float fearTimer;

    [SerializeField] private float hunger;
    [SerializeField] private float maxHunger;
    [SerializeField] private float hungerDecay;
    [SerializeField] private float hungerCooldown;
    [SerializeField] private float hungerCooldownTimer;

    private void OnEnable()
    {
        Init();

        ApplyBaseConfig();
        
        ConfigSystem.Initialize();
        ConfigSystem.ConfigChanged += ApplyRuntimeConfig;

        if (ConfigSystem.Data != null)
            ApplyRuntimeConfig(ConfigSystem.Data);

        hunger = maxHunger;
        EnterIdle();
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
        foodSensor = GetComponent<FoodSensor>();
        if (foodSensor == null)
            foodSensor = gameObject.AddComponent<FoodSensor>();
        if (config == null)
            config = ScriptableObject.CreateInstance<FishConfig>();
    }

    void Start()
    {
        state = State.Wander;
    }

    private void Update()
    {
        float dt = Time.deltaTime;

        TickHunger(dt);
        TickState(dt);

        Vector2 sep = separation.Calculate(transform.position, transform);
        Vector2 moveDir = (direction + sep).normalized;

        movement.Move(moveDir, speed, dt);

        if (moveDir.sqrMagnitude > Mathf.Epsilon)
        {
            direction = moveDir;
        }
    }

    // =========================
    // STATE LOGIC
    // =========================

    private void TickState(float dt)
    {
        switch (state)
        {
            case State.Idle:
                speed = 0f;
                break;

            case State.Wander:
                wanderTimer -= dt;
                if (wanderTimer <= 0f)
                {
                    direction = Random.insideUnitCircle.normalized;
                    speed = Random.Range(minSpeed, maxSpeed);
                    wanderTimer = wanderInterval;
                }
                break;

            case State.SeekFood:
                if (foodSensor.TryGetNearest(transform.position, out var food))
                {
                    Vector2 toFood = food.transform.position - transform.position;

                    if (toFood.sqrMagnitude <= foodArriveDistance * foodArriveDistance)
                    {
                        Consume(food);
                        return;
                    }

                    direction = toFood.normalized;
                    speed = maxSpeed;
                }
                break;

            case State.Fear:
                fearTimer -= dt;
                speed = maxSpeed * fearSpeedMultiplier;

                if (fearTimer <= 0f)
                    EnterWander();
                break;
        }
    }

    private void TickHunger(float dt)
    {
        if (hungerCooldownTimer > 0f)
        {
            hungerCooldownTimer -= dt;
            hunger -= hungerDecay * dt;
        }

        if (hunger <= 0f && state != State.Fear)
            EnterSeekFood();
    }

    // =========================
    // STATE TRANSITIONS
    // =========================

    public void TriggerFear()
    {
        state = State.Fear;
        fearTimer = fearDuration;
        direction = -direction;
    }

    private void EnterIdle()
    {
        state = State.Idle;
    }

    private void EnterWander()
    {
        state = State.Wander;
        wanderTimer = 0f;
    }

    private void EnterSeekFood()
    {
        state = State.SeekFood;
    }

    // =========================
    // ACTIONS
    // =========================

    private void Consume(Collider2D food)
    {
        if (SpawnSystem.Instance != null)
            SpawnSystem.Instance.Despawn(food.gameObject, food.tag);
        else
            Destroy(food.gameObject);

        hunger = maxHunger;
        hungerCooldownTimer = hungerCooldown;

        EnterWander();
    }

    // =========================
    // CONFIG
    // =========================

    private void ApplyBaseConfig()
    {
        if (config == null)
        {
            Debug.LogWarning($"{name} missing FishBehaviorConfig");
            return;
        }

        minSpeed = config.minSpeed;
        maxSpeed = config.maxSpeed;

        float cMin = config.maxSpeed;
        float cMax = config.minSpeed;

        if (cMin > 0f) minSpeed = cMin;
        if (cMax > 0f) maxSpeed = cMax;

        ValidateSpeed();

        speed = Mathf.Clamp(speed, minSpeed, maxSpeed);

        maxHunger =  config.maxHunger;
        hungerCooldown = config.hungerCooldown;
        hungerDecay = config.maxHunger / config.hungerCooldown;
        hungerCooldownTimer = hungerCooldown;

        float radius = config.detectionRadius > 0 ? config.detectionRadius : config.detectionRadius;
        foodSensor.SetRadius(radius);
    }

    private void ApplyRuntimeConfig(ConfigData r)
    {
        if (r == null) return;

        float rMin = r.fishMinSpd;
        float rMax = r.fishMaxSpd;

        if (rMin > 0f) minSpeed = rMin;
        if (rMax > 0f) maxSpeed = rMax;

        ValidateSpeed();

        speed = Mathf.Clamp(speed, minSpeed, maxSpeed);      
        
        maxHunger = r.fishMaxHungerMeter > 0 ? r.fishMaxHungerMeter : config.maxHunger;
        hungerCooldown = r.fishHungerCooldown > 0 ? r.fishHungerCooldown : config.hungerCooldown;
        hungerDecay = maxHunger / hungerCooldown;
        hungerCooldownTimer = hungerCooldown;
    
        float radius = r.foodDetectionRadius > 0 ? r.foodDetectionRadius : config.detectionRadius;
        foodSensor.SetRadius(radius);
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
}