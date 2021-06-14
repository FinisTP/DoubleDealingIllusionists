using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class GameManager_ : MonoBehaviour
{

    public GameObject Player;
    public Camera MainCamera;
    public bool GameOver = false;
    public SoundManager SoundPlayer;
    public ParticleManager ParticlePlayer;
    public UIManager UIPlayer;
    public Animator ScreenChangeAnimator;
    public Animator TransitionAnimator;

    // public Vector2 StartPoint;

    public bool EnableFlipping = true;
    public bool GoodSide = true;
    
    // public SceneManager ScenePlayer;

    // public Text VictoryText;
    // public GameObject Crosshair;

    public bool IsRunningGame = false;

    private bool _isChanging = false;

    private static GameManager_ instance = null;
    public static GameManager_ Instance
    {
        get
        {
            if (instance == null)
            {
                instance = FindObjectOfType<GameManager_>();
                if (instance == null)
                {
                    GameObject go = new GameObject();
                    go.name = "GameManager_";
                    instance = go.AddComponent<GameManager_>();

                    // DontDestroyOnLoad(go);
                }

            }
            return instance;
        }
    }

    void Awake()
    {
        if (instance == null)
        {
            instance = this;
            // DontDestroyOnLoad(this.gameObject);
        }
        else
        {
            Destroy(gameObject);
        }

    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space) && Player.GetComponent<PlayerController>()._currentDirection == Direction.UNKNOWN && EnableFlipping && !_isChanging)
        {
            StartCoroutine(ChangeScreen());
        }
    }

    private IEnumerator ChangeScreen()
    {
        _isChanging = true;
        IsRunningGame = false;
        if (GoodSide) ScreenChangeAnimator.Play("FlipScreenToBad");
        else ScreenChangeAnimator.Play("FlipScreenToGood");
        yield return new WaitForSeconds(1f);
        IsRunningGame = true;
        _isChanging = false;
    }

    public IEnumerator GameOverAction()
    {
        IsRunningGame = false;
        Player.SetActive(false);
        yield return new WaitForSeconds(1f);

        // respawn
        GameObject checkpoint = GameObject.Find("Checkpoint");
        if (checkpoint != null)
        {
            Player.SetActive(true);
            IsRunningGame = true;
        }
    }

    public void TriggerTransitionScreenResetGame()
    {
        TransitionAnimator.SetTrigger("Reset");
        print("Called");
    }

    public void ResetGame()
    {
        // Do transition
        LoadLevel(SceneManager.GetActiveScene().buildIndex);
    }
    public IEnumerator VictoryAction()
    {
        IsRunningGame = false;
        yield return null;
        LoadLevel(0);
    }


    public void WinGame()
    {
        StartCoroutine(VictoryAction());
    }

    public void LoadNextScene()
    {
        Player.GetComponent<PlayerController>().holder.UpdateMove(SceneManager.GetActiveScene().buildIndex - 2, UIPlayer.MoveCount, UIPlayer.MinMoveRequired);
        StartCoroutine(NextScene());
    }

    public void LoadScene(int i)
    {
        StartCoroutine(Load(i));
    }

    private IEnumerator NextScene()
    {
        
        TransitionAnimator.SetTrigger("Load");
        yield return new WaitForSeconds(1f);
        if (SceneManager.GetActiveScene().buildIndex >= 10)
        {
            LoadLevel(1); // level select
        }
        else LoadLevel(SceneManager.GetActiveScene().buildIndex + 1);
    }

    private IEnumerator Load(int i)
    {
        TransitionAnimator.SetTrigger("Load");
        yield return new WaitForSeconds(1f);
        LoadLevel(i);
    }


    public void LoadLevel(int levelIndex)
    {
        // play transition
        if (levelIndex == 0) Time.timeScale = 1f;
        SceneManager.LoadScene(levelIndex);
        // yield return new WaitForSeconds(1f);

        // if (levelIndex == 0) UIPlayer.ToggleMenuCanvas(true);
        // else UIPlayer.ToggleMenuCanvas(false);

    }

}
