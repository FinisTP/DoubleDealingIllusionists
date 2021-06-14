using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TransitionScreen : MonoBehaviour
{
    public void ResetLevel()
    {
        GameManager_.Instance.ResetGame();
    }

    public void BackToMainMenu()
    {
        GameManager_.Instance.LoadLevel(0);
    }

    public void BackToLevelSelect()
    {
        GameManager_.Instance.LoadLevel(1);
    }


}
