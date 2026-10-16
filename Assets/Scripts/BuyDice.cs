using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.UI;

public class BuyDice : MonoBehaviour
{
    public int diceValue;
    public int price;
    public DiceBagScript bag;
    private Button button;

    void Awake()
    {
        button = GetComponent<Button>();
        button.onClick.AddListener(OnButtonClicked);
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
    }

    private void OnButtonClicked()
    {
        if (Clickable.Clicks >= price)
        {
            Clickable.Clicks -= price;
            bag.AddDice(diceValue);
        }
    }
}
