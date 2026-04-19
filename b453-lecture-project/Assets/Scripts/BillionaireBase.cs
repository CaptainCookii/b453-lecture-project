using TMPro;
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
    public GameObject capturePointPrefab;
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
    private float spawnInterval = 1f;
    private int maxBillions = 15;
    public int currentCount;
    private float billionTimer;

    // health variables
    private float maxHealth = 25f;
    private float currentHealth;

    // experience variables
    private int expToRankUp = 10;
    private int currentExp = 0;
    private int rank = 1;

    // turret variables
    private float rotationSpeed = 45f;
    private float rotationOffset = -90f;
    private float fireInterval = 3f;
    private float attackRange = 5f;
    private float shotDamage = 3;
    private float projectileSpeed = 9f;
    private float projectileMaxTravelDistance = 12f;
    private float shotTimer;

    private bool initialized = false;

    public void Initialize(Team team, int rank)
    {
        this.team = team;
        this.rank = rank;

        maxHealth = (rank * 3f) + 25;
        currentHealth = maxHealth;
        shotDamage = rank;

        SetSprite();
        UpdateUI();

        initialized = true;
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
        if (!initialized)
            return;

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
            GameObject capturePointObj = Instantiate(capturePointPrefab, transform.position, transform.rotation);
            capturePointObj.GetComponent<CapturePoint>().Initialize(Mathf.Max(1,rank/2));
            Debug.Log(capturePointObj);
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
            maxHealth = (rank * 3f) + 25;
            currentHealth = maxHealth;
            shotDamage = rank;
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
