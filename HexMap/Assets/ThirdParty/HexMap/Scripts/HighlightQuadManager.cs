using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HighlightQuadManager : MonoBehaviour
{
    public HighlightQuad prefabHighlightQuad;

    Transform container;

    public void Clear()
    {
        if (container)
        {
            Destroy(container.gameObject);
        }
        container = new GameObject("HighlightQuad Container").transform;
        container.SetParent(transform, false);
    }

    public void InitBuild(HexCell cell)
    {
        HighlightQuad highlightQuad = CreateQuad(cell);
        highlightQuad.buildEnable = true;
    }

    public void InitPath(HexCell cell)
    {
        HighlightQuad highlightQuad = CreateQuad(cell);
        highlightQuad.buildEnable = false;
    }

    private HighlightQuad CreateQuad(HexCell cell)
    {
        if (cell.highlightQuad != null)
        {
            return cell.highlightQuad;
        }
        HighlightQuad highlightQuad = Instantiate(prefabHighlightQuad);

        highlightQuad.UpdatePostion(cell.Position, 0);

        highlightQuad.transform.SetParent(container, false);
        cell.highlightQuad = highlightQuad;
        return highlightQuad;
    }
}
