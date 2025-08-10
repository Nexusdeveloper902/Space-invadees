using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

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
            if (enemiesArray.Length == 0)
            {
                SceneManager.LoadScene("WinMenu");
                yield break;
            }
            GameObject randomEnemy =  enemiesArray[Random.Range(0, enemiesArray.Length)];
            yield return new WaitForSeconds(1f);
            randomEnemy.GetComponent<Enemy>().Shoot();
        }
    }
}
