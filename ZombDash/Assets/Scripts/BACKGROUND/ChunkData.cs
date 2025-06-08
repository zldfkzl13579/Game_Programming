using UnityEngine;

public class ChunkData : MonoBehaviour
{
	[Header("Spawn Probabilities (0 to 1)")]
	[Range(0f, 1f)][SerializeField] public float CoinSpawnChance = 0.8f; // 이 청크의 코인 스폰 확률
	[Range(0f, 1f)][SerializeField] public float ItemSpawnChance = 0.2f; // 이 청크의 아이템 스폰 확률
	[Range(0f, 1f)][SerializeField] public float MonsterSpawnChance = 0.3f; // 이 청크의 몬스터 스폰 확률

	[Header("Spawn Limits Per Chunk")]
	[SerializeField] public int MaxCoinsPerChunk = 5; // 이 청크에서 생성될 수 있는 최대 코인 수
	[SerializeField] public int MaxItemsPerChunk = 2; // 이 청크에서 생성될 수 있는 최대 아이템 수
	[SerializeField] public int MaxMonstersPerChunk = 3; // 이 청크에서 생성될 수 있는 최대 몬스터 수

	// (선택 사항) EndPoint 참조를 여기에 두면 MapGenerator에서 Find("EndPoint")를 하지 않고 직접 접근 가능
	// [SerializeField] private Transform endPoint;
	// public Transform EndPoint => endPoint;

	// ChunkData 컴포넌트가 없는 청크 프리팹이 MapGenerator에 할당되면 에러가 발생하므로,
	// 이 스크립트를 모든 청크 프리팹의 루트에 추가해야 합니다.
}
