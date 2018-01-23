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
    private int numTrials;

    // Internal Global config data
    private int studyTimeInMilliseconds;
    private int delayTimeInMilliseconds;
    private int itemXSizeInPixels;
    private int itemYSizeInPixels;
    private string stimuliFolder;
    private string logFolder;
    private string pid;
    private int mostItemsInTrial;

    // Accessors for Global variables
    public int StudyTimeInMilliseconds { get { return studyTimeInMilliseconds; } }
    public int DelayTimeInMilliseconds { get { return delayTimeInMilliseconds; } }
    public int ItemXSizeInPixels { get { return itemXSizeInPixels; } }
    public int ItemYSizeInPixels { get { return itemYSizeInPixels; } }
    public string StimuliFolder { get { return stimuliFolder; } }
    public string LogFolder { get { return logFolder; } }
    public string ParticipantID { get { return pid; } }
    public int MostItemsInTrial { get { return mostItemsInTrial; } }

    // Use this for initialization
    void Start () {
        pid = PlayerPrefs.GetString("pid").Trim();

        mostItemsInTrial = 0;

        // Open the INI file
        INIParser ini = new INIParser();

        string configFilePath = Application.dataPath + '/' + configFile;

        if (!File.Exists(configFilePath))
            Application.Quit();

        ini.Open(configFilePath);

        // Read the global configuration variables
        studyTimeInMilliseconds = (int)ini.ReadValue("Global", "StudyTimeInMilliseconds", 15000);
        delayTimeInMilliseconds = (int)ini.ReadValue("Global", "DelayTimeInMilliseconds", 5000);
        itemXSizeInPixels = (int)ini.ReadValue("Global", "ItemXSizeInPixels", 50);
        itemYSizeInPixels = (int)ini.ReadValue("Global", "ItemYSizeInPixels", 50);
        stimuliFolder = ini.ReadValue("Global", "StimuliFolder", Application.dataPath).Trim();
        logFolder = ini.ReadValue("Global", "LogFolder", Application.dataPath).Trim();

        // Read the raw contents and isolate the Trials section (assumed to be at the end of the file)
        string contents = ini.ToString();
        string trialContents = contents.Split(new string[] { "[Trials]" }, System.StringSplitOptions.RemoveEmptyEntries)[1].Trim();
        string[] trialStrings = trialContents.Split(new char[] { '\n' }, System.StringSplitOptions.RemoveEmptyEntries);

        // Generate variables for the filenames and positions of items
        numTrials = 0;
        List<string[]> trialFilenamesTmp = new List<string[]>();
        List<Vector2[]> trialPositionsTmp = new List<Vector2[]>();
        List<Texture2D[]> trialStimuliTmp = new List<Texture2D[]>();

        // Parse each trial, skipping any that fail (i.e. that row in the jagged arrays defined above will be null)
        for (int i = 0; i < trialStrings.Length; i++)
        {
            try
            {
                if (trialStrings[i].Trim()[0] == ';') continue; // Check for comment lines
                string[] trialSplit = trialStrings[i].Trim().Split(new char[] { ' ' });
                if (trialSplit.Length % 3 != 0) continue; // Skip lines that don't have a multiple of 3 elements (filename, x, y)

                // Allocate within-trial variables
                int numItems = trialSplit.Length / 3;
                if (numItems > mostItemsInTrial) mostItemsInTrial = numItems;
                string[] filenames = new string[numItems];
                Vector2[] positions = new Vector2[numItems];
                Texture2D[] stimuli = new Texture2D[numItems];

                // Parse the items
                for (int j = 0; j < numItems; j++)
                {
                    filenames[j] = Path.Combine(stimuliFolder, trialSplit[j].Trim());
                    positions[j] = new Vector2(float.Parse(trialSplit[j * 2 + numItems].Trim()), float.Parse(trialSplit[j * 2 + numItems + 1].Trim()));
                    stimuli[j] = LoadTexture(filenames[j]);
                    if (stimuli[j] == null) continue;
                }

                // Store the values in the trial tables
                trialFilenamesTmp.Add(filenames);
                trialPositionsTmp.Add(positions);
                trialStimuliTmp.Add(stimuli);

                numTrials++;
            }
            catch (System.Exception) { continue; } // If anything goes weird, just skip the line

            trialFilenames = trialFilenamesTmp.ToArray();
            trialPositions = trialPositionsTmp.ToArray();
            trialStimuli = trialStimuliTmp.ToArray();
        }
    }

    // Accessors for trial-by-trial data

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

    // Helper function for loading textures
    public Texture2D LoadTexture(string filePath)
    {
        Texture2D tex = null;
        byte[] fileData;

        if (File.Exists(filePath))
        {
            fileData = File.ReadAllBytes(filePath);
            tex = new Texture2D(itemXSizeInPixels, itemYSizeInPixels);
            tex.LoadImage(fileData); //..this will auto-resize the texture dimensions.
        }

        return tex;
    }
}
