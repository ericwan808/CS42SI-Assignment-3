using TMPro;
using Unity.VisualScripting;
using UnityEngine;

public class ButtonSpawner : MonoBehaviour
{
    [SerializeField] private BuyDice buttonPrefab;
    [SerializeField] private Transform buttonContainer;

    public DiceBagScript bag;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    private void Start()
    {
        CreateItem(1, 10f, new Vector2(-175, -125));
        CreateItem(4, 20f, new Vector2(- 175, -225));
        CreateItem(6, 30f, new Vector2(-175, -325));
        CreateItem(8, 40f, new Vector2(-175, -425));
        CreateItem(12, 50f, new Vector2(-175, -525));
        CreateItem(20, 60f, new Vector2(-175, -625));
    }

    private void CreateItem(int value, float price, Vector2 position)
    {
        BuyDice button = Instantiate(buttonPrefab, buttonContainer);
        RectTransform buttonRect = (RectTransform)button.transform;
        buttonRect.anchoredPosition = position;

        button.diceValue = value;
        button.price = price;
        button.bag = bag;
        button.diceSprite.sprite = bag.getSprite(value);
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
