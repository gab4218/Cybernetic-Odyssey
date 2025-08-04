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
    [SerializeField] private GameObject reward, startReward, refightReward;
    [SerializeField] private int moneyReward = 30;
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
        if (anim == null) return;

        if (lookinAtYa)
        {
            Vector3 dir = player.transform.position - transform.position;
            dir.y = 0;
            dir.Normalize();
            transform.forward = Vector3.Lerp(transform.forward, dir, Mathf.Pow(0.1f, Time.deltaTime));
        }
        else
        {
            if (_doneLooking && anim.speed == 0)
            {
                anim.speed = 1;
            }
            else if(anim.speed != 0)
            {
                if (Vector3.Distance(transform.forward, _originalForward) > Mathf.Epsilon)
                {
                    transform.forward = Vector3.Lerp(transform.forward, _originalForward, Mathf.Pow(0.1f, Time.deltaTime));
                }
                else
                {
                    _doneLooking = true;
                }
            }
        }
    }

    public void onInteract()
    {
        if (walker)
        {
            _originalForward = transform.forward;
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
                        DialogueManager.instance.setDialogues(defaultDialogue, audioSource);
                    }
                    //else
                    //{
                    //    if (!ProgressManager.refoughtBear)
                    //    {
                    //        DialogueManager.instance.setDialogues(refightDialogue, audioSource);
                    //    }
                    //    else
                    //    {
                    //        if (!ProgressManager.gotBearRefight)
                    //        {
                    //            DialogueManager.instance.setDialogues(refightDialogue, audioSource);
                    //        }
                    //    }
                    //}
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
                        DialogueManager.instance.setDialogues(defaultDialogue, audioSource);
                    }
                    //else
                    //{
                    //    if (!ProgressManager.refoughtSalamander)
                    //    {
                    //        DialogueManager.instance.setDialogues(refightDialogue, audioSource);
                    //    }
                    //    else
                    //    {
                    //        if (!ProgressManager.gotSalamanderRefight)
                    //        {
                    //            DialogueManager.instance.setDialogues(refightDialogue, audioSource);
                    //        }
                    //    }
                    //}
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
                        DialogueManager.instance.setDialogues(defaultDialogue, audioSource);
                    }
                    //else
                    //{
                    //    if (!ProgressManager.refoughtSpider)
                    //    {
                    //        DialogueManager.instance.setDialogues(refightDialogue, audioSource);
                    //    }
                    //    else
                    //    {
                    //        if (!ProgressManager.gotSpiderRefight)
                    //        {
                    //            DialogueManager.instance.setDialogues(refightDialogue, audioSource);
                    //        }
                    //    }
                    //}
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
            default:
                break;
        }

    }
}
