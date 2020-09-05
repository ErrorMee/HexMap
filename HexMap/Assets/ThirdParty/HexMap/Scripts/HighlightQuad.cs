using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HighlightQuad : MonoBehaviour
{
    private int elevation;

    public bool buildEnable = false;
    public MeshFilter meshFilter;
    public MeshRenderer meshRenderer;
    private Color[] meshColors;

    public void UpdatePostion(Vector3 postion, int elevation)
    {
        this.elevation = elevation;
        postion.y += 0.1f;
        transform.position = postion;
        meshRenderer.enabled = elevation == 0;
    }

    public void ClearColor()
    {
        if (buildEnable && elevation == 0)
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
