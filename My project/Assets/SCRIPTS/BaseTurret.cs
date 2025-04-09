using UnityEngine;

public class BaseTurret : MonoBehaviour
{
    [Header("References")]
    public Transform turretTip;                  
    public GameObject baseProjectilePrefab;      
    public Color baseColor;                      

    [Header("Targeting")]
    public float rotationSpeed = 45f;            // Degrees per second
    private Billions currentTarget;              // The nearest opposing Billion
    public float detectionRange = 100f;          

    [Header("Firing Settings")]
    public float fireInterval = 2.5f;            // Different from Billions' 1.5f
    private float fireTimer = 0f;
    public float projectileDamage = 15f;         // Different from Billions' 10f
    public float projectileRange = 12f;          // Different from Billions' 8f

    void Update()
    {
        // 1. Find the nearest enemy
        FindNearestOpposingBillion();

        // 2. Smoothly rotate toward the target
        if (currentTarget != null)
        {
            RotateTowardTarget(currentTarget.transform.position);

            // 3. Attempt to fire if target is in range
            FireAtTarget();
        }
    }

    private void FindNearestOpposingBillion()
    {
        currentTarget = null;
        float nearestDistance = Mathf.Infinity;

        // Check all Billions in the scene 
        foreach (Billions billion in FlagController.Instance.allBillions)
        {
            // Skip same-colored Billions 
            if (billion.GetComponent<SpriteRenderer>().color == baseColor)
                continue;

            // Distance check
            float dist = Vector2.Distance(transform.position, billion.transform.position);
            if (dist < nearestDistance && dist < detectionRange)
            {
                nearestDistance = dist;
                currentTarget = billion;
            }
        }
    }

    private void RotateTowardTarget(Vector3 targetPos)
    {
        Vector3 direction = targetPos - transform.position;
        float targetAngle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;

        // Smoothly rotate toward the desired angle
        float currentAngle = transform.rotation.eulerAngles.z;
        float newAngle = Mathf.MoveTowardsAngle(currentAngle, targetAngle, rotationSpeed * Time.deltaTime);
        transform.rotation = Quaternion.Euler(0f, 0f, newAngle);
    }

    private void FireAtTarget()
    {
        fireTimer += Time.deltaTime;

        // If enough time has passed, fire a projectile
        if (fireTimer >= fireInterval)
        {
            fireTimer = 0f;

            // Spawn new base projectile
            GameObject newShot = Instantiate(baseProjectilePrefab, turretTip.position, turretTip.rotation);

            // 
            BlasterShot shotScript = newShot.GetComponent<BlasterShot>();
            if (shotScript != null)
            {
                shotScript.Initialize(
                    baseColor,                // or a unique color if you want
                    turretTip.right,          // direction based on your turret orientation
                    projectileDamage,
                    gameObject                // the owner, so we don't damage ourselves
                );

                // Overwrite some properties to differ from Billions
                shotScript.maxTravelDistance = projectileRange;
                // Possibly scale up the sprite or change the speed
                shotScript.speed = 6f; // or keep same
            }
        }
    }
}
