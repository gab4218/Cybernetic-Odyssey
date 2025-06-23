using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class AsyncLoadManager : MonoBehaviour
{
    public static AsyncLoadManager instance;

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
            DontDestroyOnLoad(this);
        }
        else if (instance != this)
        {
            Destroy(gameObject);
        }
    }


    [SerializeField] private Image loadingScreenImage, loadingBarImage;
    [SerializeField] private TMP_Text loadingText;
    private bool loadingScene;


    private void Start()
    {
        loadingBarImage.gameObject.SetActive(false);
        loadingScreenImage.gameObject.SetActive(false);
        loadingText.gameObject.SetActive(false);
    }

    public void LoadScene(string scene)
    {
        if (!loadingScene)
        {
            loadingScreenImage.gameObject.SetActive(true);
            loadingBarImage.gameObject.SetActive(true);
            loadingScene = true;
            StartCoroutine(AsyncLoad(scene));
        }
    }


    private IEnumerator AsyncLoad(string scene)
    {

        AsyncOperation aOp = SceneManager.LoadSceneAsync(scene);
        aOp.allowSceneActivation = false;
        loadingBarImage.color = Color.red;
        while (aOp.progress < 0.9f)
        {
            loadingBarImage.fillAmount = aOp.progress / 0.9f;
            yield return null;
        }
        loadingBarImage.fillAmount = 1;
        loadingBarImage.color = Color.green;
        loadingText.gameObject.SetActive(true);
        while (!Input.anyKeyDown)
        {
            yield return null;
        }
        loadingBarImage.color = Color.red;
        loadingBarImage.gameObject.SetActive(false);
        loadingScreenImage.gameObject.SetActive(false);
        loadingText.gameObject.SetActive(false);
        loadingScene = false;
        aOp.allowSceneActivation = true;
    }
}
