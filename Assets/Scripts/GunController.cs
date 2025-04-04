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
    [HideInInspector] public Quaternion weaponRotationQuaternion;
    public Vector3 weaponAimingRotation; // Rotation locale de l'arme en visée
    [HideInInspector] public Quaternion weaponAimingRotationQuaternion;

    public float aimSmoothing = 10; // Lissage de la visée
    public float maxFireDistance = 500f; // Distance maximale de tir

    [Header("Accuracy Settings")]
    public float hipFireSpread = 5f; // Dispersion en degrés quand on tire sans viser
    public float aimingSpread = 0.5f; // Dispersion en degrés quand on vise
    public float maxSpreadIncrease = 5f; // Augmentation maximale de la dispersion lors de tirs consécutifs
    public float spreadIncreasePerShot = 0.5f; // Augmentation de dispersion par tir
    public float spreadRecoveryTime = 0.5f; // Temps nécessaire pour récupérer la précision initiale
    public float spreadRecoveryDelay = 0.2f; // Délai avant de commencer à récupérer la précision

    [Header("Impact Effects / Sounds")]
    public GameObject enemyImpactEffect; // Effet d'impact sur les ennemis (comme du sang)
    public GameObject surfaceImpactEffect; // Effet d'impact sur les surfaces normales
    public AudioSource audioSource;
    public AudioClip[] soundClips; // [0] = gunshot, [1] = enemy hit, [2] = surface hit, [3] = reload, [4] = empty clip
    public float impactForce = 500f; // Force appliquée aux objets touchés
    public float impactLifetime = 3f; // Durée de vie des effets d'impact en secondes
    public float impactSoundVolume = 0.7f; // Volume des sons d'impact

    private bool _wasAiming = false; // Variable pour suivre l'état de visée précédent

    private void Awake()
    {
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

        transform.localPosition = weaponPosition;
        transform.localRotation = weaponRotationQuaternion;

        if (audioSource == null)
            audioSource = GetComponent<AudioSource>();
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
            if (audioSource != null && soundClips.Length > 0)
            {
                audioSource.PlayOneShot(soundClips[3]);
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

        float lerpFactor = Mathf.Clamp01(aimSmoothing * Time.deltaTime);

        Vector3 desiredPosition = Vector3.Lerp(transform.localPosition, targetPos, lerpFactor);

        if (Vector3.Distance(transform.localPosition, targetPos) < 0.001f)
            transform.localPosition = targetPos;
        else
            transform.localPosition = desiredPosition;

        float rotationFactor = Mathf.Clamp01((aimSmoothing * 0.8f) * Time.deltaTime);
        transform.localRotation = Quaternion.Slerp(transform.localRotation, targetRot, rotationFactor);

        if (Quaternion.Angle(transform.localRotation, targetRot) < 0.5f)
        {
            transform.localRotation = targetRot;
        }

        _wasAiming = isAiming;
    }

    void DetermineRecoil()
    {
        transform.localPosition -= Vector3.forward * 0.1f;
    }

    void UpdateSpread()
    {
        float baseSpread = Input.GetMouseButton(1) ? aimingSpread : hipFireSpread;

        if (Time.time > _lastShotTime + spreadRecoveryDelay)
        {
            _currentSpread = Mathf.Lerp(_currentSpread, baseSpread, Time.deltaTime / spreadRecoveryTime);
        }

        _currentSpread = Mathf.Clamp(_currentSpread, baseSpread, baseSpread + maxSpreadIncrease);
    }

    IEnumerator ShootGun()
    {
        DetermineRecoil();
        StartCoroutine(MuzzleFlash());
        if (audioSource != null && soundClips.Length > 0)
        {
            audioSource.PlayOneShot(soundClips[0]);
        }
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

            bool isEnemy = hit.collider.gameObject.layer == LayerMask.NameToLayer("Enemy");

            Rigidbody rb = hit.transform.GetComponent<Rigidbody>();
            if (rb != null)
            {
                rb.constraints = RigidbodyConstraints.None;
                rb.AddForce(direction * impactForce);
            }

            int soundIndex = isEnemy ? 1 : 2; // 1 pour ennemi, 2 pour surface
            if (soundClips.Length > soundIndex)
            {
                PlaySoundAtPosition(soundClips[soundIndex], hit.point, impactSoundVolume);
            }

            GameObject impactEffectToSpawn = isEnemy ? enemyImpactEffect : surfaceImpactEffect;
            if (impactEffectToSpawn != null)
            {
                GameObject impactInstance = Instantiate(
                    impactEffectToSpawn,
                    hit.point,
                    Quaternion.LookRotation(hit.normal)
                );

                Destroy(impactInstance, impactLifetime);
            }

            Color debugColor = isEnemy ? Color.red : Color.green;
            Debug.DrawLine(start, hit.point, debugColor, 1f);
        }
        else
        {
            Debug.Log("Missed");
            Debug.DrawLine(start, start + direction * maxFireDistance, Color.blue, 1f);
        }
    }

    void PlaySoundAtPosition(AudioClip clip, Vector3 position, float volume = 1.0f)
    {
        GameObject tempAudio = new GameObject("TempAudio");
        tempAudio.transform.position = position;

        AudioSource tempAudioSource = tempAudio.AddComponent<AudioSource>();
        tempAudioSource.clip = clip;
        tempAudioSource.spatialBlend = 1.0f;
        tempAudioSource.volume = volume;
        tempAudioSource.Play();

        float clipLength = clip != null ? clip.length : 0.5f;
        Destroy(tempAudio, clipLength);
    }

    Vector3 CalculateSpreadDirection(Vector3 originalDirection)
    {
        const float minSpreadThreshold = 0.001f;
        if (_currentSpread <= minSpreadThreshold)
        {
            return originalDirection;
        }

        float spreadAngleX = Random.Range(-_currentSpread, _currentSpread);
        float spreadAngleY = Random.Range(-_currentSpread, _currentSpread);

        Quaternion spreadRotation = Quaternion.Euler(spreadAngleX, spreadAngleY, 0);

        return spreadRotation * originalDirection;
    }
}