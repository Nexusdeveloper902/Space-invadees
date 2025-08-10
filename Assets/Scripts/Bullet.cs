using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Bullet : MonoBehaviour
{
    private float bulletSpeed;
    private bool shootByPlayer;

    public void Initialize(float bulletSpeed, bool shootByPlayer)
    {
        this.bulletSpeed = bulletSpeed;
        this.shootByPlayer = shootByPlayer;
    }

    private void Start()
    {
        Destroy(gameObject, 3);
    }

    void Update()
    {
        if (shootByPlayer)
        {
            transform.position += new Vector3(0, bulletSpeed, 0) *  Time.deltaTime;
        }
        else
        {
            transform.position -= new Vector3(0, bulletSpeed, 0) *  Time.deltaTime;
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.gameObject.CompareTag("Enemy") && shootByPlayer)
        {
            other.GetComponent<Enemy>().Die();
            Destroy(gameObject);
        }
        else if (other.gameObject.CompareTag("Player") && !shootByPlayer)
        {
            other.GetComponent<Player>().Die();
            Destroy(gameObject);
        }
    }
}
