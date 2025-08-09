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
}
