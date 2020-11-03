using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MenuManager : Singleton<MenuManager>
{
    Scene scene;

    public GameObject tutorialPanel;

    public void StartGame()
    {
        if (scene != null)
        {
            int chanceForAd = Random.Range(1, 100);

            if(chanceForAd < 67)
            {
                int randomBuildNumber = Random.Range(1, 20);

                SceneManager.LoadScene(randomBuildNumber);
            }
            else
            {
                if (AdManager.Insatance != null)
                {
                    AdManager.Insatance.WatchAd();
                }
            }
        }
    }

    public void TutorialButton()
    {
        tutorialPanel.SetActive(true);
    }

    public void ExitButton()
    {
        tutorialPanel.SetActive(false);
    }
}
