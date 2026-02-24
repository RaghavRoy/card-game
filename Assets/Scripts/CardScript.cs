using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement;

public class CardScript : MonoBehaviour
{
    [Header("UI Settings")]
    public GameObject buttonPrefab;
    public RectTransform parentPanel;
    public TextMeshProUGUI counterText;
    public TextMeshProUGUI matchCounterText;
    public int totalButtons = 18;
    public int buttonsPerRow = 4;
    public float spacing = 10f;

    [Header("Card Images")]
    public Sprite[] fruitImages;
    public Sprite cardBackSprite;
    public Sprite whiteSprite;
    private List<Button> allButtons = new List<Button>();
    private Sprite[] assignedImages;

    [Header("Game Over UI")]
    public GameObject gameOverPanel;
    public Button restartButton;
    public Button homeButton;
    public TextMeshProUGUI turnsTaken;
    public TextMeshProUGUI totalMatches;

    private int clickCount = 0;
    private int outsideCounter = 0;
    private int matchCounter = 0;

    private Button firstButton;
    private Button secondButton;

    void Start()
    {
        if (gameOverPanel != null)
            gameOverPanel.SetActive(false);

        if (restartButton != null)
            restartButton.onClick.AddListener(RestartGame);
        if (homeButton != null)
            homeButton.onClick.AddListener(GoHome);

        GridLayoutGroup grid = parentPanel.GetComponent<GridLayoutGroup>();
        if (!grid) grid = parentPanel.gameObject.AddComponent<GridLayoutGroup>();

        grid.constraint = GridLayoutGroup.Constraint.FixedColumnCount;
        grid.constraintCount = buttonsPerRow;
        grid.spacing = new Vector2(spacing, spacing);
        grid.childAlignment = TextAnchor.MiddleCenter;
        grid.startAxis = GridLayoutGroup.Axis.Vertical;

        float totalSpacingX = (buttonsPerRow - 1) * spacing;
        float buttonWidth = (parentPanel.rect.width - totalSpacingX) / buttonsPerRow;
        int totalRows = Mathf.CeilToInt((float)totalButtons / buttonsPerRow);
        float totalSpacingY = (totalRows - 1) * spacing;
        float buttonHeight = (parentPanel.rect.height - totalSpacingY) / totalRows;
        float finalSize = Mathf.Min(buttonWidth, buttonHeight);
        grid.cellSize = new Vector2(finalSize, finalSize);

        // ✅ Use only IsNewGame flag to decide game load/setup
        if (PlayerPrefs.GetInt("IsNewGame", 0) == 1)
        {
            PlayerPrefs.SetInt("IsNewGame", 0); // reset
            AssignImagesToButtons(); // fresh setup
        }
        else
        {
            LoadGame(); // continue saved game
        }

        UpdateCounterText();
        UpdateMatchCounterText();
    }

    void AssignImagesToButtons()
    {
        assignedImages = new Sprite[totalButtons];
        List<Sprite> tempList = new List<Sprite>();

        for (int i = 0; i < totalButtons / 2; i++)
        {
            tempList.Add(fruitImages[i % fruitImages.Length]);
            tempList.Add(fruitImages[i % fruitImages.Length]);
        }

        for (int i = 0; i < tempList.Count; i++)
        {
            Sprite temp = tempList[i];
            int rand = Random.Range(i, tempList.Count);
            tempList[i] = tempList[rand];
            tempList[rand] = temp;
        }

        for (int i = 0; i < totalButtons; i++)
            CreateButton(i, tempList[i]);
    }

    void CreateButton(int index, Sprite image)
    {
        assignedImages[index] = image;

        GameObject btnObj = Instantiate(buttonPrefab, parentPanel);
        btnObj.name = "Button_" + (index + 1);

        Image btnImg = btnObj.GetComponent<Image>();
        btnImg.sprite = cardBackSprite;

        Transform child = btnObj.transform.GetChild(0);
        Image fruitImage = child.GetComponent<Image>();
        fruitImage.sprite = assignedImages[index];
        fruitImage.enabled = false;

        Button btn = btnObj.GetComponent<Button>();
        int idx = index;
        btn.onClick.AddListener(() => OnButtonClick(btn, idx));

        allButtons.Add(btn);
    }

    void OnButtonClick(Button clickedButton, int index)
    {
        if (clickedButton == null || clickedButton == firstButton) return;

        clickedButton.GetComponent<Image>().sprite = whiteSprite;
        clickedButton.transform.GetChild(0).GetComponent<Image>().enabled = true;

        clickCount++;

        if (firstButton == null)
        {
            firstButton = clickedButton;
        }
        else
        {
            secondButton = clickedButton;
            StartCoroutine(CheckMatch(index));
        }

        if (clickCount == 2)
        {
            outsideCounter++;
            UpdateCounterText();
            clickCount = 0;
        }
    }

    IEnumerator CheckMatch(int secondIndex)
    {
        yield return new WaitForSeconds(0.5f);

        int firstIndex = allButtons.IndexOf(firstButton);

        if (assignedImages[firstIndex] == assignedImages[secondIndex])
        {
            matchCounter++;
            UpdateMatchCounterText();
            DisableCard(firstButton);
            DisableCard(secondButton);

            if (matchCounter == totalButtons / 2 && gameOverPanel != null)
                gameOverPanel.SetActive(true);

            turnsTaken.text = outsideCounter.ToString();
            totalMatches.text = matchCounter.ToString();


        }
        else
        {
            ResetButton(firstButton);
            ResetButton(secondButton);
        }

        firstButton = null;
        secondButton = null;
    }

    void DisableCard(Button btn)
    {
        btn.transform.GetChild(0).gameObject.SetActive(false);
        btn.image.color = new Color(0, 0, 0, 0);
        btn.interactable = false;
    }

    void ResetButton(Button btn)
    {
        btn.GetComponent<Image>().sprite = cardBackSprite;
        btn.transform.GetChild(0).GetComponent<Image>().enabled = false;
    }

    void UpdateCounterText()
    {
        counterText.text = outsideCounter.ToString();
    }

    void UpdateMatchCounterText()
    {
        matchCounterText.text = matchCounter.ToString();
    }

    void SaveGame()
    {
        // Save matched cards
        List<int> matchedIndexes = new List<int>();
        for (int i = 0; i < allButtons.Count; i++)
        {
            if (!allButtons[i].interactable)
                matchedIndexes.Add(i);
        }
        PlayerPrefs.SetString("MatchedCards", string.Join(",", matchedIndexes));

        // Save moves and matches using the SAME keys we will load from
        PlayerPrefs.SetInt("Moves", outsideCounter);
        PlayerPrefs.SetInt("Matches", matchCounter);

        // Save card order by image index
        List<int> cardOrder = new List<int>();
        foreach (var img in assignedImages)
        {
            int idx = System.Array.IndexOf(fruitImages, img);
            cardOrder.Add(idx);
        }
        PlayerPrefs.SetString("CardOrder", string.Join(",", cardOrder));

        PlayerPrefs.Save();
    }

    void LoadGame()
    {
        // Load moves and matches using correct keys
        outsideCounter = PlayerPrefs.GetInt("Moves", 0);
        matchCounter = PlayerPrefs.GetInt("Matches", 0);
        assignedImages = new Sprite[totalButtons];

        // Load saved card order
        string cardOrderStr = PlayerPrefs.GetString("CardOrder", "");
        if (!string.IsNullOrEmpty(cardOrderStr))
        {
            string[] orderParts = cardOrderStr.Split(',');
            for (int i = 0; i < orderParts.Length && i < totalButtons; i++)
            {
                int imgIndex;
                if (int.TryParse(orderParts[i], out imgIndex) && imgIndex >= 0 && imgIndex < fruitImages.Length)
                {
                    assignedImages[i] = fruitImages[imgIndex];
                    CreateButton(i, assignedImages[i]);
                }
            }
        }

        // Restore matched cards
        string matchedStr = PlayerPrefs.GetString("MatchedCards", "");
        if (!string.IsNullOrEmpty(matchedStr))
        {
            string[] matchedParts = matchedStr.Split(',');
            foreach (string part in matchedParts)
            {
                int matchedIndex;
                if (int.TryParse(part, out matchedIndex) && matchedIndex >= 0 && matchedIndex < allButtons.Count)
                {
                    DisableCard(allButtons[matchedIndex]);
                }
            }
        }
    }


    void RestartGame()
    {
        PlayerPrefs.SetInt("IsNewGame", 1); // restart as new game
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    void GoHome()
    {
        SceneManager.LoadScene("Home");
    }
}
