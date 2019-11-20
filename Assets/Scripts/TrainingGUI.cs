using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class TrainingGUI : MonoBehaviour
{
    public GameObject stopTrainingButton;
    public GameObject continueButton;
    public GameObject resultDisplayPanel;

    public void DisableTrainingButton()
    {
        stopTrainingButton.SetActive(false);

        resultDisplayPanel.SetActive(true);
        continueButton.SetActive(true);
    }

    public void LoadNextScene()
    {
        SceneManager.LoadScene(0);
    }
}
