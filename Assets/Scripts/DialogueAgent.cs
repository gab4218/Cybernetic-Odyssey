using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DialogueAgent : MonoBehaviour, IInteractable
{
    [SerializeField] private string[] dialogue;
    public void onInteract()
    {
        DialogueManager.instance.setDialogues(dialogue);
    }
}
