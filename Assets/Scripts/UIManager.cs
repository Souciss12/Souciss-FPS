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
        if (ammoText == null)
            ammoText = GameObject.Find("AmmoText")?.GetComponent<TextMeshProUGUI>();

        if (healthText == null)
            healthText = GameObject.Find("HealthText")?.GetComponent<TextMeshProUGUI>();

        // Si la recherche par nom échoue, chercher par tag ou par type
        // if (ammoText == null || healthText == null)
        // {
        //     TextMeshProUGUI[] allTexts = GetComponentsInChildren<TextMeshProUGUI>(true);
        //     foreach (TextMeshProUGUI text in allTexts)
        //     {
        //         if (ammoText == null && text.gameObject.name.Contains("Ammo"))
        //             ammoText = text;
        //         else if (healthText == null && text.gameObject.name.Contains("Health"))
        //             healthText = text;
        //     }
        // }

        // Vérification et message d'erreur si les textes ne sont pas trouvés
        // if (ammoText == null)
        //     Debug.LogError("AmmoText non trouvé dans la scène. Veuillez vérifier que l'objet existe avec un composant TextMeshProUGUI.");

        // if (healthText == null)
        //     Debug.LogError("HealthText non trouvé dans la scène. Veuillez vérifier que l'objet existe avec un composant TextMeshProUGUI.");

        // Initialisation du texte de santé
        if (healthText != null && playerController != null)
            healthText.text = playerController.currentHealth.ToString();

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
            healthText.text = playerController.currentHealth.ToString();
        }
    }
}
