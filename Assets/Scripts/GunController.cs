using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GunController : MonoBehaviour
{
    [Header("Gun Settings")]
    public float fireRate = 0.1f; // Cadence de tir en secondes
    public int clipSize = 30; // Capacité du chargeur
    public int reservedAmmoCapacity = 270; // Capacité de munitions en réserve

    //Variables that change throughout the code
    public bool _canShoot; // Indique si l'arme peut tirer
    public int _currentAmmoInClip; // Munitions actuelles dans le chargeur
    public int _ammoInReserve; // Munitions en réserve
    private float _currentSpread; // Dispersion actuelle calculée en fonction des tirs et de l'état de visée
    private float _lastShotTime; // Moment du dernier tir pour gérer la récupération de la précision

    //Muzzle Flash
    public Image muzzleFlashImage; // Image de l'éclair de bouche
    public Sprite[] flashes; // Tableaux de sprites pour l'éclair de bouche

    //Aiming
    public Vector3 normalLocalPosition; // Position locale normale de l'arme
    public Quaternion normalLocalRotation; // Rotation locale normale de l'arme
    public Vector3 aimingLocalPosition; // Position locale de l'arme en visée
    public Quaternion aimingLocalRotation; // Rotation locale de l'arme en visée

    public float aimSmoothing = 10; // Lissage de la visée
    public float maxFireDistance = 500f; // Distance maximale de tir

    [Header("Mouse Settings")]
    public float mouseSensitivity = 1; // Sensibilité de la souris
    Vector2 _currentRotation; // Rotation actuelle de la caméra

    //Weapon Recoil
    public bool randomizeRecoil; // Indique si le recul est aléatoire
    public Vector2 randomRecoilConstraints; // Contraintes de recul aléatoire
    public Vector2[] recoilPattern; // Modèle de recul

    [Header("Accuracy Settings")]
    public float hipFireSpread = 5f; // Dispersion en degrés quand on tire sans viser
    public float aimingSpread = 0.5f; // Dispersion en degrés quand on vise
    public float maxSpreadIncrease = 5f; // Augmentation maximale de la dispersion lors de tirs consécutifs
    public float spreadIncreasePerShot = 0.5f; // Augmentation de dispersion par tir
    public float spreadRecoveryTime = 0.5f; // Temps nécessaire pour récupérer la précision initiale
    public float spreadRecoveryDelay = 0.2f; // Délai avant de commencer à récupérer la précision

    private void Start()
    {
        _currentAmmoInClip = clipSize;
        _ammoInReserve = reservedAmmoCapacity;
        _canShoot = true;
        _currentSpread = hipFireSpread;
        _lastShotTime = -spreadRecoveryDelay;
    }

    private void Update()
    {
        DetermineAim();
        UpdateSpread();

        if (Input.GetMouseButton(0) && _canShoot && _currentAmmoInClip > 0)
        {
            _canShoot = false;
            _currentAmmoInClip--;
            _lastShotTime = Time.time;

            if (!Input.GetMouseButton(1) || aimingSpread > 0)
            {
                _currentSpread += spreadIncreasePerShot;
            }

            StartCoroutine(ShootGun());
        }
        else if (Input.GetKeyDown(KeyCode.R) && _currentAmmoInClip < clipSize && _ammoInReserve > 0)
        {
            int amountNeeded = clipSize - _currentAmmoInClip;
            if (amountNeeded >= _ammoInReserve)
            {
                _currentAmmoInClip += _ammoInReserve;
                _ammoInReserve -= amountNeeded;
            }
            else
            {
                _currentAmmoInClip = clipSize;
                _ammoInReserve -= amountNeeded;
            }
        }
    }

    void DetermineAim()
    {
        Vector3 targetPos = normalLocalPosition;
        Quaternion targetRot = normalLocalRotation;
        if (Input.GetMouseButton(1))
        {
            targetPos = aimingLocalPosition;
            targetRot = aimingLocalRotation;
        }

        Vector3 desirePosition = Vector3.Lerp(transform.localPosition, targetPos, Time.deltaTime * aimSmoothing);

        transform.localPosition = desirePosition;
        transform.localRotation = Quaternion.Lerp(transform.localRotation, targetRot, Time.deltaTime * aimSmoothing);
    }

    void DetermineRecoil()
    {
        transform.localPosition -= Vector3.forward * 0.1f;

        //Recoil of the camera
        if (randomizeRecoil)
        {
            float xRecoil = Random.Range(-randomRecoilConstraints.x, randomRecoilConstraints.x);
            float yRecoil = Random.Range(-randomRecoilConstraints.y, randomRecoilConstraints.y);

            Vector2 recoild = new Vector2(xRecoil, yRecoil);

            _currentRotation += recoild;
        }
        else
        {
            int currentStep = clipSize + 1 - _currentAmmoInClip;
            currentStep = Mathf.Clamp(currentStep, 0, recoilPattern.Length - 1);

            _currentRotation += recoilPattern[currentStep];
        }
    }

    void UpdateSpread()
    {
        // Déterminer la dispersion de base en fonction de l'état de visée
        float baseSpread = Input.GetMouseButton(1) ? aimingSpread : hipFireSpread;

        // Récupérer progressivement la précision après un certain délai
        if (Time.time > _lastShotTime + spreadRecoveryDelay)
        {
            _currentSpread = Mathf.Lerp(_currentSpread, baseSpread, Time.deltaTime / spreadRecoveryTime);
        }

        // Limiter la dispersion maximale
        _currentSpread = Mathf.Clamp(_currentSpread, baseSpread, baseSpread + maxSpreadIncrease);
    }

    IEnumerator ShootGun()
    {
        DetermineRecoil();
        StartCoroutine(MuzzleFlash());

        RayCastForEnnemy();

        yield return new WaitForSeconds(fireRate);
        _canShoot = true;
    }

    IEnumerator MuzzleFlash()
    {
        muzzleFlashImage.sprite = flashes[Random.Range(0, flashes.Length)];
        muzzleFlashImage.color = Color.white;
        yield return new WaitForSeconds(0.05f);
        muzzleFlashImage.sprite = null;
        muzzleFlashImage.color = new Color(0, 0, 0, 0);
    }

    void RayCastForEnnemy()
    {
        RaycastHit hit;
        Vector3 start = transform.parent.position;
        Vector3 direction = CalculateSpreadDirection(transform.parent.forward);

        if (Physics.Raycast(start, direction, out hit, maxFireDistance, 1 << LayerMask.NameToLayer("Enemy")))
        {
            try
            {
                Debug.Log("Hit " + hit.collider.gameObject.name);
                Rigidbody rb = hit.transform.GetComponent<Rigidbody>();
                rb.constraints = RigidbodyConstraints.None;
                rb.AddForce(direction * 500);
            }
            catch
            {
                // Gestion des exceptions
            }
            Debug.DrawLine(start, hit.point, Color.red, 1f);
        }
        else if (Physics.Raycast(start, direction, out hit, maxFireDistance))
        {
            Debug.Log("Hit " + hit.collider.gameObject.name);
            Rigidbody rb = hit.transform.GetComponent<Rigidbody>();
            if (rb != null)
            {
                rb.constraints = RigidbodyConstraints.None;
                rb.AddForce(direction * 500);
            }
            Debug.DrawLine(start, hit.point, Color.green, 1f);
        }
        else
        {
            Debug.Log("Missed");
            Debug.DrawLine(start, start + direction * maxFireDistance, Color.blue, 1f);
        }
    }

    Vector3 CalculateSpreadDirection(Vector3 originalDirection)
    {
        // Si la dispersion actuelle est quasiment nulle, retourner la direction exacte
        const float minSpreadThreshold = 0.001f;
        if (_currentSpread <= minSpreadThreshold)
        {
            return originalDirection;
        }

        // Créer une dispersion aléatoire
        float spreadAngleX = Random.Range(-_currentSpread, _currentSpread);
        float spreadAngleY = Random.Range(-_currentSpread, _currentSpread);

        // Convertir en quaternion pour appliquer la rotation
        Quaternion spreadRotation = Quaternion.Euler(spreadAngleX, spreadAngleY, 0);

        // Appliquer la dispersion à la direction originale
        return spreadRotation * originalDirection;
    }
}