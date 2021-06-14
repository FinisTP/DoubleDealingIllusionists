using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TrapBehavior : MonoBehaviour
{
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            GameManager_.Instance.ParticlePlayer.PlayEffect("PlayerDeath", collision.transform.position);
            StartCoroutine(ResetLevel());
            Destroy(collision.gameObject);
            
        }
    }

    private IEnumerator ResetLevel()
    {
        
        yield return new WaitForSeconds(1f);
        GameManager_.Instance.TriggerTransitionScreenResetGame();
    }
}
