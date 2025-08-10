using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyMovement : MonoBehaviour
{
    private GameObject[] enemiesArray;
    private int totalEnemiesAtStart;

    void Start()
    {
        StartCoroutine(Movement());
        
    }   
    IEnumerator Movement()
    {
        float moveSpeed = 0.1f;        // Horizontal step size
        float stepDown = 0.5f;         // Vertical step size
        int direction = 1;             // 1 = right, -1 = left
        float baseDelay = 0.5f;        // Slowest delay
        float minDelay = 0.05f;        // Fastest delay

        while (true)
        {
            enemiesArray = GameObject.FindGameObjectsWithTag("Enemy");

            if (enemiesArray.Length == 0)
                yield break;

            bool hitEdge = false;

            // Move all enemies horizontally
            foreach (GameObject enemy in enemiesArray)
            {
                if (enemy != null)
                {
                    enemy.transform.position += Vector3.right * direction * moveSpeed;

                    Vector3 viewportPos = Camera.main.WorldToViewportPoint(enemy.transform.position);
                    if (viewportPos.x < 0.05f || viewportPos.x > 0.95f)
                    {
                        hitEdge = true;
                    }
                }
            }

            // If an enemy hit the edge, reverse direction and move all down
            if (hitEdge)
            {
                direction *= -1;
                foreach (GameObject enemy in enemiesArray)
                {
                    if (enemy != null)
                        enemy.transform.position += Vector3.down * stepDown;
                }
            }

            // Speed scaling: fewer enemies = faster movement
            float t = 1f - ((float)enemiesArray.Length / (float)totalEnemiesAtStart);
            float currentDelay = Mathf.Lerp(baseDelay, minDelay, t);

            yield return new WaitForSeconds(currentDelay);
        }
    }

}