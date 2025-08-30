using UnityEngine;

[RequireComponent(typeof(Collider2D))]
public class SeedPickup : MonoBehaviour
{
    public SeedData seed;
    public int amount = 1;

    public float magnetRadius = 2.5f;
    public float magnetSpeed = 10f;

    Transform player;
    SeedInventory inv;
    Collider2D col;

    void Awake()
    {
        col = GetComponent<Collider2D>();
        col.isTrigger = true;
    }

    void Start()
    {
        var p = GameObject.FindGameObjectWithTag("Player");
        if (p)
        {
            player = p.transform;
            inv = p.GetComponent<SeedInventory>();
        }
    }

    void Update()
    {
        if (!player) return;
        float d = Vector2.Distance(transform.position, player.position);
        if (d <= magnetRadius)
        {
            Vector3 dir = (player.position - transform.position).normalized;
            transform.position += dir * magnetSpeed * Time.deltaTime;
        }
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag("Player")) return;
        if (inv && seed) inv.Add(seed, amount);
        Destroy(gameObject);
    }
}