using System.Collections.Generic;
using Unity.Mathematics;
using Unity.VisualScripting;
using UnityEngine;

public class PlayerHand : MonoBehaviour
{
    Camera playerCamera;
	[SerializeField] Transform targetBlock;
	[SerializeField] float handReach;
	[SerializeField] Vector2 highlightRange;
	[SerializeField] float highlightSpeed;
	[SerializeField] Texture2D[] breakStages;

	WorldManager world;
	MeshRenderer targetBlockMeshRenderer;
	float highT;
	List<Vector3Int> positions = new();

	BlockData blockBreaking;
	private void Awake()
	{
		playerCamera = GetComponent<Camera>();
		targetBlockMeshRenderer = targetBlock.GetComponent<MeshRenderer>();
		world = WorldManager.Instance;
	}
	void Update()
	{
		if (Player.player.inUI) return;

		highT += highlightSpeed * Time.deltaTime;
		float prog = (math.sin(highT) + 1) / 2f;
		targetBlockMeshRenderer.material.SetFloat("_HighlightOpacity", math.lerp(highlightRange.x, highlightRange.y, prog));
		targetBlock.gameObject.SetActive(false);
		float val = Input.GetAxisRaw("Mouse ScrollWheel") * -10;
		if (val != 0)
		{
			PlayerStorage.Instance.selectedHotbarSlot += (int)val;
		}

		const float increment = 0.1f;
		Vector3 offset = Vector3.zero;
		Vector3Int lastPosition = Vector3Int.FloorToInt(playerCamera.transform.position);

		int action = -1;

		if (Input.GetKey(KeyCode.Mouse0)) action = 0;
		else if (Input.GetKeyDown(KeyCode.Mouse1)) action = 1;


		positions.Clear();
		positions.Add(lastPosition);
		while (offset.magnitude < handReach)
		{
			Vector3 nudge = playerCamera.transform.forward * increment;
			offset += nudge;
			Vector3Int currentPosition = Vector3Int.FloorToInt(playerCamera.transform.position + offset);
			positions.Add(currentPosition);
			if (world.IsBlockAtPosition(currentPosition).Item1)
			{
				BlockData currentBlock = world.GetBlockAtPosition(currentPosition).Value;
				if (action == 0)//break
				{
					if (currentBlock.BlockName == blockBreaking.BlockName)
					{
						if (currentBlock.blockBehaviour.Break())
						{
							(bool broke, short blockID) = world.SetBlockAtPosition(currentPosition, 0, true, true);
							if (broke)
							{
								PlayerStorage.Instance.GiveItem(world.blocks[blockID].ItemToGive);
							}
						}
					}
					else
					{
						blockBreaking = world.GetBlockAtPosition(currentPosition).Value;
						BlockBehaviour.t = 0;
					}
				}
				else if (action == 1)// Build-Interact
				{
					BlockBehaviour.t = 0;
					if (Input.GetKey(KeyCode.LeftShift) || !currentBlock.blockBehaviour.Interact())
					{
						if (!Player.player.playerBlockedBlocks.Contains(lastPosition))
						{
							short blockIndex = PlayerStorage.Instance.GetPlacableBlock();
							if(blockIndex >= 0)
							{
								world.SetBlockAtPosition(lastPosition, blockIndex, true);
								PlayerStorage.Instance.PlacedHotbarBlock();
							}
						}
					}
				}
				targetBlock.gameObject.SetActive(true);
				targetBlock.position = currentPosition + (Vector3.one * 0.5f);

				break;
			}
			lastPosition = currentPosition;

			if (action == -1)
			{
				BlockBehaviour.t = 0;
			}
		}
		int stage = 0;
		try
		{
			stage = (int)(((BlockBehaviour.t / blockBreaking.blockBehaviour.BreakTime) + 0.1f) * 4);
			targetBlockMeshRenderer.material.SetTexture("_MainTex", breakStages[stage]);

		}
		catch {
			targetBlockMeshRenderer.material.SetTexture("_MainTex", breakStages[0]);
		}

	}

	private void OnDrawGizmos()
	{
		foreach (var pos in positions)
		{
			Gizmos.DrawSphere(pos, 0.125f);
		}
	}

	private void OnGUI()
	{
		short gpb = PlayerStorage.Instance.GetPlacableBlock();
		if (gpb >= 0)
		{
			string[] nameFields = world.blocks[gpb].BlockName.Split('.');
			GUI.Label(new Rect(19f, 30f, 500, 500), $"Item: {nameFields[nameFields.Length - 1].FirstCharacterToUpper()}");
		}
	}
}
