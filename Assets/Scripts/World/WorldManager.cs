using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WorldManager : MonoBehaviour
{
	[Range(0, 1), SerializeField] float globalLightLevel;
	[Range(0, 1), SerializeField] float minGlobalLightLevel;
	[Range(0, 1), SerializeField] float maxGlobalLightLevel;


	static WorldManager instance;
	public static WorldManager Instance
	{
		get
		{
			if (instance == null)
			{
				instance = FindObjectOfType<WorldManager>();
			}
			return instance;
		}
	}

	public BlockData[] blocks;

	public Material[] terrainMaterials;


	[SerializeField] Transform player;
	public int renderDistance;
	[SerializeField] int seed, surfaceLevel, amplitude;
	[SerializeField] float frequency, caveFrequency, noodleFrequency;
	[SerializeField, Range(0, 1)] float cavesValue, cavesRange;
	[SerializeField, Range(0, 1)] float noodlesRange;

	[HideInInspector] public Dictionary<Vector2Int, Chunk> chunks = new Dictionary<Vector2Int, Chunk>();

	Vector2Int playerPos = Vector2Int.one * 100;

	List<Vector2Int> activeChunks = new();
	List<Chunk> newChunks = new();
	bool generatingNewChunks;

	[SerializeField] float tmp;

	public AnimationCurve Erosion;

	public Queue<CustomStructureData> queuedStructures = new Queue<CustomStructureData>();
	public Queue<Vector3Int> queuedStructuresPlaces = new Queue<Vector3Int>();
	bool placingStructures;

	private void Awake()
	{
		WorldTable.Init();
	}
	void Start()
	{
		RenderSettings.fogStartDistance = renderDistance * WorldTable.chunkSize.x * 0.6f;
		RenderSettings.fogEndDistance = renderDistance * WorldTable.chunkSize.x * 0.7f;
		if(seed == -1)
		{
			seed = Random.Range(1, 9999999);
		}

		System.Diagnostics.Stopwatch sw = new();
		Debug.Log("Starting world generation!");
		for (int z = -renderDistance; z < renderDistance; z++)
		{
			for (int x = -renderDistance; x < renderDistance; x++)
			{
				sw.Reset();
				sw.Start();
				Vector2Int offset = new Vector2Int(x, z);
				Chunk chunk = new Chunk(offset, this, WorldTable.chunkSize, seed, surfaceLevel, amplitude, frequency, caveFrequency, noodleFrequency, cavesValue, cavesRange, noodlesRange);
				chunks.Add(offset, chunk);
				sw.Stop();
				Debug.Log($"Finished generating Data for chunk {offset} in {sw.ElapsedMilliseconds}ms!");
			}
		}
		Debug.Log("Finished generating data for starting area!");

		//for (int z = -renderDistance; z < renderDistance; z++)
		//{
		//	for (int x = -renderDistance; x < renderDistance; x++)
		//	{
		//		sw.Reset();
		//		sw.Start();
		//		Vector2Int offset = new Vector2Int(x, z);
		//		chunks[offset].GenerateLighting();
		//		sw.Stop();
		//		Debug.Log($"Finished generating Lights for chunk {offset} in {sw.ElapsedMilliseconds}ms!");
		//	}
		//}
		//Debug.Log("Finished generating lighting for starting area!");

		for (int z = -renderDistance; z < renderDistance; z++)
		{
			for (int x = -renderDistance; x < renderDistance; x++)
			{
				sw.Reset();
				sw.Start();
				Vector2Int offset = new Vector2Int(x, z);
				chunks[offset].GenerateLighting();
				chunks[offset].GenerateChunkMesh();
				sw.Stop();
				Debug.Log($"Finished generating Mesh for chunk {offset} in {sw.ElapsedMilliseconds}ms!");
			}
		}
		Debug.Log("Finished generating meshes for starting area!");

		Vector3Int spawnPoint = new Vector3Int(8, 0, 8);

		for (int y = WorldTable.chunkSize.y - 1; y > 0; y--)
		{
			if (chunks[Vector2Int.zero].GetBlock(new Vector3Int(8, y, 8)).BlockID != (short)DefaultBlocks.AIR)
			{
				spawnPoint.y = y + 1;
				break;
			}
		}
		player.position = spawnPoint + new Vector3(0.5f, 0, 0.5f);

		if (queuedStructures.Count > 0)
		{
			StartCoroutine("PlaceStructures");
		}
	}
	private void Update()
	{
		Shader.SetGlobalFloat("GlobalLightLevel", globalLightLevel);
		Shader.SetGlobalFloat("minGlobalLightLevel", minGlobalLightLevel);
		Shader.SetGlobalFloat("maxGlobalLightLevel", maxGlobalLightLevel);
	}
	IEnumerator PlaceStructures()
	{
		placingStructures = true;
		while(queuedStructures.Count > 0)
		{
			CustomStructureData customStructure = queuedStructures.Dequeue();
			Vector3Int customStructurePlace = queuedStructuresPlaces.Dequeue();

			List<Chunk> affectedChunks = new List<Chunk>();

			Debug.Log($"Placing {customStructure.name} at {customStructurePlace}");
			if (customStructure.forceAir)
			{
				for (int x = 0; x < customStructure.data.GetLength(0); x++)
				{
					for (int z = 0; z < customStructure.data.GetLength(2); z++)
					{
						for (int y = 0; y < customStructure.data.GetLength(1); y++)
						{
							Vector3Int placePos = customStructurePlace + new Vector3Int(x, y, z) - customStructure.pivot;
							if (!SetBlockAtPosition(placePos, customStructure.data[x, y, z].BlockID).Item1)
							{
								queuedStructures.Enqueue(customStructure);
								queuedStructuresPlaces.Enqueue(customStructurePlace);
							}
							else
							{
								if (TryGetChunkIndexAtPoint(placePos, out Vector2Int affectedChunk, out Vector3Int tmp))
								{
									if (!affectedChunks.Contains(chunks[affectedChunk]))
										affectedChunks.Add(chunks[affectedChunk]);
								}
							}
						}
					}
				}
			}
			else
			{
				for (int x = 0; x < customStructure.data.GetLength(0); x++)
				{
					for (int z = 0; z < customStructure.data.GetLength(2); z++)
					{
						for (int y = 0; y < customStructure.data.GetLength(1); y++)
						{
							if(customStructure.data[x, y, z].BlockID == (short)DefaultBlocks.AIR) continue;

							Vector3Int placePos = customStructurePlace + new Vector3Int(x, y, z) - customStructure.pivot;

							if (!SetBlockAtPosition(placePos, customStructure.data[x, y, z].BlockID).Item1)
							{
								queuedStructures.Enqueue(customStructure);
								queuedStructuresPlaces.Enqueue(customStructurePlace);
							}
							else
							{
								if (TryGetChunkIndexAtPoint(placePos, out Vector2Int affectedChunk, out Vector3Int tmp))
								{
									if (!affectedChunks.Contains(chunks[affectedChunk]))
										affectedChunks.Add(chunks[affectedChunk]);
								}
							}
						}
					}
				}
			}

			for (int a = 0; a < affectedChunks.Count; a++)
			{
				affectedChunks[a].GenerateLighting();
				affectedChunks[a].GenerateChunkMesh();
				yield return new WaitForSeconds(Time.deltaTime);
			}

			yield return new WaitForSeconds(Time.deltaTime);
		}
		placingStructures = false;
	}
	private void LateUpdate()
	{
		Vector2Int viewerChunkCoord = new Vector2Int(
			Mathf.FloorToInt((float)player.position.x / (float)WorldTable.chunkSize.x),
			Mathf.FloorToInt((float)player.position.z / (float)WorldTable.chunkSize.x));

		if(viewerChunkCoord != playerPos)
		{
			for (int z = -renderDistance; z < renderDistance; z++)
			{
				for (int x = -renderDistance; x < renderDistance; x++)
				{
					Vector2Int offset = viewerChunkCoord + new Vector2Int(x, z);
					float distance = new Vector2Int(x, z).magnitude;
					bool active = distance <= (renderDistance);

					if (chunks.ContainsKey(offset))
					{
						chunks[offset].meshFilter.gameObject.SetActive(active);
					}
					else
					{
						Chunk chunk = new Chunk(offset, this, WorldTable.chunkSize, seed, surfaceLevel, amplitude, frequency, caveFrequency, noodleFrequency, cavesValue, cavesRange, noodlesRange);
						chunks.Add(offset, chunk);
						newChunks.Add(chunk);
						chunks[offset].meshFilter.gameObject.SetActive(active);
					}
					if (active && !activeChunks.Contains(offset))
					{
						activeChunks.Add(offset);
					}
				}
			}

			int _of = 0;
			int _ind = 0;
			while (_ind < activeChunks.Count)
			{
				if (Vector2.Distance(activeChunks[_ind + _of], viewerChunkCoord) <= renderDistance + tmp)
				{
					chunks[activeChunks[_ind + _of]].meshFilter.gameObject.SetActive(true);
					if (chunks[activeChunks[_ind + _of]].needsUpdate)
					{
						newChunks.Add(chunks[activeChunks[_ind + _of]]);
						Debug.Log($"updated for mesh {chunks[activeChunks[_ind + _of]].chunkIndex}");
					}
				}
				else
				{
					chunks[activeChunks[_ind + _of]].meshFilter.gameObject.SetActive(false);
					activeChunks.RemoveAt(_ind + _of);
					_of--;
				}
				_ind++;
			}


			if (newChunks.Count > 0 && !generatingNewChunks)
			{
				StartCoroutine("GenerateMeshes");
			}

			if (queuedStructures.Count > 0 && !placingStructures)
			{
				StartCoroutine("PlaceStructures");
			}
		}

		playerPos = viewerChunkCoord;
	}
	IEnumerator GenerateMeshes()
	{
		generatingNewChunks = true;
		while (newChunks.Count > 0)
		{

			newChunks.Sort((chunkA, chunkB) =>
			{
				float distanceA = Vector2.Distance(playerPos, chunkA.chunkIndex);
				float distanceB = Vector2.Distance(playerPos, chunkB.chunkIndex);
				return distanceA.CompareTo(distanceB);
			});

			newChunks[0].GenerateLighting();
			newChunks[0].GenerateChunkMesh();

			newChunks.RemoveAt(0);
			yield return new WaitForSeconds(Time.deltaTime);
		}
		generatingNewChunks = false;
	}
	public (bool, bool) IsBlockAtPosition(float x, float y, float z)
	{
		return IsBlockAtPosition(Vector3Int.FloorToInt(new Vector3(x, y, z)));
	}
	public (bool, bool) IsBlockAtPosition(Vector3Int position)
	{
		if (position.y < 0 || position.y >= WorldTable.chunkSize.y)
			return (false, true);

		Vector2Int chunkIndex = new Vector2Int(
			Mathf.FloorToInt((float)position.x / (float)WorldTable.chunkSize.x),
			Mathf.FloorToInt((float)position.z / (float)WorldTable.chunkSize.x)
		);

		position.x = position.x - chunkIndex.x * WorldTable.chunkSize.x;
		position.z = position.z - chunkIndex.y * WorldTable.chunkSize.x;

		if (chunks.TryGetValue(chunkIndex, out Chunk targetChunk))
		{
			return (blocks[targetChunk.GetBlock(position).BlockID].isSolid, true);
		}
		else
		{
			return (true, false);
		}
	}
	public (bool, bool) IsOpaqueBlockAtPosition(Vector3Int position)
	{
		if (position.y < 0 || position.y >= WorldTable.chunkSize.y)
			return (false, true);

		Vector2Int chunkIndex = new Vector2Int(
			Mathf.FloorToInt((float)position.x / (float)WorldTable.chunkSize.x),
			Mathf.FloorToInt((float)position.z / (float)WorldTable.chunkSize.x)
		);

		position.x = position.x - chunkIndex.x * WorldTable.chunkSize.x;
		position.z = position.z - chunkIndex.y * WorldTable.chunkSize.x;

		if (chunks.TryGetValue(chunkIndex, out Chunk targetChunk))
		{
			return (!blocks[targetChunk.GetBlock(position).BlockID].isTransparent, true);
		}
		else
		{
			return (false, false);
		}
	}
	public (byte, bool) GetLightAt(Vector3Int position)
	{
		if (position.y < 0 || position.y >= WorldTable.chunkSize.y)
			return (WorldTable.LightLevels, true);

		Vector2Int chunkIndex = new Vector2Int(
			Mathf.FloorToInt((float)position.x / (float)WorldTable.chunkSize.x),
			Mathf.FloorToInt((float)position.z / (float)WorldTable.chunkSize.x)
		);

		position.x = position.x - chunkIndex.x * WorldTable.chunkSize.x;
		position.z = position.z - chunkIndex.y * WorldTable.chunkSize.x;

		if (chunks.TryGetValue(chunkIndex, out Chunk targetChunk))
		{
			return (targetChunk.GetBlock(position).LightLevel, true);
		}
		else
		{
			return (WorldTable.LightLevels, false);
		}
	}
	public (bool, short) SetBlockAtPosition(Vector3Int position, short block, bool update = false, bool updateAround = false)
	{
		if (position.y < 0 || position.y >= WorldTable.chunkSize.y)
			return (true, (short)0);

		Vector2Int chunkIndex = new Vector2Int(
			Mathf.FloorToInt((float)position.x / (float)WorldTable.chunkSize.x),
			Mathf.FloorToInt((float)position.z / (float)WorldTable.chunkSize.x)
		);

		position.x = position.x - chunkIndex.x * WorldTable.chunkSize.x;
		position.z = position.z - chunkIndex.y * WorldTable.chunkSize.x;

		if (chunks.TryGetValue(chunkIndex, out Chunk targetChunk))
		{
			Debug.Log($"changing block at chunk {chunkIndex}, block {position} to {block}");

			short wasBlock = targetChunk.GetBlock(position).BlockID;

			targetChunk.SetBlock(position, block, update);
			
			if (updateAround)
			{
				foreach (var side in WorldTable.SimpleSidesTable3D)
				{
					if (position.x + side.x > WorldTable.chunkSize.x - 1 || position.x + side.x < 0 || position.z + side.z > WorldTable.chunkSize.x - 1 || position.z + side.z < 0)
					{
						Vector2Int newIndex = chunkIndex + new Vector2Int(side.x, side.z);
						if (chunks.TryGetValue(newIndex, out Chunk nextChunk))
						{
							nextChunk.GenerateLighting();
							nextChunk.GenerateChunkMesh();
						}
					}
				}
			}
			return (true, wasBlock);
		}

		return (false, -1);
	}
	public BlockData? GetBlockAtPosition(Vector3Int position)
	{
		if (position.y < 0 || position.y >= WorldTable.chunkSize.y)
			return null;

		Vector2Int chunkIndex = new Vector2Int(
			Mathf.FloorToInt((float)position.x / (float)WorldTable.chunkSize.x),
			Mathf.FloorToInt((float)position.z / (float)WorldTable.chunkSize.x)
		);

		position.x = position.x - chunkIndex.x * WorldTable.chunkSize.x;
		position.z = position.z - chunkIndex.y * WorldTable.chunkSize.x;

		if (chunks.TryGetValue(chunkIndex, out Chunk targetChunk))
		{
			return blocks[targetChunk.GetBlock(position).BlockID];
		}

		return null;
	}
	public bool TryGetChunkIndexAtPoint(Vector3Int position, out Vector2Int outChunkIndex, out Vector3Int posInChunk)
	{
		if (position.y < 0 || position.y >= WorldTable.chunkSize.y)
		{
			outChunkIndex = Vector2Int.zero;
			posInChunk = Vector3Int.zero;
			return false;
		}

		Vector2Int chunkIndex = new Vector2Int(
			Mathf.FloorToInt((float)position.x / (float)WorldTable.chunkSize.x),
			Mathf.FloorToInt((float)position.z / (float)WorldTable.chunkSize.x)
		);
		position.x = position.x - chunkIndex.x * WorldTable.chunkSize.x;
		position.z = position.z - chunkIndex.y * WorldTable.chunkSize.x;

		outChunkIndex = chunkIndex;
		posInChunk = position;
		return true;
	}
}
