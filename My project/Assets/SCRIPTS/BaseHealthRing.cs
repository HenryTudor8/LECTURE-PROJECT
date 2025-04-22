using UnityEngine;


public class BaseHealthRing : MonoBehaviour
{
    SpriteRenderer sr;
    Vector3 initialScale;

    void Awake()
    {
        sr = GetComponent<SpriteRenderer>();
        initialScale = transform.localScale;   // remember full size
    }

    
    public void SetFill(float percent)
    {
        percent = Mathf.Clamp01(percent);
        transform.localScale = new Vector3(
            initialScale.x * percent,
            initialScale.y * percent,
            1f);
    }
}

