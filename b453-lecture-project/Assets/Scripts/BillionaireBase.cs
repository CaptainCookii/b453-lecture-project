using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public enum Team {
    green,
    yellow,
    red,
    blue
}

public class BillionaireBase : MonoBehaviour
{
    // reference variables
    public GameObject billionPrefab;
    public GameObject bulletPrefab;
    public Transform turret;
    public Transform firePoint;
    public SpriteRenderer sr;
    public Sprite redBase;
    public Sprite blueBase;
    public Sprite yellowBase;
    public Sprite greenBase;
    public Team team;
    public Image healthBar;
    public Image experienceBar;
    public TextMeshProUGUI rankText;

    // spawn variables
    public float spawnInterval = 1f;
    public int maxBillions = 10;
    public int currentCount;
    private float billionTimer;

    // health variables
    public float maxHealth = 25f;
    public float currentHealth;

    // experience variables
    public int expToRankUp = 10;
    public int currentExp = 0;
    public int rank = 1;

    // turret variables
    public float rotationSpeed = 45f;
    public float rotationOffset = -90f;
    public float fireInterval = 3f;
    public float attackRange = 5f;
    public int shotDamage = 2;
    public float projectileSpeed = 9f;
    public float projectileMaxTravelDistance = 12f;
    private float shotTimer;

    void Start()
    {    
        currentHealth = maxHealth;
        UpdateUI();
        SetSprite();
    }

    private void OnEnable()
    {
        BillionaireRegistry.Register(gameObject);
    }

    private void OnDisable()
    {
        BillionaireRegistry.Unregister(gameObject);
    }

    void Update()
    {
        billionTimer += Time.deltaTime;
        shotTimer += Time.deltaTime;

        if (billionTimer >= spawnInterval && currentCount < maxBillions)
        {
            SpawnBillion();
            billionTimer = 0f;
        }

        GameObject target = BillionTargeting.FindNearestEnemy(team, transform.position);
        if (target == null)
            return;

        Vector2 toTarget = target.transform.position - turret.position;
        float targetAngle = Mathf.Atan2(toTarget.y, toTarget.x) * Mathf.Rad2Deg + rotationOffset;
        Quaternion targetRotation = Quaternion.Euler(0f, 0f, targetAngle);
        turret.rotation = Quaternion.RotateTowards(
            turret.rotation,
            targetRotation,
            rotationSpeed * Time.deltaTime
        );

        float distance = Vector2.Distance(transform.position, target.transform.position);
        if (distance > attackRange || shotTimer < fireInterval)
            return;

        Fire(targetAngle - rotationOffset);
        shotTimer = 0f;
    }

    public void TakeDamage(float amount)
    {
        currentHealth -= amount;
        UpdateUI();

        if (currentHealth <= 0)
        {
            Destroy(gameObject);
        }
    }

    public void AddExperience(int amount)
    {
        currentExp += amount;
        UpdateUI();

        if (currentExp >= expToRankUp)
        {
            currentExp -= expToRankUp;
            rank++;
            expToRankUp = Mathf.CeilToInt(2f * Mathf.Pow(rank, 2.2f) + 8f);
        }
    }
    private void UpdateUI()
    {
        if (healthBar != null)
            healthBar.fillAmount = Mathf.Clamp01(currentHealth / maxHealth);

        if (experienceBar != null)
            experienceBar.fillAmount = Mathf.Clamp01((float)currentExp / expToRankUp);

        if (rankText != null)
            rankText.text = rank.ToString();
    }

    private void Fire(float targetAngle)
    {
        if (turret == null || bulletPrefab == null || firePoint == null)
            return;

        Quaternion targetRotation = Quaternion.Euler(turret.rotation.x, turret.rotation.y, targetAngle);
        GameObject shotObj = Instantiate(
            bulletPrefab,
            firePoint.position,
            targetRotation
        );

        Bullet shot = shotObj.GetComponent<Bullet>();
        shot.Initialize(
            this,
            team,
            shotDamage,
            projectileSpeed,
            projectileMaxTravelDistance,
            true
        );
    }

    void SpawnBillion()
    {
        Vector2 spawnPos = (Vector2)transform.position + new Vector2(1, 0);
        Vector2 direction = Random.insideUnitCircle.normalized;

        GameObject newBillion = Instantiate(
            billionPrefab,
            spawnPos,
            Quaternion.identity
        );

        Billion billion = newBillion.GetComponent<Billion>();
        billion.Initialize(
            gameObject.GetComponent<BillionaireBase>(),
            team,
            direction,
            rank
        );

        currentCount++;
    }

    void SetSprite()
    {
        switch (team)
        {
            case Team.blue:
                sr.sprite = blueBase;
                break;
            case Team.yellow:
                sr.sprite = yellowBase;
                break;
            case Team.red:
                sr.sprite = redBase;
                break;
            case Team.green:
                sr.sprite = greenBase;
                break;
        }
    }
}
