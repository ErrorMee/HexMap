using UnityEngine;

[RequireComponent(typeof(MeshFilter), typeof(MeshRenderer))]

public class PrismMesh : MonoBehaviour
{
	/// <summary>
	/// 柱子的半径
	/// </summary>
	public static float Prism_Radius = 0.8f;

	/// <summary>
	/// 柱子的单位高度
	/// </summary>
	public static float Prism_UnitHeight = 0.5f;

	[SerializeField]
	[Range(3,32)]
	private int m_Edges = 3;
	public int EdgeNumber
	{
		get { return m_Edges; }
		set { m_Edges = Mathf.Clamp(value, 3, 32); UpdateMesh(); }
	}

	[SerializeField]
	[Range(0, 16)]
	private float m_HeightLevel = 1f;

	[SerializeField]
	[Range(0, 8)]
	private float m_CoverHeight = 0.2f;

	private Mesh mesh;

	private void Awake()
    {
		Generate();
	}

	//private void OnValidate()
	//{
	//	Generate();
	//}

	private void Generate()
	{
		GetComponent<MeshFilter>().mesh = mesh = new Mesh();

		UpdateMesh();
	}

	private void UpdateMesh()
	{
		mesh.name = "Prism" + m_Edges;

		UpdateVertices();
		UpdateTriangles();
	}

	private void UpdateVertices()
	{
		Vector3[] vertices = new Vector3[m_Edges * 9 + 1];
		Vector3[] normals = new Vector3[vertices.Length];
		Vector2[] uvs = new Vector2[vertices.Length];

		float cliffHeight = m_HeightLevel * Prism_UnitHeight;

		float angleE = Mathf.PI * 2 / m_Edges;

		float shrink = m_CoverHeight > 1 ? 1 : m_CoverHeight;

		for (int e = 0; e < m_Edges; e++)
		{
			float angle = angleE * e;

			float sin = Mathf.Sin(angle);
			float cos = Mathf.Cos(angle);

			int index0 = (e + m_Edges * 0) * 2; int index1 = index0 + 1;
			int index2 = (e + m_Edges * 1) * 2; int index3 = index2 + 1;
			int index4 = (e + m_Edges * 2) * 2; int index5 = index4 + 1;
			int index6 = (e + m_Edges * 3) * 2; int index7 = index6 + 1;
			int index8 = e + m_Edges * 8;

			//vertices
			vertices[index0] = vertices[index1] = new Vector3(cos, 0, sin) * Prism_Radius;
			vertices[index2] = vertices[index3] =
			vertices[index4] = vertices[index5] = vertices[index0] + Vector3.up * cliffHeight;
			vertices[index6] = vertices[index7] = vertices[index8] =
				vertices[index2] - vertices[index0] * shrink + Vector3.up * Prism_Radius * m_CoverHeight;

			//uvs
			float uScale = 1;// (vertices[2] - vertices[1]).magnitude * m_Edges / cliffHeight;
			if (e == 0)
			{
				uvs[index0] = new Vector2(1 * uScale, 0);
				uvs[index2] = uvs[index4] = new Vector2(1 * uScale, 1);
			}
			else
			{
				uvs[index0] = new Vector2(e / (float)m_Edges * uScale, 0);
				uvs[index2] = uvs[index4] = new Vector2(e / (float)m_Edges * uScale, 1);
			}

			uvs[index1] = new Vector2(e / (float)m_Edges * uScale, 0);
			uvs[index3] = new Vector2(e / (float)m_Edges * uScale, 1);
			uvs[index4] = uvs[index5] = new Vector2(cos, sin) * 0.5f + Vector2.one * 0.5f;
			uvs[index6] = uvs[index7] = uvs[index8] = uvs[index4] - new Vector2(cos, sin) * Prism_Radius * m_CoverHeight;
		}

		//normals
		for (int e = 0; e < m_Edges; e++)
		{
			int index0 = (e + m_Edges * 0) * 2; int index1 = index0 + 1;
			int index2 = (e + m_Edges * 1) * 2; int index3 = index2 + 1;
			int index4 = (e + m_Edges * 2) * 2; int index5 = index4 + 1;
			int index6 = (e + m_Edges * 3) * 2; int index7 = index6 + 1;
			int index8 = e + m_Edges * 8;
			
			normals[index0] = normals[index2] = (vertices[index0] + vertices[index0 == 0 ? m_Edges * 2 - 1 : index0 - 1]) / 2;
			normals[index4] = normals[index6] = ((vertices[index4] + vertices[index4 == m_Edges * 2 ? m_Edges * 4 - 1 : index4 - 1]) / 2
				+ Vector3.up * (cliffHeight + m_CoverHeight)) / 2 - Vector3.up * cliffHeight;
			normals[index1] = normals[index3] = (vertices[index1] + vertices[index1 == m_Edges * 2 - 1 ? 0 : index1 + 1]) / 2;
			normals[index5] = normals[index7] = ((vertices[index5] + vertices[index5 == m_Edges * 4 - 1 ? m_Edges * 2 : index5 + 1]) / 2
				+ Vector3.up * (cliffHeight + m_CoverHeight)) / 2 - Vector3.up * cliffHeight;
			normals[index8] = Vector3.up;
		}

		vertices[vertices.Length - 1] = Vector3.up * (cliffHeight + Prism_Radius * m_CoverHeight);
		normals[vertices.Length - 1] = Vector3.up;
		uvs[vertices.Length - 1] = Vector2.one * 0.5f;

		mesh.vertices = vertices;
		
		mesh.uv = uvs;
		mesh.normals = normals;

	}

	private void UpdateTriangles()
	{
		int[] trianglesCliff = new int[m_Edges * 5 * 3];

		int index = 0;
		for (int ti = 0; ti < m_Edges; ti++)
		{
			int startIndex = ti * 2 + 1;
			trianglesCliff[index++] = startIndex;//1
			trianglesCliff[index++] = startIndex + m_Edges * 2;//2

			if (ti == m_Edges - 1)//3
			{
				trianglesCliff[index++] = m_Edges * 2;
			}
			else
			{
				trianglesCliff[index++] = startIndex + m_Edges * 2 + 1;
			}

			trianglesCliff[index++] = startIndex;//4

			if (ti == m_Edges - 1)//5
			{
				trianglesCliff[index++] = m_Edges * 2;
			}
			else
			{
				trianglesCliff[index++] = startIndex + m_Edges * 2 + 1;
			}

			if (ti == m_Edges - 1)//6
			{
				trianglesCliff[index++] = 0;
			}
			else
			{
				trianglesCliff[index++] = startIndex + 1;
			}
		}

		int count = index;
		for (int ti = 0; ti < count; ti++)
		{
			trianglesCliff[index++] = trianglesCliff[ti] + m_Edges * 4;
		}

		//for (int ti = 0; ti < trianglesCliff.Length; ti++)
		//{
		//	trianglesCover[ti] = trianglesCliff[ti] + m_Edges * 4;
		//}

		for (int ti = 0; ti < m_Edges; ti++)
		{
			trianglesCliff[index++] = m_Edges * 8 + ti;
			trianglesCliff[index++] = m_Edges * 9 - 1;
			if (trianglesCliff.Length + ti * 3 + 2 == trianglesCliff.Length - 1)
			{
				trianglesCliff[index++] = m_Edges * 8;
			}
			else
			{
				trianglesCliff[index++] = m_Edges * 8 + ti + 1;
			}
		}

		//for (int ti = 0; ti < m_Edges; ti++)
		//{
		//	trianglesCover[trianglesCliff.Length + ti * 3 + 0] = m_Edges * 8 + ti;
		//	trianglesCover[trianglesCliff.Length + ti * 3 + 1] = m_Edges * 9 - 1;
		//	if (trianglesCliff.Length + ti * 3 + 2 == trianglesCover.Length - 1)
		//	{
		//		trianglesCover[trianglesCliff.Length + ti * 3 + 2] = m_Edges * 8;
		//	}
		//	else
		//	{
		//		trianglesCover[trianglesCliff.Length + ti * 3 + 2] = m_Edges * 8 + ti + 1;
		//	}
		//}

		mesh.subMeshCount = 1;
		mesh.SetTriangles(trianglesCliff, 0);
		//mesh.SetTriangles(trianglesCover, 1);
	}
}