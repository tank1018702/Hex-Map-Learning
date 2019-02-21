using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HexGridChunk : MonoBehaviour
{
    HexCell[] cells;

    HexMesh hexMesh;
    Canvas gridCanvas;

    private void Awake()
    {
        gridCanvas = GetComponentInChildren<Canvas>();
        hexMesh = GetComponentInChildren<HexMesh>();

        cells = new HexCell[HexMetrics.chunkSizeX * HexMetrics.chunkSizeZ];
        ShowLabelUI(false);
    }

    //void Start()
    //{
    //    hexMesh.TriangulateAll(cells);
    //}

    public void AddCell(int index,HexCell cell)
    {
        cells[index] = cell;
        cell.transform.SetParent(transform, false);
        cell.uiRect.SetParent(gridCanvas.transform, false);
        cell.chunk = this;
    }

    public void Refresh()
    {
        enabled = true;     
    }


    private void LateUpdate()
    {
        hexMesh.TriangulateAll(cells);
        enabled = false;
    }

    public void ShowLabelUI(bool visible)
    {
        gridCanvas.gameObject.SetActive(visible);
    }
}
