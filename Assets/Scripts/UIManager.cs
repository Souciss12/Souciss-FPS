using UnityEngine;
using TMPro;

public class UIManager : MonoBehaviour
{

    public TextMeshProUGUI ammoText;
    public TextMeshProUGUI healthText;
    public GunController gunStats;
    public FPSController playerController;

    void Start()
    {
        healthText.text = playerController.health.ToString();
        if (gunStats == null)
        {
            GunController[] allGuns = FindObjectsOfType<GunController>(true);
            foreach (GunController gun in allGuns)
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
