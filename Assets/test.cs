using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class test : MonoBehaviour
{
    public MeshRenderer rend;
    
    void Start()
    {

        Texture2D graymap = new Texture2D(12,12);

        for (int i = 0; i < graymap.height; i++)
        {
            for(int j = 0; j < graymap.width; j++)
            {
                float val = Random.Range(0, 100)/100f;
                Color col = new Color(val, val, val, 1f);
                graymap.SetPixel(j, i, col);
                Debug.Log("SetPixel " + j + i + col);
            }
        }
        graymap.Apply();

        rend.material.SetTexture("_UV", graymap);

    }

}
