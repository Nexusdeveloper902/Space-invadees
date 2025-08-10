using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Bullet : MonoBehaviour
{
    [SerializeField] private Sprite explosion;
    private float bulletSpeed;
    private bool shootByPlayer;
    private bool atBunker = false;

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
        if (shootByPlayer && !atBunker)
        {
            transform.position += new Vector3(0, bulletSpeed, 0) *  Time.deltaTime;
        }
        else if (!atBunker)
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
        else if (other.gameObject.CompareTag("Bunker"))
        {
            SpriteRenderer spriteRenderer = gameObject.GetComponent<SpriteRenderer>();
            spriteRenderer.sprite = explosion;
            atBunker = true;
            other.GetComponent<Bunker>().TakeDamage();
            Destroy(gameObject, 0.5f);
        }
    }
}
