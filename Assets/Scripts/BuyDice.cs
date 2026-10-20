using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class BuyDice : MonoBehaviour
{
    public int diceValue;
    public double price;
    public DiceBagScript bag;
    private Button button;

    public TextMeshProUGUI priceText;
    public Image diceSprite;
    
    

    void Awake()
    {
        button = GetComponent<Button>();
        button.onClick.AddListener(OnButtonClicked);

        priceText = transform.Find("Label").GetComponent<TextMeshProUGUI>();
        diceSprite = transform.Find("Icon").GetComponent<Image>();
    }

    void OnDestroy()
    {
        button.onClick.RemoveListener(OnButtonClicked);
    }

    void Update()
    {
        bool canAfford = Clickable.Clicks >= price;
        button.interactable = canAfford;

        string hex = canAfford ? "#00FF0B" : "#FF0000";
        if (ColorUtility.TryParseHtmlString(hex, out Color c))
            button.image.color = c;

        // priceText.text = $"D{diceValue}:{price:C}";  # Old priceText, updated below to include number of dice already owned.
        // priceText.text = $"D{diceValue}:{price:C}\nOwned: {bag.CountOf(diceValue)}";
        priceText.text = $"D{diceValue}:{price:C} (x{bag.CountOf(diceValue)})";
    }

    private void OnButtonClicked()
    {
        if (Clickable.Clicks >= price)
        {
            Clickable.Clicks -= price;
            bag.AddDice(diceValue);
            price = Math.Round( Math.Pow(price, 1.05), 2 );
        }
    }
}
