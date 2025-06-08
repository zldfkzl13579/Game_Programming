using UnityEngine;
using System.Collections.Generic; // List를 사용하기 위해 필요

public class BackgroundManager : MonoBehaviour
{
	[Header("References")]
	[SerializeField] private Camera mainCamera; // 씬의 메인 카메라 (Inspector에서 연결)

	[Header("Mountain Settings")]
	[SerializeField] private GameObject[] mountainPrefabs; // 산 프리팹 배열 (순서 랜덤 소환)
	[SerializeField] private float mountainScrollSpeed = 1f; // 산이 왼쪽으로 움직이는 속도
	[SerializeField] private float minMountainSpawnInterval = 5f; // 산 스폰 최소 간격
	[SerializeField] private float maxMountainSpawnInterval = 10f; // 산 스폰 최대 간격
	[SerializeField] private float mountainSpawnY = -2f; // 산 스폰 Y 위치 (지면에 맞춤)
	[SerializeField] private float mountainSpawnOffsetX = 5f; // 카메라 오른쪽 밖에서 스폰될 X 오프셋
	[SerializeField] private float mountainDespawnOffsetX = 5f; // 카메라 왼쪽 밖에서 사라질 X 오프셋

	[Header("Cloud Settings")]
	[SerializeField] private GameObject[] cloudPrefabs; // 구름 프리팹 배열 (순서 랜덤 소환)
	[SerializeField] private float cloudScrollSpeed = 0.5f; // 구름이 왼쪽으로 움직이는 속도 (산보다 느리게)
	[SerializeField] private float minCloudSpawnInterval = 3f; // 구름 스폰 최소 간격
	[SerializeField] private float maxCloudSpawnInterval = 7f; // 구름 스폰 최대 간격
	[SerializeField] private float minCloudSpawnY = 2f; // 구름 스폰 최소 Y 범위
	[SerializeField] private float maxCloudSpawnY = 4f; // 구름 스폰 최대 Y 범위
	[SerializeField] private float cloudSpawnOffsetX = 5f; // 카메라 오른쪽 밖에서 스폰될 X 오프셋
	[SerializeField] private float cloudDespawnOffsetX = 5f; // 카메라 왼쪽 밖에서 사라질 X 오프셋

	private List<GameObject> activeMountains = new List<GameObject>(); // 현재 활성화된 산 오브젝트 리스트
	private List<GameObject> activeClouds = new List<GameObject>(); // 현재 활성화된 구름 오브젝트 리스트

	private float nextMountainSpawnTime; // 다음 산이 스폰될 시간
	private float nextCloudSpawnTime; // 다음 구름이 스폰될 시간

	void Awake()
	{
		// 메인 카메라 참조가 없으면 자동으로 찾음
		if (mainCamera == null)
		{
			mainCamera = Camera.main;
			if (mainCamera == null)
			{
				Debug.LogError("BackgroundManager: Main Camera not found in scene! Please assign it or ensure a camera is tagged 'MainCamera'.");
				enabled = false; // 스크립트 비활성화
				return;
			}
		}

		// 프리팹 배열들이 할당되었는지 확인 (선택 사항, 경고만)
		if (mountainPrefabs == null || mountainPrefabs.Length == 0) Debug.LogWarning("BackgroundManager: No mountain prefabs assigned!");
		if (cloudPrefabs == null || cloudPrefabs.Length == 0) Debug.LogWarning("BackgroundManager: No cloud prefabs assigned!");
	}

	void Start()
	{
		// 초기 스폰 시간 설정
		nextMountainSpawnTime = Time.time + Random.Range(minMountainSpawnInterval, maxMountainSpawnInterval);
		nextCloudSpawnTime = Time.time + Random.Range(minCloudSpawnInterval, maxCloudSpawnInterval);
	}

	void Update()
	{
		// === 스폰 로직 ===
		// 산 스폰
		if (Time.time >= nextMountainSpawnTime && mountainPrefabs.Length > 0)
		{
			SpawnBackgroundElement(mountainPrefabs, mountainScrollSpeed, mountainSpawnY, mountainSpawnOffsetX, activeMountains);
			nextMountainSpawnTime = Time.time + Random.Range(minMountainSpawnInterval, maxMountainSpawnInterval);
		}

		// 구름 스폰
		if (Time.time >= nextCloudSpawnTime && cloudPrefabs.Length > 0)
		{
			float randomCloudY = Random.Range(minCloudSpawnY, maxCloudSpawnY);
			SpawnBackgroundElement(cloudPrefabs, cloudScrollSpeed, randomCloudY, cloudSpawnOffsetX, activeClouds);
			nextCloudSpawnTime = Time.time + Random.Range(minCloudSpawnInterval, maxCloudSpawnInterval);
		}

		// === 이동 로직 ===
		MoveBackgroundElements(activeMountains, mountainScrollSpeed);
		MoveBackgroundElements(activeClouds, cloudScrollSpeed);

		// === 제거 로직 ===
		DespawnBackgroundElements(activeMountains, mountainDespawnOffsetX);
		DespawnBackgroundElements(activeClouds, cloudDespawnOffsetX);
	}

	/// <summary>
	/// 배경 요소를 스폰하는 공통 로직
	/// </summary>
	/// <param name="prefabs">스폰할 프리팹 배열.</param>
	/// <param name="speed">오브젝트의 스크롤 속도 (사용되지는 않지만 일관성을 위해 포함).</param>
	/// <param name="yPosition">스폰될 Y 위치.</param>
	/// <param name="offsetX">카메라 오른쪽 끝에서부터의 X 오프셋.</param>
	/// <param name="activeList">활성 오브젝트 리스트.</param>
	void SpawnBackgroundElement(GameObject[] prefabs, float speed, float yPosition, float offsetX, List<GameObject> activeList)
	{
		if (prefabs.Length == 0) return; // 프리팹이 없으면 스폰하지 않음

		GameObject selectedPrefab = prefabs[Random.Range(0, prefabs.Length)]; // 랜덤 프리팹 선택

		// 카메라의 오른쪽 끝 월드 좌표 계산 (Y는 임의, Z는 0)
		Vector3 cameraRightEdge = mainCamera.ViewportToWorldPoint(new Vector3(1, 0.5f, mainCamera.nearClipPlane));
		float spawnXPosition = cameraRightEdge.x + offsetX;

		// 스폰 위치 설정
		Vector3 spawnPosition = new Vector3(spawnXPosition, yPosition, transform.position.z); // 배경 오브젝트의 Z는 매니저 Z 사용

		GameObject newElement = Instantiate(selectedPrefab, spawnPosition, Quaternion.identity);
		newElement.transform.SetParent(this.transform); // BackgroundManager의 자식으로 설정하여 Hierarchy 정리
		activeList.Add(newElement);

		// (선택 사항) 배경 오브젝트에 ParallaxBackground 스크립트를 붙여 속도를 제어하는 대신,
		// 이 BackgroundManager에서 직접 속도를 제어하도록 변경했습니다.
		// 각 배경 요소의 Z 깊이를 Inspector에서 설정해야 합니다.
	}

	/// <summary>
	/// 배경 요소들을 왼쪽으로 이동시킵니다.
	/// </summary>
	/// <param name="elements">이동시킬 GameObject 리스트.</param>
	/// <param name="speed">이동 속도.</param>
	void MoveBackgroundElements(List<GameObject> elements, float speed)
	{
		foreach (GameObject element in elements)
		{
			if (element != null) // 오브젝트가 파괴되지 않았는지 확인
			{
				element.transform.Translate(Vector2.left * speed * Time.deltaTime);
			}
		}
	}

	/// <summary>
	/// 카메라 왼쪽 밖으로 나간 배경 요소들을 제거합니다.
	/// </summary>
	/// <param name="elements">제거할 GameObject 리스트.</param>
	/// <param name="despawnOffsetX">카메라 왼쪽 끝에서부터 사라질 X 오프셋.</param>
	void DespawnBackgroundElements(List<GameObject> elements, float despawnOffsetX)
	{
		// 리스트를 뒤에서부터 순회하여 제거 시 인덱스 문제 발생을 방지합니다.
		for (int i = elements.Count - 1; i >= 0; i--)
		{
			GameObject element = elements[i];
			if (element == null) // 이미 파괴된 경우 건너뛰기
			{
				elements.RemoveAt(i);
				continue;
			}

			// 카메라의 왼쪽 끝 월드 좌표 계산
			Vector3 cameraLeftEdge = mainCamera.ViewportToWorldPoint(new Vector3(0, 0.5f, mainCamera.nearClipPlane));

			// 오브젝트의 X 위치가 카메라 왼쪽 끝을 넘어섰는지 확인
			if (element.transform.position.x < cameraLeftEdge.x - despawnOffsetX)
			{
				elements.RemoveAt(i); // 리스트에서 제거
				Destroy(element); // 게임 오브젝트 파괴
			}
		}
	}
}
