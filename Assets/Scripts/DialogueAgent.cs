using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DialogueAgent : MonoBehaviour, IInteractable
{
    [SerializeField] private PlayerActions player;
    [SerializeField] private string[] defaultDialogue;
    [SerializeField] private string[] dialogue;
    [SerializeField] private string[] completedDialogue;
    [SerializeField] private string[] refightDialogue;
    [SerializeField] private string[] refightRewardDialogue;
    [SerializeField] private AudioSource audioSource, pickupSource;
    [SerializeField] private GameObject reward, startReward, storyThing;
    [SerializeField] private int moneyReward = 30, refightMoney = 40;
    [SerializeField] private FastTravel ft;
    [SerializeField] private Animator anim;
    [SerializeField] private Shop shop;
    [SerializeField] private bool walker;
    [SerializeField] private AgentType type;

    public enum AgentType
    {
        bear, salamander, spider, military, shop, filler
    }
    public bool lookinAtYa = false;
    private bool _doneLooking = false;
    private Vector3 _originalForward;


    private void Update()
    {
        if (Pause.paused) return;
        switch (type)
        {
            case AgentType.bear:
                if (!ProgressManager.gotBearRewards)
                {
                    storyThing.SetActive(true);
                }
                else
                {
                    storyThing.SetActive(false);
                }
                break;
            case AgentType.salamander:
                if (!ProgressManager.gotSalamanderRewards && ProgressManager.gotBearRewards)
                {
                    storyThing.SetActive(true);
                }
                else
                {
                    storyThing.SetActive(false);
                }
                break;
            case AgentType.spider:
                if (!ProgressManager.gotSpiderRewards && ProgressManager.gotSalamanderRewards)
                {
                    storyThing.SetActive(true);
                }
                else
                {
                    storyThing.SetActive(false);
                }
                break;
            case AgentType.military:
                if (ProgressManager.gotSpiderRewards)
                {
                    storyThing.SetActive(true);
                }
                else
                {
                    storyThing.SetActive(false);
                }
                break;
        }

        if (anim == null) return;
        if (lookinAtYa)
        {
            Vector3 dir = player.transform.position - transform.position;
            dir.y = 0;
            dir.Normalize();
            transform.right = Vector3.Lerp(transform.right, -dir, 1 - Mathf.Pow(0.1f, Time.deltaTime));
        }
        else
        {
            if (_doneLooking && anim.speed == 0)
            {
                anim.speed = 1;
            }
            else if(anim.speed == 0)
            {
                if (Vector3.Distance(transform.right, _originalForward) > 0.1f)
                {
                    transform.right = Vector3.Lerp(transform.right, _originalForward, 1 - Mathf.Pow(0.1f, Time.deltaTime));
                }
                else
                {
                    transform.right = _originalForward;
                    _doneLooking = true;
                }
            }
        }
    }

    public void onInteract()
    {
        if (walker)
        {
            _originalForward = transform.right;
            anim.speed = 0;
            lookinAtYa = true;
            _doneLooking = false;
        }

        switch (type)
        {
            case AgentType.bear:
                if (ProgressManager.beatBear)
                {
                    if (!ProgressManager.gotBearRewards)
                    {
                        DialogueManager.instance.setDialogues(completedDialogue, audioSource);
                        Inventory.money += moneyReward;
                        //reward.SetActive(true);
                        AudioSource pp = Instantiate(pickupSource, transform.position, Quaternion.identity);
                        pp.pitch = Random.Range(0.75f, 1.25f);
                        pp.Play();
                        ProgressManager.gotBearRewards = true;
                    }
                    else
                    {
                        if (!ProgressManager.refoughtBear)
                        {
                            DialogueManager.instance.setDialogues(refightDialogue, audioSource);
                        }
                        else
                        {
                            if (!ProgressManager.gotBearRefight)
                            {
                                DialogueManager.instance.setDialogues(refightRewardDialogue, audioSource);
                                Inventory.money += refightMoney;
                                ProgressManager.refoughtBear = false;
                            }
                        }
                    }
                }
                else
                {
                    DialogueManager.instance.setDialogues(dialogue, audioSource);
                    if (!ProgressManager.gotStartRewards)
                    {
                        startReward.SetActive(true);
                        ProgressManager.gotStartRewards = true;
                    }
                }
                break;
            case AgentType.salamander:
                
                if (ProgressManager.beatSalamander)
                {
                    if (!ProgressManager.gotSalamanderRewards)
                    {
                        DialogueManager.instance.setDialogues(completedDialogue, audioSource);
                        //reward.SetActive(true);
                        Inventory.money += moneyReward;
                        AudioSource pp = Instantiate(pickupSource, transform.position, Quaternion.identity);
                        pp.pitch = Random.Range(0.75f, 1.25f);
                        pp.Play();
                        ProgressManager.gotSalamanderRewards = true;
                    }
                    else
                    {
                        if (!ProgressManager.refoughtSalamander)
                        {
                            DialogueManager.instance.setDialogues(refightDialogue, audioSource);
                        }
                        else
                        {
                            if (!ProgressManager.gotSalamanderRefight)
                            {
                                DialogueManager.instance.setDialogues(refightRewardDialogue, audioSource);
                                Inventory.money += refightMoney;
                                ProgressManager.refoughtSalamander = false;
                            }
                        }
                    }
                }
                else
                {
                    if (ProgressManager.beatBear)
                    {
                        DialogueManager.instance.setDialogues(dialogue, audioSource);
                        ft.UnlockLava();
                    }
                    else
                    {
                        DialogueManager.instance.setDialogues(defaultDialogue, audioSource);
                    }
                }
                break;
            case AgentType.spider:
                if (ProgressManager.beatSpider)
                {
                    if (!ProgressManager.gotSpiderRewards)
                    {
                        DialogueManager.instance.setDialogues(completedDialogue, audioSource);
                        //reward.SetActive(true);
                        AudioSource pp = Instantiate(pickupSource, transform.position, Quaternion.identity);
                        pp.pitch = Random.Range(0.75f, 1.25f);
                        pp.Play();
                        Inventory.money += moneyReward;
                        ProgressManager.gotSpiderRewards = true;
                    }
                    else
                    {
                        if (!ProgressManager.refoughtSpider)
                        {
                            DialogueManager.instance.setDialogues(refightDialogue, audioSource);
                        }
                        else
                        {
                            if (!ProgressManager.gotSpiderRefight)
                            {
                                DialogueManager.instance.setDialogues(refightRewardDialogue, audioSource);
                                Inventory.money += refightMoney;
                                ProgressManager.refoughtSpider = false;
                            }
                        }
                    }
                }
                else
                {
                    if (ProgressManager.beatSalamander)
                    {
                        DialogueManager.instance.setDialogues(dialogue, audioSource);
                        ft.UnlockAcid();
                    }
                    else
                    {
                        DialogueManager.instance.setDialogues(defaultDialogue, audioSource);
                    }
                }
                break;
            case AgentType.military:
                if (ProgressManager.beatSpider)
                {
                    DialogueManager.instance.setDialogues(completedDialogue, audioSource);
                    ft.UnlockBoss();
                }
                else
                {
                    DialogueManager.instance.setDialogues(dialogue, audioSource);
                }
                break;
            case AgentType.shop:
                shop.EnableMenu();
                break;
            case AgentType.filler:
                DialogueManager.instance.setDialogues(dialogue, audioSource);
                break;
            default:
                break;
        }

    }
}
