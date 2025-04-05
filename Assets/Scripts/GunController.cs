using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GunController : MonoBehaviour
{
    [Header("Gun Settings")]
    public float fireRate = 0.1f; // Fire rate in seconds
    public int clipSize = 30; // Clip capacity
    public int reservedAmmoCapacity = 270; // Reserved ammo capacity

    //Variables that change throughout the code
    public bool _canShoot; // Indicates if the weapon can shoot
    public int _currentAmmoInClip; // Current ammo in clip
    public int _ammoInReserve; // Ammo in reserve
    private float _currentSpread; // Current spread calculated based on shots and aiming state
    private float _lastShotTime; // Time of the last shot to manage accuracy recovery
    private bool _canEmptyClipSound; // Indicates if the empty clip sound can be played

    //Muzzle Flash
    public Image muzzleFlashImage; // Muzzle flash image
    public Sprite[] flashes; // Array of sprites for muzzle flash

    //Aiming
    public Vector3 weaponPosition; // Normal local position of the weapon
    public Vector3 weaponAimingPosition; // Aiming local position of the weapon
    public Vector3 weaponRotation; // Normal local rotation of the weapon
    [HideInInspector] public Quaternion weaponRotationQuaternion;
    public Vector3 weaponAimingRotation; // Aiming local rotation of the weapon
    [HideInInspector] public Quaternion weaponAimingRotationQuaternion;

    public float aimSmoothing = 10; // Aiming smoothing
    public float maxFireDistance = 500f; // Maximum firing distance

    [Header("Accuracy Settings")]
    public float hipFireSpread = 5f; // Spread in degrees when firing from the hip
    public float aimingSpread = 0.5f; // Spread in degrees when aiming
    public float maxSpreadIncrease = 5f; // Maximum spread increase during consecutive shots
    public float spreadIncreasePerShot = 0.5f; // Spread increase per shot
    public float spreadRecoveryTime = 0.5f; // Time needed to recover initial accuracy
    public float spreadRecoveryDelay = 0.2f; // Delay before starting to recover accuracy

    [Header("Impact Effects / Sounds")]
    public GameObject enemyImpactEffect; // Impact effect on enemies (like blood)
    public GameObject surfaceImpactEffect; // Impact effect on normal surfaces
    public AudioSource audioSource;
    public AudioClip[] soundClips; // [0] = gunshot, [1] = enemy hit, [2] = surface hit, [3] = reload, [4] = empty clip
    public float impactForce = 500f; // Force applied to hit objects
    public float impactLifetime = 3f; // Lifetime of impact effects in seconds
    public float impactSoundVolume = 0.7f; // Volume of impact sounds

    private bool _wasAiming = false; // Variable to track previous aiming state

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
        _canEmptyClipSound = true;

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
        else if (Input.GetKeyDown(KeyCode.R) && _currentAmmoInClip < clipSize && _ammoInReserve > 0 && _canShoot)
        {
            StartCoroutine(ReloadGun());
        }
        else if (Input.GetMouseButtonDown(0) && _currentAmmoInClip <= 0 && _canEmptyClipSound)
        {
            _canEmptyClipSound = false;
            if (audioSource != null && soundClips.Length > 0)
            {
                audioSource.PlayOneShot(soundClips[4]);
            }
        }
        else if (Input.GetMouseButtonUp(0))
        {
            _canEmptyClipSound = true;
        }
    }

    IEnumerator ReloadGun()
    {
        _canShoot = false;

        if (audioSource != null && soundClips.Length > 3)
        {
            audioSource.PlayOneShot(soundClips[3]);
            yield return new WaitForSeconds(soundClips[3].length);
        }

        int amountNeeded = clipSize - _currentAmmoInClip;
        if (amountNeeded >= _ammoInReserve)
        {
            _currentAmmoInClip += _ammoInReserve;
            _ammoInReserve = 0;
        }
        else
        {
            _currentAmmoInClip = clipSize;
            _ammoInReserve -= amountNeeded;
        }

        _canShoot = true;
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

            int soundIndex = isEnemy ? 1 : 2; // 1 for enemy, 2 for surface
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