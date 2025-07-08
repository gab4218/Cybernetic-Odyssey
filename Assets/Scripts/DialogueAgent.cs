using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DialogueAgent : MonoBehaviour, IInteractable
{
    [SerializeField] private string[] dialogue;
    [SerializeField] private string[] bearDialogue;
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private GameObject bearReward, startReward;
    
    public void onInteract()
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
            if (!ProgressManager.gotStartRewards)
            {
                startReward.SetActive(true);
                ProgressManager.gotStartRewards = true;
            }
            DialogueManager.instance.setDialogues(dialogue, audioSource);
        }
    }
}
