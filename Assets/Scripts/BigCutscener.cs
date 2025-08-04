using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BigCutscener : MonoBehaviour
{
    [SerializeField] private GameObject[] _pages;
    [SerializeField] private string _scene;
    private int _current = 0;

    private void Update()
    {
        if (Pause.paused) return;
        if (Input.GetKeyDown(KeyCode.Mouse0))
        {
            _pages[_current].SetActive(false);
            _current++;
            if (_current < _pages.Length)
            {
                _pages[_current].SetActive(true);
            }
            else
            {
                AsyncLoadManager.instance.LoadScene(_scene);
            }
        }
    }
}
