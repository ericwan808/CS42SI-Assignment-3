using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class BuyDice : MonoBehaviour
{
    // Eric W
    // value of dice: 1,4,6,8,12,20
    public int diceValue;
    // price of buying one new dice
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

        // set color based on ability to buy, created with help of Claude
        string hex = canAfford ? "#00FF0B" : "#FF0000";
        if (ColorUtility.TryParseHtmlString(hex, out Color c))
            button.image.color = c;

        priceText.text = $"D{diceValue}:{price:C} (x{bag.CountOf(diceValue)})";
    }

    private void OnButtonClicked()
    {
        if (Clickable.Clicks >= price)
        {
            // buy dice if possible, add to bag, and increase price of next purchase
            Clickable.Clicks -= price;
            bag.AddDice(diceValue);
            price = Math.Round( Math.Pow(price, 1.1), 2 );
        }
    }
}
