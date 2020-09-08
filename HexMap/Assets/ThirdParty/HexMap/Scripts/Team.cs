using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.IO;

public class Team : MonoBehaviour {

	const float travelSpeed = 2.0f;

	public static Teamer[] teamerPrefabs;

	public List<Teamer> children = new List<Teamer>();

	public short ID { get; set; }

	public HexGrid Grid { get; set; }

	public HexCell Location {
		get {
			return location;
		}
		set {
			if (location) {
				Grid.DecreaseVisibility(location, VisionRange);
				location.Team = null;
			}
			location = value;
			value.Team = this;
			Grid.IncreaseVisibility(value, VisionRange);
			transform.localPosition = value.Position;
			Grid.MakeChildOfColumn(transform, value.ColumnIndex);
		}
	}

	HexCell location, currentTravelLocation;

	public int Speed {
		get {
			return 8;
		}
	}

	public int VisionRange {
		get {
			return 10;
		}
	}

	List<HexCell> pathToTravel;

	public void ValidateLocation () {
		transform.localPosition = location.Position;
	}

	public bool IsValidDestination (HexCell cell) {
		return !cell.IsUnderwater && cell.SpecialIndex == 0;
		//return cell.IsExplored && !cell.IsUnderwater && !cell.Unit;
	}

	public void Travel (List<HexCell> path) {
		location.Team = null;
		location = path[path.Count - 1];
		location.Team = this;
		pathToTravel = path;
		StopAllCoroutines();
		StartCoroutine(TravelPath());
	}

	IEnumerator TravelPath () {
		Vector3 a, b, c = pathToTravel[0].Position;

		for (int i = 0; i < children.Count - 1; i++)
		{
			StartCoroutine(children[i].LookAt(pathToTravel[1].Position));
		}
		yield return children[children.Count - 1].LookAt(pathToTravel[1].Position);

		if (!currentTravelLocation) {
			currentTravelLocation = pathToTravel[0];
		}
		Grid.DecreaseVisibility(currentTravelLocation, VisionRange);
		int currentColumn = currentTravelLocation.ColumnIndex;

		float t = Time.deltaTime * travelSpeed;
		for (int i = 1; i < pathToTravel.Count; i++) {
			currentTravelLocation = pathToTravel[i];
			a = c;
			b = pathToTravel[i - 1].Position;

			int nextColumn = currentTravelLocation.ColumnIndex;
			if (currentColumn != nextColumn) {
				if (nextColumn < currentColumn - 1) {
					a.x -= HexMetrics.innerDiameter * HexMetrics.wrapSize;
					b.x -= HexMetrics.innerDiameter * HexMetrics.wrapSize;
				}
				else if (nextColumn > currentColumn + 1) {
					a.x += HexMetrics.innerDiameter * HexMetrics.wrapSize;
					b.x += HexMetrics.innerDiameter * HexMetrics.wrapSize;
				}
				Grid.MakeChildOfColumn(transform, nextColumn);
				currentColumn = nextColumn;
			}

			c = (b + currentTravelLocation.Position) * 0.5f;
			Grid.IncreaseVisibility(pathToTravel[i], VisionRange);

			for (; t < 1f; t += Time.deltaTime * travelSpeed) {
				transform.localPosition = Bezier.GetPoint(a, b, c, t);
				Vector3 d = Bezier.GetDerivative(a, b, c, t);
				d.y = 0f;
				for (int j = 0; j < children.Count; j++)
				{
					children[j].transform.localRotation = Quaternion.LookRotation(d);
				}
				
				yield return null;
			}
			Grid.DecreaseVisibility(pathToTravel[i], VisionRange);
			pathToTravel[i].DisableHighlight();
			t -= 1f;
		}
		currentTravelLocation = null;
		
		a = c;
		b = location.Position;
		c = b;
		Grid.IncreaseVisibility(location, VisionRange);
		for (; t < 1f; t += Time.deltaTime * travelSpeed) {
			transform.localPosition = Bezier.GetPoint(a, b, c, t);
			Vector3 d = Bezier.GetDerivative(a, b, c, t);
			d.y = 0f;
			for (int j = 0; j < children.Count; j++)
			{
				children[j].transform.localRotation = Quaternion.LookRotation(d);
			}
			yield return null;

			if (t > 0.6f)
			{
				for (int i = 0; i < children.Count; i++)
				{
					children[i].Idle();
				}
			}
		}
		HexCell cell =
			Grid.GetCell(transform.position);
		cell.DisableHighlight();
		transform.localPosition = location.Position;

		for (int i = 0; i < children.Count; i++)
		{
			children[i].Orientation = children[i].transform.localRotation.eulerAngles.y;
		}

		ListPool<HexCell>.Add(pathToTravel);
		pathToTravel = null;
	}

	public int GetMoveCost (
		HexCell fromCell, HexCell toCell, HexDirection direction)
	{
		if (!IsValidDestination(toCell)) {
			return -1;
		}
		HexEdgeType edgeType = fromCell.GetEdgeType(toCell);
		if (edgeType == HexEdgeType.Cliff) {
			return -1;
		}
		int moveCost;
		if (fromCell.HasRoadThroughEdge(direction)) {
			moveCost = 1;
		}
		else if (fromCell.Walled != toCell.Walled) {
			return -1;
		}
		else {
			moveCost = edgeType == HexEdgeType.Flat ? 5 : 10;
			moveCost +=
				toCell.UrbanLevel + toCell.FarmLevel + toCell.PlantLevel;
		}
		return moveCost;
	}

	public void Die () {
		if (location) {
			Grid.DecreaseVisibility(location, VisionRange);
		}
		location.Team = null;
		Destroy(gameObject);
	}

	public void Save (BinaryWriter writer) {
		location.coordinates.Save(writer);
		writer.Write(ID);
		writer.Write(children[0].Orientation);
	}

	public static void Load (BinaryReader reader, HexGrid grid) {
		HexCoordinates coordinates = HexCoordinates.Load(reader);
		short id = reader.ReadInt16();
		float orientation = reader.ReadSingle();
		grid.AddTeam(id, grid.GetCell(coordinates), orientation);
	}

	public void InitTeamer(float orientation)
	{
		Teamer teamer = teamerPrefabs[ID - 1];

		Teamer child = Instantiate(teamer, transform, false);
		child.transform.localPosition = Vector3.zero;
		child.transform.LookAt(new Vector3(121.2435f,0,0));
		//child.Orientation = orientation;
		children.Add(child);

		float radius = 4.0f;
		for (int i = 0; i < 6; i++)
		{
			child = Instantiate(teamer, transform, false);
			float angle = Mathf.PI * 2 / 6 * i;
			child.transform.localPosition = new Vector3(Mathf.Sin(angle) * radius, 0, Mathf.Cos(angle) * radius);
			//child.Orientation = orientation;
			child.transform.LookAt(new Vector3(121.2435f, 0, 0));
			children.Add(child);
		}

		//radius = 5.5f;
		//for (int i = 0; i < 6; i++)
		//{
		//	child = Instantiate(teamer, transform, false);
		//	float angle = Mathf.PI * 2 / 6 * (i + 0.5f);
		//	child.transform.localPosition = new Vector3(Mathf.Sin(angle) * radius, 0, Mathf.Cos(angle) * radius);
		//	child.Orientation = orientation;
		//	children.Add(child);
		//}
	}

	void OnEnable () {

		if (location) {
			transform.localPosition = location.Position;
			if (currentTravelLocation) {
				Grid.IncreaseVisibility(location, VisionRange);
				Grid.DecreaseVisibility(currentTravelLocation, VisionRange);
				currentTravelLocation = null;
			}
		}
	}
}