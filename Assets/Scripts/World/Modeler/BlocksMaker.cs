using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class BlocksMaker : MonoBehaviour
{
	int modelToPreview;
	int faceToPreview;

	public BlockModel[] blocks;

	public List<Vector3> verts = new();
	public List<Vector2> uvs = new();
	public List<int> tris = new();

	MeshFilter meshFilter;

	private void Start()
	{
		meshFilter = GetComponent<MeshFilter>();
		WorldTable.Init();
		blocks = WorldTable.blocks;
	}

	private void Update()
	{
		float scroll = Input.GetAxisRaw("Mouse ScrollWheel");
		int prevModel = modelToPreview;

		modelToPreview+= ((int)scroll);
		modelToPreview = Mathf.Clamp(modelToPreview, 0, WorldTable.blocks.Length - 1);
		if(modelToPreview != prevModel)
		{
			faceToPreview = 0;
			PreviewModel();
		}
		else if(Input.GetKeyDown(KeyCode.Mouse2))
		{
			faceToPreview++;
			if (faceToPreview >= WorldTable.blocks[modelToPreview].Faces.GetLength(0))
				faceToPreview = 0;
			PreviewModel();
		}
	}

	void PreviewModel()
	{
		verts.Clear();
		uvs.Clear();
		tris.Clear();


		for (int vert = 0; vert < WorldTable.blocks[modelToPreview].Faces.GetLength(1); vert++)
		{
			Vector3 vertPos = WorldTable.blocks[modelToPreview].Faces[faceToPreview, vert];
			Vector2 uvPos = WorldTable.blocks[modelToPreview].UVS[faceToPreview, vert];

			verts.Add(vertPos);
			uvs.Add(uvPos);
		}
		for (int tri = 0; tri < WorldTable.blocks[modelToPreview].triangleTable.GetLength(1); tri++)
		{
			int triPos = WorldTable.blocks[modelToPreview].triangleTable[faceToPreview, tri];
			tris.Add(triPos);
		}

		Mesh mesh = new Mesh();
		mesh.vertices = verts.ToArray();
		mesh.uv = uvs.ToArray();
		mesh.triangles = tris.ToArray();

		mesh.RecalculateNormals();
		meshFilter.sharedMesh = mesh;
	}

	public void PreviewEdited()
	{
		Mesh mesh = new Mesh();
		mesh.vertices = verts.ToArray();
		mesh.uv = uvs.ToArray();
		mesh.triangles = tris.ToArray();

		mesh.RecalculateNormals();
		meshFilter.sharedMesh = mesh;
	}
}
