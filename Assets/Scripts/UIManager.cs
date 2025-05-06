using UnityEngine;
using TMPro;
using Photon.Pun;

public class UIManager : MonoBehaviour
{
    public TextMeshProUGUI ammoText;
    public AutomaticGun gunStats;
    public FPSController playerController;

    private PhotonView photonView;

    void Start()
    {
        // Récupérer la référence au PhotonView du joueur
        photonView = GetComponentInParent<PhotonView>();

        // S'il s'agit d'un joueur distant (pas le nôtre), on désactive l'UI
        if (photonView != null && !photonView.IsMine)
        {
            // Désactiver les éléments d'UI pour les joueurs distants
            Canvas canvas = transform.parent.GetComponentInChildren<Canvas>();
            if (canvas != null)
            {
                canvas.gameObject.SetActive(false);
            }
            enabled = false; // Désactiver ce script pour les joueurs distants
            return;
        }

        // Récupérer le FPSController du parent
        if (playerController == null)
        {
            playerController = GetComponentInParent<FPSController>();
        }

        // Initialiser les références d'UI si elles ne sont pas déjà définies
        InitializeUIReferences();

        // Récupérer l'arme active initiale
        if (gunStats == null && playerController != null)
        {
            AutomaticGun[] allGuns = playerController.GetComponentsInChildren<AutomaticGun>(true);
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

    private void InitializeUIReferences()
    {
        // Chercher le Canvas parent
        Canvas canvas = transform.parent.GetComponentInChildren<Canvas>();
        if (canvas == null)
        {
            Debug.LogError("Canvas non trouvé sur le joueur!");
            return;
        }

        Debug.Log("Canvas trouvé: " + canvas.name);

        // Si ammoText n'est pas déjà assigné, essayer de le trouver
        if (ammoText == null)
        {
            // 1. Essayer d'abord la recherche directe
            ammoText = canvas.transform.Find("AmmoText")?.GetComponent<TextMeshProUGUI>();

            // 2. Si ça ne fonctionne pas, essayer une recherche récursive
            if (ammoText == null)
            {
                ammoText = FindTextRecursive(canvas.transform, "AmmoText");
            }

            // 3. Si ça ne fonctionne toujours pas, chercher par mots-clés
            if (ammoText == null)
            {
                TextMeshProUGUI[] allTexts = canvas.GetComponentsInChildren<TextMeshProUGUI>(true);
                foreach (TextMeshProUGUI text in allTexts)
                {
                    Debug.Log("TextMeshProUGUI trouvé: " + text.gameObject.name);
                    if (text.gameObject.name.Contains("Ammo") ||
                        text.gameObject.name.Contains("ammo") ||
                        text.gameObject.name.Contains("Munition"))
                    {
                        ammoText = text;
                        Debug.Log("AmmoText assigné: " + text.gameObject.name);
                        break;
                    }
                }
            }
        }

        // Vérification finale
        if (ammoText == null)
        {
            Debug.LogError("AmmoText non trouvé dans le Canvas. Veuillez vérifier que l'objet existe avec un composant TextMeshProUGUI.");
        }
    }

    // Fonction de recherche récursive pour trouver un TextMeshProUGUI par nom
    private TextMeshProUGUI FindTextRecursive(Transform parent, string textName)
    {
        // Vérifier directement cet objet
        TextMeshProUGUI text = parent.GetComponent<TextMeshProUGUI>();
        if (text != null && parent.name == textName)
            return text;

        // Parcourir tous les enfants
        foreach (Transform child in parent)
        {
            // Vérifier si l'enfant correspond au nom
            if (child.name == textName)
            {
                TextMeshProUGUI childText = child.GetComponent<TextMeshProUGUI>();
                if (childText != null)
                    return childText;
            }

            // Chercher récursivement dans cet enfant
            TextMeshProUGUI found = FindTextRecursive(child, textName);
            if (found != null)
                return found;
        }

        return null;
    }

    void Update()
    {
        // Ne mettre à jour l'interface que pour le joueur local
        if (photonView == null || !photonView.IsMine)
            return;

        // Mettre à jour le texte des munitions
        if (gunStats != null && ammoText != null)
        {
            ammoText.text = gunStats._currentAmmoInClip + " / " + gunStats._ammoInReserve;
        }

        // Si l'arme a changé, mettre à jour la référence
        if (playerController != null)
        {
            Item currentItem = playerController.GetCurrentItem();
            if (currentItem is AutomaticGun currentGun)
            {
                if (gunStats != currentGun)
                {
                    gunStats = currentGun;
                }
            }
        }
    }
}
