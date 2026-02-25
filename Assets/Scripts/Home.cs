using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class Home : MonoBehaviour
{
    public Button loadButton;
    public Button newGameButton;
    public string gameSceneName = "CardScene";

    void Start()
    {
        loadButton.onClick.AddListener(LoadGame);
        newGameButton.onClick.AddListener(NewGame);
    }

    void LoadGame()
    {
        // Set flag for load game
        PlayerPrefs.SetInt("IsNewGame", 0); // 0 = Load saved game
        PlayerPrefs.Save();

        // Load game scene and saved state will be applied automatically
        SceneManager.LoadScene(gameSceneName);
    }

    void NewGame()
    {
        // Set flag for new game
        PlayerPrefs.SetInt("IsNewGame", 1); // 1 = Start fresh

        // Clear saved game data
        PlayerPrefs.DeleteKey("MatchedCards");
        PlayerPrefs.DeleteKey("Moves");
        PlayerPrefs.DeleteKey("Matches");
        PlayerPrefs.DeleteKey("CardOrder"); // If you save card arrangement
        PlayerPrefs.Save();

        // Load fresh game scene
        SceneManager.LoadScene(gameSceneName);
    }
}
