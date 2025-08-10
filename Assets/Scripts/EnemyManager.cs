using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyManager : MonoBehaviour
{
    private GameObject[] enemiesArray;

    void Start()
    {
       StartCoroutine(RandomEnemyShoot());
    }

    IEnumerator RandomEnemyShoot()
    {
        while (true)
        {
            enemiesArray = GameObject.FindGameObjectsWithTag("Enemy");
            GameObject randomEnemy =  enemiesArray[Random.Range(0, enemiesArray.Length)];
            yield return new WaitForSeconds(1f);
            randomEnemy.GetComponent<Enemy>().Shoot();
        }
    }
}
