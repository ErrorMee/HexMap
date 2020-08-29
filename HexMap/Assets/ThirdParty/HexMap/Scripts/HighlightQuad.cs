using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HighlightQuad : MonoBehaviour
{
    public bool buildEnable = false;
    public MeshFilter meshFilter;
    public MeshRenderer meshRenderer;
    private Color[] meshColors;

    public void UpdatePostion(Vector3 postion)
    {
        postion.y += HexMetrics.elevationPerturbStrength * 0.2f;
        transform.position = postion;
    }

    public void ClearColor()
    {
        if (buildEnable)
        {
            if (meshColors == null)
            {
                Mesh mesh = meshFilter.mesh;

                Vector3[] vertices = mesh.vertices;

                meshColors = new Color[vertices.Length];
            }

            for (int i = 0; i < meshColors.Length; i++)
            {
                meshColors[i] = Color.white;
            }

            meshFilter.mesh.colors = meshColors;
        }
        else {
            meshRenderer.enabled = false;
        }
    }

    public void SetColor(Color color)
    {
        meshRenderer.enabled = true;
        if (meshColors == null)
        {
            Mesh mesh = meshFilter.mesh;

            Vector3[] vertices = mesh.vertices;

            meshColors = new Color[vertices.Length];
        }
        for (int i = 0; i < meshColors.Length; i++)
        {
            meshColors[i] = color;
        }

        meshFilter.mesh.colors = meshColors;
    }
}
