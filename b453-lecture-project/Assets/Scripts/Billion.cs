using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class Billion : MonoBehaviour
{
    // reference variables
    private Rigidbody2D rb;
    private Flag targetFlag;
    public SpriteRenderer sr;
    public Sprite blueBillion;
    public Sprite yellowBillion;
    public Sprite redBillion;
    public Sprite greenBillion;
    public SpriteRenderer srTurret;
    public Sprite blueTurret;
    public Sprite yellowTurret;
    public Sprite redTurret;
    public Sprite greenTurret;
    public Team team;
    public LayerMask billionLayer;
    public Transform innerCircle;
    public Transform turretMuzzle;
    public GameObject bulletPrefab;
    public BillionaireBase owningBase;
    
    //health variables
    public int maxHealth = 10;
    public int currentHealth = 10;
    public float minInnerScale = 0.30f;

    // movement variables
    public float maxSpeed = 3f;
    public float acceleration = 8f;
    public float decelerationRadius = 1.5f;
    public float separationRadius = .5f;
    public float separationForce = 5f;
    public float rotationOffset = 0f;

    //turret variables
    public float fireInterval = 3f;
    public float attackRange = 3f;
    public int shotDamage = 1;
    private float timer;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        
    }

    private void OnEnable()
    {
        BillionRegistry.Register(this);
    }

    private void OnDisable()
    {
        BillionRegistry.Unregister(this);
    }

    private void FixedUpdate()
    {
        
        ApplySeparation();
        MoveTowardFlag();
    }

    private void Update()
    {
        timer += Time.deltaTime;
        Billion target = FindNearestEnemy();
        if (target == null)
            return;

        Vector2 toTarget = target.transform.position - transform.position;
        float angle = Mathf.Atan2(toTarget.y, toTarget.x) * Mathf.Rad2Deg;
        transform.rotation = Quaternion.Euler(0f, 0f, angle + rotationOffset);
        
        float distance = Vector2.Distance(transform.position, target.transform.position);
        if (distance > attackRange || timer < fireInterval)
            return;

        Fire();
        timer = 0f;
    }

    private void Fire()
    {
        if (turretMuzzle == null || bulletPrefab == null)
            return;

        
        GameObject shotObj = Instantiate(
            bulletPrefab,
            turretMuzzle.position,
            transform.rotation
        );

        Bullet shot = shotObj.GetComponent<Bullet>();
        if (shot != null)
        {
            shot.Initialize(
                gameObject,
                team,
                shotDamage
            );
        }
    }

    private Billion FindNearestEnemy()
    {
        Billion nearest = null;
        float bestDistSqr = float.PositiveInfinity;
        Vector2 myPos = transform.position;

        foreach (var billion in BillionRegistry.All)
        {
            if (billion == null || billion == this)
                continue;

            if (billion.team == this.team)
                continue;

            float distSqr = ((Vector2)billion.transform.position - myPos).sqrMagnitude;
            if (distSqr < bestDistSqr)
            {
                bestDistSqr = distSqr;
                nearest = billion;
            }
        }

        return nearest;
    }

    private void UpdateVisual()
    {
        if (innerCircle == null)
            return;

        float health01 = (float)currentHealth / maxHealth;
        float scale = Mathf.Lerp(minInnerScale, 1f, health01);
        innerCircle.localScale = new Vector3(scale, scale, 1f);
    }

    private void MoveTowardFlag()
    {
        targetFlag = FlagManager.Instance.GetNearestFlag(transform.position, team);
        if (targetFlag == null) return;
        

        Vector2 toTarget = (Vector2)targetFlag.transform.position - rb.position;       
        float distance = toTarget.magnitude;
        if (distance < 0.05f) return;

        Vector2 desiredDirection = toTarget.normalized;        
        float speedFactor = Mathf.Clamp01(distance / decelerationRadius);       
        float targetSpeed = maxSpeed * speedFactor;        
        Vector2 desiredVelocity = desiredDirection * targetSpeed;       
        Vector2 steering = desiredVelocity - rb.linearVelocity;        
        rb.AddForce(steering * acceleration);
    }

    void ApplySeparation()
    {
        Collider2D[] neighbors = Physics2D.OverlapCircleAll(
            transform.position,
            separationRadius,
            billionLayer
        );

        Vector2 separationVector = Vector2.zero;

        foreach (Collider2D neighbor in neighbors)
        {
            if (neighbor.gameObject == gameObject) continue;

            Vector2 diff = (Vector2)transform.position - (Vector2)neighbor.transform.position;
            float dist = diff.magnitude;

            if (dist > 0)
                separationVector += diff.normalized / dist;
        }

        rb.AddForce(separationVector * separationForce);
    }

    public void TakeDamage(int amount)
    {
        if (amount <= 0 || currentHealth <= 0)
            return;

        currentHealth -= amount;

        if (currentHealth <= 0)
        {
            owningBase.currentCount--;
            Destroy(gameObject);
            return;
        }

        UpdateVisual();
    }

    public void SetDirection(Vector2 direction)
    {
        rb.linearVelocity = direction.normalized * .1f;
    }

    public void SetSprite()
    {
        switch (team)
        {
            case Team.blue:
                sr.sprite = blueBillion;
                srTurret.sprite = blueTurret;
                break;
            case Team.yellow:
                sr.sprite = yellowBillion;
                srTurret.sprite = yellowTurret;
                break;
            case Team.red:
                sr.sprite = redBillion;
                srTurret.sprite = redTurret;
                break;
            case Team.green:
                sr.sprite = greenBillion;
                srTurret.sprite = greenTurret;
                break;
        }
    }
}
