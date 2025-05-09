using UnityEngine;

public class FlagScript : MonoBehaviour
{
    private LineRenderer lineRenderer; 
    private Vector3 originalPosition;  
    private bool isDragging = false;   

    void Start()
    {
        Debug.Log($"FlagScript Start() called for {gameObject.name}");
        
        FlagController.Instance.allClickableObjects.Add(gameObject);
        Debug.Log($"Flag added to allClickableObjects: {gameObject.name}, Color: {GetComponent<SpriteRenderer>().color}");

        FlagController.Instance.allFlags.Add(gameObject);
        // Get the SpriteRenderer component
        SpriteRenderer sr = GetComponent<SpriteRenderer>();

        
        sr.sortingLayerName = "Flags";
        sr.sortingOrder = 3; // Ensure flags render in front of billions

        // Initialize the Line Renderer component for visualizing movement
        lineRenderer = gameObject.AddComponent<LineRenderer>();
        lineRenderer.startWidth = 0.1f;
        lineRenderer.endWidth = 0.1f;
        lineRenderer.positionCount = 2;
        lineRenderer.material = new Material(Shader.Find("Sprites/Default"));
        lineRenderer.startColor = Color.white;
        lineRenderer.endColor = Color.white;
        lineRenderer.enabled = false;  // Initially hide the line
    }

    void OnMouseDown()
    {
        Debug.Log("Flag clicked, dragging started.");
        isDragging = true;  // Set dragging state to true
        originalPosition = transform.position;  // Store the initial position
        lineRenderer.enabled = true;  // Enable line rendering

        // Ensure that exactly two flags of this color exist
        EnsureTwoFlagsExist();
    }

    void OnMouseDrag()
    {
        Debug.Log($"[Drag] Dragging flag: {gameObject.name}");

        if (lineRenderer == null)
        {
            lineRenderer = GetComponent<LineRenderer>() ?? gameObject.AddComponent<LineRenderer>();
            lineRenderer.useWorldSpace = true;
            lineRenderer.positionCount = 2;
            lineRenderer.startWidth = 0.05f;
            lineRenderer.endWidth = 0.05f;
            lineRenderer.material = new Material(Shader.Find("Sprites/Default"));
            lineRenderer.startColor = Color.white;
            lineRenderer.endColor = Color.white;
            lineRenderer.sortingOrder = 10;
        }

        Vector3 currentMouse = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        currentMouse.z = 0;

        lineRenderer.SetPosition(0, originalPosition);
        lineRenderer.SetPosition(1, currentMouse);
    }


    void OnMouseUp()
    {
        if (isDragging)
        {
            Debug.Log("Dragging ended, moving flag instead of spawning.");

            // Get final position where the flag will be placed
            Vector3 finalPosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            finalPosition.z = 0;  // Ensure flag is placed in 2D space

            // Find the closest flag of the same color and move it
            FlagScript closestFlag = GetClosestFlagOfSameColor();
            if (closestFlag != null)
            {
                closestFlag.transform.position = finalPosition;  // Move flag to correct position
                Debug.Log("Flag moved to: " + finalPosition);
            }

            // Reset dragging state
            isDragging = false;
            lineRenderer.enabled = false;
        }
    }

    
    /// Ensures that exactly two flags of the same color exist.
    /// If there is only one, it spawns a second flag.
    
    private void EnsureTwoFlagsExist()
    {
        int flagCount = 0;  // Counter for how many flags of the same color exist

        // Loop through all clickable objects to count flags of the same color
        foreach (GameObject obj in FlagController.Instance.allClickableObjects)
        {
            if (obj.TryGetComponent<FlagScript>(out FlagScript flagScript))
            {
                // Check if this flag has the same color as the current flag
                if (flagScript.GetComponent<SpriteRenderer>().color == GetComponent<SpriteRenderer>().color)
                {
                    flagCount++;
                }
            }
        }

        // If only one flag of this color exists, spawn a second one
        if (flagCount < 2)
        {
            Vector3 spawnPosition = originalPosition + new Vector3(1f, 1f, 0);  // Slight offset from original position
            GameObject newFlag = Instantiate(gameObject, spawnPosition, Quaternion.identity); // Duplicate flag
            newFlag.name = gameObject.name;  // Keep naming convention consistent

            // Add the new flag to the clickable objects list in FlagController
            FlagController.Instance.allClickableObjects.Add(newFlag);
        }
    }

    
    /// Finds the closest flag of the same color (excluding the current flag).
   
    /// <returns>The closest FlagScript instance of the same color.</returns>
    private FlagScript GetClosestFlagOfSameColor()
    {
        FlagScript closestFlag = null;
        float closestDistance = Mathf.Infinity;  // Set to a very high value initially

        // Loop through all objects to find the nearest flag of the same color
        foreach (GameObject obj in FlagController.Instance.allClickableObjects)
        {
            if (obj != gameObject && obj.TryGetComponent<FlagScript>(out FlagScript flagScript))
            {
                // Ensure it's the same color as the current flag
                if (flagScript.GetComponent<SpriteRenderer>().color == GetComponent<SpriteRenderer>().color)
                {
                    float distance = Vector2.Distance(transform.position, obj.transform.position);
                    if (distance < closestDistance)
                    {
                        closestFlag = flagScript;
                        closestDistance = distance;
                    }
                }
            }
        }
        return closestFlag;  // Return the closest flag found
    }
}



