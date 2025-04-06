using UnityEngine;
using TMPro;

public class UIManager : MonoBehaviour
{

    public TextMeshProUGUI ammoText;
    public TextMeshProUGUI healthText;
    public AutomaticGun gunStats;
    public FPSController playerController;

    void Start()
    {
        healthText.text = playerController.health.ToString();
        if (gunStats == null)
        {
            AutomaticGun[] allGuns = FindObjectsOfType<AutomaticGun>(true);
            foreach (AutomaticGun gun in allGuns)
            {
                if (gun.gameObject.activeInHierarchy)
                {
                    gunStats = gun;
                    break;
                }
            }
        }
    }

    void Update()
    {
        if (gunStats != null)
        {
            ammoText.text = gunStats._currentAmmoInClip + " / " + gunStats._ammoInReserve;
        }
        if (playerController != null)
        {
            healthText.text = playerController.health.ToString();
        }
    }
}
