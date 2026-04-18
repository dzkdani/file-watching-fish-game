using UnityEngine;

public class FishController : MonoBehaviour
{
    private enum FishState
    {
        Idle,
        Wander,
        SeekFood,
        Fear
    }

    [Header("Behavior")]
    [SerializeField] private float wanderDirectionChangeInterval = 1.5f;
    [SerializeField] private float movementBoundsRadius = 5.0f;
    [SerializeField] private float fearDuration = 1f;
    [SerializeField] private float fearSpeedMultiplier = 1.5f;
    [SerializeField] private float foodArriveDistance = 0.15f;
    [SerializeField] private float facingThreshold = 0.1f;
    [SerializeField] private float hungerCooldownSeconds;
    [SerializeField] private float hungerDecay;
    [SerializeField] private float maxHungerMeter;
    [SerializeField] private float currentHungerMeter;
    [SerializeField] private float hungerCooldownTimer;

    [Header("Separation")]
    [SerializeField] private float separationRadius = 0.9f;
    [SerializeField] private float separationStrength = 1.25f;

    [Header("Fallback Config")]
    [SerializeField] private float defaultMinSpeed = 1f;
    [SerializeField] private float defaultMaxSpeed = 2f;
    [SerializeField] private float defaultDetectionRadius = 5f;
    [SerializeField] private float defaultHungerCooldown = 3f;
    [SerializeField] private float defaultHungerDecay = 10f;
    [SerializeField] private float defaultMaxHungerMeter = 100f;

    private readonly Collider2D[] foodBuffer = new Collider2D[32];
    private readonly Collider2D[] separationBuffer = new Collider2D[32];
    private ContactFilter2D overlapFilter;

    [SerializeField] private FishState state = FishState.Wander;
    private Vector2 direction;
    private float speed;
    private float minSpeed;
    private float maxSpeed;
    private float detectionRadius;
    private float fearTimer;
    private float wanderTimer;
    private float baseScaleAbsX;

    private void OnEnable()
    {
        CacheBaseScaleAndFaceLeft();
        overlapFilter = new ContactFilter2D();
        overlapFilter.useTriggers = true;

        ConfigSystem.Initialize();
        ConfigSystem.ConfigChanged += ApplyConfig;

        if (ConfigSystem.Data != null)
        {
            ApplyConfig(ConfigSystem.Data);
        }

        currentHungerMeter = maxHungerMeter;
        EnterIdleState();
    }

    private void OnDisable()
    {
        ConfigSystem.ConfigChanged -= ApplyConfig;
    }

    private void Update()
    {
        float deltaTime = Time.deltaTime;

        TickHunger(deltaTime);
        TickState(deltaTime);
        Move(deltaTime);
    }

    public void TriggerFear()
    {
        state = FishState.Fear;
        fearTimer = fearDuration;

        if (direction.sqrMagnitude > Mathf.Epsilon)
        {
            direction = (-direction).normalized;
        }
        else
        {
            PickNewWanderDirection();
        }

        speed = maxSpeed * fearSpeedMultiplier;
    }

    private void TickHunger(float deltaTime)
    {
        if (hungerCooldownTimer > 0f)
        {
            hungerCooldownTimer -= deltaTime;

            if (hungerCooldownTimer <= 0f && state == FishState.Idle)
            {
                EnterWanderState(forceNewDirection: true);
            }

            return;
        }

        if (currentHungerMeter <= 0f)
        {
            return;
        }

        currentHungerMeter = Mathf.Max(0f, currentHungerMeter - hungerDecay * deltaTime);
        if (currentHungerMeter <= 0f && state != FishState.Fear)
        {
            EnterSeekFoodState();
        }
    }

    private void TickState(float deltaTime)
    {
        switch (state)
        {
            case FishState.Idle:
                TickIdle();
                break;
            case FishState.Wander:
                TickWander(deltaTime);
                break;
            case FishState.SeekFood:
                TickSeekFood();
                break;
            case FishState.Fear:
                TickFear(deltaTime);
                break;
        }
    }

    private void TickIdle()
    {
        speed = 0f;
        direction = Vector2.zero;

        if (hungerCooldownTimer <= 0f)
        {
            EnterWanderState(forceNewDirection: true);
        }
    }

    private void TickWander(float deltaTime)
    {
        if (currentHungerMeter <= 0f)
        {
            EnterSeekFoodState();
            return;
        }

        wanderTimer -= deltaTime;
        if (wanderTimer <= 0f || direction.sqrMagnitude <= Mathf.Epsilon)
        {
            PickNewWanderDirection();
            speed = Random.Range(minSpeed, maxSpeed);
        }

        KeepDirectionInsideBounds();
    }

    private void TickSeekFood()
    {
        if (!TryGetNearestFood(out Collider2D nearestFood))
        {
            EnterSeekFoodState();
            return;
        }

        Vector2 toFood = (Vector2)nearestFood.transform.position - (Vector2)transform.position;
        if (toFood.sqrMagnitude <= foodArriveDistance * foodArriveDistance)
        {
            ConsumeFood(nearestFood);
            return;
        }

        direction = toFood.normalized;
        speed = maxSpeed;
    }

    private void TickFear(float deltaTime)
    {
        fearTimer -= deltaTime;
        if (fearTimer <= 0f)
        {
            if (currentHungerMeter <= 0f)
            {
                EnterSeekFoodState();
            }
            else if (hungerCooldownTimer > 0f)
            {
                EnterIdleState();
            }
            else
            {
                EnterWanderState(forceNewDirection: false);
            }
            return;
        }

        speed = maxSpeed * fearSpeedMultiplier;
        if (direction.sqrMagnitude <= Mathf.Epsilon)
        {
            PickNewWanderDirection();
        }

        KeepDirectionInsideBounds();
    }

    private void Move(float deltaTime)
    {
        Vector2 separation = ComputeSeparation();
        Vector2 moveDirection = direction;
        if (separation.sqrMagnitude > Mathf.Epsilon)
        {
            moveDirection = (moveDirection + separation * separationStrength).normalized;
        }

        float currentSpeed = speed;
        if (state == FishState.Idle && separation.sqrMagnitude > Mathf.Epsilon)
        {
            currentSpeed = Mathf.Max(minSpeed * 0.5f, 0.01f);
        }

        if (moveDirection.sqrMagnitude <= Mathf.Epsilon || currentSpeed <= 0f)
        {
            return;
        }

        Vector3 nextPosition = transform.position + (Vector3)(moveDirection * currentSpeed * deltaTime);
        nextPosition = ClampToBounds(nextPosition);
        transform.position = nextPosition;

        if (state != FishState.Idle)
        {
            direction = moveDirection;
        }

        UpdateFacing(moveDirection.x);
    }

    private void EnterIdleState()
    {
        state = FishState.Idle;
        speed = 0f;
        direction = Vector2.zero;
    }

    private void EnterWanderState(bool forceNewDirection)
    {
        state = FishState.Wander;
        speed = Random.Range(minSpeed, maxSpeed);

        if (forceNewDirection || direction.sqrMagnitude <= Mathf.Epsilon)
        {
            PickNewWanderDirection();
        }
    }

    private void EnterSeekFoodState()
    {
        state = FishState.SeekFood;
        speed = minSpeed;
    }

    private void PickNewWanderDirection()
    {
        direction = Random.insideUnitCircle.normalized;
        wanderTimer = wanderDirectionChangeInterval;
    }

    private bool TryGetNearestFood(out Collider2D nearestFood)
    {
        nearestFood = null;
        float nearestDistanceSquared = float.MaxValue;

        int count = Physics2D.OverlapCircle(transform.position, detectionRadius, overlapFilter, foodBuffer);
        for (int i = 0; i < count; i++)
        {
            Collider2D candidate = foodBuffer[i];
            if (candidate == null || !candidate.CompareTag("Food"))
            {
                continue;
            }

            Vector2 delta = (Vector2)candidate.transform.position - (Vector2)transform.position;
            float distanceSquared = delta.sqrMagnitude;
            if (distanceSquared >= nearestDistanceSquared)
            {
                continue;
            }

            nearestDistanceSquared = distanceSquared;
            nearestFood = candidate;
        }

        return nearestFood != null;
    }

    private Vector2 ComputeSeparation()
    {
        Vector2 result = Vector2.zero;
        int count = Physics2D.OverlapCircle(transform.position, separationRadius, overlapFilter, separationBuffer);

        for (int i = 0; i < count; i++)
        {
            Collider2D collider = separationBuffer[i];
            if (collider == null || collider.gameObject == gameObject) 
            {
                continue;
            }

            if (!collider.CompareTag("Fish") && !collider.CompareTag("Trash")) 
            {
                continue;
            }

            Vector2 away = (Vector2)transform.position - (Vector2)collider.transform.position;
            float distance = away.magnitude;
            if (distance <= 0.001f)
            {
                continue;
            }

            float weight = 1f - Mathf.Clamp01(distance / separationRadius);
            result += (away / distance) * weight;
        }

        return result;
    }

    private void ConsumeFood(Collider2D foodCollider)
    {
        if (foodCollider != null)
        {
            Destroy(foodCollider.gameObject);
        }

        currentHungerMeter = maxHungerMeter;
        hungerCooldownTimer = hungerCooldownSeconds;
        EnterIdleState();
    }

    private void KeepDirectionInsideBounds()
    {
        if (!TryGetActiveBounds(out Rect bounds))
        {
            return;
        }

        Vector2 currentPosition = transform.position;
        if (bounds.Contains(currentPosition))
        {
            return;
        }

        Vector2 nearestInBounds = new Vector2(
            Mathf.Clamp(currentPosition.x, bounds.xMin, bounds.xMax),
            Mathf.Clamp(currentPosition.y, bounds.yMin, bounds.yMax));

        Vector2 inward = nearestInBounds - currentPosition;
        if (inward.sqrMagnitude <= Mathf.Epsilon)
        {
            PickNewWanderDirection();
            return;
        }

        direction = inward.normalized;
    }

    private Vector3 ClampToBounds(Vector3 position)
    {
        if (!TryGetActiveBounds(out Rect bounds))
        {
            return position;
        }

        float clampedX = Mathf.Clamp(position.x, bounds.xMin, bounds.xMax);
        float clampedY = Mathf.Clamp(position.y, bounds.yMin, bounds.yMax);
        return new Vector3(clampedX, clampedY, position.z);
    }

    private bool TryGetActiveBounds(out Rect bounds)
    {
        if (SpawnSystem.Instance != null && SpawnSystem.Instance.TryGetMovementBounds(out bounds))
        {
            return true;
        }

        Vector2 center = GetFallbackBoundsCenter();
        float radius = Mathf.Max(0.01f, movementBoundsRadius);
        bounds = Rect.MinMaxRect(center.x - radius, center.y - radius, center.x + radius, center.y + radius);
        return true;
    }

    private Vector2 GetFallbackBoundsCenter()
    {
        if (SpawnSystem.Instance != null && SpawnSystem.Instance.spawnParent != null)
        {
            return SpawnSystem.Instance.spawnParent.position;
        }

        return Vector2.zero;
    }

    private void CacheBaseScaleAndFaceLeft()
    {
        baseScaleAbsX = Mathf.Abs(transform.localScale.x);
        if (baseScaleAbsX <= Mathf.Epsilon)
        {
            baseScaleAbsX = 1f;
        }

        transform.localScale = new Vector3(baseScaleAbsX, transform.localScale.y, transform.localScale.z);
    }

    private void UpdateFacing(float horizontalDirection)
    {
        if (horizontalDirection > facingThreshold)
        {
            transform.localScale = new Vector3(-baseScaleAbsX, transform.localScale.y, transform.localScale.z);
        }
        else if (horizontalDirection < -facingThreshold)
        {
            transform.localScale = new Vector3(baseScaleAbsX, transform.localScale.y, transform.localScale.z);
        }
    }

    private void ApplyConfig(ConfigData config)
    {
        if (config == null)
        {
            return;
        }

        minSpeed = config.fishMinSpd > 0f ? config.fishMinSpd : defaultMinSpeed;
        maxSpeed = config.fishMaxSpd > 0f ? config.fishMaxSpd : defaultMaxSpeed;

        if (maxSpeed < minSpeed)
        {
            float temp = minSpeed;
            minSpeed = maxSpeed;
            maxSpeed = temp;
        }

        detectionRadius = config.foodDetectionRadius > 0f ? config.foodDetectionRadius : defaultDetectionRadius;
        maxHungerMeter = config.fishMaxHungerMeter > 0f ? config.fishMaxHungerMeter : defaultMaxHungerMeter; 
        hungerCooldownSeconds = config.fishHungerCooldown > 0f ? config.fishHungerCooldown : defaultHungerCooldown;
        hungerDecay = config.fishHungerDecay > 0f ? config.fishHungerDecay : defaultHungerDecay;

        hungerCooldownTimer = hungerCooldownSeconds;
        currentHungerMeter = Mathf.Clamp(currentHungerMeter, 0f, maxHungerMeter);

        switch (state)
        {
            case FishState.Wander:
                speed = Mathf.Clamp(speed, minSpeed, maxSpeed);
                break;
            case FishState.SeekFood:
                speed = minSpeed;
                break;
            case FishState.Fear:
                speed = maxSpeed * fearSpeedMultiplier;
                break;
            case FishState.Idle:
                speed = 0f;
                break;
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (state != FishState.SeekFood)
        {
            return;
        }

        if (!other.CompareTag("Food"))
        {
            return;
        }

        ConsumeFood(other);
    }
}
