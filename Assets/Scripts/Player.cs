using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player : MonoBehaviour
{
    [SerializeField] private float speed;
    [SerializeField] private float delay;
    [SerializeField] private GameObject bulletPrefab;
    [SerializeField] private Transform bulletSpawn;
    [SerializeField] private float bulletSpeed;
    [SerializeField] private float bulletDespawnTime = 3f;
    
    private Vector3 movement;
    private float timer;
    
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        movement = new Vector3(Input.GetAxisRaw("Horizontal"), 0, 0);
        transform.position += movement * speed * Time.deltaTime;

        if (Input.GetButtonDown("Jump") && Time.time >= timer)
        {
            Shoot();
            timer = Time.time + delay;
        }
    }

    void Shoot()
    {
        var bullet = Instantiate(bulletPrefab, bulletSpawn.position, Quaternion.identity * Quaternion.Euler(0, 0, 180));
        var bulletScript = bullet.GetComponent<Bullet>();
        bulletScript.Initialize(bulletSpeed);
        
        Destroy(bullet, bulletDespawnTime);
    }
}
