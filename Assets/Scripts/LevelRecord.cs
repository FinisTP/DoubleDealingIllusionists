using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class LevelRecord : MonoBehaviour
{
    // Start is called before the first frame update
    public RecordHolder holder;
    public TMP_Text[] levelText;
    public TMP_Text titleText;
    void Start()
    {
        holder = GameObject.FindObjectOfType<RecordHolder>();
        for (int i = 0; i < levelText.Length; ++i)
        {
            if (holder.Moves[i] == -1)
            {
                levelText[i].text = "Uncleared";
                levelText[i].fontSize = 15;
                levelText[i].color = new Color(1, 1, 0, 1);
            }
            else
            {
                levelText[i].text = $"{holder.Moves[i]}/{holder.MinMoves[i]}";
                levelText[i].fontSize = 24;
                if (holder.Moves[i] > holder.MinMoves[i]) levelText[i].color = new Color(1, 0, 0, 1);
                else if (holder.Moves[i] <= holder.MinMoves[i]) levelText[i].color = new Color(0, 1, 0, 1);
                else levelText[i].color = new Color(1, 1, 1, 1);
            }
        }
        bool allComplete = true;
        for (int i = 0; i < levelText.Length; ++i)
        {
            if (holder.Moves[i] == -1) allComplete = false;
        }
        if (allComplete) titleText.text = "Congratulations on beating the game!";
    }
}
