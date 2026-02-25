using TMPro;
using UnityEngine.SceneManagement;
using UnityEngine;

public class GameUIManager : MonoBehaviour
{
    public TextMeshProUGUI moveText, matchText;
    public GameObject gameOverPanel;
    public TextMeshProUGUI finalMoves, finalMatches;

    public void UpdateUI(int moves, int matches)
    {
        moveText.text = moves.ToString();
        matchText.text = matches.ToString();
    }

    public void ShowGameOver(CardData data)
    {
        gameOverPanel.SetActive(true);
        finalMoves.text = data.Moves.ToString();
        finalMatches.text = data.Matches.ToString();
    }

    public void Restart()
    {
        PlayerPrefs.SetInt("IsNewGame", 1);
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

   public void GoHome()
    {
        SceneManager.LoadScene("Home");
    }
}
