using UnityEngine;

public class FlagScript : MonoBehaviour
{

    void Start()
    {
        FlagController.Instance.allClickableObjects.Add(gameObject);
    }

    // Update is called once per frame
    void Update()
    {

    }

    private void OnMouseDown()
    {
        Debug.Log("clickedFlag");
    }
}
