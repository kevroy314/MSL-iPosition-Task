using System.IO;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ConfigurationLoader : MonoBehaviour {

    // The configuration filename (assumed to be in the Application.dataPath directory)
    public static string configFile = "configuration.ini";

    // Internal Trial config data
    private string[][] trialFilenames;
    private Vector2[][] trialPositions;
    private Texture2D[][] trialStimuli;
    private int[] trialStudyTimeInMilliseconds;
    private int[] trialDelayTimeInMilliseconds;
    private string[] trialLogInstruction;
    private Vector2Int[] trialItemSize;
    private int numTrials;

    // Internal Global config data
    private string stimuliFolder;
    private string logFolder;
    private string pid;
    private int mostItemsInTrial;
    private KeyCode nextTrialKey;

    // Accessors for Global variables
    public string StimuliFolder { get { return stimuliFolder; } }
    public string LogFolder { get { return logFolder; } }
    public string ParticipantID { get { return pid; } }
    public int MostItemsInTrial { get { return mostItemsInTrial; } }
    public KeyCode NextTrialKey { get { return nextTrialKey; } }


    // Use this for initialization
    void Start () {
        pid = PlayerPrefs.GetString("pid").Trim();
        configFile = PlayerPrefs.GetString("config", "configuration.ini").Trim();
        Debug.Log(configFile);
        mostItemsInTrial = 0;

        // Open the INI file
        INIParser ini = new INIParser();

        string configFilePath = Application.dataPath + '/' + configFile;

        if (!File.Exists(configFilePath))
            Application.Quit();

        ini.Open(configFilePath);

        // Read the global configuration variables
        stimuliFolder = ini.ReadValue("Global", "StimuliFolder", Application.dataPath).Trim();
        logFolder = ini.ReadValue("Global", "LogFolder", Application.dataPath).Trim();
        string nextTrialKeyString = ini.ReadValue("Global", "NextButton", "Space");
        nextTrialKey = (KeyCode)System.Enum.Parse(typeof(KeyCode), nextTrialKeyString);

        // Read the raw contents and isolate the Trials section (assumed to be at the end of the file)
        string contents = ini.ToString();
        string trialContents = contents.Split(new string[] { "[Trials]" }, System.StringSplitOptions.RemoveEmptyEntries)[1].Trim();
        string[] trialStrings = trialContents.Split(new char[] { '\n' }, System.StringSplitOptions.RemoveEmptyEntries);

        // Generate variables for the filenames and positions of items
        numTrials = 0;
        List<Vector2Int> trialItemSizeTmp = new List<Vector2Int>();
        List<string> trialLogInstructionTmp = new List<string>();
        List<string[]> trialFilenamesTmp = new List<string[]>();
        List<Vector2[]> trialPositionsTmp = new List<Vector2[]>();
        List<Texture2D[]> trialStimuliTmp = new List<Texture2D[]>();
        List<int> trialStudyTimesTmp = new List<int>();
        List<int> trialDelayTimesTmp = new List<int>();

        int leadingElements = 5;

        // Parse each trial, skipping any that fail (i.e. that row in the jagged arrays defined above will be null)
        for (int i = 0; i < trialStrings.Length; i++)
        {
            try
            {
                if (trialStrings[i].Trim()[0] == ';') continue; // Check for comment lines
                string[] trialSplit = trialStrings[i].Trim().Split(new char[] { ' ' });
                if (trialSplit.Length < leadingElements + 3) continue; //Skip lines that don't have at least one valid item plus the times
                if ((trialSplit.Length - leadingElements) % 3 != 0) continue; // Skip lines that don't have a multiple of 3 elements (filename, x, y)

                // Allocate within-trial variables
                int numItems = (trialSplit.Length - leadingElements) / 3;
                if (numItems > mostItemsInTrial) mostItemsInTrial = numItems;
                string[] filenames = new string[numItems];
                Vector2[] positions = new Vector2[numItems];
                Texture2D[] stimuli = new Texture2D[numItems];

                Vector2Int size = new Vector2Int(int.Parse(trialSplit[3]), int.Parse(trialSplit[4]));

                // Parse the items
                for (int j = 0; j < numItems; j++)
                {
                    filenames[j] = Path.Combine(stimuliFolder, trialSplit[j + leadingElements].Trim());
                    positions[j] = new Vector2(float.Parse(trialSplit[j * 2 + numItems + leadingElements].Trim()), float.Parse(trialSplit[j * 2 + numItems + leadingElements + 1].Trim()));
                    stimuli[j] = LoadTexture(filenames[j], size);
                    if (stimuli[j] == null) continue;
                }

                // Store the values in the trial tables
                trialLogInstructionTmp.Add(trialSplit[0].Trim());
                trialStudyTimesTmp.Add(int.Parse(trialSplit[1]));
                trialDelayTimesTmp.Add(int.Parse(trialSplit[2]));
                trialItemSizeTmp.Add(size);
                trialFilenamesTmp.Add(filenames);
                trialPositionsTmp.Add(positions);
                trialStimuliTmp.Add(stimuli);

                numTrials++;
            }
            catch (System.Exception) { continue; } // If anything goes weird, just skip the line
        }

        trialItemSize = trialItemSizeTmp.ToArray();
        trialLogInstruction = trialLogInstructionTmp.ToArray();
        trialStudyTimeInMilliseconds = trialStudyTimesTmp.ToArray();
        trialDelayTimeInMilliseconds = trialDelayTimesTmp.ToArray();
        trialFilenames = trialFilenamesTmp.ToArray();
        trialPositions = trialPositionsTmp.ToArray();
        trialStimuli = trialStimuliTmp.ToArray();
    }

    // Accessors for trial-by-trial data

    public Vector2Int GetTrialItemSize(int trialNum)
    {
        if (trialNum >= numTrials) return Vector2Int.zero;
        return trialItemSize[trialNum];
    }

    public string GetStimuliLogInstructions(int trialNum)
    {
        if (trialNum >= numTrials) return null;
        return trialLogInstruction[trialNum];
    }

    public Texture2D[] GetStimuliTextures(int trialNum)
    {
        if (trialNum >= numTrials) return null;
        return trialStimuli[trialNum];
    }

    public Vector2[] GetStimuliPositions(int trialNum)
    {
        if (trialNum >= numTrials) return null;
        return trialPositions[trialNum];
    }

    public string[] GetStimuliFilenames(int trialNum)
    {
        if (trialNum >= numTrials) return null;
        return trialFilenames[trialNum];
    }

    public int GetNumberOfItemsOnTrial(int trialNum)
    {
        if (trialNum >= numTrials) return -1;
        return trialFilenames[trialNum].Length;
    }

    public int GetNumberOfTrials()
    {
        return numTrials;
    }

    public int GetTrialStudyTime(int trialNum)
    {
        if (trialNum >= numTrials) return -1;
        return trialStudyTimeInMilliseconds[trialNum];
    }

    public int GetTrialDelayTime(int trialNum)
    {
        if (trialNum >= numTrials) return -1;
        return trialDelayTimeInMilliseconds[trialNum];
    }

    // Helper function for loading textures
    public Texture2D LoadTexture(string filePath, Vector2Int size)
    {
        Texture2D tex = null;
        byte[] fileData;

        if (File.Exists(filePath))
        {
            fileData = File.ReadAllBytes(filePath);
            tex = new Texture2D(size.x, size.y);
            tex.LoadImage(fileData); //..this will auto-resize the texture dimensions.
        }

        return tex;
    }
}
