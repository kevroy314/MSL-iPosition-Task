using UnityEngine;
using System.Collections.Generic;

public class Squiggles : MonoBehaviour
{
    public Transform prefab;
    public Material stimuliMaterial;
    public Vector2 xRange;
    public Vector2 yRange;
    public int randomSeed = 42;
    public int numItems;
    private int numberOfStimuli = 150;
    public float topTestLocationProportion = 0.1f;
    public Texture2D[] activeTextures;

    void Start()
    {
        UnityEngine.Random.seed = randomSeed;
        // RandomizeLocationsAndStimuli();
    }

    private GameObject[] getParentChildren()
    {
        GameObject[] children = new GameObject[transform.childCount];
        for (int i = 0; i < children.Length; i++)
            children[i] = transform.GetChild(i).gameObject;
        return children;
    }

    public void RandomizeLocationsAndStimuli()
    {
        // Destroy old objects (if they exist)
        GameObject[] children = getParentChildren();
        for (int i = 0; i < children.Length; i++)
            DestroyImmediate(children[i]);

        // Make new objects
        for(var i = 0; i < numItems; i++)
        {
            var tex = Resources.Load<Texture2D>("white/" + UnityEngine.Random.Range(1, numberOfStimuli));
            var obj = Instantiate(prefab) as Transform;
            obj.SetParent(transform);
            obj.GetComponent<MeshRenderer>().material = stimuliMaterial;
            obj.GetComponent<MeshRenderer>().material.mainTexture = tex;
            obj.localPosition = new Vector3(Random.Range(xRange[0], xRange[1]), Random.Range(yRange[0], yRange[1]), 0);
        }
    }

    public void SetStimuliAndPositions(Texture2D[] stimuliTextures, Vector2[] stimuliPositions, Vector2 itemSize)
    {
        // Destroy old objects (if they exist)
        GameObject[] children = getParentChildren();
        for (int i = 0; i < children.Length; i++)
            DestroyImmediate(children[i]);

        activeTextures = stimuliTextures;

        // Make new objects
        for (var i = 0; i < stimuliTextures.Length; i++)
        {
            prefab.localScale = new Vector3(itemSize.x, 0, itemSize.y);
            var tex = stimuliTextures[i];
            var obj = Instantiate(prefab) as Transform;
            obj.SetParent(transform);
            obj.GetComponent<MeshRenderer>().material = stimuliMaterial;
            obj.GetComponent<MeshRenderer>().material.mainTexture = tex;
            obj.localPosition = new Vector3(stimuliPositions[i].x, stimuliPositions[i].y, 0f);
            Debug.Log(obj.localScale);
            Debug.Log(obj.localPosition);
        }
    }

    public void StimuliToTop()
    {
        GameObject[] children = getParentChildren();
        for (int i = 0; i < children.Length; i++)
            children[i].transform.localPosition = new Vector3(Mathf.Lerp(xRange[0], xRange[1], (float)i / (float)children.Length), yRange[1] - (yRange[1] - yRange[0]) * topTestLocationProportion, 0);
    }
}