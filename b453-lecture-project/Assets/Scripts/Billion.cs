using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class Billion : MonoBehaviour
{
    // reference variables
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
    public LayerMask billionLayer;
    public Team team;
    public Transform innerCircle;
    public Transform firePoint;
    public GameObject bulletPrefab;
    private Rigidbody2D rb;
    private Flag targetFlag;
    private BillionaireBase owningBase;
    public Transform ringRoot;
    public GameObject spikePrefab;
    private Color32 spikeColor;

    // health and experience variables
    private float maxHealth;
    private float currentHealth;
    [SerializeField] private float minInnerScale = 0.30f;
    private int expOnDeath;

    // movement variables
    private float maxSpeed;
    [SerializeField] private float acceleration = 6f;
    [SerializeField] private float decelerationRadius = 1.5f;
    [SerializeField] private float separationRadius = .1f;
    [SerializeField] private float separationForce = .15f;

    // turret variables
    [SerializeField] private float fireInterval = 1.5f;
    [SerializeField] private float attackRange = 3f;
    private float shotDamage;
    [SerializeField] private float projSpeed = 12f;
    [SerializeField] private float projMaxTravelDist = 7f;
    private float timer;

    // rank variables
    private int rank;
    [SerializeField] private float radius = 0.38f;
    [SerializeField] private float baseRotationSpeed = 40f;
    [SerializeField] private float rotationSpeedPerRank = 8f;
    private float rotationSpeed;

    // initialization variable
    private bool initialized = false;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    private void OnEnable()
    {
        BillionaireRegistry.Register(gameObject);
    }

    private void OnDisable()
    {
        BillionaireRegistry.Unregister(gameObject);
    }

    public void Initialize(BillionaireBase owner, Team team, Vector2 direction, int rank)
    {
        owningBase = owner;
        this.team = team;
        this.rank = rank;
        
        maxHealth = rank * 2.5f;
        currentHealth = maxHealth;
        expOnDeath = rank;
        maxSpeed = (rank /2f) + 3;
        shotDamage = rank / 2f;

        SetSprite();
        SetRank();
        SetDirection(direction);

        initialized = true;
    }

    private void FixedUpdate()
    {
        if (!initialized)
            return;

        ApplySeparation();
        MoveTowardFlag();
    }

    private void Update()
    {
        if (!initialized)
            return;

        if (ringRoot != null)
            ringRoot.Rotate(0f, 0f, rotationSpeed * Time.deltaTime);

        timer += Time.deltaTime;
        GameObject targetObj = BillionTargeting.FindNearestEnemy(team, transform.position);
        if (targetObj == null)
            return;

        Vector2 toTarget = targetObj.transform.position - transform.position;
        float angle = Mathf.Atan2(toTarget.y, toTarget.x) * Mathf.Rad2Deg;
        transform.rotation = Quaternion.Euler(0f, 0f, angle);
        
        float distance = Vector2.Distance(transform.position, targetObj.transform.position);
        if (distance > attackRange || timer < fireInterval)
            return;

        Fire();
        timer = 0f;
    }

    public void SetRank()
    {
        rotationSpeed = baseRotationSpeed + (rank * rotationSpeedPerRank);

        if (ringRoot == null)
            return;

        for (int i = 0; i < rank; i++)
        {
            float t = (float)i / rank;
            float angle = t * Mathf.PI * 2f;

            GameObject spike = Instantiate(spikePrefab, ringRoot);
            spike.transform.localPosition = new Vector3(Mathf.Cos(angle), Mathf.Sin(angle), 0f) * radius;
            spike.transform.localRotation = Quaternion.Euler(0f, 0f, angle * Mathf.Rad2Deg + 90f);
            spike.GetComponent<SpriteRenderer>().color = spikeColor;
        }
    }

    private void Fire()
    {
        if (firePoint == null || bulletPrefab == null)
            return;

        GameObject bulletObj = Instantiate(
            bulletPrefab,
            firePoint.position,
            transform.rotation
        );

        Bullet shot = bulletObj.GetComponent<Bullet>();
        shot.Initialize(
                owningBase,
                team,
                shotDamage,
                projSpeed,
                projMaxTravelDist,
                false
        );
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

    private void ApplySeparation()
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

    public void TakeDamage(float amount, BillionaireBase source)
    {
        if (!initialized)
            return;

        if (amount <= 0 || currentHealth <= 0)
            return;

        currentHealth -= amount;

        if (currentHealth <= 0)
        {
            source.AddExperience(expOnDeath);
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
                spikeColor = new Color32(15, 97, 127, 255);
                break;
            case Team.yellow:
                sr.sprite = yellowBillion;
                srTurret.sprite = yellowTurret;
                spikeColor = new Color32(133, 100, 51, 255);
                break;
            case Team.red:
                sr.sprite = redBillion;
                srTurret.sprite = redTurret;
                spikeColor = new Color32(101, 33, 55, 255);
                break;
            case Team.green:
                sr.sprite = greenBillion;
                srTurret.sprite = greenTurret;
                spikeColor = new Color32(7, 92, 67, 255);
                break;
        }
    }
}
