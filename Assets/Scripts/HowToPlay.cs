﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class HowToPlay : MonoBehaviour {

	public List<GameObject> bookContents;
	private List<Fader> bookContentFaders;
	private int pageNumber;
	public Animator bookFrameAnim;
	public List<Animator> animators;
	private bool stopped;
	private float counter = 0f;


	//private List<GameObject> bookContents;

	// Use this for initialization
	void Start () {
		bookContentFaders = new List<Fader> ();
		// first image
		pageNumber = 0;
		counter = 0;
		// fade out everything else
		foreach (var bookContent in bookContents) {
			Fader fader = bookContent.GetComponent<Fader> ();
			bookContentFaders.Add (fader);
			fader.Hide ();
		}
		Fader firstFader = bookContentFaders[0];
		firstFader.FadeIn ();
		stopped = true;
	}

	// Update is called once per frame
	void Update () {
		if (Input.GetButtonDown ("Next") && stopped) {
			if (pageNumber < 3) {
				StartCoroutine(TurnPage ());
			} else {
				SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex + 1);
			}
		}

		if (counter <= 0) {
			foreach (var anim in animators) {
				anim.SetTrigger ("change");
			}
			counter = 1f;
		} else {
			counter -= Time.deltaTime;
		}
	}

	private IEnumerator TurnPage() {
		// fade out current image
		stopped = false;
		Fader theFader = bookContentFaders[pageNumber];
		theFader.FadeOut ();
		yield return new WaitForSeconds (0.5f);


		// turn page
		pageNumber++;
		bookFrameAnim.SetTrigger ("flip");
		yield return new WaitForSeconds (1f);


		// fade in new image
		bookContentFaders[pageNumber].FadeIn();
		yield return new WaitForSeconds (0.5f);
		stopped = true;
	}
}
