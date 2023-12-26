using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public static class WorldTable
{
	public static readonly string blocksPath = Path.Combine(Application.dataPath, "CustomData", "Blocks.json");
	public static readonly string structuresPath = Path.Combine(Application.dataPath, "CustomData", "Structures");

	public static readonly int imageGrid = 256;

	public static Vector2Int chunkSize = new Vector2Int(16, 256);
	public static float CavesHeight = 3f;
	public static BlockModel[] blocks;
	public static byte LightLevels = 15;

	public static  List<CustomStructureData> structures;

	public static void Init()
	{
		#region Load Blocks Models
		string blocksContents = File.ReadAllText(blocksPath);
		blocks = JsonConvert.DeserializeObject<BlockModel[]>(blocksContents);
		#endregion

		#region Load Structures
		structures = new List<CustomStructureData>();
		string[] files = Directory.GetFiles(structuresPath);
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
		#endregion

	}

	public static Vector3Int[] SidesTable = new Vector3Int[] {
		new Vector3Int(0, 1, 0),//Top
        new Vector3Int(0, -1, 0),//Bottom
        new Vector3Int(0, 0, 1),//Front
        new Vector3Int(0, 0, -1),//Back
        new Vector3Int(-1, 0, 0),//Left
        new Vector3Int(1, 0, 0),//Right
    };
	public static Vector3Int[] LightDirections = new Vector3Int[] {
		new Vector3Int(0, 1, 0),//Top
        new Vector3Int(0, -1, 0),//Bottom
        new Vector3Int(0, 0, 1),//Front
        new Vector3Int(0, 0, -1),//Back
        new Vector3Int(-1, 0, 0),//Left
        new Vector3Int(1, 0, 0),//Right
    };
	public static Vector2Int[] SimpleSidesTable = new Vector2Int[] {
		new Vector2Int(0, 1),//Front
        new Vector2Int(0, -1),//Back
        new Vector2Int(-1, 0),//Left
        new Vector2Int(1, 0),//Right
    };
	public static Vector3Int[] SimpleSidesTable3D = new Vector3Int[] {
		new Vector3Int(0, 0, 1),//Front
        new Vector3Int(0, 0, -1),//Back
        new Vector3Int(-1, 0, 0),//Left
        new Vector3Int(1, 0, 0),//Right
	};



	#region Deprecated
	[Obsolete("This method is Deprecated", true)]
	public static void Save()
	{
		blocks = new BlockModel[1];
		blocks[0].Faces = Faces;
		blocks[0].UVS = UVS;

		var settings = new JsonSerializerSettings()
		{
			ReferenceLoopHandling = ReferenceLoopHandling.Ignore,
			Formatting = Formatting.Indented
		};
		string contents = JsonConvert.SerializeObject(blocks, settings);
		File.WriteAllText(blocksPath, contents);
	}
	
	[Obsolete("Handling FaceTriangleTable has been moved to block model", true)]
	static int[] FaceTriangleTable = new int[] {
		0, 1, 2, 0, 2, 3
	};
	[Obsolete("Handling Faces has been moved to block model", true)]
	static Vector3[,] Faces = new Vector3[,]
	{
		{ new Vector3(0, 1, 0), new Vector3(0, 1, 1), new Vector3(1, 1, 1), new Vector3(1, 1, 0)},//Top
		{ new Vector3(0, 0, 0), new Vector3(1, 0, 0), new Vector3(1, 0, 1), new Vector3(0, 0, 1)},//Bottom
		{ new Vector3(0, 0, 1), new Vector3(1, 0, 1), new Vector3(1, 1, 1), new Vector3(0, 1, 1)},//Front
		{ new Vector3(0, 0, 0), new Vector3(0, 1, 0), new Vector3(1, 1, 0), new Vector3(1, 0, 0)},//Back
		{ new Vector3(0, 0, 1), new Vector3(0, 1, 1), new Vector3(0, 1, 0), new Vector3(0, 0, 0)},//Left
		{ new Vector3(1, 0, 1), new Vector3(1, 0, 0), new Vector3(1, 1, 0), new Vector3(1, 1, 1)},//Right
    };
	[Obsolete("Handling UVS has been moved to block model", true)]
	static Vector2[,] UVS = new Vector2[,]
	{
		{ new Vector2(0, 0), new Vector2(0, 1), new Vector2(1, 1), new Vector2(1, 0)},//Top
        { new Vector2(0, 0), new Vector2(1, 0), new Vector2(1, 1), new Vector2(0, 1)},//Bottom
		{ new Vector2(0, 0), new Vector2(1, 0), new Vector2(1, 1), new Vector2(0, 1)},//Front
		{ new Vector2(0, 0), new Vector2(0, 1), new Vector2(1, 1), new Vector2(1, 0)},//Back
		{ new Vector2(0, 1), new Vector2(1, 1), new Vector2(1, 0), new Vector2(0, 0)},//Left
		{ new Vector2(0, 1), new Vector2(0, 0), new Vector2(1, 0), new Vector2(1, 1)},//Right
	};
	#endregion
}