﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LightPlayerObject : PlayerObject {

    public GameObject castLight; //a reference to the light polygonal mesh generated by the CastLight script

	// Use this for initialization
	protected override void Start () {
        base.Start();

        //assign Light's controls to the Arrow keys
        keyUp = KeyCode.UpArrow;
        keyDown = KeyCode.DownArrow;
        keyLeft = KeyCode.LeftArrow;
        keyRight = KeyCode.RightArrow;

        //assign Global variables
        Globals.lightPlayer = this.gameObject;
	}
	
	// Update is called once per frame
	protected override void Update () {
        base.Update();
    }
}
