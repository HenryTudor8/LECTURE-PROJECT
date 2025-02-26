using System.Collections.Generic;
using UnityEngine;

public class IClosest : MonoBehaviour
{
    void Awake()
    {
        FlagController.Instance.allClickableObjects.Add(gameObject);
        Debug.Log("added to List");
    }
}
