using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Player : MonoBehaviour
{
    [SerializeField] private float speed;
    [SerializeField] private float delay;
    [SerializeField] private GameObject bulletPrefab;
    [SerializeField] private Transform bulletSpawn;
    [SerializeField] private float bulletSpeed;
    [SerializeField] private float bulletDespawnTime = 3f;
    
    public int lives = 3;
    
    private Vector3 movement;
    private float timer;
    
    void Update()
    {
        Debug.Log(lives);
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
        var bullet = Instantiate(bulletPrefab, bulletSpawn.position, Quaternion.identity);
        var bulletScript = bullet.GetComponent<Bullet>();
        bulletScript.Initialize(bulletSpeed, true);
        
        Destroy(bullet, bulletDespawnTime);
    }

    public void Die()
    {
        if (lives > 1)
        {
            lives--;
        }
        else
        {
            SceneManager.LoadScene("Scenes/SampleScene");
        }
    }
}
