using System.Collections.Generic;
using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEngine;

public class Chunk
{
	public MeshFilter meshFilter;
	public Vector2Int chunkIndex;
	WorldManager world;
	public bool needsUpdate;
	ChunkBlock[,,] chunkData;
	public Vector2Int chunkSize;
	Vector2Int offset
	{
		get
		{
			return chunkIndex * chunkSize.x;
		}
	}


	public Chunk(Vector2Int ind, WorldManager world, Vector2Int chunkSize, int seed, int surfaceLevel, int amplitude, float frequency, float caveFrequency, float noodleFrequency, float cavesValue, float cavesRange, float noodlesRange)
	{
		this.world = world;

		this.chunkIndex = ind;

		GameObject chunk = new GameObject($"chunk{ind}");
		chunk.transform.position = new Vector3(ind.x, 0, ind.y) * chunkSize.x;
		chunk.transform.parent = world.transform;
		meshFilter = chunk.AddComponent<MeshFilter>();
		chunk.AddComponent<MeshRenderer>().materials = world.terrainMaterials;

		chunkData = new ChunkBlock[chunkSize.x, chunkSize.y, chunkSize.x];
		this.chunkSize = chunkSize;

		GenerateNoise(seed, surfaceLevel, amplitude, frequency, caveFrequency, noodleFrequency, cavesValue, cavesRange, noodlesRange);

	}
	void GenerateNoise(int seed, int surfaceLevel, int amplitude, float frequency, float caveFrequency, float noodleFrequency, float cavesValue, float cavesRange, float noodlesRange)
	{
		using (var nmap = new NativeArray<short>(chunkData.Length, Allocator.TempJob))
		{
			System.Random rand = new System.Random(seed + offset.x + offset.y * 16);
			var gnm = new GenerateNoiseMap
			{
				seed = seed,
				surfaceLevel = surfaceLevel,
				amplitude = amplitude,
				chunkSize = new int2(chunkData.GetLength(0), chunkData.GetLength(1)),
				Offset = new int2(offset.x, offset.y),
				chunkData = nmap,
				frequency = frequency,
				caveFrequency = caveFrequency,
				noodleFrequency = noodleFrequency,
				c_Range = cavesRange,
				c_Value = cavesValue,
				nc_Range = noodlesRange,
				random = new Unity.Mathematics.Random((uint)rand.Next()),
				CavesDip = WorldTable.CavesHeight,
			};

			JobHandle h = gnm.Schedule(chunkData.Length, 128);
			h.Complete();

			chunkData = ArraysHelper.Convert1DTo3D_CB(nmap.ToArray(), chunkData.GetLength(0), chunkData.GetLength(1), chunkData.GetLength(2));
		}
	}


	public void GenerateLighting()
	{
		chunkData = LightEngine.CalculateLights(chunkData);
	}
	public void GenerateChunkMesh(bool updateLighting = false)
	{
		if(updateLighting)
			chunkData = LightEngine.CalculateLights(chunkData);

		needsUpdate = false;
		(List<Vector3> verts, List<Color> colors, List<Vector2> uvs, List<int> tris, List<int> trisTrans) = GenerateMesh();

		Mesh mesh = new Mesh();
		mesh.subMeshCount = 2;
		mesh.vertices = verts.ToArray();
		mesh.uv = uvs.ToArray();
		mesh.colors = colors.ToArray();
		mesh.SetTriangles(tris.ToArray(), 0);
		mesh.SetTriangles(trisTrans.ToArray(), 1);
		mesh.Optimize();
		mesh.RecalculateNormals();
		meshFilter.sharedMesh = mesh;

	}

	(List<Vector3>, List<Color>,List<Vector2>, List<int>, List<int>) GenerateMesh()
	{
		List<Vector3> verts = new();
		List<Color> colors = new();
		List<Vector2> uvs = new();
		List<int> tris = new();
		List<int> trisTrans = new();

		int triIndex = 0;


		int stLength = WorldTable.SidesTable.Length;

		for (int x = 0; x < chunkSize.x; x++)
		{
			for (int z = 0; z < chunkSize.x; z++)
			{
				for (int y = 0; y < chunkSize.y; y++)
				{
					short cd = chunkData[x, y, z].BlockID;

					BlockData bd = world.blocks[cd];
					if (bd.isSolid)
					{
						Vector3Int pos = new Vector3Int(x, y, z);
						int tricount = WorldTable.blocks[bd.BlockModel].triangleTable.GetLength(1);
						int vertCount = WorldTable.blocks[bd.BlockModel].Faces.GetLength(1);
						int[] textureID = bd.Textures;
						for (int side = 0; side < stLength; side++)
						{
							Vector3Int sidePos = pos + WorldTable.SidesTable[side];

							bool isOut = sidePos.x >= chunkSize.x || sidePos.x < 0 ||
										sidePos.z >= chunkSize.x || sidePos.z < 0 ||
										sidePos.y >= chunkSize.y || sidePos.y < 0;
							bool drawFace = true;
							Color sideBrightness = new Color(0, 0, 0, 1f);

							if (!isOut)
							{
								drawFace = world.blocks[chunkData[sidePos.x, sidePos.y, sidePos.z].BlockID].isTransparent;
								sideBrightness.a = chunkData[sidePos.x, sidePos.y, sidePos.z].LightLevel / (float)WorldTable.LightLevels;

							}
							else
							{
								(bool opaque, bool properly) = world.IsOpaqueBlockAtPosition(sidePos + new Vector3Int(offset.x, 0, offset.y));
								if (!properly) needsUpdate = true;
								drawFace = !opaque;
								(byte LL, bool properlyLit) = world.GetLightAt(sidePos + new Vector3Int(offset.x, 0, offset.y));
								if (!properlyLit) needsUpdate = true;
							}

							if (drawFace)
							{

								for (int i = 0; i < vertCount; i++)
								{
									verts.Add(WorldTable.blocks[bd.BlockModel].Faces[side, i] + pos);
									colors.Add(sideBrightness);//TODO: set the R channel to the amount of faces at this corner
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
								triIndex += vertCount;
							}
						}
					}
				}
			}
		}

		return (verts, colors, uvs, tris, trisTrans);
	}

	public ChunkBlock GetBlock(Vector3Int pos)
	{
		return chunkData[pos.x, pos.y, pos.z];
	}
	public ChunkBlock[,,] GetBlocks()
	{
		return chunkData;
	}
	public void SetBlocks(ChunkBlock[,,] newBlocks)
	{
		chunkData = newBlocks;
	}
	public void SetBlock(Vector3Int pos, short block, bool update = false)
	{
		chunkData[pos.x, pos.y, pos.z].BlockID = block;
		GenerateLighting();
		if (update)
			GenerateChunkMesh();
	}
}


[BurstCompile]
public struct GenerateNoiseMap : IJobParallelFor
{
	public int seed;
	public int surfaceLevel;
	public int amplitude;
	public int2 chunkSize;
	public int2 Offset;
	[WriteOnly] public NativeArray<short> chunkData;
	public float frequency;
	public float caveFrequency;
	public float noodleFrequency;
	public float c_Range;
	public float c_Value;
	public float nc_Range;
	public Unity.Mathematics.Random random;
	public float CavesDip;

	public void Execute(int index)
	{
		int z = index / (chunkSize.x * chunkSize.y);
		int remainder = index % (chunkSize.x * chunkSize.y);
		int y = remainder / chunkSize.x;
		int x = remainder % chunkSize.x;

		if (y == 0)
		{
			chunkData[index] = (short)DefaultBlocks.BEDROCK;
		}
		else
		{
			float fbm = GetSurface(x + Offset.x, z + Offset.y, 4);
			int targetHeight = (int)(amplitude * fbm + surfaceLevel);

			if (y <= targetHeight)
			{
				bool isNoodle = GetNoodleValue(x + Offset.x, y, z + Offset.y, 1, nc_Range);
				bool isCave = math.distance(GetCaveValue(x + Offset.x, y, z + Offset.y, 3, targetHeight), c_Value) <= c_Range;

				if (!isCave && !isNoodle)
				{
					if (y == targetHeight)
					{
						chunkData[index] = (short)DefaultBlocks.GRASS;

						if (random.NextInt(0, 10) < 3)
						{
							//WorldManager.Instance.queuedStructures.Enqueue(WorldTable.structures[0]);
							//WorldManager.Instance.queuedStructuresPlaces.Enqueue(new Vector3Int(x + Offset.x, y, z + Offset.y));
						}
					}
					else if (y < surfaceLevel / 2f)
						chunkData[index] = (short)DefaultBlocks.DEEPSLATE;
					else if (y < (surfaceLevel / 2f) + 16)
						chunkData[index] = (short)(random.NextBool() ? DefaultBlocks.DEEPSLATE : DefaultBlocks.STONE);
					else if (y < targetHeight - 3)
						chunkData[index] = (short)DefaultBlocks.STONE;
					else if (y < targetHeight)
						chunkData[index] = (short)DefaultBlocks.DIRT;
				}
			}
		}
	}

	float GetNoise2D(float x, float y, float freq, int octaves)
	{
		float fbm = PerlinNoise.Fbm((float)(x + seed) / (float)freq, (float)(y + seed) / (float)freq, octaves);

		return fbm;
	}
	float GetNoise3D(float x, float y, float z, int octaves)
	{
		float fbm1 = (PerlinNoise.Fbm((float)(x + seed) / (float)frequency, (float)(y + seed) / (float)frequency, (float)(z + seed) / (float)frequency, octaves));//value between -1 and 1

		return fbm1;
	}

	float GetSurface(float x, float y, int octaves)
	{
		float fbm1 = (PerlinNoise.Fbm((float)(x + seed) / (float)frequency, (float)(y + seed) / (float)frequency, octaves));//value between -1 and 1
		float fbm2 = (PerlinNoise.Fbm((float)(x + seed) * 0.5f / (float)frequency, (float)(y + seed) * 0.5f / (float)frequency, octaves));//value between -1 and 1
		float fbm3 = (PerlinNoise.Fbm((float)(x + seed) * 0.25f / (float)frequency, (float)(y + seed) * 0.25f / (float)frequency, octaves));//value between -1 and 1

		fbm1 = (math.sin(fbm1) + fbm1) * math.cos(fbm1);

		return (fbm1 + fbm2 + fbm3) / 3f;
	}
	float GetTrees(float x, float y, int octaves)
	{
		float fbm1 = (PerlinNoise.Fbm((float)(x + seed + y/2f) / (float)frequency, (float)(y + seed + frequency) / (float)frequency, octaves));//value between -1 and 1

		return fbm1;
	}
	float GetCaveValue(float x, float y, float z, int octaves, float surface)
	{
		float fbm = PerlinNoise.Fbm((float)(x + seed) / (float)caveFrequency, (float)(y + seed) / (float)caveFrequency * 2f, (float)(z + seed) / (float)caveFrequency, octaves);

		float grad = 1 - (math.distance(y, surface / CavesDip) / surface);

		return fbm * grad * grad;
	}
	bool GetNoodleValue(float x, float y, float z, int octaves, float range)
	{
		float horizontal1 = GetNoise2D(x, z, noodleFrequency, octaves);
		float horizontal2 = GetNoise2D(z, x, noodleFrequency, octaves);
		float vertical1 = GetNoise2D(x, y, noodleFrequency, octaves);
		float vertical2 = GetNoise2D(y, z, noodleFrequency, octaves);


		if (math.distance(horizontal1, vertical1) > range) return false;
		if (math.distance(horizontal2, vertical2) > range) return false;

		return true;
	}
}