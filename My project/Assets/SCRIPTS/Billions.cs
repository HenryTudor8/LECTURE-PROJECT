﻿using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class Billions : MonoBehaviour
{
    /* ───────── Rank-based stats ───────── */
    public BillionaireBase ownerBase;   // set on spawn
    public int rank;
    public float currentHealth;
    public float blasterDamage;
    public float maxHealth;

    /* ───────── Existing fields (unchanged) ───────── */
    private SpriteRenderer spriteRenderer;

    [Header("Blaster")]
    [SerializeField] GameObject blasterShotPrefab;
    [SerializeField] Transform turretTip;
    [SerializeField] float shootInterval = 1.5f;
    [SerializeField] float blasterRange = 4f;

    [Header("Movement")]
    [SerializeField] Rigidbody2D rb;
    [SerializeField] float moveSpeed = 2f;

    /* misc */
    private Color myColor;
    private float shootTimer;
    private GameObject currentTarget;
    private Transform healthCircle;

    /* ───────── INITIALISE ───────── */
    public void Initialize(Color color, BillionaireBase myBase)
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        spriteRenderer.color = color;

        ownerBase = myBase;
        rank = myBase.rank;
        maxHealth = 20;
        currentHealth = rank * 2.5f;      // formula from spec
        blasterDamage = rank * 0.5f;
        myColor = color;
        
        healthCircle = transform.Find("HealthCircle");
        UpdateHealthVisual();
    }

    private void UpdateHealthVisual() // HEALTH CIRCLE CHECKPOINT 4
    {
        if (healthCircle == null) return;

        float minScale = 1.5f;
        float maxScale = 3f;
        float t = Mathf.Clamp01(currentHealth / maxHealth);
        float scaled = Mathf.Lerp(minScale, maxScale, t);

        healthCircle.localScale = new Vector3(scaled, scaled, 1f);
        Debug.Log($"Billion HP: {currentHealth} → Scale: {scaled:F2}");

    }




    void FixedUpdate()
    {
        FlagScript flag = FindNearestFlagOfSameColor();
        if (flag == null) return;

        transform.position = Vector3.MoveTowards(
            transform.position, flag.transform.position,
            Time.deltaTime * moveSpeed);

        FindClosestTarget();
        TryShootAtTarget();
    }

    
    private void FindClosestTarget()
    {
        float closest = Mathf.Infinity;
        GameObject best = null;

        foreach (Billions b in FlagController.Instance.allBillions)
        {
            if (b == null || b == this) continue;
            if (b.myColor == myColor) continue;

            float d = Vector2.Distance(transform.position, b.transform.position);
            if (d < closest) { closest = d; best = b.gameObject; }
        }

        foreach (BillionaireBase bb in FlagController.Instance.allBases)
        {
            if (bb == null || bb.baseColor == myColor) continue;
            float d = Vector2.Distance(transform.position, bb.transform.position);
            if (d < closest) { closest = d; best = bb.gameObject; }
        }

        currentTarget = best;

        if (currentTarget != null)
        {
            Vector3 diff = currentTarget.transform.position - transform.position;
            float ang = Mathf.Atan2(diff.y, diff.x) * Mathf.Rad2Deg;
            transform.rotation = Quaternion.Euler(0, 0, ang);
        }
    }

    private FlagScript FindNearestFlagOfSameColor()
    {
        float min = Mathf.Infinity;
        FlagScript best = null;
        foreach (GameObject f in FlagController.Instance.allFlags)
        {
            if (f.GetComponent<SpriteRenderer>().color != myColor) continue;
            float d = Vector2.Distance(transform.position, f.transform.position);
            if (d < min) { min = d; best = f.GetComponent<FlagScript>(); }
        }
        return best;
    }

    /* ───────── Shooting ───────── */
    void TryShootAtTarget()
    {
        shootTimer += Time.deltaTime;
        if (currentTarget == null) return;

        if (Vector2.Distance(transform.position, currentTarget.transform.position) <= blasterRange &&
            shootTimer >= shootInterval)
        {
            shootTimer = 0f;
            GameObject g = Instantiate(blasterShotPrefab, turretTip.position, turretTip.rotation);
            g.GetComponent<BlasterShot>()
             .Initialize(myColor, transform.up, blasterDamage, gameObject);
        }
    }

    /* ───────── Damage ───────── */
    public bool TakeDamage(float amount)
    {
        Debug.Log($"{gameObject.name} took {amount} damage → HP now {currentHealth}");

        currentHealth -= amount;
        UpdateHealthVisual();

        if (currentHealth <= 0f)
        {
            Destroy(gameObject);
            return true;   // killed
        }
        return false;
    }

    void OnDestroy()
    {
        if (FlagController.Instance != null)
            FlagController.Instance.allBillions.Remove(this);
    }
}




