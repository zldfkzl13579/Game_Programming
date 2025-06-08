using UnityEngine;
using System.Collections.Generic; // List�� ����ϱ� ���� �ʿ�

public class MapGenerator : MonoBehaviour
{
	[Header("References")]
	[SerializeField] private Transform playerTransform; // �÷��̾� Transform (Inspector���� ����)
	[SerializeField] private GameObject startChunkPrefab; // ���� ���� �� �� ���� ������ ���� ûũ (��: Chunk_0)
	[SerializeField] private GameObject[] basicChunkPrefabs; // �⺻ ûũ�� (��: Chunk_1, Chunk_2, Chunk_3, Chunk_4)
	[SerializeField] private GameObject[] midGameChunkPrefabs; // �߹� ���� ûũ (��: Chunk_5)
	[SerializeField] private GameObject[] lateGameChunkPrefabs1; // �Ĺ� ���� ûũ 1 (��: Chunk_6)
	[SerializeField] private GameObject[] lateGameChunkPrefabs2; // �Ĺ� ���� ûũ 2 (��: Chunk_7)

	[Header("Generation Settings")]
	[SerializeField] private float spawnAheadDistance = 30f; // �÷��̾�� �󸶳� �տ� ûũ�� ������ ���ΰ�
	[SerializeField] private float despawnBehindDistance = 15f; // �÷��̾�� �󸶳� �ڿ� ûũ�� ������ ���ΰ�
	[SerializeField] private int initialChunksCount = 3; // ���� ûũ ����, ���� ���� �� �߰��� ������ ������ ûũ ����

	[Header("Unlock Times")]
	[SerializeField] private float midGameUnlockTime = 30f; // Chunk_5�� ���� ���������� �ð� (��)
	[SerializeField] private float lateGameUnlockTime1 = 40f; // Chunk_6�� ���� ���������� �ð� (��)
	[SerializeField] private float lateGameUnlockTime2 = 50f; // Chunk_7�� ���� ���������� �ð� (��)

	[Header("Spawnable Prefabs")]
	[SerializeField] private GameObject[] coinPrefabs; // ���� ������ �迭
	[SerializeField] private GameObject[] itemPrefabs; // ������ ������ �迭
	[SerializeField] private GameObject[] monsterPrefabs; // ���� ������ �迭

	private List<GameObject> activeChunks = new List<GameObject>(); // ���� Ȱ��ȭ�� ûũ���� ����
	private Vector3 nextSpawnPoint = Vector3.zero; // ���� ûũ�� ������ ���� ��ġ (���� ��ǥ)
	private float gameStartTime; // ���� ���� �ð��� ����Ͽ� �ð� ��� ����� ���

	void Start()
	{
		gameStartTime = Time.time; // ���� ���� �ð� ���

		// �ʼ� ������Ʈ �� ������ �Ҵ� Ȯ��
		if (playerTransform == null) { Debug.LogError("MapGenerator: Player Transform is not assigned!"); return; }
		if (startChunkPrefab == null) { Debug.LogError("MapGenerator: Start Chunk Prefab is not assigned!"); return; }
		if (basicChunkPrefabs == null || basicChunkPrefabs.Length == 0) { Debug.LogError("MapGenerator: Basic Chunk Prefabs are not assigned or empty!"); return; }

		// ûũ �����յ鿡 ChunkData ��ũ��Ʈ�� �پ� �ִ��� Ȯ�� (���� �� ���� ����)
		CheckChunkDataAssignment(startChunkPrefab);
		CheckChunkDataAssignment(basicChunkPrefabs);
		CheckChunkDataAssignment(midGameChunkPrefabs);
		CheckChunkDataAssignment(lateGameChunkPrefabs1);
		CheckChunkDataAssignment(lateGameChunkPrefabs2);

		// ���� ������ ������ �迭�� ����ִ��� ��� (���� ���࿡�� ���� ����)
		if (coinPrefabs == null || coinPrefabs.Length == 0) Debug.LogWarning("MapGenerator: No coin prefabs assigned!");
		if (itemPrefabs == null || itemPrefabs.Length == 0) Debug.LogWarning("MapGenerator: No item prefabs assigned!");
		if (monsterPrefabs == null || monsterPrefabs.Length == 0) Debug.LogWarning("MapGenerator: No monster prefabs assigned!");

		// 1. �ʱ� ���� ûũ�� (0,0,0)�� �����մϴ�.
		SpawnSpecificChunk(startChunkPrefab); // �� �޼��尡 nextSpawnPoint�� ������Ʈ�մϴ�.

		// 2. ���� ûũ ����, �ʱ� ȭ���� ä��� ���� initialChunksCount ��ŭ�� ������ ûũ�� �����մϴ�.
		for (int i = 0; i < initialChunksCount; i++)
		{
			SpawnRandomChunk(); // ���� �ð� ����, ���� ������ ûũ �߿��� �������� �����Ͽ� ����
		}
	}

	void Update()
	{
		// �÷��̾��� X ��ġ�� ���� ûũ ���� �������κ��� ���� �Ÿ� �ȿ� ������ �� ûũ ����
		if (playerTransform.position.x + spawnAheadDistance > nextSpawnPoint.x)
		{
			SpawnRandomChunk(); // ������ ûũ ����
		}

		// �÷��̾� �ڷ� �ָ� ������ ûũ ����
		DespawnChunks();
	}

	/// <summary>
	/// �������� ��� ������ ûũ �������� �����Ͽ� �����մϴ�.
	/// �� �޼���� ChunkData ��ũ��Ʈ�� ChunkType�� ������� � ûũ�� �������� �������� �ʽ��ϴ�.
	/// �ܼ��� �迭�� �߰��� ûũ���� �������� �����մϴ�.
	/// </summary>
	GameObject GetRandomAvailableChunkPrefab()
	{
		List<GameObject> availableChunks = new List<GameObject>();

		// �⺻ ûũ�� �׻� ���� (Chunk_1 ~ Chunk_4)
		if (basicChunkPrefabs != null && basicChunkPrefabs.Length > 0)
		{
			availableChunks.AddRange(basicChunkPrefabs);
		}

		float elapsedTime = Time.time - gameStartTime;

		// �ð� ����� ���� ���ο� ûũ �׷���� ���� ���� ��Ͽ� �߰�
		if (elapsedTime >= midGameUnlockTime && midGameChunkPrefabs != null && midGameChunkPrefabs.Length > 0)
		{
			availableChunks.AddRange(midGameChunkPrefabs); // Chunk_5 �߰�
		}
		if (elapsedTime >= lateGameUnlockTime1 && lateGameChunkPrefabs1 != null && lateGameChunkPrefabs1.Length > 0)
		{
			availableChunks.AddRange(lateGameChunkPrefabs1); // Chunk_6 �߰�
		}
		if (elapsedTime >= lateGameUnlockTime2 && lateGameChunkPrefabs2 != null && lateGameChunkPrefabs2.Length > 0)
		{
			availableChunks.AddRange(lateGameChunkPrefabs2); // Chunk_7 �߰�
		}

		if (availableChunks.Count == 0)
		{
			Debug.LogWarning("MapGenerator: No available chunk prefabs to choose from for random spawning!");
			return null; // ��� ������ ûũ�� ������ null ��ȯ
		}

		// ��� ������ ûũ Ǯ �߿��� �������� �ϳ� ����
		return availableChunks[Random.Range(0, availableChunks.Count)];
	}

	/// <summary>
	/// �������� ���õ� ûũ�� �����մϴ�.
	/// </summary>
	void SpawnRandomChunk()
	{
		GameObject selectedChunkPrefab = GetRandomAvailableChunkPrefab();
		if (selectedChunkPrefab == null)
		{
			Debug.LogWarning("MapGenerator: No available chunk prefabs to spawn!");
			return;
		}
		SpawnSpecificChunk(selectedChunkPrefab); // ���õ� ûũ�� ����
	}

	/// <summary>
	/// Ư�� �� ûũ �������� �����ϰ� activeChunks ����Ʈ�� �߰��ϸ�, ûũ �� ���� ������ ������Ʈ�� ��ġ�մϴ�.
	/// </summary>
	/// <param name="chunkPrefabToSpawn">������ �� ûũ ������.</param>
	void SpawnSpecificChunk(GameObject chunkPrefabToSpawn)
	{
		// ûũ �����տ� ChunkData ������Ʈ�� �ִ��� �ٽ� Ȯ��
		ChunkData chunkData = chunkPrefabToSpawn.GetComponent<ChunkData>();
		if (chunkData == null)
		{
			Debug.LogError($"MapGenerator: Chunk prefab '{chunkPrefabToSpawn.name}' is missing ChunkData component! Cannot get spawn settings. Please add it.", chunkPrefabToSpawn);
			return;
		}

		// ûũ�� nextSpawnPoint ��ġ(���� ��ǥ)�� �����մϴ�. Quaternion.identity�� ȸ�� ����
		GameObject newChunk = Instantiate(chunkPrefabToSpawn, nextSpawnPoint, Quaternion.identity);
		activeChunks.Add(newChunk); // Ȱ�� ûũ ����Ʈ�� �߰�

		// ������ ûũ ������ "EndPoint"��� �̸��� �ڽ� ������Ʈ�� ã���ϴ�.
		// �� EndPoint�� ���� ��ġ�� ���� ûũ�� ���� ������ �˴ϴ�.
		Transform chunkEndPoint = newChunk.transform.Find("EndPoint");
		if (chunkEndPoint != null)
		{
			nextSpawnPoint = chunkEndPoint.position; // ���� ���� ���� ������Ʈ
		}
		else
		{
			// EndPoint�� ������ ��� �޽����� ����, ������ ���� ������ ���� ���� ������ �����մϴ�.
			Debug.LogWarning($"Chunk prefab '{chunkPrefabToSpawn.name}' does not have an 'EndPoint' child. Please add one for proper chaining. Assuming a fixed length of 20 units for now.", chunkPrefabToSpawn);
			nextSpawnPoint += new Vector3(20f, 0, 0); // �� ���� ���� ûũ�� ���̿� ���� �����ؾ� �մϴ�.
		}

		// --- ���� ������ ������Ʈ ��ġ ---
		// ���� ûũ���� ������ �� ������Ʈ ������ ������ �����ϴ� ����
		int spawnedCoinsInChunk = 0;
		int spawnedItemsInChunk = 0;
		int spawnedMonstersInChunk = 0;

		// ������ ûũ�� ��� �ڽ� ������Ʈ�� ��ȸ�ϸ� ���� ������ ã���ϴ�.
		foreach (Transform child in newChunk.transform)
		{
			// �� ���� ����Ʈ�� �̸��� ���� ������ ������Ʈ Ÿ���� ����
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
			// �ٸ� �̸��� ���� ����Ʈ�� �ִٸ� ���⿡ �߰����� else if ����� ���� �� �ֽ��ϴ�.
		}
	}

	/// <summary>
	/// ������ ��ġ�� ������ �����մϴ�.
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
	/// ������ ��ġ�� �������� �����մϴ�.
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
	/// ������ ��ġ�� ���͸� �����մϴ�.
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
	/// �÷��̾� �ڷ� �ָ� ������ ûũ���� �����մϴ�.
	/// </summary>
	void DespawnChunks()
	{
		// ����Ʈ�� �ڿ������� ��ȸ�Ͽ� ������Ʈ ���� �� �ε��� ���� �߻��� �����մϴ�.
		for (int i = activeChunks.Count - 1; i >= 0; i--)
		{
			GameObject chunk = activeChunks[i];
			// ûũ�� ��ġ�� �÷��̾� X ��ġ���� ���� �Ÿ� �̻� �ڷ� ���� ����
			if (chunk.transform.position.x < playerTransform.position.x - despawnBehindDistance)
			{
				activeChunks.RemoveAt(i); // ����Ʈ���� ����
				Destroy(chunk); // ���� ������Ʈ �ı� (�޸� ����)
			}
		}
	}

	/// <summary>
	/// ûũ ������ �迭�� ChunkData ������Ʈ�� �Ҵ�Ǿ����� Ȯ���մϴ�.
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
	/// ���� ûũ �����տ� ChunkData ������Ʈ�� �Ҵ�Ǿ����� Ȯ���մϴ�.
	/// </summary>
	private void CheckChunkDataAssignment(GameObject prefab)
	{
		if (prefab != null && prefab.GetComponent<ChunkData>() == null)
		{
			Debug.LogError($"MapGenerator: Chunk prefab '{prefab.name}' is missing the ChunkData script! Please add it to the chunk's root GameObject.", prefab);
		}
	}
}
