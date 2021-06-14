using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PortalBehaviour : MonoBehaviour
{
    public bool IsActive;
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.gameObject.CompareTag("Player") && IsActive)
        {
            print("Portalll");
            GameManager_.Instance.LoadNextScene();
        }
    }
}
