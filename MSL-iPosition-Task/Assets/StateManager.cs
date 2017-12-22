using System.Collections;
using System.Collections.Generic;
using TouchScript.Layers;
using UnityEngine;

public class StateManager : MonoBehaviour {
    public Squiggles stimuliManager;
    public StandardLayer touchLayer;

    public enum State { Study, Delay, Test, Finished };

    public State currentState;

    private float startTime;
    public float studyTime;
    public float delayTime;

    public KeyCode advanceKey;
    public int numberOfTrials;

	// Use this for initialization
	void Start () {
        currentState = State.Study;
        startTime = Time.time;
        touchLayer.enabled = false;
    }
	
	// Update is called once per frame
	void Update () {
		if (currentState == State.Study)
        {
            if (Time.time - startTime >= studyTime)
            {
                currentState = State.Delay;
                stimuliManager.gameObject.SetActive(false);
                startTime = Time.time;
            }
        }
        else if (currentState == State.Delay)
        {
            if (Time.time - startTime >= delayTime)
            {
                stimuliManager.gameObject.SetActive(true);
                touchLayer.enabled = true;
                stimuliManager.StimuliToTop();
                currentState = State.Test;
                startTime = Time.time;
            }
        }
        else if (currentState == State.Test)
        {
            if (Input.GetKeyUp(advanceKey))
            {
                numberOfTrials -= 1;
                startTime = Time.time;
                touchLayer.enabled = false;
                if (numberOfTrials <= 0)
                {
                    currentState = State.Finished;
                }
                else
                {
                    currentState = State.Study;
                    stimuliManager.RandomizeLocationsAndStimuli();
                }
            }
        }
	}
}
