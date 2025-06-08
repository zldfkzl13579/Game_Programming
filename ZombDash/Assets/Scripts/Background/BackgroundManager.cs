using UnityEngine;
using System.Collections.Generic; // List�� ����ϱ� ���� �ʿ�

public class BackgroundManager : MonoBehaviour
{
	[Header("References")]
	[SerializeField] private Camera mainCamera; // ���� ���� ī�޶� (Inspector���� ����)

	[Header("Mountain Settings")]
	[SerializeField] private GameObject[] mountainPrefabs; // �� ������ �迭 (���� ���� ��ȯ)
	[SerializeField] private float mountainScrollSpeed = 1f; // ���� �������� �����̴� �ӵ�
	[SerializeField] private float minMountainSpawnInterval = 5f; // �� ���� �ּ� ����
	[SerializeField] private float maxMountainSpawnInterval = 10f; // �� ���� �ִ� ����
	[SerializeField] private float mountainSpawnY = -2f; // �� ���� Y ��ġ (���鿡 ����)
	[SerializeField] private float mountainSpawnOffsetX = 5f; // ī�޶� ������ �ۿ��� ������ X ������
	[SerializeField] private float mountainDespawnOffsetX = 5f; // ī�޶� ���� �ۿ��� ����� X ������

	[Header("Cloud Settings")]
	[SerializeField] private GameObject[] cloudPrefabs; // ���� ������ �迭 (���� ���� ��ȯ)
	[SerializeField] private float cloudScrollSpeed = 0.5f; // ������ �������� �����̴� �ӵ� (�꺸�� ������)
	[SerializeField] private float minCloudSpawnInterval = 3f; // ���� ���� �ּ� ����
	[SerializeField] private float maxCloudSpawnInterval = 7f; // ���� ���� �ִ� ����
	[SerializeField] private float minCloudSpawnY = 2f; // ���� ���� �ּ� Y ����
	[SerializeField] private float maxCloudSpawnY = 4f; // ���� ���� �ִ� Y ����
	[SerializeField] private float cloudSpawnOffsetX = 5f; // ī�޶� ������ �ۿ��� ������ X ������
	[SerializeField] private float cloudDespawnOffsetX = 5f; // ī�޶� ���� �ۿ��� ����� X ������

	private List<GameObject> activeMountains = new List<GameObject>(); // ���� Ȱ��ȭ�� �� ������Ʈ ����Ʈ
	private List<GameObject> activeClouds = new List<GameObject>(); // ���� Ȱ��ȭ�� ���� ������Ʈ ����Ʈ

	private float nextMountainSpawnTime; // ���� ���� ������ �ð�
	private float nextCloudSpawnTime; // ���� ������ ������ �ð�

	void Awake()
	{
		// ���� ī�޶� ������ ������ �ڵ����� ã��
		if (mainCamera == null)
		{
			mainCamera = Camera.main;
			if (mainCamera == null)
			{
				Debug.LogError("BackgroundManager: Main Camera not found in scene! Please assign it or ensure a camera is tagged 'MainCamera'.");
				enabled = false; // ��ũ��Ʈ ��Ȱ��ȭ
				return;
			}
		}

		// ������ �迭���� �Ҵ�Ǿ����� Ȯ�� (���� ����, ���)
		if (mountainPrefabs == null || mountainPrefabs.Length == 0) Debug.LogWarning("BackgroundManager: No mountain prefabs assigned!");
		if (cloudPrefabs == null || cloudPrefabs.Length == 0) Debug.LogWarning("BackgroundManager: No cloud prefabs assigned!");
	}

	void Start()
	{
		// �ʱ� ���� �ð� ����
		nextMountainSpawnTime = Time.time + Random.Range(minMountainSpawnInterval, maxMountainSpawnInterval);
		nextCloudSpawnTime = Time.time + Random.Range(minCloudSpawnInterval, maxCloudSpawnInterval);
	}

	void Update()
	{
		// === ���� ���� ===
		// �� ����
		if (Time.time >= nextMountainSpawnTime && mountainPrefabs.Length > 0)
		{
			SpawnBackgroundElement(mountainPrefabs, mountainScrollSpeed, mountainSpawnY, mountainSpawnOffsetX, activeMountains);
			nextMountainSpawnTime = Time.time + Random.Range(minMountainSpawnInterval, maxMountainSpawnInterval);
		}

		// ���� ����
		if (Time.time >= nextCloudSpawnTime && cloudPrefabs.Length > 0)
		{
			float randomCloudY = Random.Range(minCloudSpawnY, maxCloudSpawnY);
			SpawnBackgroundElement(cloudPrefabs, cloudScrollSpeed, randomCloudY, cloudSpawnOffsetX, activeClouds);
			nextCloudSpawnTime = Time.time + Random.Range(minCloudSpawnInterval, maxCloudSpawnInterval);
		}

		// === �̵� ���� ===
		MoveBackgroundElements(activeMountains, mountainScrollSpeed);
		MoveBackgroundElements(activeClouds, cloudScrollSpeed);

		// === ���� ���� ===
		DespawnBackgroundElements(activeMountains, mountainDespawnOffsetX);
		DespawnBackgroundElements(activeClouds, cloudDespawnOffsetX);
	}

	/// <summary>
	/// ��� ��Ҹ� �����ϴ� ���� ����
	/// </summary>
	/// <param name="prefabs">������ ������ �迭.</param>
	/// <param name="speed">������Ʈ�� ��ũ�� �ӵ� (�������� ������ �ϰ����� ���� ����).</param>
	/// <param name="yPosition">������ Y ��ġ.</param>
	/// <param name="offsetX">ī�޶� ������ ������������ X ������.</param>
	/// <param name="activeList">Ȱ�� ������Ʈ ����Ʈ.</param>
	void SpawnBackgroundElement(GameObject[] prefabs, float speed, float yPosition, float offsetX, List<GameObject> activeList)
	{
		if (prefabs.Length == 0) return; // �������� ������ �������� ����

		GameObject selectedPrefab = prefabs[Random.Range(0, prefabs.Length)]; // ���� ������ ����

		// ī�޶��� ������ �� ���� ��ǥ ��� (Y�� ����, Z�� 0)
		Vector3 cameraRightEdge = mainCamera.ViewportToWorldPoint(new Vector3(1, 0.5f, mainCamera.nearClipPlane));
		float spawnXPosition = cameraRightEdge.x + offsetX;

		// ���� ��ġ ����
		Vector3 spawnPosition = new Vector3(spawnXPosition, yPosition, transform.position.z); // ��� ������Ʈ�� Z�� �Ŵ��� Z ���

		GameObject newElement = Instantiate(selectedPrefab, spawnPosition, Quaternion.identity);
		newElement.transform.SetParent(this.transform); // BackgroundManager�� �ڽ����� �����Ͽ� Hierarchy ����
		activeList.Add(newElement);

		// (���� ����) ��� ������Ʈ�� ParallaxBackground ��ũ��Ʈ�� �ٿ� �ӵ��� �����ϴ� ���,
		// �� BackgroundManager���� ���� �ӵ��� �����ϵ��� �����߽��ϴ�.
		// �� ��� ����� Z ���̸� Inspector���� �����ؾ� �մϴ�.
	}

	/// <summary>
	/// ��� ��ҵ��� �������� �̵���ŵ�ϴ�.
	/// </summary>
	/// <param name="elements">�̵���ų GameObject ����Ʈ.</param>
	/// <param name="speed">�̵� �ӵ�.</param>
	void MoveBackgroundElements(List<GameObject> elements, float speed)
	{
		foreach (GameObject element in elements)
		{
			if (element != null) // ������Ʈ�� �ı����� �ʾҴ��� Ȯ��
			{
				element.transform.Translate(Vector2.left * speed * Time.deltaTime);
			}
		}
	}

	/// <summary>
	/// ī�޶� ���� ������ ���� ��� ��ҵ��� �����մϴ�.
	/// </summary>
	/// <param name="elements">������ GameObject ����Ʈ.</param>
	/// <param name="despawnOffsetX">ī�޶� ���� ���������� ����� X ������.</param>
	void DespawnBackgroundElements(List<GameObject> elements, float despawnOffsetX)
	{
		// ����Ʈ�� �ڿ������� ��ȸ�Ͽ� ���� �� �ε��� ���� �߻��� �����մϴ�.
		for (int i = elements.Count - 1; i >= 0; i--)
		{
			GameObject element = elements[i];
			if (element == null) // �̹� �ı��� ��� �ǳʶٱ�
			{
				elements.RemoveAt(i);
				continue;
			}

			// ī�޶��� ���� �� ���� ��ǥ ���
			Vector3 cameraLeftEdge = mainCamera.ViewportToWorldPoint(new Vector3(0, 0.5f, mainCamera.nearClipPlane));

			// ������Ʈ�� X ��ġ�� ī�޶� ���� ���� �Ѿ���� Ȯ��
			if (element.transform.position.x < cameraLeftEdge.x - despawnOffsetX)
			{
				elements.RemoveAt(i); // ����Ʈ���� ����
				Destroy(element); // ���� ������Ʈ �ı�
			}
		}
	}
}
