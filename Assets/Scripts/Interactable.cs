﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

/**
 * Script to attach to all interactable objects. Detects when Light or Shadow
 * are within range to interact with the object.
 */
public class Interactable : MonoBehaviour {
    private string inRange = "";    // Marks which character is in range
    public UnityEvent interact = null;
    public UnityEvent lightPlayerEnter = null;
    public UnityEvent shadowPlayerEnter = null;
    public UnityEvent lightPlayerExit = null;
    public UnityEvent shadowPlayerExit = null;

	// Update is called once per frame
	void Update () {
        if (inRange == "LightPlayer" && Input.GetKeyDown(KeyCode.DownArrow))
        {
            interact.Invoke();
        }
        if (inRange == "ShadowPlayer" && Input.GetKeyDown(KeyCode.S))
        {
            interact.Invoke();
        }
	}

    // Because OnTriggerStay2D was only being called when collider was moving
    void OnTriggerEnter2D(Collider2D other)
    {
        print("onTriggerEnter called");
        // Check that only a player object can interact with the lever
        if (other.GetComponent<PlayerController>())
        {
            // Distinguish between Light and Shadow using tags
            if (other.tag == "LightPlayer")
            {
                inRange = "LightPlayer";
                lightPlayerEnter.Invoke();
            }
            else if (other.tag == "ShadowPlayer")
            {
                inRange = "ShadowPlayer";
                shadowPlayerEnter.Invoke();
            }
        }
    }

    void OnTriggerExit2D(Collider2D other)
    {
        if (other.GetComponent<PlayerController>())
        {
            if (other.tag == "LightPlayer")
            {
                inRange = "LightPlayer";
                lightPlayerExit.Invoke();
            }
            else if (other.tag == "ShadowPlayer")
            {
                inRange = "ShadowPlayer";
                shadowPlayerExit.Invoke();
            }

            inRange = "";
        }
    }
}