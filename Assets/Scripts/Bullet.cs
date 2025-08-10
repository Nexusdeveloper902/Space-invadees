using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Bullet : MonoBehaviour
{
    private float bulletSpeed;

    public void Initialize(float bulletSpeed)
    {
        this.bulletSpeed = bulletSpeed;
    }

    void Update()
    {
        transform.position += new Vector3(0, bulletSpeed, 0) *  Time.deltaTime;
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.gameObject.CompareTag("Enemy"))
        {
            other.GetComponent<Enemy>().Die();
            Destroy(gameObject);
        }
    }
}
