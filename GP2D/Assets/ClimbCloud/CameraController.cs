using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraController : MonoBehaviour
{
    GameObject player;

	public float minX = -3.5f;
	public float maxX = 3.5f;

	void Start()
	{
		this.player = GameObject.Find("cat");
	}

	void Update()
	{
		if (this.player != null)
		{
			Vector3 playerPos = this.player.transform.position;
			Vector3 cameraPos = transform.position;
			if (playerPos.x >= minX && playerPos.x <= maxX)
			{
				cameraPos.x = playerPos.x;
			}
			transform.position = new Vector3(cameraPos.x, playerPos.y, cameraPos.z);
		}
	}
}