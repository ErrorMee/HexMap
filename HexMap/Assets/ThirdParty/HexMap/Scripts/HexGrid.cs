﻿using BehaviorDesigner.Runtime;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class HexGrid : MonoBehaviour {

	public int cellChunkCountX = 5, cellChunkCountZ = 4;
	[SerializeField]
	private bool generateMaps = true;
	[SerializeField]
	private HexMapGenerator mapGenerator;
	public bool wrapping;

	public HexCell cellPrefab;
	public HexGridChunk chunkPrefab;
	public Teamer[] teamerPrefabs;

	public ExternalBehavior externalTeamBehavior;

	public Texture2D noiseSource;

	public int seed;

	[SerializeField]
	private HexMapCamera hexMapCamera;

	/// <summary>
	/// 中心格子
	/// </summary>
	public HexCell centerCell;


	public bool HasPath {
		get {
			return currentPathExists;
		}
	}

	Transform[] columns;
	HexGridChunk[] chunks;
	HexCell[] cells;

	int chunkCountX, chunkCountZ;

	HexCellPriorityQueue searchFrontier;

	int searchFrontierPhase;

	public HexCell currentPathFrom, currentPathTo;
	bool currentPathExists;

	int currentCenterColumnIndex = -1;

	List<Team> teams = new List<Team>();

	HexCellShaderData cellShaderData;

	void Awake () {
		cellChunkCountX *= HexMetrics.chunkSizeX;
		cellChunkCountZ *= HexMetrics.chunkSizeZ;

		HexMetrics.noiseSource = noiseSource;
		HexMetrics.InitializeHashGrid(seed);
		Team.teamerPrefabs = teamerPrefabs;
		Team.externalTeamBehavior = externalTeamBehavior;
		cellShaderData = gameObject.AddComponent<HexCellShaderData>();
		cellShaderData.Grid = this;
		if (generateMaps)
		{
			mapGenerator.GenerateMap(cellChunkCountX, cellChunkCountZ, wrapping);
		}
		else
		{
			CreateMap(cellChunkCountX, cellChunkCountZ, wrapping);
		}
	}

	public void AddTeam (short id, HexCell location, float orientation) {
		Team team = new GameObject("Team").AddComponent<Team>();
		teams.Add(team);
		team.Grid = this;
		team.Location = location;
		team.ID = id;
		team.InitTeamer(orientation);
	}

	public void RemoveTeam (Team team) {
		teams.Remove(team);
		team.Die();
	}

	public void MakeChildOfColumn (Transform child, int columnIndex) {
		child.SetParent(columns[columnIndex], false);
	}

	public bool CreateMap (int x, int z, bool wrapping) {
		if (
			x <= 0 || x % HexMetrics.chunkSizeX != 0 ||
			z <= 0 || z % HexMetrics.chunkSizeZ != 0
		) {
			Debug.LogError("Unsupported map size.");
			return false;
		}

		ClearPath();
		ClearTeams();
		if (columns != null) {
			for (int i = 0; i < columns.Length; i++) {
				Destroy(columns[i].gameObject);
			}
		}

		cellChunkCountX = x;
		cellChunkCountZ = z;
		this.wrapping = wrapping;
		currentCenterColumnIndex = -1;
		HexMetrics.wrapSize = wrapping ? cellChunkCountX : 0;
		chunkCountX = cellChunkCountX / HexMetrics.chunkSizeX;
		chunkCountZ = cellChunkCountZ / HexMetrics.chunkSizeZ;
		cellShaderData.Initialize(cellChunkCountX, cellChunkCountZ);
		CreateChunks();
		CreateCells();

		centerCell = GetCell(HexCoordinates.FromOffsetCoordinates(
			Mathf.FloorToInt(cellChunkCountX / 2), 
			Mathf.FloorToInt(cellChunkCountZ / 2)));
		SetEditEnable();

		hexMapCamera.CenterAlign();
		return true;
	}

	private void SetEditEnable()
	{
		int centerX = centerCell.coordinates.X;
		int centerZ = centerCell.coordinates.Z;

		for (int r = 0, z = centerZ - HexMetrics.editRadiu; z <= centerZ; z++, r++)
		{
			for (int x = centerX - r; x <= centerX + HexMetrics.editRadiu; x++)
			{
				HexCell cell = GetCell(new HexCoordinates(x, z));
				if (cell != null)
				{
					cell.chunk.highlights.InitBuild(cell);
				}
			}
		}
		for (int r = 0, z = centerZ + HexMetrics.editRadiu; z > centerZ; z--, r++)
		{
			for (int x = centerX - HexMetrics.editRadiu; x <= centerX + r; x++)
			{
				HexCell cell = GetCell(new HexCoordinates(x, z));
				if (cell != null)
				{
					cell.chunk.highlights.InitBuild(cell);
				}
			}
		}
	}

	void CreateChunks () {
		columns = new Transform[chunkCountX];
		for (int x = 0; x < chunkCountX; x++) {
			columns[x] = new GameObject("Column").transform;
			columns[x].SetParent(transform, false);
		}

		chunks = new HexGridChunk[chunkCountX * chunkCountZ];
		for (int z = 0, i = 0; z < chunkCountZ; z++) {
			for (int x = 0; x < chunkCountX; x++) {
				HexGridChunk chunk = chunks[i++] = Instantiate(chunkPrefab);
				chunk.transform.SetParent(columns[x], false);
				chunk.highlights.Clear();
			}
		}
	}

	void CreateCells () {
		cells = new HexCell[cellChunkCountZ * cellChunkCountX];

		for (int z = 0, i = 0; z < cellChunkCountZ; z++) {
			for (int x = 0; x < cellChunkCountX; x++) {
				CreateCell(x, z, i++);
			}
		}
	}

	void ClearTeams () {
		for (int i = 0; i < teams.Count; i++) {
			teams[i].Die();
		}
		teams.Clear();
	}

	void OnEnable () {
		if (!HexMetrics.noiseSource) {
			HexMetrics.noiseSource = noiseSource;
			HexMetrics.InitializeHashGrid(seed);
			Team.teamerPrefabs = teamerPrefabs;
			Team.externalTeamBehavior = externalTeamBehavior;
			HexMetrics.wrapSize = wrapping ? cellChunkCountX : 0;
			ResetVisibility();
		}
	}

	public HexCell GetCell (Ray ray) {
		RaycastHit hit;
		if (Physics.Raycast(ray, out hit)) {
			return GetCell(hit.point);
		}
		return null;
	}

	public HexCell GetCell (Vector3 position) {
		position = transform.InverseTransformPoint(position);
		HexCoordinates coordinates = HexCoordinates.FromPosition(position);
		return GetCell(coordinates);
	}

	public HexCell GetCell (HexCoordinates coordinates) {
		int z = coordinates.Z;
		if (z < 0 || z >= cellChunkCountZ) {
			return null;
		}
		int x = coordinates.X + z / 2;
		if (x < 0 || x >= cellChunkCountX) {
			return null;
		}
		return cells[x + z * cellChunkCountX];
	}

	public HexCell GetCell (int xOffset, int zOffset) {
		return cells[xOffset + zOffset * cellChunkCountX];
	}

	public HexCell GetCell (int cellIndex) {
		return cells[cellIndex];
	}

	void CreateCell (int x, int z, int i) {
		Vector3 position;
		position.x = (x + z * 0.5f - z / 2) * HexMetrics.innerDiameter;
		position.y = 0f;
		position.z = z * (HexMetrics.outerRadius * 1.5f);

		HexCell cell = cells[i] = Instantiate<HexCell>(cellPrefab);
		cell.transform.localPosition = position;
		cell.coordinates = HexCoordinates.FromOffsetCoordinates(x, z);
		cell.Index = i;
		cell.ColumnIndex = x / HexMetrics.chunkSizeX;
		cell.ShaderData = cellShaderData;

		if (wrapping) {
			cell.Explorable = z > 0 && z < cellChunkCountZ - 1;
		}
		else {
			cell.Explorable =
				x > 0 && z > 0 && x < cellChunkCountX - 1 && z < cellChunkCountZ - 1;
		}

		if (x > 0) {
			cell.SetNeighbor(HexDirection.W, cells[i - 1]);
			if (wrapping && x == cellChunkCountX - 1) {
				cell.SetNeighbor(HexDirection.E, cells[i - x]);
			}
		}
		if (z > 0) {
			if ((z & 1) == 0) {
				cell.SetNeighbor(HexDirection.SE, cells[i - cellChunkCountX]);
				if (x > 0) {
					cell.SetNeighbor(HexDirection.SW, cells[i - cellChunkCountX - 1]);
				}
				else if (wrapping) {
					cell.SetNeighbor(HexDirection.SW, cells[i - 1]);
				}
			}
			else {
				cell.SetNeighbor(HexDirection.SW, cells[i - cellChunkCountX]);
				if (x < cellChunkCountX - 1) {
					cell.SetNeighbor(HexDirection.SE, cells[i - cellChunkCountX + 1]);
				}
				else if (wrapping) {
					cell.SetNeighbor(
						HexDirection.SE, cells[i - cellChunkCountX * 2 + 1]
					);
				}
			}
		}

		cell.Elevation = 0;

		AddCellToChunk(x, z, cell);
	}

	void AddCellToChunk (int x, int z, HexCell cell) {
		int chunkX = x / HexMetrics.chunkSizeX;
		int chunkZ = z / HexMetrics.chunkSizeZ;
		HexGridChunk chunk = chunks[chunkX + chunkZ * chunkCountX];

		int localX = x - chunkX * HexMetrics.chunkSizeX;
		int localZ = z - chunkZ * HexMetrics.chunkSizeZ;
		chunk.AddCell(localX + localZ * HexMetrics.chunkSizeX, cell);
	}

	public void Save (BinaryWriter writer) {
		writer.Write(cellChunkCountX);
		writer.Write(cellChunkCountZ);
		writer.Write(wrapping);

		for (int i = 0; i < cells.Length; i++) {
			cells[i].Save(writer);
		}

		writer.Write(teams.Count);
		for (int i = 0; i < teams.Count; i++) {
			teams[i].Save(writer);
		}
	}

	public void Load (BinaryReader reader, int header) {
		ClearPath();
		ClearTeams();
		int x = 20, z = 15;
		if (header >= 1) {
			x = reader.ReadInt32();
			z = reader.ReadInt32();
		}
		bool wrapping = header >= 5 ? reader.ReadBoolean() : false;
		if (x != cellChunkCountX || z != cellChunkCountZ || this.wrapping != wrapping) {
			if (!CreateMap(x, z, wrapping)) {
				return;
			}
		}

		bool originalImmediateMode = cellShaderData.ImmediateMode;
		cellShaderData.ImmediateMode = true;

		for (int i = 0; i < cells.Length; i++) {
			cells[i].Load(reader, header);
		}
		for (int i = 0; i < chunks.Length; i++) {
			chunks[i].Refresh();
		}

		if (header >= 2) {
			int teamCount = reader.ReadInt32();
			for (int i = 0; i < teamCount; i++) {
				Team.Load(reader, this);
			}
		}

		cellShaderData.ImmediateMode = originalImmediateMode;
	}

	public List<HexCell> GetPath () {
		if (!currentPathExists) {
			return null;
		}
		List<HexCell> path = ListPool<HexCell>.Get();
		for (HexCell c = currentPathTo; c != currentPathFrom; c = c.PathFrom) {
			path.Add(c);
		}
		path.Add(currentPathFrom);
		path.Reverse();
		return path;
	}

	public void ClearPath () {
		if (currentPathExists) {
			HexCell current = currentPathTo;
			while (current != currentPathFrom) {
				current.DisableHighlight();
				current = current.PathFrom;
			}
			current.DisableHighlight();
			currentPathExists = false;
		}
		else if (currentPathFrom) {
			currentPathFrom.DisableHighlight();
			currentPathTo.DisableHighlight();
		}
		currentPathFrom = currentPathTo = null;
	}

	void ShowPath (int speed) {
		if (currentPathExists) {
			HexCell current = currentPathTo;
			while (current != currentPathFrom) {
				int turn = (current.Distance - 1) / speed;
				current.EnableHighlight(Color.green);
				current = current.PathFrom;
			}
		}
		currentPathTo.EnableHighlight(Color.green);
	}

	public bool CanMoveIn(HexCell moveInCell)
	{
		if (moveInCell.Team == null)
		{
			return true;
		}
		else {

			for (HexDirection d = HexDirection.NE; d <= HexDirection.NW; d++)
			{
				HexCell toCell = moveInCell.GetNeighbor(d);
                if(toCell != null && toCell.Team == null && Search(moveInCell, toCell, moveInCell.Team)) 
				{
					return true;
				}
			}
		}
		return false;
	}

	public void FindPath (HexCell fromCell, HexCell toCell, Team unit) {
		ClearPath();
		currentPathFrom = fromCell;
		currentPathTo = toCell;
		currentPathExists = Search(fromCell, toCell, unit);
		ShowPath(unit.Speed);
	}

	public bool Search (HexCell fromCell, HexCell toCell, Team unit) {
		int speed = unit.Speed;
		searchFrontierPhase += 2;
		if (searchFrontier == null) {
			searchFrontier = new HexCellPriorityQueue();
		}
		else {
			searchFrontier.Clear();
		}

		fromCell.SearchPhase = searchFrontierPhase;
		fromCell.Distance = 0;
		searchFrontier.Enqueue(fromCell);
		while (searchFrontier.Count > 0) {
			HexCell current = searchFrontier.Dequeue();
			current.SearchPhase += 1;

			if (current == toCell) {
				return true;
			}

			int currentTurn = (current.Distance - 1) / speed;

			for (HexDirection d = HexDirection.NE; d <= HexDirection.NW; d++) {
				HexCell neighbor = current.GetNeighbor(d);
				if (
					neighbor == null ||
					neighbor.SearchPhase > searchFrontierPhase
				) {
					continue;
				}
				if (!unit.IsValidDestination(neighbor)) {
					continue;
				}
				int moveCost = unit.GetMoveCost(current, neighbor, d);
				if (moveCost < 0) {
					continue;
				}

				int distance = current.Distance + moveCost;
				int turn = (distance - 1) / speed;
				if (turn > currentTurn) {
					distance = turn * speed + moveCost;
				}

				if (neighbor.SearchPhase < searchFrontierPhase) {
					neighbor.SearchPhase = searchFrontierPhase;
					neighbor.Distance = distance;
					neighbor.PathFrom = current;
					neighbor.SearchHeuristic =
						neighbor.coordinates.DistanceTo(toCell.coordinates);
					searchFrontier.Enqueue(neighbor);
				}
				else if (distance < neighbor.Distance) {
					int oldPriority = neighbor.SearchPriority;
					neighbor.Distance = distance;
					neighbor.PathFrom = current;
					searchFrontier.Change(neighbor, oldPriority);
				}
			}
		}
		return false;
	}

	public void IncreaseVisibility (HexCell fromCell, int range) {
		List<HexCell> cells = GetVisibleCells(fromCell, range);
		for (int i = 0; i < cells.Count; i++) {
			cells[i].IncreaseVisibility();
		}
		ListPool<HexCell>.Add(cells);
	}

	public void DecreaseVisibility (HexCell fromCell, int range) {
		List<HexCell> cells = GetVisibleCells(fromCell, range);
		for (int i = 0; i < cells.Count; i++) {
			cells[i].DecreaseVisibility();
		}
		ListPool<HexCell>.Add(cells);
	}

	public void ResetVisibility () {
		for (int i = 0; i < cells.Length; i++) {
			cells[i].ResetVisibility();
		}
		for (int i = 0; i < teams.Count; i++) {
			Team unit = teams[i];
			IncreaseVisibility(unit.Location, unit.VisionRange);
		}
	}

	List<HexCell> GetVisibleCells (HexCell fromCell, int range) {
		List<HexCell> visibleCells = ListPool<HexCell>.Get();

		searchFrontierPhase += 2;
		if (searchFrontier == null) {
			searchFrontier = new HexCellPriorityQueue();
		}
		else {
			searchFrontier.Clear();
		}

		range += fromCell.ViewElevation;
		fromCell.SearchPhase = searchFrontierPhase;
		fromCell.Distance = 0;
		searchFrontier.Enqueue(fromCell);
		HexCoordinates fromCoordinates = fromCell.coordinates;
		while (searchFrontier.Count > 0) {
			HexCell current = searchFrontier.Dequeue();
			current.SearchPhase += 1;
			visibleCells.Add(current);

			for (HexDirection d = HexDirection.NE; d <= HexDirection.NW; d++) {
				HexCell neighbor = current.GetNeighbor(d);
				if (
					neighbor == null ||
					neighbor.SearchPhase > searchFrontierPhase ||
					!neighbor.Explorable
				) {
					continue;
				}

				int distance = current.Distance + 1;
				if (distance + neighbor.ViewElevation > range ||
					distance > fromCoordinates.DistanceTo(neighbor.coordinates)
				) {
					continue;
				}

				if (neighbor.SearchPhase < searchFrontierPhase) {
					neighbor.SearchPhase = searchFrontierPhase;
					neighbor.Distance = distance;
					neighbor.SearchHeuristic = 0;
					searchFrontier.Enqueue(neighbor);
				}
				else if (distance < neighbor.Distance) {
					int oldPriority = neighbor.SearchPriority;
					neighbor.Distance = distance;
					searchFrontier.Change(neighbor, oldPriority);
				}
			}
		}
		return visibleCells;
	}

	public void CenterMap (float xPosition) {
		int centerColumnIndex = (int)
			(xPosition / (HexMetrics.innerDiameter * HexMetrics.chunkSizeX));
		
		if (centerColumnIndex == currentCenterColumnIndex) {
			return;
		}
		currentCenterColumnIndex = centerColumnIndex;

		int minColumnIndex = centerColumnIndex - chunkCountX / 2;
		int maxColumnIndex = centerColumnIndex + chunkCountX / 2;

		Vector3 position;
		position.y = position.z = 0f;
		for (int i = 0; i < columns.Length; i++) {
			if (i < minColumnIndex) {
				position.x = chunkCountX *
					(HexMetrics.innerDiameter * HexMetrics.chunkSizeX);
			}
			else if (i > maxColumnIndex) {
				position.x = chunkCountX *
					-(HexMetrics.innerDiameter * HexMetrics.chunkSizeX);
			}
			else {
				position.x = 0f;
			}
			columns[i].localPosition = position;
		}
	}
}