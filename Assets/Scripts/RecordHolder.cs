using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class RecordHolder : MonoBehaviour
{
    public AudioSource audio;
    public AudioClip[] audioList;

    public GameObject DarkParticle;
    public GameObject LightParticle;

    public int[] Moves;
    public int[] MinMoves;

    int currSong = -1;
    private static RecordHolder instance = null;
    void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(this.gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
        audio = GetComponent<AudioSource>();
    }

    public void PlayDark()
    {
        DarkParticle.SetActive(true);
        LightParticle.SetActive(false);
    }

    public void PlayLight()
    {
        DarkParticle.SetActive(false);
        LightParticle.SetActive(true);
    }

    public void UpdateMove(int level, int move, int maxMove)
    {
        if (move < Moves[level] || Moves[level] == -1)
        Moves[level] = move;
        MinMoves[level] = maxMove;
    }
    // Update is called once per frame
    void Update()
    {
        // print(audioList[0].name);
        switch (SceneManager.GetActiveScene().buildIndex)
        {
            case 0: // menu
            case 1:
                if (currSong != 1)
                {
                    currSong = 1;
                    audio.clip = audioList[1];
                    audio.Play();
                }
                break;
            default:
                if (currSong != 0)
                {
                    currSong = 0;
                    audio.clip = audioList[0];
                    audio.Play();
                }
                break;
        }
    }
}
