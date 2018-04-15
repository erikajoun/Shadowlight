﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Assertions;

/**
 *  Class for controlling an interactable lever object. Has left and right
 *  states where either state is always toggled, and each one is associated
 *  with an event. On interaction, the current state changes to the other one.
 */
[RequireComponent(typeof(Animator))]
public class Lever : MonoBehaviour {
    public UnityEvent leftMode = null;
    public UnityEvent rightMode = null;
    private Animator animator = null;
    private bool left = true;

    private void Start()
    {
        animator = GetComponent<Animator>();
        // Should not be null because of the RequireComponent attribute.
        Assert.IsNotNull(animator, name + " requires an Animator component.");
    }

    private void OnEnable()
    {
        // Invoke the event associated with the default state of the lever
        if (left)
        {
            leftMode.Invoke();
        }
        else
        {
            rightMode.Invoke();
        }
    }

    /**
     * Switches the current state to the other state and invokes the event
     * associated with that state.
     */
    public void Toggle()
    {
        // If toggled left, switch to right
        if(left)
        {

            rightMode.Invoke();
            animator.Play("ToggleRight");
        }
        // If toggled right, switch to left
        else
        {
            leftMode.Invoke();
            animator.Play("ToggleLeft");
        }
        left = !left;
    }

    // Because OnTriggerStay2D was only being called when collider was moving
    void OnTriggerEnter2D(Collider2D other)
    {
        // Check that only a player object can interact with the lever
        if (other.GetComponent<PlayerController>())
        {
            inRange = true;
        }
    }

    void OnTriggerExit2D(Collider2D other)
    {
        if (other.GetComponent<PlayerController>())
        {
            inRange = false;
        }
    }
}
