﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Singleton : MonoBehaviour
{
    public string startingScene;
    public string endingScene;
    private static Singleton instance = null;

    // Use this for initialization
    void Awake()
    {
        if (instance != null && instance.startingScene == this.startingScene)
        {
            Destroy(gameObject);
        }
        else
        {
            instance = this;
        }
        DontDestroyOnLoad(this.gameObject);
    }

    void OnEnable()
    { 
        SceneManager.sceneLoaded += OnLevelFinishedLoading;
    }

    void OnLevelFinishedLoading(Scene scene, LoadSceneMode mode)
    {
        AudioSource audio = instance.gameObject.GetComponent<AudioSource>();
        if (scene.name == endingScene)
        {
            if (audio.isPlaying)
            {
                Destroy(gameObject);
            }
        }

        if (scene.name == startingScene)
        {
            if (!audio.isPlaying)
            {
                audio.Play();
            }
        }
    }
}
