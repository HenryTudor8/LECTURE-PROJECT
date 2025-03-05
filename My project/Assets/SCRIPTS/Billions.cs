using UnityEngine;

// Require a Rigidbody2D component for physics-based movement
[RequireComponent(typeof(Rigidbody2D))]
public class Billions : MonoBehaviour
{
    // Reference to the SpriteRenderer to control color and sorting order
    private SpriteRenderer spriteRenderer;

    // Reference to the Rigidbody2D used for applying forces
    private Rigidbody2D rb;

    // Cached color for this Billion; used to match with flags of the same color
    private Color myColor;

    [Header("Movement Settings")]
    public float maxSpeed = 5f;           // Maximum speed the Billion can reach
    public float acceleration = 10f;      // How quickly the Billion accelerates
    public float decelerationRadius = 2f; // Distance from the flag where deceleration starts
    public float minDistance = 0.5f;      // Minimum distance to the flag before stopping

    void Start()
    {
        Debug.Log("Billion Spawned at: " + transform.position);

        // Get the SpriteRenderer and Rigidbody2D components
        spriteRenderer = GetComponent<SpriteRenderer>();
        rb = GetComponent<Rigidbody2D>();

        // Cache the Billion's color from its SpriteRenderer
        myColor = spriteRenderer.color;

        // Prevent the Billion from rotating if you want them to stay upright
        rb.freezeRotation = true;

        // Set the sorting order so that the Billion appears behind flags
        // (Make sure your flags have a higher sorting order)
        spriteRenderer.sortingOrder = 0;
    }

    // FixedUpdate is used for consistent physics updates
    void FixedUpdate()
    {
        // 1. Find the nearest flag with the same color
        FlagScript targetFlag = FindNearestFlagOfSameColor();
        if (targetFlag == null)
        {
            // If no matching flag is found, do nothing this frame.
            return;
        }

        // 2. Compute the direction and distance to the target flag
        Vector2 direction = (targetFlag.transform.position - transform.position);
        float distance = direction.magnitude;

        // 3. If the Billion is very close to the flag, stop moving.
        if (distance < minDistance)
        {
            rb.linearVelocity = Vector2.zero;
            return;
        }

        // 4. Normalize the direction vector for proper force calculation.
        direction.Normalize();

        // 5. Determine the desired speed. If within the deceleration radius,
        // scale the speed proportionally so the Billion decelerates as it nears the flag.
        float desiredSpeed = maxSpeed;
        if (distance < decelerationRadius)
        {
            desiredSpeed = maxSpeed * (distance / decelerationRadius);
        }

        // 6. Compute the desired velocity vector.
        Vector2 desiredVelocity = direction * desiredSpeed;

        // 7. Calculate the "steering" force required to adjust the current velocity
        Vector2 steering = desiredVelocity - rb.linearVelocity;

        // 8. Limit the steering force by the acceleration rate
        float maxSteer = acceleration * Time.fixedDeltaTime;
        if (steering.magnitude > maxSteer)
        {
            steering = steering.normalized * maxSteer;
        }

        // 9. Apply the steering force as an impulse so the Billion accelerates toward the flag.
        rb.AddForce(steering, ForceMode2D.Impulse);
    }

    /// <summary>
    /// Initializes the Billion with the given color.
    /// This is called by BillionaireBase when spawning a new Billion.
    /// </summary>
    /// <param name="color">The color to assign to this Billion.</param>
    public void Initialize(Color color)
    {
        // Set the color on the SpriteRenderer and cache it for movement logic.
        spriteRenderer = GetComponent<SpriteRenderer>();
        spriteRenderer.color = color;
        myColor = color;

        // Set the sorting layer for the billions
        spriteRenderer.sortingLayerName = "Billions";
        spriteRenderer.sortingOrder = 1; //ensuring billions are behind flags
    }

    /// <summary>
    /// Searches for the nearest flag (FlagScript) that shares the same color as this Billion.
    /// Flags are tracked in FlagController.Instance.allClickableObjects.
    /// </summary>
    /// <returns>The nearest FlagScript with a matching color, or null if none is found.</returns>
    private FlagScript FindNearestFlagOfSameColor()
    {
        FlagScript nearestFlag = null;
        float nearestDistance = Mathf.Infinity;

        // Loop through all clickable objects registered in the FlagController.
        foreach (GameObject obj in FlagController.Instance.allClickableObjects)
        {
            // Check if the object is a flag by testing for the FlagScript component.
            if (obj.TryGetComponent<FlagScript>(out FlagScript flag))
            {
                // Compare the flag's color with this Billion's color.
                if (flag.GetComponent<SpriteRenderer>().color == myColor)
                {
                    // Calculate the distance between the Billion and the flag.
                    float dist = Vector2.Distance(transform.position, flag.transform.position);
                    if (dist < nearestDistance)
                    {
                        nearestDistance = dist;
                        nearestFlag = flag;
                    }
                }
            }
        }

        return nearestFlag;
    }
}


