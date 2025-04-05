using UnityEngine;
using TMPro;

public class UIManager : MonoBehaviour
{
    public TextMeshProUGUI AmmoText;
    public GunController gunStats;

    void Start()
    {
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
            AmmoText.text = gunStats._currentAmmoInClip + " / " + gunStats._ammoInReserve;
        }
    }
}
