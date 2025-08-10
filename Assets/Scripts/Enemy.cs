using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Enemy : MonoBehaviour
{
    [SerializeField] Sprite dieSprite;
    [SerializeField] GameObject bulletPrefab;
    [SerializeField] private Transform bulletSpawn;
    [SerializeField] private float bulletSpeed;
    [SerializeField] private float bulletDespawnTime = 3f;

    public void Die()
    {
        StartCoroutine(DieCoroutine());
    }

    private void Start()
    {
        
    }

    private IEnumerator DieCoroutine()
    {
        gameObject.GetComponentInChildren<SpriteRenderer>().sprite = dieSprite;
        gameObject.GetComponentInChildren<Animator>().enabled = false;
        yield return new WaitForSeconds(.7f);
        Destroy(gameObject);
    }

    public void Shoot()
    {
        var bullet = Instantiate(bulletPrefab, bulletSpawn.position, Quaternion.identity *  Quaternion.Euler(0, 0, 180));
        var bulletScript = bullet.GetComponent<Bullet>();
        bulletScript.Initialize(bulletSpeed, false);
        
        Destroy(bullet, bulletDespawnTime);
    }
}
