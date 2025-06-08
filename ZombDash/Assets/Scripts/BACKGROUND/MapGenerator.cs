using UnityEngine;
using System.Collections.Generic; // List를 사용하기 위해 필요

public class MapGenerator : MonoBehaviour
{
	[Header("References")]
	[SerializeField] private Transform playerTransform; // 플레이어 Transform (Inspector에서 연결)
	[SerializeField] private GameObject startChunkPrefab; // 게임 시작 시 한 번만 출현할 시작 청크 (예: Chunk_0)
	[SerializeField] private GameObject[] basicChunkPrefabs; // 기본 청크들 (예: Chunk_1, Chunk_2, Chunk_3, Chunk_4)
	[SerializeField] private GameObject[] midGameChunkPrefabs; // 중반 게임 청크 (예: Chunk_5)
	[SerializeField] private GameObject[] lateGameChunkPrefabs1; // 후반 게임 청크 1 (예: Chunk_6)
	[SerializeField] private GameObject[] lateGameChunkPrefabs2; // 후반 게임 청크 2 (예: Chunk_7)

	[Header("Generation Settings")]
	[SerializeField] private float spawnAheadDistance = 30f; // 플레이어보다 얼마나 앞에 청크를 생성할 것인가
	[SerializeField] private float despawnBehindDistance = 15f; // 플레이어보다 얼마나 뒤에 청크를 제거할 것인가
	[SerializeField] private int initialChunksCount = 3; // 시작 청크 이후, 게임 시작 시 추가로 생성할 무작위 청크 개수

	[Header("Unlock Times")]
	[SerializeField] private float midGameUnlockTime = 30f; // Chunk_5가 출현 가능해지는 시간 (초)
	[SerializeField] private float lateGameUnlockTime1 = 40f; // Chunk_6가 출현 가능해지는 시간 (초)
	[SerializeField] private float lateGameUnlockTime2 = 50f; // Chunk_7가 출현 가능해지는 시간 (초)

	[Header("Spawnable Prefabs")]
	[SerializeField] private GameObject[] coinPrefabs; // 코인 프리팹 배열
	[SerializeField] private GameObject[] itemPrefabs; // 아이템 프리팹 배열
	[SerializeField] private GameObject[] monsterPrefabs; // 몬스터 프리팹 배열

	private List<GameObject> activeChunks = new List<GameObject>(); // 현재 활성화된 청크들을 저장
	private Vector3 nextSpawnPoint = Vector3.zero; // 다음 청크가 생성될 시작 위치 (월드 좌표)
	private float gameStartTime; // 게임 시작 시간을 기록하여 시간 기반 언락에 사용

	void Start()
	{
		gameStartTime = Time.time; // 게임 시작 시간 기록

		// 필수 컴포넌트 및 프리팹 할당 확인
		if (playerTransform == null) { Debug.LogError("MapGenerator: Player Transform is not assigned!"); return; }
		if (startChunkPrefab == null) { Debug.LogError("MapGenerator: Start Chunk Prefab is not assigned!"); return; }
		if (basicChunkPrefabs == null || basicChunkPrefabs.Length == 0) { Debug.LogError("MapGenerator: Basic Chunk Prefabs are not assigned or empty!"); return; }

		// 청크 프리팹들에 ChunkData 스크립트가 붙어 있는지 확인 (개발 중 에러 방지)
		CheckChunkDataAssignment(startChunkPrefab);
		CheckChunkDataAssignment(basicChunkPrefabs);
		CheckChunkDataAssignment(midGameChunkPrefabs);
		CheckChunkDataAssignment(lateGameChunkPrefabs1);
		CheckChunkDataAssignment(lateGameChunkPrefabs2);

		// 스폰 가능한 프리팹 배열이 비어있는지 경고 (게임 진행에는 영향 없음)
		if (coinPrefabs == null || coinPrefabs.Length == 0) Debug.LogWarning("MapGenerator: No coin prefabs assigned!");
		if (itemPrefabs == null || itemPrefabs.Length == 0) Debug.LogWarning("MapGenerator: No item prefabs assigned!");
		if (monsterPrefabs == null || monsterPrefabs.Length == 0) Debug.LogWarning("MapGenerator: No monster prefabs assigned!");

		// 1. 초기 시작 청크를 (0,0,0)에 생성합니다.
		SpawnSpecificChunk(startChunkPrefab); // 이 메서드가 nextSpawnPoint를 업데이트합니다.

		// 2. 시작 청크 이후, 초기 화면을 채우기 위해 initialChunksCount 만큼의 무작위 청크를 생성합니다.
		for (int i = 0; i < initialChunksCount; i++)
		{
			SpawnRandomChunk(); // 현재 시간 기준, 출현 가능한 청크 중에서 무작위로 선택하여 생성
		}
	}

	void Update()
	{
		// 플레이어의 X 위치가 다음 청크 생성 지점으로부터 일정 거리 안에 들어오면 새 청크 생성
		if (playerTransform.position.x + spawnAheadDistance > nextSpawnPoint.x)
		{
			SpawnRandomChunk(); // 무작위 청크 생성
		}

		// 플레이어 뒤로 멀리 지나간 청크 제거
		DespawnChunks();
	}

	/// <summary>
	/// 무작위로 사용 가능한 청크 프리팹을 선택하여 생성합니다.
	/// 이 메서드는 ChunkData 스크립트의 ChunkType을 기반으로 어떤 청크를 생성할지 결정하지 않습니다.
	/// 단순히 배열에 추가된 청크들을 무작위로 선택합니다.
	/// </summary>
	GameObject GetRandomAvailableChunkPrefab()
	{
		List<GameObject> availableChunks = new List<GameObject>();

		// 기본 청크들 항상 포함 (Chunk_1 ~ Chunk_4)
		if (basicChunkPrefabs != null && basicChunkPrefabs.Length > 0)
		{
			availableChunks.AddRange(basicChunkPrefabs);
		}

		float elapsedTime = Time.time - gameStartTime;

		// 시간 경과에 따라 새로운 청크 그룹들을 출현 가능 목록에 추가
		if (elapsedTime >= midGameUnlockTime && midGameChunkPrefabs != null && midGameChunkPrefabs.Length > 0)
		{
			availableChunks.AddRange(midGameChunkPrefabs); // Chunk_5 추가
		}
		if (elapsedTime >= lateGameUnlockTime1 && lateGameChunkPrefabs1 != null && lateGameChunkPrefabs1.Length > 0)
		{
			availableChunks.AddRange(lateGameChunkPrefabs1); // Chunk_6 추가
		}
		if (elapsedTime >= lateGameUnlockTime2 && lateGameChunkPrefabs2 != null && lateGameChunkPrefabs2.Length > 0)
		{
			availableChunks.AddRange(lateGameChunkPrefabs2); // Chunk_7 추가
		}

		if (availableChunks.Count == 0)
		{
			Debug.LogWarning("MapGenerator: No available chunk prefabs to choose from for random spawning!");
			return null; // 사용 가능한 청크가 없으면 null 반환
		}

		// 사용 가능한 청크 풀 중에서 무작위로 하나 선택
		return availableChunks[Random.Range(0, availableChunks.Count)];
	}

	/// <summary>
	/// 무작위로 선택된 청크를 생성합니다.
	/// </summary>
	void SpawnRandomChunk()
	{
		GameObject selectedChunkPrefab = GetRandomAvailableChunkPrefab();
		if (selectedChunkPrefab == null)
		{
			Debug.LogWarning("MapGenerator: No available chunk prefabs to spawn!");
			return;
		}
		SpawnSpecificChunk(selectedChunkPrefab); // 선택된 청크를 생성
	}

	/// <summary>
	/// 특정 맵 청크 프리팹을 생성하고 activeChunks 리스트에 추가하며, 청크 내 스폰 지점에 오브젝트를 배치합니다.
	/// </summary>
	/// <param name="chunkPrefabToSpawn">생성할 맵 청크 프리팹.</param>
	void SpawnSpecificChunk(GameObject chunkPrefabToSpawn)
	{
		// 청크 프리팹에 ChunkData 컴포넌트가 있는지 다시 확인
		ChunkData chunkData = chunkPrefabToSpawn.GetComponent<ChunkData>();
		if (chunkData == null)
		{
			Debug.LogError($"MapGenerator: Chunk prefab '{chunkPrefabToSpawn.name}' is missing ChunkData component! Cannot get spawn settings. Please add it.", chunkPrefabToSpawn);
			return;
		}

		// 청크를 nextSpawnPoint 위치(월드 좌표)에 생성합니다. Quaternion.identity는 회전 없음
		GameObject newChunk = Instantiate(chunkPrefabToSpawn, nextSpawnPoint, Quaternion.identity);
		activeChunks.Add(newChunk); // 활성 청크 리스트에 추가

		// 생성된 청크 내에서 "EndPoint"라는 이름의 자식 오브젝트를 찾습니다.
		// 이 EndPoint의 월드 위치가 다음 청크의 시작 지점이 됩니다.
		Transform chunkEndPoint = newChunk.transform.Find("EndPoint");
		if (chunkEndPoint != null)
		{
			nextSpawnPoint = chunkEndPoint.position; // 다음 생성 지점 업데이트
		}
		else
		{
			// EndPoint가 없으면 경고 메시지를 띄우고, 임의의 고정 값으로 다음 생성 지점을 예측합니다.
			Debug.LogWarning($"Chunk prefab '{chunkPrefabToSpawn.name}' does not have an 'EndPoint' child. Please add one for proper chaining. Assuming a fixed length of 20 units for now.", chunkPrefabToSpawn);
			nextSpawnPoint += new Vector3(20f, 0, 0); // 이 값은 실제 청크의 길이에 맞춰 조절해야 합니다.
		}

		// --- 스폰 가능한 오브젝트 배치 ---
		// 현재 청크에서 스폰된 각 오브젝트 유형의 개수를 추적하는 변수
		int spawnedCoinsInChunk = 0;
		int spawnedItemsInChunk = 0;
		int spawnedMonstersInChunk = 0;

		// 생성된 청크의 모든 자식 오브젝트를 순회하며 스폰 지점을 찾습니다.
		foreach (Transform child in newChunk.transform)
		{
			// 각 스폰 포인트의 이름에 따라 스폰될 오브젝트 타입을 결정
			if (child.name.Contains("CoinSpawnPoint"))
			{
				TrySpawnCoinAtPoint(child.position, chunkData, ref spawnedCoinsInChunk);
			}
			else if (child.name.Contains("ItemSpawnPoint"))
			{
				TrySpawnItemAtPoint(child.position, chunkData, ref spawnedItemsInChunk);
			}
			else if (child.name.Contains("MonsterSpawnPoint"))
			{
				TrySpawnMonsterAtPoint(child.position, chunkData, ref spawnedMonstersInChunk);
			}
			// 다른 이름의 스폰 포인트가 있다면 여기에 추가적인 else if 블록을 넣을 수 있습니다.
		}
	}

	/// <summary>
	/// 지정된 위치에 코인을 스폰합니다.
	/// </summary>
	void TrySpawnCoinAtPoint(Vector3 spawnPosition, ChunkData chunkData, ref int coinsSpawned)
	{
		if (Random.value < chunkData.CoinSpawnChance && coinPrefabs != null && coinPrefabs.Length > 0 && coinsSpawned < chunkData.MaxCoinsPerChunk)
		{
			Instantiate(coinPrefabs[Random.Range(0, coinPrefabs.Length)], spawnPosition, Quaternion.identity);
			coinsSpawned++;
		}
	}

	/// <summary>
	/// 지정된 위치에 아이템을 스폰합니다.
	/// </summary>
	void TrySpawnItemAtPoint(Vector3 spawnPosition, ChunkData chunkData, ref int itemsSpawned)
	{
		if (Random.value < chunkData.ItemSpawnChance && itemPrefabs != null && itemPrefabs.Length > 0 && itemsSpawned < chunkData.MaxItemsPerChunk)
		{
			Instantiate(itemPrefabs[Random.Range(0, itemPrefabs.Length)], spawnPosition, Quaternion.identity);
			itemsSpawned++;
		}
	}

	/// <summary>
	/// 지정된 위치에 몬스터를 스폰합니다.
	/// </summary>
	void TrySpawnMonsterAtPoint(Vector3 spawnPosition, ChunkData chunkData, ref int monstersSpawned)
	{
		if (Random.value < chunkData.MonsterSpawnChance && monsterPrefabs != null && monsterPrefabs.Length > 0 && monstersSpawned < chunkData.MaxMonstersPerChunk)
		{
			Instantiate(monsterPrefabs[Random.Range(0, monsterPrefabs.Length)], spawnPosition, Quaternion.identity);
			monstersSpawned++;
		}
	}

	/// <summary>
	/// 플레이어 뒤로 멀리 지나간 청크들을 제거합니다.
	/// </summary>
	void DespawnChunks()
	{
		// 리스트를 뒤에서부터 순회하여 오브젝트 제거 시 인덱스 문제 발생을 방지합니다.
		for (int i = activeChunks.Count - 1; i >= 0; i--)
		{
			GameObject chunk = activeChunks[i];
			// 청크의 위치가 플레이어 X 위치에서 일정 거리 이상 뒤로 가면 제거
			if (chunk.transform.position.x < playerTransform.position.x - despawnBehindDistance)
			{
				activeChunks.RemoveAt(i); // 리스트에서 제거
				Destroy(chunk); // 게임 오브젝트 파괴 (메모리 해제)
			}
		}
	}

	/// <summary>
	/// 청크 프리팹 배열에 ChunkData 컴포넌트가 할당되었는지 확인합니다.
	/// </summary>
	private void CheckChunkDataAssignment(GameObject[] prefabs)
	{
		if (prefabs == null) return;
		foreach (GameObject prefab in prefabs)
		{
			if (prefab != null && prefab.GetComponent<ChunkData>() == null)
			{
				Debug.LogError($"MapGenerator: Chunk prefab '{prefab.name}' is missing the ChunkData script! Please add it to the chunk's root GameObject.", prefab);
			}
		}
	}
	/// <summary>
	/// 단일 청크 프리팹에 ChunkData 컴포넌트가 할당되었는지 확인합니다.
	/// </summary>
	private void CheckChunkDataAssignment(GameObject prefab)
	{
		if (prefab != null && prefab.GetComponent<ChunkData>() == null)
		{
			Debug.LogError($"MapGenerator: Chunk prefab '{prefab.name}' is missing the ChunkData script! Please add it to the chunk's root GameObject.", prefab);
		}
	}
}
