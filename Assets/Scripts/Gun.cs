using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Gun : MonoBehaviour
{
    public float damage = 10f;
    public float range = 1000f;
    public float impactForce = 120f;

    public Camera fpsCam;
    public GameObject muzzleFlash;
    private AudioSource _audioSource;

    public GameObject impactEffect;
    public GameObject fleshImpactEffect;

    void Start()
    {
       _audioSource = GetComponent<AudioSource>();
    }

    // Update is called once per frame
    void Update()
    {
        if(Input.GetButtonDown("Fire1"))
        {
            _audioSource.Play();
            Shoot();
            StartCoroutine(MuzzleFlashRoutine());
            Debug.DrawRay(fpsCam.transform.position, fpsCam.transform.forward, Color.red, 100f);
        }
    }

    IEnumerator MuzzleFlashRoutine()
    {
        muzzleFlash.SetActive(true);
        yield return new WaitForSeconds(0.1f);
        muzzleFlash.SetActive(false);
    }

    void Shoot()
    {
        RaycastHit hit;
        if (Physics.Raycast(fpsCam.transform.position, fpsCam.transform.forward, out hit, range))
        {
            Debug.Log(hit.transform.name);

            Target target = hit.transform.GetComponent<Target>();
            if (target != null)
            {
                target.TakeDamage(damage);
            }

            if (hit.rigidbody != null)
            {
                hit.rigidbody.AddForce(-hit.normal * impactForce);
                GameObject fleshGO = Instantiate(fleshImpactEffect, hit.point, Quaternion.LookRotation(hit.normal));
                Destroy(fleshGO, 1f);

            } else
            {
                GameObject impactGO = Instantiate(impactEffect, hit.point, Quaternion.LookRotation(hit.normal));
                Destroy(impactGO, 5f);
            }

        }
    }
}
