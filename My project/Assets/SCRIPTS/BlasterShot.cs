using UnityEngine;



public class BlasterShot : MonoBehaviour
{
    
    public float speed = 6f;

    
    public float maxTravelDistance = 8f;

    
    private Vector3 startPosition;

    
    private Vector3 direction;

   
    private float damage;

    
    private Color shotColor;

    
    private GameObject owner;


    public void Initialize(Color color, Vector3 dir, float dmg, GameObject shooter)
    {
        shotColor = color;
        direction = dir.normalized; // Ensure direction is a unit vector
        damage = dmg;
        owner = shooter;
        startPosition = transform.position;

        // Set the sprite color to match the shooter's color
        GetComponent<SpriteRenderer>().color = shotColor;
    }


    // Update is called once per frame
    void Update()
    {
        transform.position += direction * speed * Time.deltaTime;

        // Destroy the blaster if it exceeds its travel distance
        if (Vector3.Distance(startPosition, transform.position) > maxTravelDistance)
        {
            Destroy(gameObject);
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.gameObject == owner)
            return;

        // Check if we hit another Billion
        if (other.TryGetComponent<Billions>(out Billions target))
        {
            // Only damage if it's an enemy (different color)
            if (target.GetComponent<SpriteRenderer>().color != shotColor)
            {
                target.TakeDamage(damage); // Call method to reduce health
                Destroy(gameObject); // Blaster is destroyed on hit
            }
        }
        else if (other.CompareTag("Wall"))
        {
            Destroy(gameObject);
        }
        else if (other.TryGetComponent<BillionaireBase>(out BillionaireBase baseTarget))
        {
            if (baseTarget.baseColor != shotColor)
            {
                baseTarget.TakeDamage(damage);
                Destroy(gameObject);
            }
        }
    }
}
