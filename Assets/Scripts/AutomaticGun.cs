using System.Collections;
using System.Collections.Generic;
using Photon.Pun;
using UnityEngine;
using UnityEngine.UI;

public class AutomaticGun : Gun
{
    public override void Use()
    {
        if (_canShoot && _currentAmmoInClip > 0)
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
    }

    void DetermineRecoil()
    {
        StartCoroutine(RecoilEffect());
    }

    IEnumerator RecoilEffect()
    {
        transform.localPosition -= Vector3.forward * 0.1f;

        yield return new WaitForSeconds(0.05f);

        transform.localPosition += Vector3.forward * 0.1f;
    }

    public override IEnumerator ShootGun()
    {
        PV.RPC("RPC_DetermineRecoil", RpcTarget.All);
        PV.RPC("RPC_MuzzleFlash", RpcTarget.All);
        PV.RPC("RPC_PlaySoundShoot", RpcTarget.All);

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
                PV.RPC("RPC_PlaySoundImpact", RpcTarget.All, soundIndex, hit.point);
            }

            Color debugColor = isEnemy ? Color.red : Color.green;
            Debug.DrawLine(start, hit.point, debugColor, 1f);

            hit.collider.gameObject.GetComponent<IDamageable>()?.TakeDamage(itemInfo.damage);
            PV.RPC("RPC_EffectImpact", RpcTarget.All, hit.point, hit.normal);

        }
        else
        {
            Debug.Log("Missed");
            Debug.DrawLine(start, start + direction * maxFireDistance, Color.blue, 1f);
        }
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

    [PunRPC]
    void RPC_EffectImpact(Vector3 hitPosition, Vector3 hitNormal)
    {
        GameObject impactInstance = Instantiate(
            surfaceImpactEffect,
            hitPosition,
            Quaternion.LookRotation(hitNormal, Vector3.up)
        );

        Destroy(impactInstance, impactLifetime);
    }

    [PunRPC]
    void RPC_PlaySoundImpact(int soundIndex, Vector3 position)
    {
        if (soundClips.Length > soundIndex)
        {
            GameObject tempAudio = new GameObject("TempAudio");
            tempAudio.transform.position = position;

            AudioSource tempAudioSource = tempAudio.AddComponent<AudioSource>();
            tempAudioSource.clip = soundClips[soundIndex];
            tempAudioSource.spatialBlend = 1.0f;
            tempAudioSource.volume = impactSoundVolume;
            tempAudioSource.Play();

            float clipLength = soundClips[soundIndex] != null ? soundClips[soundIndex].length : 0.5f;
            Destroy(tempAudio, clipLength);
        }
    }

    [PunRPC]
    void RPC_PlaySoundShoot()
    {
        if (audioSource != null && soundClips.Length > 0)
        {
            audioSource.PlayOneShot(soundClips[0]);
        }
    }

    [PunRPC]
    void RPC_MuzzleFlash()
    {
        StartCoroutine(MuzzleFlash());
    }

    [PunRPC]
    void RPC_DetermineRecoil()
    {
        DetermineRecoil();
    }

    protected override void PlayEmptyClipSound()
    {
        if (PV != null)
        {
            PV.RPC("RPC_PlaySoundEmptyClip", RpcTarget.All);
        }
    }

    protected override void PlayReloadSound()
    {
        if (PV != null)
        {
            PV.RPC("RPC_PlaySoundReload", RpcTarget.All);
        }
    }

    [PunRPC]
    void RPC_PlaySoundEmptyClip()
    {
        if (audioSource != null && soundClips.Length > 4)
        {
            audioSource.PlayOneShot(soundClips[4]);
        }
    }

    [PunRPC]
    void RPC_PlaySoundReload()
    {
        if (audioSource != null && soundClips.Length > 3)
        {
            audioSource.PlayOneShot(soundClips[3]);
        }
    }
}
