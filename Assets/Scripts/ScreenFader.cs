using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public interface IFadeObserver
{
    public void FadeComplete();
}

public class ScreenFader : MonoBehaviour
{
    public static ScreenFader Instance { get; private set; }
    public Image screen;
    float timer;
    bool fading = false;
    Color color;

    List<IFadeObserver> fadeObservers = new List<IFadeObserver>();
    // Start is called before the first frame update

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        //DontDestroyOnLoad(gameObject);
    }

    public void TriggerFade(Color col, IFadeObserver observer)
    {
        fading = true;
        timer = 0;
        screen.color = new Color(col.r, col.g, col.b, 0);
        fadeObservers.Add(observer);
    }

    void Update()
    {
        if(fading)
        {
            screen.color = new Color(screen.color.r, screen.color.g, screen.color.b, timer);
            timer += Time.deltaTime;

            if(timer > 1)
            {
                fading = false;

                for (int i = 0; i < fadeObservers.Count; i++)
                {
                    fadeObservers[i].FadeComplete();
                }
                fadeObservers.Clear();
            }
        }
    }







}
