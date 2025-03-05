using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class Billions : MonoBehaviour
{
    private SpriteRenderer spriteRenderer;
    private Rigidbody2D rb;
    private Color myColor;

    [Header("Movement Settings")]
    public float maxSpeed = 5f;          // Maximum movement speed
    public float acceleration = 10f;     // Acceleration rate
    public float decelerationRadius = 2f; // Distance at which to begin slowing down
    public float minDistance = 0.5f;     // Stop moving when within this distance

    void Start()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        rb = GetComponent<Rigidbody2D>();

        // Ensure Gravity Scale is 0 so billions don't fall
        rb.gravityScale = 0;

        // Cache the Billion's color for matching flags
        myColor = spriteRenderer.color;

        // Prevent rotation
        rb.freezeRotation = true;

        // Set sorting layer to ensure billions appear behind flags
        spriteRenderer.sortingLayerName = "Billions";
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

        // 2. Compute direction and distance to the flag
        Vector2 directionToFlag = (targetFlag.transform.position - transform.position);
        float distance = directionToFlag.magnitude;

        // 3. Stop moving if close enough
        if (distance < minDistance)
        {
            rb.linearVelocity = Vector2.zero;
            return;
        }

        // 4. Normalize direction
        directionToFlag.Normalize();

        // 5. Compute desired speed (scales down when near the flag)
        float desiredSpeed = maxSpeed;
        if (distance < decelerationRadius)
        {
            desiredSpeed = maxSpeed * (distance / decelerationRadius); // Scale speed as we near target
        }

        // 6. Compute desired velocity
        Vector2 desiredVelocity = directionToFlag * desiredSpeed;

        // 7. Compute steering force needed to reach desired velocity
        Vector2 steeringForce = desiredVelocity - rb.linearVelocity;

        // 8. Limit steering force to prevent abrupt changes
        float maxSteeringForce = acceleration * Time.fixedDeltaTime;
        if (steeringForce.magnitude > maxSteeringForce)
        {
            steeringForce = steeringForce.normalized * maxSteeringForce;
        }

        // 9. Apply the force to accelerate smoothly
        rb.AddForce(steeringForce, ForceMode2D.Impulse);
    }

    private FlagScript FindNearestFlagOfSameColor()
    {
        FlagScript nearestFlag = null;
        float nearestDistance = Mathf.Infinity;

        foreach (GameObject obj in FlagController.Instance.allClickableObjects)
        {
            if (obj.TryGetComponent<FlagScript>(out FlagScript flag))
            {
                if (flag.GetComponent<SpriteRenderer>().color == myColor)
                {
                    float dist = Vector2.Distance(transform.position, flag.transform.position);
                    if (dist < nearestDistance)
                    {
                        nearestDistance = dist;
                        nearestFlag = flag;
                    }
                }
            }
        }

        if (nearestFlag != null)
        {
            Debug.Log($"Billion ({myColor}) found nearest flag at {nearestFlag.transform.position}");
        }
        else
        {
            Debug.LogWarning($"Billion ({myColor}) could not find any flag!");
        }

        return nearestFlag;
    }
}




