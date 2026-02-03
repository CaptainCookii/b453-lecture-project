using UnityEngine;
enum Teams {
    Blue,
    Green,
    Orange,
    Yellow
}
public class BillionareBase : MonoBehaviour
{
    [SerializeField] private Teams team;
    [SerializeField] private Billionare billionareTemplate;

    void Start()
    {
        
    }
    
    void Update()
    {
        SpawnBillionare();
    }

    void SpawnBillionare()
    {
        
    }
}
