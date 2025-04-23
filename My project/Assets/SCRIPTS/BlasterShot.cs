using UnityEngine;

[RequireComponent(typeof(SpriteRenderer))]
public class BlasterShot : MonoBehaviour
{
    [Header("Runtime")]
    public Vector2 direction;
    public float speed = 6f;
    public float damage;
    public float maxTravelDistance = 12f;
    public Color shotColor;

    public GameObject shooter;            // who fired
    public BillionaireBase ownerBase;     // which base owns the projectile

    private Vector3 spawnPos;

    // ---------- INITIALISE ----------
    public void Initialize(Color c, Vector2 dir, float dmg, GameObject s)
    {
        shotColor = c;
        direction = dir.normalized;
        damage = dmg;
        shooter = s;

        // resolve owner base (projectile may come from a billion or a base-turret)
        ownerBase = s.GetComponent<BillionaireBase>();
        if (ownerBase == null)
            ownerBase = s.GetComponent<Billions>()?.ownerBase;

        GetComponent<SpriteRenderer>().color = shotColor;
        spawnPos = transform.position;
    }

    // ---------- UPDATE ----------
    void Update()
    {
        transform.position += (Vector3)direction * speed * Time.deltaTime;

        if (Vector3.Distance(spawnPos, transform.position) >= maxTravelDistance)
            Destroy(gameObject);
    }

    // ---------- COLLISION ----------
    void OnTriggerEnter2D(Collider2D other)
    {
        // ignore self
        if (other.gameObject == shooter) return;

        // hit enemy billion
        if (other.TryGetComponent(out Billions targetBillion))
        {
            if (targetBillion.ownerBase != ownerBase)   // enemy?
            {
                bool killed = targetBillion.TakeDamage(damage);
                if (killed && ownerBase != null)
                    ownerBase.AddExperience(5);          // award XP
                Destroy(gameObject);
            }
            return;
        }

        // hit enemy base
        if (other.TryGetComponent(out BillionaireBase targetBase))
        {
            if (targetBase != ownerBase)
            {
                targetBase.TakeDamage(damage);
                Destroy(gameObject);
            }
        }

        // walls / others
        if (other.gameObject.layer == LayerMask.NameToLayer("Walls"))
            Destroy(gameObject);
    }
}

