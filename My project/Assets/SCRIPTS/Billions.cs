using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class Billions : MonoBehaviour
{

    private SpriteRenderer spriteRenderer;
    private Billions currentTargetEnemy; // refactored closest enemy from LookAtClosestBillion

    // Blaster shooting
    [SerializeField] GameObject blasterShotPrefab;
    [SerializeField] Transform turretTip;
    [SerializeField] float shootInterval = 1.5f;
    [SerializeField] float blasterRange = 4f;
    [SerializeField] float blasterDamage = 10f;
    //

    //Adding Billion Health
    [SerializeField] float maxHealth = 30f;
    private float currentHealth;

    [SerializeField] Rigidbody2D rb;
    [SerializeField] float moveSpeed = 2;
    [SerializeField] GameObject cannonObject;
    private Color myColor;
    private float shootTimer = 0f;
    [Header("Movement Settings")]
    public float maxSpeed = 5f;          // Maximum movement speed
    public float acceleration = 10f;     // Acceleration rate
    public float decelerationRadius = 2f; // Distance at which to begin slowing down
    public float minDistance = 0.5f;     // Stop moving when within this distance

    private GameObject currentTarget; // Could be an enemy base or billion




    private void Awake()
    {
        currentHealth = maxHealth; 
    }

    void Start()
    {
        FlagController.Instance.allBillions.Add(this);
    }
    /// <summary>
    /// Called by BillionaireBase when spawning a Billion.
    /// This assigns the correct color to the Billion.
    /// </summary>
    public void Initialize(Color color)
    {
        // Ensure the sprite renderer exists
        spriteRenderer = GetComponent<SpriteRenderer>();

        // Assign the correct color
        spriteRenderer.color = color;

        // Cache the color for movement logic
        myColor = color;
    }


    void FixedUpdate()
    {
        // 1. Find the nearest flag of the same color
        FlagScript targetFlag = FindNearestFlagOfSameColor();
        if (targetFlag == null) return; // No flag found, do nothing

        transform.position = Vector3.MoveTowards(transform.position, targetFlag.transform.position, Time.deltaTime * moveSpeed);

        //LookAtClosestBillion();
        FindClosestTarget();

        TryShootAtEnemy(); // To validate billion firing a blaster
    }

    /*private void LookAtClosestBillion()
    {
        Billions nearestBillion = null;
        float nearestDistance = Mathf.Infinity;

        foreach (Billions billion in FlagController.Instance.allBillions)
        {
            if (!billion.GetComponent<SpriteRenderer>().color.Equals(myColor))
            {
                float dist = Vector2.Distance(transform.position, billion.transform.position);
                if (dist < nearestDistance)
                {
                    nearestDistance = dist;
                    nearestBillion = billion;
                }
            }
        }
        if (nearestBillion != null)
        {
            //What is the difference in position?
            Vector3 diff = (nearestBillion.transform.position - transform.position);

            //We use aTan2 since it handles negative numbers and division by zero errors. 
            float angle = Mathf.Atan2(diff.y, diff.x);

            //Now we set our new rotation. 
            transform.rotation = Quaternion.Euler(0f, 0f, angle * Mathf.Rad2Deg);

            // storing the target so TryShootAtEnemy can use it

            currentTargetEnemy = nearestBillion;
        }
        else
        {
            currentTargetEnemy = null;
        }

    } */
  // New method added in place of LookAtClosestBillion
    private void FindClosestTarget()
    {
        float closestDistance = Mathf.Infinity;
        GameObject bestTarget = null;

        // Check enemy billions
        foreach (Billions b in FlagController.Instance.allBillions)
        {
            if (b == this) continue; // Skip self
            if (b.GetComponent<SpriteRenderer>().color == myColor) continue; // Skip friendlies

            float dist = Vector2.Distance(transform.position, b.transform.position);
            if (dist < closestDistance)
            {
                closestDistance = dist;
                bestTarget = b.gameObject;
            }
        }

        // Check enemy bases
        foreach (BillionaireBase baseObj in FlagController.Instance.allBases)
        {
            if (baseObj.baseColor == myColor) continue; // Skip friendlies

            float dist = Vector2.Distance(transform.position, baseObj.transform.position);
            if (dist < closestDistance)
            {
                closestDistance = dist;
                bestTarget = baseObj.gameObject;
            }
        }

        currentTarget = bestTarget;

        // Rotate to face the target
        if (currentTarget != null)
        {
            Vector3 diff = currentTarget.transform.position - transform.position;
            float angle = Mathf.Atan2(diff.y, diff.x);
            transform.rotation = Quaternion.Euler(0f, 0f, angle * Mathf.Rad2Deg);
        }
    }


    private FlagScript FindNearestFlagOfSameColor()
    {
        FlagScript nearestFlag = null;
        float nearestDistance = Mathf.Infinity;

        foreach (GameObject flag in FlagController.Instance.allFlags)
        {
            if (flag.GetComponent<SpriteRenderer>().color.Equals(myColor))
            {
                float dist = Vector2.Distance(transform.position, flag.transform.position);
                if (dist < nearestDistance)
                {
                    nearestDistance = dist;
                    nearestFlag = flag.GetComponent<FlagScript>();
                }
            }
        }

        if (nearestFlag != null)
        {
            //Debug.Log($"Billion ({myColor}) found nearest flag at {nearestFlag.transform.position}");
        }
        else
        {
            //Debug.LogWarning($"Billion ({myColor}) could not find any flag!");
        }

        return nearestFlag;
    }

    private void TryShootAtEnemy()
    {
        shootTimer += Time.deltaTime;

        if (currentTargetEnemy == null) return; // Prevent accessing destroyed targets

        float distance = Vector2.Distance(transform.position, currentTargetEnemy.transform.position);
        if (distance <= blasterRange && shootTimer >= shootInterval)
        {
            shootTimer = 0f;

            // Instantiate blaster shot
            GameObject blaster = Instantiate(blasterShotPrefab, turretTip.position, turretTip.rotation);

            BlasterShot blasterScript = blaster.GetComponent<BlasterShot>();
            blasterScript.Initialize(myColor, transform.up, blasterDamage, gameObject);
        }
    }


    public void TakeDamage(float amount)
    {
        currentHealth -= amount;

        if (currentHealth <= 0)
        {
            Destroy(gameObject); // Remove Billion from game
        }
    }



}




