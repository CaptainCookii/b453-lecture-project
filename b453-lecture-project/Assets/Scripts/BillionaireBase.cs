using UnityEngine;

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
    public SpriteRenderer sr;
    public Sprite redBase;
    public Sprite blueBase;
    public Sprite yellowBase;
    public Sprite greenBase;
    public Team team;

    // spawn variables
    public float spawnInterval = 1f;
    public int maxBillions = 10;
    public float spawnRadius = 0.5f;
    public int currentCount;
    private float timer;

    // turret variables
    public float rotationSpeed = 90f;
    public float rotationOffset = -90f;
    public float fireInterval = 1.1f;
    public float attackRange = 6f;
    public int shotDamage = 2;
    public float projectileSpeed = 9f;
    public float projectileMaxTravelDistance = 11f;

    void Start()
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
    void Update()
    {
        timer += Time.deltaTime;

        if (timer >= spawnInterval && currentCount < maxBillions)
        {
            SpawnBillion();
            timer = 0f;
        }

        Billion target = BillionTargeting.FindNearestEnemy(team, transform.position);
        if (target == null)
            return;

        Vector2 toTarget = target.transform.position - transform.position;
        float targetAngle = Mathf.Atan2(toTarget.y, toTarget.x) * Mathf.Rad2Deg + rotationOffset;

        Quaternion desiredRotation = Quaternion.Euler(0f, 0f, targetAngle);
        turret.rotation = Quaternion.RotateTowards(
            turret.rotation,
            desiredRotation,
            rotationSpeed * Time.deltaTime
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
            direction
        );

        currentCount++;
    }
}
