using System.Collections;
using UnityEngine;

public class BillionaireBase : MonoBehaviour
{
    /* ──────────────  XP / Rank fields  ────────────── */
    public int rank = 1;      // starts at 1
    public int xp = 0;      // running XP
    public int xpThreshold = 20;     // first level-up need
    public float thresholdGrowth = 1.5f;   // nextThreshold = old * growth

    /* XP visual ring (white) */
    public SpriteRenderer xpRingRenderer;          // drag in Inspector
    private Vector3 xpRingInitialScale;

    /* ──────────────  Existing fields  ────────────── */
    public FlagScript flagPrefab;
    public int flagCount;
    public GameObject billionPrefab;
    public Color baseColor;
    public float spawnInterval = 3f;

    [SerializeField] float maxHealth = 100f;
    private float currentHealth;

    public GameObject baseHealthRingPrefab;
    private BaseHealthRing healthRing;

    /* ─────────────────────────────────────────────── */
    void Start()
    {
        currentHealth = maxHealth;

        /* cache XP-ring scale */
        if (xpRingRenderer != null)
            xpRingInitialScale = xpRingRenderer.transform.localScale;

        /* health ring */
        if (baseHealthRingPrefab != null)
        {
            Vector3 offset = new Vector3(0f, 0.05f, 0f);
            GameObject ring = Instantiate(baseHealthRingPrefab,
                                          transform.position + offset,
                                          Quaternion.identity,
                                          transform);

            healthRing = ring.GetComponent<BaseHealthRing>();
            healthRing?.SetFill(1f);
        }

        // set main sprite render order (to show above floor/walls)
        var sr = GetComponentInChildren<SpriteRenderer>();
        if (sr != null) sr.sortingOrder = 3;


        FlagController.Instance.RegisterBase(this);
        FlagController.Instance.allClickableObjects.Add(gameObject);
        StartCoroutine(SpawnBillionRoutine());
    }

    /* ───────── spawn loop ───────── */
    IEnumerator SpawnBillionRoutine()
    {
        while (true)
        {
            yield return new WaitForSeconds(spawnInterval);
            SpawnBillion();
        }
    }

    void SpawnBillion()
    {
        Vector2 spawnPos = GetValidSpawnPosition();
        if (spawnPos == Vector2.zero) return;

        GameObject g = Instantiate(billionPrefab, spawnPos, Quaternion.identity);
        Billions b = g.GetComponent<Billions>();
        b.Initialize(baseColor, this);                  // pass THIS base
    }

    /* ───────── damage / destroy ───────── */
    public void TakeDamage(float amount)
    {
        currentHealth -= amount;
        healthRing?.SetFill(currentHealth / maxHealth);

        Debug.Log($"[Damage] {name} {currentHealth}/{maxHealth}");

        if (currentHealth <= 0f)
        {
            if (healthRing) Destroy(healthRing.gameObject);
            Destroy(gameObject);
        }
    }

    void OnDestroy()
    {
        if (FlagController.Instance != null)
            FlagController.Instance.allBases.Remove(this);
    }

    /* ───────── XP / Rank logic ───────── */
    public void AddExperience(int amount)
    {
        xp += amount;

        if (xpRingRenderer != null)
        {
            float p = (float)xp / xpThreshold;
            xpRingRenderer.transform.localScale =
                xpRingInitialScale * Mathf.Clamp01(p);
        }

        while (xp >= xpThreshold)
        {
            xp -= xpThreshold;
            RankUp();
        }
    }

    void RankUp()
    {
        rank++;
        xpThreshold = Mathf.CeilToInt(xpThreshold * thresholdGrowth);
        Debug.Log($"[RankUp] {name} → Rank {rank}  (next {xpThreshold})");

        if (xpRingRenderer != null)
            xpRingRenderer.transform.localScale = Vector3.zero;
        /* optional: update a rank label here */
    }

    /* ───────── helpers ───────── */
    Vector2 GetValidSpawnPosition()
    {
        for (int i = 0; i < 10; i++)
        {
            Vector2 pos = (Vector2)transform.position + Random.insideUnitCircle * 3f;
            if (!Physics2D.OverlapCircle(pos, 0.2f)) return pos;
        }
        return Vector2.zero;
    }
}




