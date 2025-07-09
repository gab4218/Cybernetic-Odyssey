using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DialogueAgent : MonoBehaviour, IInteractable
{
    [SerializeField] private string[] dialogue;
    [SerializeField] private string[] bearDialogue;
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private GameObject bearReward, startReward;
    [SerializeField] private bool bearGuy = false;
    [SerializeField] private bool militaryGuy = false;
    [SerializeField] private FastTravel ft;
    
    public void onInteract()
    {
        if (bearGuy)
        {
            if (ProgressManager.beatBear)
            {
                DialogueManager.instance.setDialogues(bearDialogue, audioSource);
                if (!ProgressManager.gotBearRewards)
                {
                    bearReward.SetActive(true);
                    ProgressManager.gotBearRewards = true;
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
        }
        else if(militaryGuy)
        {
            if (ProgressManager.beatBear)
            {
                DialogueManager.instance.setDialogues(bearDialogue, audioSource);
                ft.UnlockBoss();
            }
            else
            {
                DialogueManager.instance.setDialogues(dialogue, audioSource);
            }

        }
    }
}
