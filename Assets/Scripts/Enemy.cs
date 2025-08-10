using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Enemy : MonoBehaviour
{
    [SerializeField] Sprite dieSprite;

    public void Die()
    {
        StartCoroutine(DieCoroutine());
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
        
    }
}
