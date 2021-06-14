using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ChangeScreen : MonoBehaviour
{
    public void ChangeSideToGood()
    {
        GameManager_.Instance.Player.GetComponent<PlayerController>().SetScreenToGood();

    }

    public void ChangeSideToBad()
    {
        GameManager_.Instance.Player.GetComponent<PlayerController>().SetScreenToBad();
    }
}
