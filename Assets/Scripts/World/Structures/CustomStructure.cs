using Newtonsoft.Json;
using System.Collections.Generic;
using System.IO;
using Unity.Mathematics;
using UnityEngine;
public class CustomStructure : MonoBehaviour
{
	public static string path = Path.Combine(Application.dataPath, "CustomData", "Structures");

	public ChunkBlock[,,] blocks;
	public Vector3Int structureScale;
	public BlockData[] blockModels;
	public BlockModel[] blockTypes;

	public List<CustomStructureData> structures;
	public int structure;
	public string currentStructureName;

	MeshFilter mf;

	public Vector3 targetBlock;
	public Vector3 pivot;
	public short model;

	public bool forceAir;
	[Range(0, 1), SerializeField] float globalLightLevel;
	[Range(0, 1), SerializeField] float minGlobalLightLevel;
	[Range(0, 1), SerializeField] float maxGlobalLightLevel;

	private void OnValidate()
	{
		FixValues();
		Shader.SetGlobalFloat("GlobalLightLevel", globalLightLevel);
		Shader.SetGlobalFloat("minGlobalLightLevel", minGlobalLightLevel);
		Shader.SetGlobalFloat("maxGlobalLightLevel", maxGlobalLightLevel);
	}
	public void FixValues()
	{
		targetBlock = new Vector3Int(
			(int)Mathf.Clamp(targetBlock.x, 0, structureScale.x - 1),
			(int)Mathf.Clamp(targetBlock.y, 0, structureScale.y - 1),
			(int)Mathf.Clamp(targetBlock.z, 0, structureScale.z - 1));

		pivot = new Vector3Int((int)pivot.x, (int)pivot.y, (int)pivot.z);

		model = (short)Mathf.Clamp(model, 0, blockModels.Length - 1);
		structure = (short)Mathf.Clamp(structure, 0, structures.Count - 1);
	}
	public void PrepareData()
	{
		blocks = new ChunkBlock[structureScale.x, structureScale.y, structureScale.z];
		mf = GetComponent<MeshFilter>();
		targetBlock = new Vector3Int();
		mf.sharedMesh = null;
		currentStructureName = "";
		WorldTable.Init();
		blockTypes = WorldTable.blocks;
	}
	private void OnDrawGizmosSelected()
	{
		if (blocks == null)
		{
			mf = GetComponent<MeshFilter>();
			mf.sharedMesh = null;
			targetBlock = new Vector3();
			return;
		}
		Gizmos.DrawWireCube((Vector3)structureScale / 2f, structureScale);
		Gizmos.color = Color.green;
		Gizmos.DrawWireCube(targetBlock + (Vector3.one * 0.5f), Vector3.one);

		Gizmos.color = Color.red;
		Gizmos.DrawWireCube(pivot + (Vector3.one * 0.5f), Vector3.one * (1.1f + (math.sin(Time.time * 10f)) / 10f));
	}

	public void BuildMesh()
	{
		List<Vector3> verts = new();
		List<Color> colors = new();
		List<Vector2> uvs = new();
		List<int> tris = new();
		List<int> trisTrans = new();

		int triIndex = 0;

		int stLength = WorldTable.SidesTable.Length;

		for (int x = 0; x < blocks.GetLength(0); x++)
		{
			for (int z = 0; z < blocks.GetLength(2); z++)
			{
				for (int y = 0; y < blocks.GetLength(1); y++)
				{
					short cd = blocks[x, y, z].BlockID;

					BlockData bd = blockModels[cd];
					if (bd.isSolid)
					{
						Vector3Int pos = new Vector3Int(x, y, z);
						int tricount = WorldTable.blocks[bd.BlockModel].triangleTable.GetLength(1);
						int faceCount = WorldTable.blocks[bd.BlockModel].Faces.GetLength(1);
						int[] textureID = bd.Textures;
						for (int side = 0; side < stLength; side++)
						{
							Vector3Int sidePos = pos + WorldTable.SidesTable[side];

							bool isOut = IsStructureEdge(sidePos);
							bool drawFace = true;
							Color sideBrightness = new Color(1, 1, 1, 1);

							if (!isOut)
							{
								drawFace = blockModels[blocks[sidePos.x, sidePos.y, sidePos.z].BlockID].isTransparent;
							}
							if (drawFace)
							{

								for (int i = 0; i < faceCount; i++)
								{
									verts.Add(WorldTable.blocks[bd.BlockModel].Faces[side, i] + pos);
									colors.Add(sideBrightness);
									float normalizedImageGrid = 1f / WorldTable.imageGrid;

									float uy = textureID[side] / WorldTable.imageGrid;
									float ux = textureID[side] - (uy * WorldTable.imageGrid);


									ux *= normalizedImageGrid;
									uy *= normalizedImageGrid;

									uy = 1 - uy - normalizedImageGrid;

									Vector2 uvPos = new(ux + (normalizedImageGrid * WorldTable.blocks[bd.BlockModel].UVS[side, i].x), uy + (normalizedImageGrid * WorldTable.blocks[bd.BlockModel].UVS[side, i].y));
									uvs.Add(uvPos);
								}
								if (bd.isTransparent)
								{
									for (int tri = 0; tri < tricount; tri++)
									{
										trisTrans.Add(triIndex + WorldTable.blocks[bd.BlockModel].triangleTable[side, tri]);
									}
								}
								else
								{
									for (int tri = 0; tri < tricount; tri++)
									{
										tris.Add(triIndex + WorldTable.blocks[bd.BlockModel].triangleTable[side, tri]);
									}
								}
								triIndex += faceCount;
							}
						}
					}
				}
			}
		}

		Mesh mesh = new Mesh();
		mesh.subMeshCount = 2;
		mesh.vertices = verts.ToArray();
		mesh.uv = uvs.ToArray();
		mesh.colors = colors.ToArray();
		mesh.SetTriangles(tris.ToArray(), 0);
		mesh.SetTriangles(trisTrans.ToArray(), 1);

		mf.sharedMesh = mesh;
	}
	public void SetBlock()
	{
		SetBlock(((int)targetBlock.x), ((int)targetBlock.y), ((int)targetBlock.z));
		BuildMesh();
	}
	public void SetBlock(int x, int y, int z)
	{
		blocks[x, y, z].BlockID = model;
		BuildMesh();
	}
	bool IsStructureEdge(Vector3Int pos)
	{
		return (pos.x >= blocks.GetLength(0) || pos.x < 0 ||
				pos.z >= blocks.GetLength(2) || pos.z < 0 ||
				pos.y >= blocks.GetLength(1) || pos.y < 0);
	}


	public void Save()
	{
		bool contains = false;
		int matchNumber = -1;

		for (int i = 0; i < structures.Count; i++)
		{
			if (structures[i].name.Equals(currentStructureName))
			{
				contains = true;
				matchNumber = i;
				break;
			}
		}

		CustomStructureData newStructure = new(currentStructureName, blocks, (new Vector3Int((int)pivot.x, (int)pivot.y, (int)pivot.z)), forceAir);

		if (!contains)
		{
			structures.Add(newStructure);
		}
		else
		{
			structures[matchNumber] = newStructure;
		}

		for (int i = 0; i < structures.Count; i++)
		{
			var settings = new JsonSerializerSettings()
			{
				ReferenceLoopHandling = ReferenceLoopHandling.Ignore,
				Formatting = Formatting.Indented
			};
			string contents = JsonConvert.SerializeObject(structures[i], settings);
			File.WriteAllText(Path.Combine(path, $"{structures[i].name}.structure"), contents);
		}
	}
	public void Load()
	{
		string[] files = Directory.GetFiles(path);
		for (int i = 0; i < files.Length; i++)
		{
			if (files[i].EndsWith(".structure"))
			{
				string contents = File.ReadAllText(files[i]);
				CustomStructureData st = JsonConvert.DeserializeObject<CustomStructureData>(contents);

				bool contains = false;
				int matchNumber = -1;

				for (int s = 0; s < structures.Count; s++)
				{
					if (structures[s].name.Equals(st.name))
					{
						contains = true;
						matchNumber = s;
						break;
					}
				}

				if (contains)
				{
					structures[matchNumber] = st;
				}
				else
				{
					structures.Add(st);
				}
			}
		}

		if(structures.Count > 0)
			LoadStructure(0);
	}

	public void LoadStructure(int ind)
	{
		if (structures == null || structures.Count == 0) return;

		PrepareData();
		CustomStructureData sd = structures[ind];

		blocks = sd.data;
		structureScale = new Vector3Int(blocks.GetLength(0), blocks.GetLength(1), blocks.GetLength(2));
		currentStructureName = sd.name;
		pivot = sd.pivot;
		FixValues();
		BuildMesh();
	}
	public void Bucket(int method, bool forced = false)
	{

		switch (method)
		{
			case 0:
				{
					for (int y = 0; y < structureScale.y; y++)
					{
						for (int z = 0; z < structureScale.z; z++)
						{
							if (forced || blocks[((int)targetBlock.x), y, z].BlockID == (short)DefaultBlocks.AIR)
							{
								SetBlock(((int)targetBlock.x), y, z);
							}
						}
					}
					break;
				}
			case 1:
				{
					for (int x = 0; x < structureScale.x; x++)
					{
						for (int z = 0; z < structureScale.z; z++)
						{
							if (forced || blocks[x, ((int)targetBlock.y), z].BlockID == (short)DefaultBlocks.AIR)
							{
								SetBlock(x, ((int)targetBlock.y), z);
							}
						}
					}
					break;
				}
			case 2:
				{
					for (int y = 0; y < structureScale.y; y++)
					{
						for (int x = 0; x < structureScale.x; x++)
						{
							if (forced || blocks[x, y, ((int)targetBlock.x)].BlockID == (short)DefaultBlocks.AIR)
							{
								SetBlock(x, y, ((int)targetBlock.x));
							}
						}
					}
					break;
				}
		}
	}
	public void Line(int method, bool forced = false)
	{

		switch (method)
		{
			case 0:
				{
					for (int x = 0; x < structureScale.x; x++)
					{
						if (forced || blocks[x, ((int)targetBlock.y), ((int)targetBlock.z)].BlockID == (short)DefaultBlocks.AIR)
						{
							SetBlock(x, ((int)targetBlock.y), ((int)targetBlock.z));
						}
					}
					break;
				}
			case 1:
				{
					for (int y = 0; y < structureScale.y; y++)
					{
						if (forced || blocks[((int)targetBlock.x), y, ((int)targetBlock.z)].BlockID == (short)DefaultBlocks.AIR)
						{
							SetBlock(((int)targetBlock.x), y, ((int)targetBlock.z));
						}
					}
					break;
				}
			case 2:
				{
					for (int z = 0; z < structureScale.z; z++)
					{
						if (forced || blocks[((int)targetBlock.x), ((int)targetBlock.y), z].BlockID == (short)DefaultBlocks.AIR)
						{
							SetBlock(((int)targetBlock.x), ((int)targetBlock.y), z);
						}
					}
					break;
				}
		}
	}
	public void ShrinkSize(int method)
	{
		Vector3Int newsize = new Vector3Int(blocks.GetLength(0) - ((method == 0) ? 1 : 0), blocks.GetLength(1) - ((method == 1) ? 1 : 0), blocks.GetLength(2) - ((method == 2) ? 1 : 0));
		if (newsize.x < 1) newsize.x = 1;
		if (newsize.y < 1) newsize.y = 1;
		if (newsize.z < 1) newsize.z = 1;

		ChunkBlock[,,] newBlocks = new ChunkBlock[newsize.x, newsize.y, newsize.z];
		for (int x = 0; x < newBlocks.GetLength(0); x++)
		{
			for (int y = 0; y < newBlocks.GetLength(1); y++)
			{
				for (int z = 0; z < newBlocks.GetLength(2); z++)
				{
					newBlocks[x, y, z] = blocks[x, y, z];
				}
			}
		}
		structureScale.x = newBlocks.GetLength(0);
		structureScale.y = newBlocks.GetLength(1);
		structureScale.z = newBlocks.GetLength(2);
		blocks = newBlocks;
		BuildMesh();
	}
	public void ExpandSize(int method)
	{
		ChunkBlock[,,] newBlocks = new ChunkBlock[blocks.GetLength(0) + ((method == 0) ? 1 : 0), blocks.GetLength(1) + ((method == 1) ? 1 : 0), blocks.GetLength(2) + ((method == 2) ? 1 : 0)];
		for (int x = 0; x < blocks.GetLength(0); x++)
		{
			for (int y = 0; y < blocks.GetLength(1); y++)
			{
				for (int z = 0; z < blocks.GetLength(2); z++)
				{
					newBlocks[x, y, z] = blocks[x, y, z];
				}
			}
		}
		structureScale.x = newBlocks.GetLength(0);
		structureScale.y = newBlocks.GetLength(1);
		structureScale.z = newBlocks.GetLength(2);
		blocks = newBlocks;

		BuildMesh();
	}
}