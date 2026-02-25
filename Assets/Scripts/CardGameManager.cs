using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CardGameManager : MonoBehaviour
{
    [Header("Settings")]
    public GameObject buttonPrefab;
    public RectTransform parentPanel;
    public int totalButtons = 18;
    public int buttonsPerRow = 4;
    public float spacing = 10f;

    [Header("Sprites")]
    public Sprite[] fruitImages;
    public Sprite cardBackSprite;
    public Sprite whiteSprite;

    [Header("References")]
    public GameUIManager uiManager;

    private List<CardComponent> allCards = new List<CardComponent>();
    private CardComponent firstCard, secondCard;
    private bool isProcessing = false;
    private CardData gameData = new CardData();

    void Start()
    {
        // Force UI to calculate dimensions before grid setup
        Canvas.ForceUpdateCanvases();
        SetupGrid();

        if (PlayerPrefs.GetInt("IsNewGame", 0) == 1)
        {
            PlayerPrefs.SetInt("IsNewGame", 0);
            StartNewGame();
        }
        else
        {
            LoadGame();
        }
    }

    void SetupGrid()
    {
        GridLayoutGroup grid = parentPanel.GetComponent<GridLayoutGroup>() ?? parentPanel.gameObject.AddComponent<GridLayoutGroup>();
        grid.padding = new RectOffset((int)spacing, (int)spacing, (int)spacing, (int)spacing);
        grid.constraint = GridLayoutGroup.Constraint.FixedColumnCount;
        grid.constraintCount = buttonsPerRow;
        grid.spacing = new Vector2(spacing, spacing);
        grid.childAlignment = TextAnchor.MiddleCenter;

        float width = parentPanel.rect.width;
        float height = parentPanel.rect.height;

        float totalSpacingX = (buttonsPerRow - 1) * spacing + grid.padding.left + grid.padding.right;
        float buttonWidth = (width - totalSpacingX) / buttonsPerRow;

        int totalRows = Mathf.CeilToInt((float)totalButtons / buttonsPerRow);
        float totalSpacingY = (totalRows - 1) * spacing + grid.padding.top + grid.padding.bottom;
        float buttonHeight = (height - totalSpacingY) / totalRows;

        float finalSize = Mathf.Min(buttonWidth, buttonHeight);
        grid.cellSize = new Vector2(finalSize, finalSize);
    }

    void StartNewGame()
    {
        List<Sprite> pairList = new List<Sprite>();
        for (int i = 0; i < totalButtons / 2; i++)
        {
            pairList.Add(fruitImages[i % fruitImages.Length]);
            pairList.Add(fruitImages[i % fruitImages.Length]);
        }

        // Shuffle Logic
        for (int i = 0; i < pairList.Count; i++)
        {
            Sprite temp = pairList[i];
            int rand = Random.Range(i, pairList.Count);
            pairList[i] = pairList[rand];
            pairList[rand] = temp;
        }

        for (int i = 0; i < totalButtons; i++) CreateCard(i, pairList[i]);
    }

    void CreateCard(int index, Sprite fruitSprite)
    {
        GameObject go = Instantiate(buttonPrefab, parentPanel);
        CardComponent card = go.GetComponent<CardComponent>();
        card.Index = index;

        // ✅ Matches your Setup(Sprite fruit, System.Action<CardComponent> onClick)
        card.Setup(fruitSprite, OnCardClicked);

        allCards.Add(card);
    }

    void OnCardClicked(CardComponent clicked)
    {
        // Click-lock logic (isProcessing) prevents selecting 3rd card
        if (isProcessing || clicked == null || clicked == firstCard) return;

        clicked.Show(whiteSprite);

        if (firstCard == null)
        {
            firstCard = clicked;
        }
        else
        {
            secondCard = clicked;
            isProcessing = true;
            gameData.Moves++;
            uiManager.UpdateUI(gameData.Moves, gameData.Matches);
            StartCoroutine(CheckMatch());
        }
    }

    IEnumerator CheckMatch()
    {
        yield return new WaitForSeconds(0.6f);

        if (firstCard.fruitImage.sprite == secondCard.fruitImage.sprite)
        {
            gameData.Matches++;
            gameData.MatchedIndexes.Add(firstCard.Index);
            gameData.MatchedIndexes.Add(secondCard.Index);

            firstCard.DisableCard();
            secondCard.DisableCard();

            uiManager.UpdateUI(gameData.Moves, gameData.Matches);

            if (gameData.Matches == totalButtons / 2)
                uiManager.ShowGameOver(gameData);
        }
        else
        {
            // ✅ Matches your ResetCard(Sprite back)
            firstCard.ResetCard(cardBackSprite);
            secondCard.ResetCard(cardBackSprite);
        }

        firstCard = null;
        secondCard = null;
        isProcessing = false;
        SaveGame();
    }

    public void SaveGame()
    {
        // 1. Save matched cards by checking the Index property of disabled cards
        List<int> matchedIndexes = new List<int>();
        for (int i = 0; i < allCards.Count; i++)
        {
            // In the new script, we use the button's interactable state to check for matches
            if (!allCards[i].button.interactable)
                matchedIndexes.Add(allCards[i].Index);
        }
        PlayerPrefs.SetString("MatchedCards", string.Join(",", matchedIndexes));

        // 2. Save moves and matches from the gameData object
        PlayerPrefs.SetInt("Moves", gameData.Moves);
        PlayerPrefs.SetInt("Matches", gameData.Matches);

        // 3. Save the card order (which fruit sprite is on which card)
        List<int> cardOrder = new List<int>();
        foreach (var card in allCards)
        {
            // Find the index of the sprite in the original fruitImages array
            int idx = System.Array.IndexOf(fruitImages, card.fruitImage.sprite);
            cardOrder.Add(idx);
        }
        PlayerPrefs.SetString("CardOrder", string.Join(",", cardOrder));

        PlayerPrefs.Save();
    }

    public void LoadGame()
    {
        // 1. Restore scores into the gameData object
        gameData.Moves = PlayerPrefs.GetInt("Moves", 0);
        gameData.Matches = PlayerPrefs.GetInt("Matches", 0);

        // Update the UI immediately after loading values
        uiManager.UpdateUI(gameData.Moves, gameData.Matches);

        // 2. Reconstruct the cards in the correct order
        string cardOrderStr = PlayerPrefs.GetString("CardOrder", "");
        if (!string.IsNullOrEmpty(cardOrderStr))
        {
            string[] orderParts = cardOrderStr.Split(',');
            for (int i = 0; i < orderParts.Length && i < totalButtons; i++)
            {
                if (int.TryParse(orderParts[i], out int imgIndex) && imgIndex >= 0 && imgIndex < fruitImages.Length)
                {
                    // Re-create the card using the saved sprite index
                    CreateCard(i, fruitImages[imgIndex]);
                }
            }
        }

        // 3. Re-disable the cards that were already matched
        string matchedStr = PlayerPrefs.GetString("MatchedCards", "");
        if (!string.IsNullOrEmpty(matchedStr))
        {
            string[] matchedParts = matchedStr.Split(',');
            foreach (string part in matchedParts)
            {
                if (int.TryParse(part, out int matchedIndex) && matchedIndex >= 0 && matchedIndex < allCards.Count)
                {
                    allCards[matchedIndex].DisableCard();
                    // Ensure the data object also knows these are matched
                    if (!gameData.MatchedIndexes.Contains(matchedIndex))
                        gameData.MatchedIndexes.Add(matchedIndex);
                }
            }
        }
    }

}
