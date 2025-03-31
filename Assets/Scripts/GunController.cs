using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GunController : MonoBehaviour
{
    [Header("Gun Settings")]
    public float fireRate = 0.1f;
    public int clipSize = 30;
    public int reservedAmmoCapacity = 270;

    //Variables that change throughout the code
    public bool _canShoot;
    public int _currentAmmoInClip;
    public int _ammoInReserve;

    //Muzzle Flash
    public Image muzzleFlashImage;
    public Sprite[] flashes;

    //Aiming
    public Vector3 normalLocalPosition;
    public Quaternion normalLocalRotation;
    public Vector3 aimingLocalPosition;
    public Quaternion aimingLocalRotation;

    public float aimSmoothing = 10;

    private void Start()
    {
        _currentAmmoInClip = clipSize;
        _ammoInReserve = reservedAmmoCapacity;
        _canShoot = true;
    }

    private void Update()
    {
        DetermineAim();
        if (Input.GetMouseButton(0) && _canShoot && _currentAmmoInClip > 0)
        {
            _canShoot = false;
            _currentAmmoInClip--;
            StartCoroutine(ShootGun());
        }
        else if(Input.GetKeyDown(KeyCode.R) && _currentAmmoInClip < clipSize && _ammoInReserve > 0)
        {
            int amountNeeded = clipSize - _currentAmmoInClip;
            if(amountNeeded >= _ammoInReserve)
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

    IEnumerator ShootGun()
    {
        StartCoroutine(MuzzleFlash());
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
}