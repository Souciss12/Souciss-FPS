using System.Collections;
using System.Collections.Generic;
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
        transform.localPosition -= Vector3.forward * 0.1f;
    }

    public override IEnumerator ShootGun()
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

            hit.collider.gameObject.GetComponent<IDamageable>()?.TakeDamage(itemInfo.damage);
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