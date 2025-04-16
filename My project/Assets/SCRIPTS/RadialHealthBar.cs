using UnityEngine;
using UnityEngine.UI;

public class RadialHealthBar : MonoBehaviour
{
    [SerializeField] private Image fillImage;

    public void SetHealthPercent(float percent)
    {
        if (fillImage != null)
        {
            fillImage.fillAmount = Mathf.Clamp01(percent);
        }
    }
}
