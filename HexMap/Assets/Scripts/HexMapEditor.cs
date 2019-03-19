using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class HexMapEditor : MonoBehaviour
{
    public Color[] colors;

    public HexGrid hexGrid;

    private Color activeColor;

    int activeElevation;

    bool applyColor;
    bool applyElevation;

    bool isDrag;
    HexDirection dragDirection;
    HexCell previousCell;


    enum OptionalToggle
    {
        Ignore,Yes,No
    }

    OptionalToggle riverMode,roadMode;

    void Awake()
    {
        SelectColor(0);
    }

    void Update()
    {
        if (Input.GetMouseButton(0)&& !EventSystem.current.IsPointerOverGameObject())
        {
            HandleInput();
        }
        else
        {
            previousCell = null;
        }
    }



    void HandleInput()
    {
        Ray inputRay = Camera.main.ScreenPointToRay(Input.mousePosition);
        RaycastHit hit;
        if (Physics.Raycast(inputRay, out hit))
        {
            
            HexCell currentCell = hexGrid.GetCell(hit.point);
            if(previousCell&&previousCell!=currentCell)
            {
                ValidateDrag(currentCell);
            }
            else
            {
                isDrag = false;
            }
            EditCells(currentCell);
            previousCell = currentCell;

            //hexGrid.ColorCell(hit.point, activeColor);
            //hexGrid.TouchCell(hit.point);
        }
        else
        {
            previousCell = null;
        }
    }

    void ValidateDrag(HexCell currentCell)
    {
        for(dragDirection=HexDirection.NE;
            dragDirection<=HexDirection.NW;
            dragDirection++)
        {
            if(previousCell.GetNeighbor(dragDirection)==currentCell)
            {
                isDrag = true;
                return;
            }
        }
        isDrag = false;
    }

    void EditCells(HexCell center)
    {
        int centerX = center.coordinates.X;
        int centerZ = center.coordinates.Z;

        for(int r=0,z=centerZ-brushSize;z<=centerZ;z++,r++)
        {
            for(int x=centerX-r;x<=centerX+brushSize;x++)
            {
                EditCell(hexGrid.GetCell(new HexCoordinates(x, z)));
            }
        }

        for (int r=0,z=centerZ+brushSize;z>centerZ;z--,r++)
        {
            for(int x=centerX-brushSize;x<=centerX+r;x++)
            {
                EditCell(hexGrid.GetCell(new HexCoordinates(x, z)));
            }
        }
    }

    void EditCell(HexCell cell)
    {
        if(cell)
        {
            if (applyColor)
            {
                cell.Color = activeColor;
            }
            if (applyElevation)
            {
                cell.Elevation = activeElevation;
            }
            if(riverMode==OptionalToggle.No)
            {
                cell.RemoveRiver();
            }
            if(roadMode==OptionalToggle.No)
            {
                cell.RemoveRoads();
            }
           if(isDrag)
            {
                HexCell otherCell = cell.GetNeighbor(dragDirection.Opposite());
                if(otherCell)
                {
                    if(riverMode==OptionalToggle.Yes)
                    {
                        otherCell.SetOutgoingRiver(dragDirection);
                    }
                    if(roadMode==OptionalToggle.Yes)
                    {
                        otherCell.AddRoad(dragDirection);
                    }
                }
            }
        }
      
        //hexGrid.Refresh();
    }

    public void SetApplyElevation(bool toggle)
    {
        applyElevation = toggle;
    }

    public void SelectColor(int index)
    {
        applyColor = index >= 0;
        if (applyColor)
        {
            activeColor = colors[index];
        }
    }

    int brushSize;

    public  void SetBrushSize(float size)
    {
        brushSize = (int)size;
    }

    public void SetEvelation(float evelation)
    {
        activeElevation = (int)evelation;
        
    }

    public void ShowLabelUI(bool visible)
    {
        hexGrid.ShowLabelUI(visible);
    }

    public void  SetRiverMode(int mode)
    {
        riverMode = (OptionalToggle)mode;
    }
    public  void SetRoadMode(int mode)
    {
        roadMode = (OptionalToggle)mode;
    }
}
