using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GraphUnit : MonoBehaviour
{
    [SerializeField] private MeshRenderer unitMesh;
    [SerializeField] private TextMesh unitLabel;
    public void SetColor(Color tint)
    {
        unitMesh.material.SetColor("_Color", tint);
    }

    public void SetMaterialOffset(Vector2 tiling, Vector2 offset, Texture2D text)
    {
        unitMesh.material.SetTexture("_Base", text);
        unitMesh.material.SetTextureScale("_Base", tiling);
        unitMesh.material.SetTextureOffset("_Base", offset);
    }

    public void SetText(float weibul)
    {
        unitLabel.text = weibul.ToString();
        if (weibul > 25)
            unitLabel.color = Color.black;
        else
            unitLabel.color = Color.white;
    }
}
