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
    public Vector3 weaponPosition; // Position locale normale de l'arme
    public Vector3 weaponAimingPosition; // Position locale de l'arme en visée
    public Vector3 weaponRotation; // Rotation locale normale de l'arme
    private Quaternion weaponRotationQuaternion;
    public Vector3 weaponAimingRotation; // Rotation locale de l'arme en visée
    private Quaternion weaponAimingRotationQuaternion;

    public float aimSmoothing = 10; // Lissage de la visée
    public float maxFireDistance = 500f; // Distance maximale de tir

    [Header("Accuracy Settings")]
    public float hipFireSpread = 5f; // Dispersion en degrés quand on tire sans viser
    public float aimingSpread = 0.5f; // Dispersion en degrés quand on vise
    public float maxSpreadIncrease = 5f; // Augmentation maximale de la dispersion lors de tirs consécutifs
    public float spreadIncreasePerShot = 0.5f; // Augmentation de dispersion par tir
    public float spreadRecoveryTime = 0.5f; // Temps nécessaire pour récupérer la précision initiale
    public float spreadRecoveryDelay = 0.2f; // Délai avant de commencer à récupérer la précision

    [Header("Impact Effects")]
    public GameObject enemyImpactEffect; // Effet d'impact sur les ennemis (comme du sang)
    public GameObject surfaceImpactEffect; // Effet d'impact sur les surfaces normales
    public float impactForce = 500f; // Force appliquée aux objets touchés
    public float impactLifetime = 3f; // Durée de vie des effets d'impact en secondes

    private bool _wasAiming = false; // Variable pour suivre l'état de visée précédent

    private void Awake()
    {
        // Convert Euler angles to Quaternions
        weaponRotationQuaternion = Quaternion.Euler(weaponRotation);
        weaponAimingRotationQuaternion = Quaternion.Euler(weaponAimingRotation);
    }

    private void Start()
    {
        _currentAmmoInClip = clipSize;
        _ammoInReserve = reservedAmmoCapacity;
        _canShoot = true;
        _currentSpread = hipFireSpread;
        _lastShotTime = -spreadRecoveryDelay;

        // Initialize weapon to normal position and rotation
        transform.localPosition = weaponPosition;
        transform.localRotation = weaponRotationQuaternion;
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
        Vector3 targetPos = weaponPosition;
        Quaternion targetRot = weaponRotationQuaternion;

        bool isAiming = Input.GetMouseButton(1);

        if (isAiming && !_wasAiming)
        {
            _currentSpread = aimingSpread;
        }

        if (isAiming)
        {
            targetPos = weaponAimingPosition;
            targetRot = weaponAimingRotationQuaternion;
        }

        // Use a more consistent interpolation factor
        float lerpFactor = Mathf.Clamp01(aimSmoothing * Time.deltaTime);

        // Interpolate position
        Vector3 desiredPosition = Vector3.Lerp(transform.localPosition, targetPos, lerpFactor);

        // Snap to exact position if very close (prevents floating point imprecision)
        if (Vector3.Distance(transform.localPosition, targetPos) < 0.001f)
            transform.localPosition = targetPos;
        else
            transform.localPosition = desiredPosition;

        // Improved rotation handling with higher smoothing for rotation
        float rotationFactor = Mathf.Clamp01((aimSmoothing * 0.8f) * Time.deltaTime);
        transform.localRotation = Quaternion.Slerp(transform.localRotation, targetRot, rotationFactor);

        // Only snap to target rotation when very close to avoid jitter
        if (Quaternion.Angle(transform.localRotation, targetRot) < 0.5f)
        {
            transform.localRotation = targetRot;
        }

        _wasAiming = isAiming;
    }

    void DetermineRecoil()
    {
        transform.localPosition -= Vector3.forward * 0.1f;

        // if (randomizeRecoil)
        // {
        //     float xRecoil = Random.Range(-randomRecoilConstraints.x, randomRecoilConstraints.x);
        //     float yRecoil = Random.Range(-randomRecoilConstraints.y, randomRecoilConstraints.y);

        //     Vector2 recoild = new Vector2(xRecoil, yRecoil);

        //     _currentRotation += recoild;
        // }
        // else
        // {
        //     int currentStep = clipSize + 1 - _currentAmmoInClip;
        //     currentStep = Mathf.Clamp(currentStep, 0, recoilPattern.Length - 1);

        //     _currentRotation += recoilPattern[currentStep];
        // }
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

        if (Physics.Raycast(start, direction, out hit, maxFireDistance))
        {
            Debug.Log("Hit " + hit.collider.gameObject.name);

            // Vérifier si l'objet touché est sur la couche "Enemy"
            bool isEnemy = hit.collider.gameObject.layer == LayerMask.NameToLayer("Enemy");

            // Appliquer la force aux objets avec Rigidbody
            Rigidbody rb = hit.transform.GetComponent<Rigidbody>();
            if (rb != null)
            {
                rb.constraints = RigidbodyConstraints.None;
                rb.AddForce(direction * impactForce);
            }

            // Instancier l'effet d'impact approprié
            GameObject impactEffectToSpawn = isEnemy ? enemyImpactEffect : surfaceImpactEffect;
            if (impactEffectToSpawn != null)
            {
                // Créer l'effet d'impact
                GameObject impactInstance = Instantiate(
                    impactEffectToSpawn,
                    hit.point,
                    Quaternion.LookRotation(hit.normal)
                );

                // Détruire l'effet après un certain temps
                Destroy(impactInstance, impactLifetime);
            }

            // Dessiner la ligne de débug avec la couleur appropriée
            Color debugColor = isEnemy ? Color.red : Color.green;
            Debug.DrawLine(start, hit.point, debugColor, 1f);
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