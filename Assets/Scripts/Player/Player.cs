using System;
using System.Collections.Generic;
using UnityEngine;

public class Player : MonoBehaviour
{
	public static Player player;
	static readonly float gravity = -19.6f;

	public bool activated;

	public bool isGrounded;
	public bool isSprinting;

	public Transform cam;
	private WorldManager world;

	public float walkSpeed = 3f;
	public float sprintSpeed = 6f;
	public float jumpForce = 5f;

	public float  playerWidth = 0.15f;
	[Range(0.25f, 2.9f)]public float playerHeight = 1.8f;

	private float horizontal;
	private float vertical;
	private float mouseHorizontal;
	private float mouseVertical;
	private Vector3 velocity;
	private float verticalMomentum = 0;
	private bool jumpRequest;

	float xRotation;

	FreeCamera freeCamera;

	public List<Vector3Int> playerBlockedBlocks = new List<Vector3Int>();

	public PlayerUIWindow[] UIs;

	[SerializeField] KeyCode[] HotbarKeys;

	public bool inUI
	{
		get
		{
			return Cursor.visible;
		}
		set
		{
			Cursor.visible = value;
			Cursor.lockState = value ? CursorLockMode.None : CursorLockMode.Locked;
		}
	}

	private void Start()
	{
		world = WorldManager.Instance;
		player = this;

		freeCamera = GetComponent<FreeCamera>();
		freeCamera.enabled = !activated;

		inUI = false;
		OpenUI(UITypes.None);
	}

	private void FixedUpdate()
	{
		if (inUI) return;
		if (!activated) return;
		CalculateVelocity();
		if (jumpRequest)
			Jump();

		transform.Rotate(Vector3.up * mouseHorizontal);
		xRotation -= mouseVertical;
		xRotation= Mathf.Clamp(xRotation, -90, 90);
		cam.localRotation = Quaternion.Euler(xRotation, 0, 0);
		transform.Translate(velocity, Space.World);

	}

	private void Update()
	{
		if (inUI)
		{
			if (Input.GetKeyDown(KeyCode.Escape)) OpenUI(UITypes.None);
			return;
		}
		if(Input.GetKeyDown(KeyCode.Tab)) OpenUI(UITypes.Inventory);
		for (int i = 0; i < HotbarKeys.Length; i++)
		{
			if (Input.GetKeyDown(HotbarKeys[i]))
			{
				PlayerStorage.Instance.selectedHotbarSlot = i;
			}
		}
		cam.localPosition = new Vector3(cam.localPosition.x, playerHeight, cam.localPosition.z);
		if(activated)
		{
			GetPlayerInputs();
			GetBlockedArea();
		}
		if (Input.GetKeyDown(KeyCode.F4) && Input.GetKey(KeyCode.F3))
		{
			activated = !activated;
			freeCamera.enabled = !activated;
		}
	}

	void Jump()
	{
		verticalMomentum = Mathf.Sqrt(-2.0f * gravity * jumpForce);
		isGrounded = false;
		jumpRequest = false;
	}

	private void CalculateVelocity()
	{

		// Affect vertical momentum with gravity.
		if (verticalMomentum > gravity)
			verticalMomentum += Time.fixedDeltaTime * gravity;

		// if we're sprinting, use the sprint multiplier.
		if (isSprinting)
			velocity = ((transform.forward * vertical) + (transform.right * horizontal)) * Time.fixedDeltaTime * sprintSpeed;
		else
			velocity = ((transform.forward * vertical) + (transform.right * horizontal)) * Time.fixedDeltaTime * walkSpeed;

		// Apply vertical momentum (falling/jumping).
		velocity += Vector3.up * verticalMomentum * Time.fixedDeltaTime;

		ManageCollisions();

		if (isGrounded && !jumpRequest) verticalMomentum = 0;

	}
	void ManageCollisions()
	{
		Vector3[] downPoints = new Vector3[]
		{
			transform.position + new Vector3(playerWidth / 2f, -0.01f, playerWidth / 2f),
			transform.position + new Vector3(-playerWidth / 2f, -0.01f, playerWidth / 2f),
			transform.position + new Vector3(-playerWidth / 2f, -0.01f, -playerWidth / 2f),
			transform.position + new Vector3(playerWidth / 2f, -0.01f, -playerWidth / 2f),
		};
		bool collidedDown = false;
		foreach (var pos in downPoints)
		{
			if(world.IsBlockAtPosition(pos.x, pos.y, pos.z).Item1)
			{
				collidedDown = true;
				break;
			}
		}
		isGrounded = false;
		if (collidedDown)
		{
			transform.position = new Vector3(transform.position.x, Mathf.Round(transform.position.y), transform.position.z);
			if(velocity.y < 0) velocity.y = 0;
			isGrounded= true;
		}


		Vector3[] frontPoints = new Vector3[]
{
			transform.position + new Vector3(playerWidth / 2f, 0.5f, (playerWidth / 2f) + 0.01f),
			transform.position + new Vector3(playerWidth / 2f, 0.5f, (-playerWidth / 2f) + 0.01f),
			transform.position + new Vector3(playerWidth / 2f, (playerHeight / 2f) + 0.5f, (playerWidth / 2f) + 0.01f),
			transform.position + new Vector3(playerWidth / 2f, (playerHeight / 2f) + 0.5f, (-playerWidth / 2f) + 0.01f),
};
		bool collidedFront = false;
		foreach (var pos in frontPoints)
		{
			Debug.DrawLine(transform.position, pos, Color.white);
			Debug.DrawLine(transform.position, Vector3Int.CeilToInt(pos) - Vector3.one * 0.5f, Color.red);
			if (world.IsBlockAtPosition(pos.x, pos.y, pos.z).Item1)
			{
				collidedFront = true;
				break;
			}
		}
		if (collidedFront)
		{
			transform.position = new Vector3(transform.position.x, transform.position.y, Mathf.Round(transform.position.z + (playerWidth/2f))- (playerWidth / 2f));
			if (velocity.z > 0) velocity.z = 0;
		}

	}

	void GetBlockedArea()
	{
		playerBlockedBlocks.Clear();

		if (!player.activated) return;

		Vector3 fst = new Vector3();


		fst = new Vector3(transform.position.x - (playerWidth / 2f), transform.position.y + 0.5f, transform.position.z - (playerWidth / 2f));
		playerBlockedBlocks.Add(Vector3Int.FloorToInt(fst));

		fst = new Vector3(transform.position.x + (playerWidth / 2f), transform.position.y + 0.5f, transform.position.z - (playerWidth / 2f));

		playerBlockedBlocks.Add(Vector3Int.FloorToInt(fst));

		fst = new Vector3(transform.position.x + (playerWidth / 2f), transform.position.y + 0.5f, transform.position.z + (playerWidth / 2f));
		playerBlockedBlocks.Add(Vector3Int.FloorToInt(fst));

		fst = new Vector3(transform.position.x - (playerWidth / 2f), transform.position.y + 0.5f, transform.position.z + (playerWidth / 2f));
		playerBlockedBlocks.Add(Vector3Int.FloorToInt(fst));


		fst = new Vector3(transform.position.x - (playerWidth / 2f), transform.position.y + playerHeight, transform.position.z - (playerWidth / 2f));
		playerBlockedBlocks.Add(Vector3Int.FloorToInt(fst));											  
																										  
		fst = new Vector3(transform.position.x + (playerWidth / 2f), transform.position.y + playerHeight, transform.position.z - (playerWidth / 2f));
																										  
		playerBlockedBlocks.Add(Vector3Int.FloorToInt(fst));											  
																										  
		fst = new Vector3(transform.position.x + (playerWidth / 2f), transform.position.y + playerHeight, transform.position.z + (playerWidth / 2f));
		playerBlockedBlocks.Add(Vector3Int.FloorToInt(fst));											 
																										 
		fst = new Vector3(transform.position.x - (playerWidth / 2f), transform.position.y + playerHeight, transform.position.z + (playerWidth / 2f));
		playerBlockedBlocks.Add(Vector3Int.FloorToInt(fst));
	}

	private void GetPlayerInputs()
	{

		horizontal = Input.GetAxis("Horizontal");
		vertical = Input.GetAxis("Vertical");
		Vector2 v2 = new Vector2(horizontal, vertical).normalized;
		horizontal = v2.x;
		vertical = v2.y;
		mouseHorizontal = Input.GetAxis("Mouse X");
		mouseVertical = Input.GetAxis("Mouse Y");

		isSprinting = Input.GetKey(KeyCode.LeftShift);

		if (isGrounded && Input.GetButton("Jump"))
			jumpRequest = true;

	}

	private void OnGUI()
	{
		GUI.Label(new Rect(19f, 10f, 500, 500), $"FPS: {1f / Time.deltaTime}");
	}
	private void OnDrawGizmos()
	{
		Gizmos.DrawWireCube(transform.position + new Vector3(0, playerHeight/2f, 0), new Vector3(playerWidth, playerHeight, playerWidth));
	}

	public void OpenUI(UITypes uIType)
	{
		for (int i = 0; i < UIs.Length; i++)
		{
			UIs[i].uiObject.SetActive(uIType == UIs[i].uiType);
		}
		inUI = uIType != UITypes.None;
	}
}
[System.Serializable]
public struct PlayerUIWindow
{
	public UITypes uiType;
	public GameObject uiObject;
}

public enum UITypes
{
	Inventory,
	Crafting,
	None
}