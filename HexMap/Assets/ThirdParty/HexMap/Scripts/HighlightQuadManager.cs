using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum HighlightQuadType
{
    None,
    /// <summary>
    /// 可以构建区域
    /// </summary>
    Build,
    /// <summary>
    /// 可以移动的区域
    /// </summary>
    Move,
    /// <summary>
    /// 选中的移动区域
    /// </summary>
    MoveSelect
}

public class HighlightQuadManager : MonoBehaviour
{
    public Transform prefabHighlightQuad;

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

    public void SetHighlightQuad(HexCell cell, Vector3 position)
    {
        if (cell.HighlightQuad == HighlightQuadType.None)
        {
            return;
        }
        position.y += HexMetrics.elevationPerturbStrength * 0.2f;
        //HexHash hash = HexMetrics.SampleHashGrid(position);
        Transform instance = Instantiate(prefabHighlightQuad);
        instance.localPosition = position;
        //instance.localRotation = Quaternion.Euler(0f, 360f * hash.e, 0f);
        instance.SetParent(container, false);
    }
}
