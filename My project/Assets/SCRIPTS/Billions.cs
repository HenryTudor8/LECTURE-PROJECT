using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class Billions : MonoBehaviour
{

    private SpriteRenderer spriteRenderer;
    [SerializeField] Rigidbody2D rb;
    [SerializeField] float moveSpeed = 2;
    [SerializeField] GameObject cannonObject;
    private Color myColor;

    [Header("Movement Settings")]
    public float maxSpeed = 5f;          // Maximum movement speed
    public float acceleration = 10f;     // Acceleration rate
    public float decelerationRadius = 2f; // Distance at which to begin slowing down
    public float minDistance = 0.5f;     // Stop moving when within this distance

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

        LookAtClosestBillion();
    }

    private void LookAtClosestBillion()
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
}




