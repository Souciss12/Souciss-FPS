using System.Collections;
using System.Collections.Generic;
using Photon.Pun;
using UnityEngine;
using UnityEngine.UI;

public abstract class Gun : Item
{
    [Header("Gun Settings")]
    public bool isAutomatic = true; // Indicates if the gun is automatic
    public float fireRate = 0.1f; // Fire rate in seconds
    public int clipSize = 30; // Clip capacity
    public int reservedAmmoCapacity = 270; // Reserved ammo capacity

    public bool _canShoot; // Indicates if the weapon can shoot
    public int _currentAmmoInClip; // Current ammo in clip
    public int _ammoInReserve; // Ammo in reserve
    public float _currentSpread; // Current spread calculated based on shots and aiming state
    public float _lastShotTime; // Time of the last shot to manage accuracy recovery
    private bool _canEmptyClipSound; // Indicates if the empty clip sound can be played
    public bool _semiAutoReadyToFire = true; // Tracks if the player can fire again in semi-auto mode

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

    public abstract override void Use();

    public PhotonView PV;

    private void Awake()
    {
        weaponRotationQuaternion = Quaternion.Euler(weaponRotation);
        weaponAimingRotationQuaternion = Quaternion.Euler(weaponAimingRotation);
        PV = GetComponent<PhotonView>();
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
        UpdateSpread();

        if (Input.GetMouseButtonUp(0) && !isAutomatic)
        {
            _semiAutoReadyToFire = true;
        }

        if (Input.GetKeyDown(KeyCode.R) && _currentAmmoInClip < clipSize && _ammoInReserve > 0 && _canShoot)
        {
            StartCoroutine(ReloadGun());
        }
        else if (Input.GetMouseButtonDown(0) && _currentAmmoInClip <= 0 && _canEmptyClipSound)
        {
            _canEmptyClipSound = false;
            PlayEmptyClipSound();
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
            PlayReloadSound();
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

    protected virtual void PlayEmptyClipSound()
    {
        if (audioSource != null && soundClips.Length > 4)
        {
            audioSource.PlayOneShot(soundClips[4]);
        }
    }

    protected virtual void PlayReloadSound()
    {
        if (audioSource != null && soundClips.Length > 3)
        {
            audioSource.PlayOneShot(soundClips[3]);
        }
    }

    public override void Aim()
    {
        DetermineAim();
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

    void UpdateSpread()
    {
        float baseSpread = Input.GetMouseButton(1) ? aimingSpread : hipFireSpread;

        if (Time.time > _lastShotTime + spreadRecoveryDelay)
        {
            _currentSpread = Mathf.Lerp(_currentSpread, baseSpread, Time.deltaTime / spreadRecoveryTime);
        }

        _currentSpread = Mathf.Clamp(_currentSpread, baseSpread, baseSpread + maxSpreadIncrease);
    }

    public abstract IEnumerator ShootGun();
}
