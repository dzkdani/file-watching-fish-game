using UnityEngine;

public class TrashController : MonoBehaviour
{
    [Header("Movement")]
    [SerializeField] private float directionChangeInterval = 2f;
    [SerializeField] private float movementBoundsRadius = 5f;
    [SerializeField] private float separationRadius = 1.0f;
    [SerializeField] private float separationStrength = 1.2f;

    private readonly Collider2D[] separationBuffer = new Collider2D[24];
    private ContactFilter2D overlapFilter;

    private Vector2 direction;
    private float speed;
    private float minSpeed;
    private float maxSpeed;
    private float moveTimer;
    
    private bool initialized;

    private void OnEnable()
    {
        overlapFilter = new ContactFilter2D();
        overlapFilter.useTriggers = true;

        ConfigSystem.Initialize();
        ConfigSystem.ConfigChanged += ApplyConfig;

        if (ConfigSystem.Data != null)
        {
            ApplyConfig(ConfigSystem.Data);
        }
    }

    private void OnDisable()
    {
        ConfigSystem.ConfigChanged -= ApplyConfig;
    }

    public void Initialize(float minSpeedValue, float maxSpeedValue)
    {
        minSpeed = Mathf.Max(0f, minSpeedValue);
        maxSpeed = Mathf.Max(0f, maxSpeedValue);
        if (maxSpeed < minSpeed)
        {
            float temp = minSpeed;
            minSpeed = maxSpeed;
            maxSpeed = temp;
        }

        PickNewDirectionAndSpeed();
        initialized = true;
    }

    private void Update()
    {
        if (!initialized)
        {
            return;
        }

        float deltaTime = Time.deltaTime;
        moveTimer -= deltaTime;
        if (moveTimer <= 0f || direction.sqrMagnitude <= Mathf.Epsilon)
        {
            PickNewDirectionAndSpeed();
        }

        Move(deltaTime);
    }

    private void Move(float deltaTime)
    {
        Vector2 separation = ComputeFishSeparation();
        Vector2 moveDirection = direction;
        if (separation.sqrMagnitude > Mathf.Epsilon)
        {
            moveDirection = (moveDirection + separation * separationStrength).normalized;
        }

        if (moveDirection.sqrMagnitude <= Mathf.Epsilon)
        {
            return;
        }

        Vector3 previousPosition = transform.position;
        Vector3 nextPosition = previousPosition + (Vector3)(moveDirection * speed * deltaTime);
        Vector3 clampedPosition = ClampToBounds(nextPosition);
        transform.position = clampedPosition;

        Vector2 actualMove = (Vector2)clampedPosition - (Vector2)previousPosition;
        if (actualMove.sqrMagnitude <= Mathf.Epsilon)
        {
            direction = (-moveDirection).normalized;
        }
        else
        {
            direction = moveDirection;
        }
    }

    private Vector2 ComputeFishSeparation()
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

            if (!collider.CompareTag("Fish"))
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

    private void PickNewDirectionAndSpeed()
    {
        direction = Random.insideUnitCircle.normalized;
        speed = Random.Range(minSpeed, maxSpeed);
        moveTimer = directionChangeInterval;
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

    private void ApplyConfig(ConfigData config)
    {
        if (config == null)
        {
            return;
        }

        float configMin = config.trashMinSpd > 0f ? config.trashMinSpd : minSpeed;
        float configMax = config.trashMaxSpd > 0f ? config.trashMaxSpd : maxSpeed;

        if (configMin > 0f)
        {
            minSpeed = configMin;
        }

        if (configMax > 0f)
        {
            maxSpeed = configMax;
        }

        if (maxSpeed < minSpeed)
        {
            float temp = minSpeed;
            minSpeed = maxSpeed;
            maxSpeed = temp;
        }

        speed = Mathf.Clamp(speed, minSpeed, maxSpeed);
    }
}
