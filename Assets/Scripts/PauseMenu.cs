using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class PauseMenu : MonoBehaviour
{
	public GameObject PauseMenuObject;

    void Start()
    {
		ShowMenu(false);
	}

    void Update()
    {
		if (Input.GetKeyDown(KeyCode.Escape))
			ShowMenu(!PauseMenuObject.activeSelf);
    }

	public void ShowMenu(bool b)
	{
		if (PauseMenuObject.activeSelf == b)
			return;

		GameManager_.Instance.IsRunningGame = !b;
		PauseMenuObject.SetActive(b);
		Time.timeScale = b ? 0f : 1f;
	}

	public void Resume()
	{
		ShowMenu(false);
	}

	public void ToLevelSelect()
    {
		SceneManager.LoadScene(1);
		Time.timeScale = 1f;
	}

	public void ToMenu()
	{
		SceneManager.LoadScene(0);
		Time.timeScale = 1f;
	}

	public void QuitGame()
	{
		Application.Quit();
	}
}
