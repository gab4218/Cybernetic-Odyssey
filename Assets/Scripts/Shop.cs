using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class Shop : MonoBehaviour
{
    [SerializeField] private GameObject _menu;
    [SerializeField] private GameObject[] pages;
    [SerializeField] private BuyStuff[] _buyStuffs;
    [SerializeField] private TMP_Text _moneyText;

    void Start()
    {
        _menu.SetActive(false);
    }

    
    void Update()
    {
        if (Pause.paused) return;

        if (_menu.activeSelf && Time.timeScale == 0)
        {
            _moneyText.text = Inventory.money.ToString();


            if (Input.GetKeyDown(KeyCode.Escape))
            {
                DisableMenu();   
            }
        }

    }

    public void EnableMenu()
    {
        _menu.SetActive(true);
        Time.timeScale = 0;
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
        if (Inventory.hasMelee) _buyStuffs[3].hasBeenBought = true;
        //if (Inventory.expanded) _buyStuffs[0].hasBeenBought = true;

        foreach (BuyStuff bs in _buyStuffs)
        {
            if (bs.thing > 2)
            {
                foreach (int f in Inventory.secondarySlots)
                {
                    if ((bs.thing == 4 && f == 10) || (bs.thing == 5 && f == 13)) bs.hasBeenBought = true;
                }
                if (bs.thing == 4 && Inventory.availableUpgrades.Contains(10))
                {
                    bs.hasBeenBought = true;
                } 
                else if (bs.thing == 5 && Inventory.availableUpgrades.Contains(13))
                {
                    bs.hasBeenBought = true;
                }
            }
            if (bs.price <= Inventory.money)
            {
                bs.canBuy = true;
            }
            else
            {
                bs.canBuy = false;
            }

        }
    }

    public void DisableMenu()
    {
        _menu.SetActive(false);
        Time.timeScale = 1;
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }


    public void goToPage(int p)
    {
        foreach (GameObject pa in pages)
        {
            pa.SetActive(false);
        }
        pages[p].SetActive(true);

    }
}
