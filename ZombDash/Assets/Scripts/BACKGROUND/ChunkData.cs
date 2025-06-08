using UnityEngine;

public class ChunkData : MonoBehaviour
{
	[Header("Spawn Probabilities (0 to 1)")]
	[Range(0f, 1f)][SerializeField] public float CoinSpawnChance = 0.8f; // �� ûũ�� ���� ���� Ȯ��
	[Range(0f, 1f)][SerializeField] public float ItemSpawnChance = 0.2f; // �� ûũ�� ������ ���� Ȯ��
	[Range(0f, 1f)][SerializeField] public float MonsterSpawnChance = 0.3f; // �� ûũ�� ���� ���� Ȯ��

	[Header("Spawn Limits Per Chunk")]
	[SerializeField] public int MaxCoinsPerChunk = 5; // �� ûũ���� ������ �� �ִ� �ִ� ���� ��
	[SerializeField] public int MaxItemsPerChunk = 2; // �� ûũ���� ������ �� �ִ� �ִ� ������ ��
	[SerializeField] public int MaxMonstersPerChunk = 3; // �� ûũ���� ������ �� �ִ� �ִ� ���� ��

	// (���� ����) EndPoint ������ ���⿡ �θ� MapGenerator���� Find("EndPoint")�� ���� �ʰ� ���� ���� ����
	// [SerializeField] private Transform endPoint;
	// public Transform EndPoint => endPoint;

	// ChunkData ������Ʈ�� ���� ûũ �������� MapGenerator�� �Ҵ�Ǹ� ������ �߻��ϹǷ�,
	// �� ��ũ��Ʈ�� ��� ûũ �������� ��Ʈ�� �߰��ؾ� �մϴ�.
}
