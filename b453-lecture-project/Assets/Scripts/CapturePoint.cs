using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CapturePoint : MonoBehaviour
{
    // reference variables
    [SerializeField] private GameObject billionaireBasePrefab;
    [SerializeField] private Image captureFillImage;

    // capturepoint variables
    [SerializeField] private float captureRadius = 2f;
    [SerializeField] private float pointsPerSecondPerPawn = 2f;
    [SerializeField] private float pointsToCapture = 100f;
    private float captureProgress;

    // billionareBase variables
    [SerializeField] private float pushRadius = 2.8f;
    [SerializeField] private float pushForce = 8f;
    private int rank = 1;
    private Team? capturingTeam;

    public void Initialize(int rank)
    {
        this.rank = rank;
    }

    private void Update()
    {
        EvaluateCapture();
        UpdateVisuals();
    }

    private void EvaluateCapture()
    {
        Dictionary<Team, int> counts = CountBillionsInRange();

        if (counts.Count == 0)
            return;

        Team leadingTeam;
        int leadingCount;
        int secondCount;

        GetLeaderInfo(counts, out leadingTeam, out leadingCount, out secondCount);

        if (leadingCount <= 0)
            return;

        int margin = leadingCount - secondCount;
        if (margin <= 0)
            return;

        float delta = margin * pointsPerSecondPerPawn * Time.deltaTime;

        if (capturingTeam == null || capturingTeam == leadingTeam)
        {
            capturingTeam = leadingTeam;
            captureProgress += delta;
        }
        else
        {
            captureProgress -= delta;

            if (captureProgress <= 0f)
            {
                captureProgress = 0f;
                capturingTeam = leadingTeam;
            }
        }

        captureProgress = Mathf.Clamp(captureProgress, 0f, pointsToCapture);

        if (captureProgress >= pointsToCapture)
        {
            CompleteCapture(capturingTeam ?? leadingTeam);
        }
    }

    private Dictionary<Team, int> CountBillionsInRange()
    {
        Dictionary<Team, int> counts = new Dictionary<Team, int>();

        foreach (GameObject billion in BillionaireRegistry.All)
        {
            if (billion == null)
                continue;
            if (billion.GetComponent<BillionaireBase>())
                continue;

            float dist = Vector2.Distance(transform.position, billion.transform.position);
            if (dist > captureRadius)
                continue;

            if (!counts.ContainsKey(billion.GetComponent<Billion>().team))
                counts[billion.GetComponent<Billion>().team] = 0;

            counts[billion.GetComponent<Billion>().team]++;
        }

        return counts;
    }

    private void GetLeaderInfo(Dictionary<Team, int> counts, out Team leadingTeam, out int leadingCount, out int secondCount)
    {
        leadingTeam = default;
        leadingCount = 0;
        secondCount = 0;

        foreach (var kvp in counts)
        {
            int count = kvp.Value;

            if (count > leadingCount)
            {
                secondCount = leadingCount;
                leadingCount = count;
                leadingTeam = kvp.Key;
            }
            else if (count > secondCount)
            {
                secondCount = count;
            }
        }
    }

    private void UpdateVisuals()
    {
        if (captureFillImage == null)
            return;

        captureFillImage.fillAmount = captureProgress / pointsToCapture;

        if (!capturingTeam.Equals(null))
            captureFillImage.color = GetColor(capturingTeam.Value);
    }

    private void CompleteCapture(Team winningTeam)
    {
        if (billionaireBasePrefab == null)
            return;

        PushNearbyBillionsAway();

        GameObject billionaireBase = Instantiate(billionaireBasePrefab, transform.position, Quaternion.identity);
        billionaireBase.GetComponent<BillionaireBase>().Initialize(winningTeam, rank);

        Destroy(gameObject);
    }

    private void PushNearbyBillionsAway()
    {
        foreach (GameObject billion in BillionaireRegistry.All)
        {
            if (billion == null)
                continue;

            Rigidbody2D rb = billion.GetComponent<Rigidbody2D>();
            if (rb == null)
                continue;

            Vector2 away = (Vector2)billion.transform.position - (Vector2)transform.position;
            float dist = away.magnitude;

            if (dist > pushRadius)
                continue;

            if (dist < 0.001f)
                away = Random.insideUnitCircle.normalized;
            else
                away.Normalize();

            rb.AddForce(away * pushForce, ForceMode2D.Impulse);
        }
    }

    public static Color GetColor(Team team)
    {
        switch (team)
        {
            case Team.green: return new Color32(7, 92, 67, 255);
            case Team.yellow: return new Color32(133, 100, 51, 255);
            case Team.blue: return new Color32(15, 97, 127, 255);
            case Team.red: return new Color32(101, 33, 55, 255);
            default: return Color.white;
        }
    }
}
