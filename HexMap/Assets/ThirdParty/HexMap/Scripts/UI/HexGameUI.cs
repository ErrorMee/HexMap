using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class HexGameUI : MonoBehaviour {

	public HexGrid grid;

	HexCell currentCell;

	public Team selectedTeam;

	public void SetEditMode (bool toggle) {
		enabled = !toggle;
		grid.ClearPath();
		if (toggle) {
			Shader.EnableKeyword("HEX_MAP_EDIT_MODE");
		}
		else {
			Shader.DisableKeyword("HEX_MAP_EDIT_MODE");
		}
	}

	void Update () {
		if (!EventSystem.current.IsPointerOverGameObject()) {

			if (selectedTeam)
			{
				if (Input.GetMouseButtonUp(0))
				{
					if (currentCell)
					{
						currentCell.DisableHighlight();
					}
					bool hasPath = DoPathfinding();
					if (hasPath)
					{
						DoMove();
					}
					
					if (selectedTeam)
					{
						selectedTeam = null;
					}
				}
			}
			else 
			{
				if (Input.GetMouseButtonUp(0))
				{
					DoSelection();
				}
			}

			//if (Input.GetMouseButtonDown(0)) {
			//	DoSelection();
			//}
			//else if (selectedUnit) {
			//	if (Input.GetMouseButtonDown(1)) {
			//		DoMove();
			//	}
			//	else {
			//		DoPathfinding();
			//	}
			//}
		}
	}

	void DoSelection () {
		grid.ClearPath();
		UpdateCurrentCell();
		if (currentCell) {
			selectedTeam = currentCell.Team;
			if (selectedTeam)
			{
				currentCell.EnableHighlight(Color.yellow);
			}
		}
	}

	bool DoPathfinding () {
		if (UpdateCurrentCell()) {
			if (currentCell.highlightQuad && currentCell.highlightQuad.buildEnable)
			{
				if (currentCell && selectedTeam.IsValidDestination(currentCell) 
					&& currentCell.Team == null)
					//grid.CanMoveIn(currentCell))
				{
					grid.FindPath(selectedTeam.Location, currentCell, selectedTeam);
					return true;
				}
				else
				{
					grid.ClearPath();
				}
			}
		}
		return false;
	}

	void DoMove () {
		if (grid.HasPath) {
			List<HexCell> pathCell = grid.GetPath();

			//HexCell endCell = pathCell[pathCell.Count - 1];
			//if (endCell.Unit != null)
			//{
			//	for (HexDirection d = HexDirection.NE; d <= HexDirection.NW; d++)
			//	{
			//		HexCell toCell = endCell.GetNeighbor(d);
			//		if (toCell != null && toCell.Unit == null && grid.Search(endCell, toCell, endCell.Unit))
			//		{
			//			endCell.Unit.Travel(new List<HexCell>() { endCell, toCell });
			//			break;
			//		}
			//	}
			//}

			selectedTeam.Travel(pathCell);
            //grid.ClearPath();
        }
	}

	bool UpdateCurrentCell () {
		HexCell cell =
			grid.GetCell(Camera.main.ScreenPointToRay(Input.mousePosition));
		if (cell != currentCell) {
			currentCell = cell;
			return true;
		}
		return false;
	}
}