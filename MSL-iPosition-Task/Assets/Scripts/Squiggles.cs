using UnityEngine;
using System.Collections.Generic;

public class Squiggles : MonoBehaviour
{
    // For stimuli generation
    public Transform prefab;
    public Material stimuliMaterial;

    // For "top" generation
    public Vector2 xRange;
    public Vector2 yRange;

    // For random generation
    public int numItems = 6;
    private int numberOfStimuli = 150;

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

        // Make new objects
        for (var i = 0; i < stimuliTextures.Length; i++)
        {
            prefab.localScale = new Vector3(-itemSize.x, 0, -itemSize.y);
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

    public Vector3[] StimuliToTop(float horizontalPadding, float verticalPadding)
    {
        GameObject[] children = getParentChildren();
        Vector3[] topPositions = new Vector3[children.Length];
        for (int i = 0; i < children.Length; i++)
        {
            float x = Mathf.Lerp(xRange[0] + horizontalPadding, xRange[1] - horizontalPadding, (float)i / (float)(children.Length-1));
            if (children.Length <= 1)
                x = 0f;
            float y = yRange[1] - verticalPadding;
            children[i].transform.localPosition = new Vector3(x, y, 0);
            topPositions[i] = children[i].transform.localPosition;
        }

        return topPositions;
    }

    public bool AllStimuliHaveMoved(Vector3[] topPositions)
    {
        GameObject[] children = getParentChildren();

        bool match = false;

        for(int i = 0; i < topPositions.Length; i++)
            match |= children[i].transform.localPosition == topPositions[i];

        return !match;
    }

    public Vector2[] GetStimuliPositions()
    {
        GameObject[] children = getParentChildren();
        Vector2[] positions = new Vector2[children.Length];
        for (int i = 0; i < children.Length; i++)
        {
            positions[i] = new Vector2(children[i].transform.localPosition.x, children[i].transform.localPosition.y);
        }

        return positions;
    }
}