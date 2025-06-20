using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class DialogueManager : MonoBehaviour
{
    public static DialogueManager instance;
    public GameObject dialogueBox;
    public TMP_Text dialogueText;
    public string[] dialogues;
    public bool inDialogue = false;
    public int currentIndex = 0;
    public AudioSource audioSource;
    public AudioClip talking;

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }
        else
        {
            Destroy(this);
        }
        dialogueBox.SetActive(false);
    }


    public void nextDialogue()
    {
        SoundSingleton.Instance.Boton();
        if (currentIndex < dialogues.Length - 1)
        {
            currentIndex++;
            dialogueText.text = dialogues[currentIndex];
        }
        else
        {
            dialogueBox.SetActive(false);
            Cursor.visible = false;
            Cursor.lockState = CursorLockMode.Locked;
            inDialogue = false;
            audioSource.Stop();
        }
    }

    public void setDialogues(string[] newDialogue)
    {
        dialogues = newDialogue;
        dialogueBox.SetActive(true);
        audioSource.clip = talking;
        audioSource.Play();
        inDialogue = true;
        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;
        currentIndex = 0;
        dialogueText.text = dialogues[0];
    }
}
