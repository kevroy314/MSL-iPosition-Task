using System.Collections;
using System.Collections.Generic;
using TouchScript.Layers;
using UnityEngine;

public class StateManager : MonoBehaviour {
    public Squiggles stimuliManager;
    public StandardLayer touchLayer;
    public ConfigurationLoader configuration;

    public enum State { Study, Delay, Test, Finished };

    public State currentState;

    private float startTime;
    public float studyTime;
    public float delayTime;

    public KeyCode advanceKey;
    public int numberOfTrials;
    private int currentTrialNumber;

    private bool firstUpdate;

    private Vector3[] topPositions;

    // Use this for initialization
    void Start () {
        currentState = State.Study;
        startTime = Time.time;
        touchLayer.enabled = false;
        currentTrialNumber = 0;

        firstUpdate = true;
    }

    private void Init()
    {
        studyTime = (float)configuration.StudyTimeInMilliseconds / 1000f;
        delayTime = (float)configuration.DelayTimeInMilliseconds / 1000f;
        numberOfTrials = configuration.GetNumberOfTrials();

        stimuliManager.SetStimuliAndPositions(
            configuration.GetStimuliTextures(currentTrialNumber),
            configuration.GetStimuliPositions(currentTrialNumber),
            new Vector2(configuration.ItemXSizeInPixels, configuration.ItemYSizeInPixels)
            );
    }

    // Update is called once per frame
    void Update () {
        if (firstUpdate)
        { // To avoid race conditions with the ConfigurationLoader Start() function, we perform the last of the config on the first update
            Init();
            firstUpdate = false;
        }
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
                topPositions = stimuliManager.StimuliToTop(configuration.ItemXSizeInPixels, configuration.ItemYSizeInPixels);
                currentState = State.Test;
                startTime = Time.time;
            }
        }
        else if (currentState == State.Test)
        {
            if (Input.GetKeyUp(advanceKey) && stimuliManager.AllStimuliHaveMoved(topPositions))
            {
                numberOfTrials -= 1;
                startTime = Time.time;
                touchLayer.enabled = false;
                if (numberOfTrials <= 0)
                {
                    stimuliManager.gameObject.SetActive(false);
                    currentState = State.Finished;
                }
                else
                {
                    currentState = State.Study;
                    currentTrialNumber++;
                    stimuliManager.SetStimuliAndPositions(
                        configuration.GetStimuliTextures(currentTrialNumber), 
                        configuration.GetStimuliPositions(currentTrialNumber),
                        new Vector2(configuration.ItemXSizeInPixels, configuration.ItemYSizeInPixels)
                        );
                }
            }
        }
	}
}
