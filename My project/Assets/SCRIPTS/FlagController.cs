using JetBrains.Annotations;
using NUnit.Framework;
using System.Collections.Generic;
using UnityEngine;

public class FlagController : MonoBehaviour
{
    [SerializeField] int flagLimit;
    public List<GameObject> allClickableObjects = new List<GameObject>();
    public List<GameObject> allFlags = new List<GameObject>();
    public List<Billions> allBillions = new List<Billions>();
    public List<BillionaireBase> allBases = new List<BillionaireBase>(); // added for handling FireAtClosestTarget in billions script


    public void RegisterBase(BillionaireBase baseObj)
    {
        if (!allBases.Contains(baseObj))
        {
            allBases.Add(baseObj);
        }
    }


    public static FlagController Instance;
    void Awake()
    {
        Instance = this;
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            Vector3 mousePosition = Input.mousePosition;
            mousePosition = Camera.main.ScreenToWorldPoint(mousePosition);
            mousePosition.z = -1;

            GameObject closest = GetClosest(mousePosition);

            if (closest.TryGetComponent<FlagScript>(out FlagScript closestFlag))
            {
                Debug.Log("Flag Found" + closestFlag);
                closestFlag.transform.position = mousePosition;
            }
            else if (closest.TryGetComponent<BillionaireBase>(out BillionaireBase closestBase))
            {
                Debug.Log("Base Found" + closestBase);
                if (closestBase.flagCount >= flagLimit)
                {
                    Debug.Log("MaxFlags" + closestBase);
                }
                else
                {
                    Instantiate(closestBase.flagPrefab, mousePosition, Quaternion.identity);
                    closestBase.flagCount++;
                }

            }
        }
    }

    private GameObject GetClosest(Vector3 position)
    {
        GameObject closest = null;
        float closestDistance = 100f;

        foreach (GameObject target in allClickableObjects)
        {
            if (Vector2.Distance(position, target.transform.position) < closestDistance)
            {
                closest = target;
                closestDistance = Vector2.Distance(position, target.transform.position);
            }
        }

        return closest;
    }
}
