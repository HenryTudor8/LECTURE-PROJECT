using System.Collections;
using UnityEngine;

public class BillionaireBase : MonoBehaviour
{
    public FlagScript flagPrefab;
    public int flagCount;
    public GameObject billionPrefab;
    public Color baseColor;
    public float spawnInterval = 3f;
    [SerializeField] float maxHealth = 100f;
    private float currentHealth;

    public GameObject baseHealthRingPrefab;
    private BaseHealthRing healthRing;        // cached reference




    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        currentHealth = maxHealth;

        if (baseHealthRingPrefab != null)
        {
            // Slight upward offset so the ring isn't hidden by the base sprite
            Vector3 offset = new Vector3(0f, 0.05f, 0f);
            GameObject ringObj = Instantiate(
                baseHealthRingPrefab,
                transform.position + offset,
                Quaternion.identity,
                transform);                 // parent to the base

            healthRing = ringObj.GetComponent<BaseHealthRing>();
            if (healthRing != null)
                healthRing.SetFill(1f);
        }

        // existing setup
        FlagController.Instance.RegisterBase(this);
        FlagController.Instance.allClickableObjects.Add(gameObject);
        StartCoroutine(SpawnBillionRoutine());
    }

    private void Awake()
    {
        //FlagController.Instance.RegisterBase(this);

    }

    //The Coroutine itself
    private IEnumerator SpawnBillionRoutine()
    {
        while (true)
        {
            yield return new WaitForSeconds(spawnInterval);
            SpawnBillion();
        }
    }

    // Function to spawn billion
    private void SpawnBillion()
    {
        //Debug.Log("Spawning Billion..."); // Confirms function is running
        Vector2 spawnPosition = GetValidSpawnPosition();

        //Debug.Log("Calculated Spawn Position: " + spawnPosition); // Check what position is being generated

        if (spawnPosition != Vector2.zero)
        {
            GameObject newBillion = Instantiate(billionPrefab, spawnPosition, Quaternion.identity);
            //Debug.Log("Billion successfully spawned at: " + spawnPosition);
            newBillion.GetComponent<Billions>().Initialize(baseColor);
        }
        else
        {
            //Debug.LogError("Spawn position is invalid (Vector2.zero). Check GetValidSpawnPosition()!");
        }
    }


    // Finding a non-overlapping spawn position
    private Vector2 GetValidSpawnPosition()
    {
        int maxAttempts = 10; // Maximum number of attempts to find a valid position

        for (int i = 0; i < maxAttempts; i++)
        {
            // Generate a random small offset from the base position
            Vector2 randomOffset = Random.insideUnitCircle * 3.0f;
            Vector2 spawnPos = (Vector2)transform.position + randomOffset;

            // Check if there are any colliders in this position
            Collider2D hit = Physics2D.OverlapCircle(spawnPos, 0.2f);
            if (!hit) return spawnPos; // If no collisions, return this as a valid spawn position
        }

        return Vector2.zero; // If no valid position found after max attempts, return zero
    }
    public void TakeDamage(float amount)
    {
        currentHealth -= amount;
        Debug.Log($"[Damage] {name} {currentHealth}/{maxHealth}");

        if (healthRing != null)
            healthRing.SetFill(currentHealth / maxHealth);

        if (currentHealth <= 0)
        {
            if (healthRing != null)
                Destroy(healthRing.gameObject);

            Destroy(gameObject);
        }
    }

    void OnDestroy()
    {
        if (FlagController.Instance != null)
            FlagController.Instance.allBases.Remove(this);
    }



}



