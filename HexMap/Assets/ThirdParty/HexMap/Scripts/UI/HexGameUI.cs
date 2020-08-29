using UnityEngine;
using UnityEngine.EventSystems;

public class HexGameUI : MonoBehaviour {

	public HexGrid grid;

	HexCell currentCell;

	public HexUnit selectedUnit;

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

			if (selectedUnit)
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
					
					if (selectedUnit)
					{
						selectedUnit = null;
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
			selectedUnit = currentCell.Unit;
			if (selectedUnit)
			{
				currentCell.EnableHighlight(Color.yellow);
			}
		}
	}

	bool DoPathfinding () {
		if (UpdateCurrentCell()) {
			if (currentCell.highlightQuad && currentCell.highlightQuad.buildEnable)
			{
				if (currentCell && selectedUnit.IsValidDestination(currentCell))
				{
					grid.FindPath(selectedUnit.Location, currentCell, selectedUnit);
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
			selectedUnit.Travel(grid.GetPath());
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