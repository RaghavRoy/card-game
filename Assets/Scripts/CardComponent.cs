using UnityEngine;
using UnityEngine.UI;

public class CardComponent : MonoBehaviour
{
    public Image cardImage;
    public Image fruitImage;
    public Button button;
    public int Index { get; set; }

    public void Setup(Sprite fruit, System.Action<CardComponent> onClick)
    {
       this.transform.localScale = Vector3.one;
        fruitImage.sprite = fruit;
        fruitImage.enabled = false;
        button.onClick.AddListener(() => onClick(this));
    }

    public void Show(Sprite whiteSprite)
    {
        cardImage.sprite = whiteSprite;
        fruitImage.enabled = true;
    }

    public void ResetCard(Sprite back)
    {
        cardImage.sprite = back;
        fruitImage.enabled = false;
    }

    public void DisableCard()
    {
        fruitImage.gameObject.SetActive(false);
        button.image.color = new Color(0, 0, 0, 0);
        button.interactable = false;
    }
}
