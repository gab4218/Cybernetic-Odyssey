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
    private AudioSource audioSource;
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
            audioSource.pitch = Random.Range(0.4f,1.6f);
            audioSource.Play();
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

    public void setDialogues(string[] newDialogue, AudioSource aS)
    {
        dialogues = newDialogue;
        dialogueBox.SetActive(true);
        audioSource = aS;
        audioSource.clip = talking;
        audioSource.pitch = Random.Range(0.4f, 1.6f);
        audioSource.Play();
        inDialogue = true;
        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;
        currentIndex = 0;
        dialogueText.text = dialogues[0];
    }
}
