using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using TouchScript.Layers;
using UnityEngine;

public class StateManager : MonoBehaviour {

    public StreamWriter logFile;
    public StreamWriter actualCoordinatesLogFile;

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
        numberOfTrials = configuration.GetNumberOfTrials();

        stimuliManager.SetStimuliAndPositions(
            configuration.GetStimuliTextures(currentTrialNumber),
            configuration.GetStimuliPositions(currentTrialNumber),
            new Vector2(configuration.ItemXSizeInPixels, configuration.ItemYSizeInPixels)
            );

        string dtString = DateTime.Now.ToString("yyyy_MM_dd_HH_mm_ss_");
        if (PlayerPrefs.GetString("config").ToLower().Contains("practice"))
            dtString += "practice_";
        string logFilename = Path.Combine(configuration.LogFolder, dtString + configuration.ParticipantID + "_position_data_coordinates.txt");
        logFile = new StreamWriter(logFilename);
        string actualCoordinatesLogFilename = Path.Combine(configuration.LogFolder, dtString + configuration.ParticipantID + "_actual_coordinates.txt");
        actualCoordinatesLogFile = new StreamWriter(actualCoordinatesLogFilename);

        studyTime = (float)configuration.GetTrialStudyTime(0) / 1000f;
        delayTime = (float)configuration.GetTrialDelayTime(0) / 1000f;
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
                AppendToLog();

                numberOfTrials -= 1;
                startTime = Time.time;
                touchLayer.enabled = false;
                if (numberOfTrials <= 0)
                {
                    stimuliManager.gameObject.SetActive(false);
                    currentState = State.Finished;

                    logFile.Close();
                    actualCoordinatesLogFile.Close();
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
                    studyTime = (float)configuration.GetTrialStudyTime(currentTrialNumber) / 1000f;
                    delayTime = (float)configuration.GetTrialDelayTime(currentTrialNumber) / 1000f;
                }
            }
        }
	}

    private void AppendToLog()
    {
        string dataLine = "";
        string actualCoordinatesLine = "";

        Vector2[] dataPositions = stimuliManager.GetStimuliPositions();
        Vector2[] actualPositions = configuration.GetStimuliPositions(currentTrialNumber);

        for(int i = 0; i < configuration.MostItemsInTrial; i++)
        {
            if (i < dataPositions.Length)
            {
                dataLine += dataPositions[i].x + "\t" + dataPositions[i].y + "\t";
                actualCoordinatesLine += actualPositions[i].x + "\t" + actualPositions[i].y + "\t";
            }
            else
            {
                dataLine += "nan\tnan\t";
                actualCoordinatesLine += "nan\tnan\t";
            }
        }

        logFile.WriteLine(dataLine.Trim());
        actualCoordinatesLogFile.WriteLine(actualCoordinatesLine.Trim());

        logFile.Flush();
        actualCoordinatesLogFile.Flush();
    }
}
