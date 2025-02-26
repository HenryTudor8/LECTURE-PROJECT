using UnityEngine;

public class Billions : MonoBehaviour
{

    private SpriteRenderer spriteRenderer; // Reference to the SpriteRenderer component

    // Function to initialize the billion with the correct color
    public void Initialize(Color color)
    {
        spriteRenderer = GetComponent<SpriteRenderer>(); // Get the sprite renderer component
        spriteRenderer.color = color; // Assign the color of the base that spawned it
    }
}

