using UnityEngine;

public class Obstacle : MonoBehaviour
{
	// ��ֹ� ������ �����ϱ� ���� Enum
	public enum ObstacleType
	{
		Cactus, // ������
		FallenStone // ������ ��
	}

	[Header("Obstacle Settings")]
	[SerializeField] private ObstacleType obstacleType; // ��ֹ��� ����
	[SerializeField] private int baseDamageToPlayer = 1; // �� ��ֹ��� �÷��̾�� �ִ� �⺻ ���ط� (Inspector���� ����)

	// �÷��̾� ��Ʈ�ѷ����� ���� �����ϵ��� public �Ӽ� �߰�
	public int DamageAmount
	{
		get
		{
			// ��ֹ� Ÿ�Կ� ���� �⺻ ���ط� ��ȯ. �����̵� ���δ� PlayerController���� �Ǵ�
			return baseDamageToPlayer;
		}
	}

	// ��ֹ� Ÿ���� �ܺο��� ���� �� �ֵ��� public �Ӽ� �߰�
	public ObstacleType Type => obstacleType;


	void OnTriggerEnter2D(Collider2D other)
	{
		// ��ֹ��� �÷��̾�� ���ظ� �ִ� ������ PlayerController�� OnTriggerEnter2D���� ȣ��
		// ���⼭�� ���� TakeDamage�� ȣ������ �ʰ�, PlayerController�� Obstacle ������Ʈ�� ���� ������ ���������� ��
		// (���� ����) ��ֹ��� �÷��̾�� ���ظ� �� �� �ı��ǰų� ������� ���� �߰�
		// if (other.CompareTag("Player")) {
		//     Destroy(gameObject); // ����: �� �� ���ظ� �� �� �����
		// }
	}
}
