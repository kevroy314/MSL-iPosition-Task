﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class BeginTask : MonoBehaviour {

    public InputField participantIDInput;

    public void Begin()
    {
        if (participantIDInput.text.Trim() == "") return;
        Debug.Log("Loading scene with Participant ID: " + participantIDInput.text.Trim());
        PlayerPrefs.SetString("pid", participantIDInput.text.Trim());
        SceneManager.LoadScene(1);
    }
}
