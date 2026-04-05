using UnityEngine;

public class BillionTurretAim : MonoBehaviour
{
    public Transform owner;
    public float rotationOffset = -90f;

    private Billion ownerBillion;

    private void Awake()
    {
        if (owner == null)
            owner = transform.parent;

        ownerBillion = owner != null ? owner.GetComponent<Billion>() : null;
    }

    private void LateUpdate()
    {
        if (ownerBillion == null)
            return;

        Billion nearestEnemy = FindNearestEnemy();
        if (nearestEnemy == null)
            return;

        Vector2 toTarget = nearestEnemy.transform.position - owner.position;
        float angle = Mathf.Atan2(toTarget.y, toTarget.x) * Mathf.Rad2Deg;

        transform.rotation = Quaternion.Euler(0f, 0f, angle + rotationOffset);
    }

    private Billion FindNearestEnemy()
    {
        Billion nearest = null;
        float bestDistSqr = float.PositiveInfinity;
        Vector2 myPos = owner.position;

        foreach (var billion in BillionRegistry.All)
        {
            if (billion == null || billion == ownerBillion)
                continue;

            if (billion.team == ownerBillion.team)
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
}