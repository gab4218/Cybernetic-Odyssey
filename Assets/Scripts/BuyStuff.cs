using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class BuyStuff : MonoBehaviour
{
    public int price;
    public int thing;
    public bool hasBeenBought = false;
    public bool canBuy = false;
    private bool _allowedToBuy = true;
    [SerializeField] Button _button;

    private void Update()
    {
        if (Pause.paused) return;
        if (Time.timeScale > 0) return;
        if (thing == 4)
        {
            _allowedToBuy = Inventory.hasMelee;
        }
        canBuy = price <= Inventory.money;
        if (hasBeenBought) gameObject.SetActive(false);
        _button.interactable = canBuy && _allowedToBuy;
    }


}
