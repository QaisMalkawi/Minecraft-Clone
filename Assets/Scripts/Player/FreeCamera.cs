using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FreeCamera : MonoBehaviour
{
    [SerializeField] Vector2 sensitivity;
    [SerializeField] float acceleration, maxSpeed, drag, sprintMultiplier, panSpeed;
    Vector3 velocity;
    Camera cam;

	private void Start()
	{
        cam = GetComponentInChildren<Camera>();
        RenderSettings.fogEndDistance = (WorldManager.Instance.renderDistance) * WorldTable.chunkSize.x - 10;
        RenderSettings.fogStartDistance = RenderSettings.fogEndDistance - 10f;
    }
    void Update()
    {
        if (Player.player.inUI) return;

        float speedMultiplier = Input.GetKey(KeyCode.LeftShift) ? sprintMultiplier : 1;

        if (Input.GetKey(KeyCode.Mouse2))
        {
            float horizontal = Input.GetAxis("Mouse X");
            float vertical = Input.GetAxis("Mouse Y");

            velocity -= ((cam.transform.up * vertical + cam.transform.right * horizontal) * acceleration) * speedMultiplier * panSpeed;

        }
        else
        {
            float horizontal = Input.GetAxis("Horizontal");
            float vertical = Input.GetAxis("Vertical");

            float alt = 0;
            if (Input.GetKey(KeyCode.LeftControl)) alt = -acceleration;
            else if (Input.GetKey(KeyCode.Space)) alt = acceleration;


            velocity += (((cam.transform.forward * vertical + cam.transform.right * horizontal) * acceleration) + Vector3.up * alt) * speedMultiplier;

            if (velocity.magnitude > maxSpeed * speedMultiplier) velocity = velocity.normalized * maxSpeed;

            float mouseX = Input.GetAxis("Mouse X") * sensitivity.x;
            float mouseY = Input.GetAxis("Mouse Y") * sensitivity.y;

            cam.transform.Rotate(-Vector3.right * mouseY);
            transform.Rotate(Vector3.up * mouseX);
        }
        transform.position += velocity * Time.deltaTime;
        velocity /= drag;

    }
}
