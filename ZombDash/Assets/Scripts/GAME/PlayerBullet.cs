using UnityEngine;

public class PlayerBullet : MonoBehaviour
{
	[Header("Bullet Settings")]
	[SerializeField] private float speed = 15f; // �Ѿ� �ӵ�
	[SerializeField] public int damage = 1; // �Ѿ� ���ط� (public���� PlayerController���� ���� ����)
	[SerializeField] private float lifeTime = 2f; // �Ѿ��� �ڵ����� ����� �ð�

	void Start()
	{
		// Rigidbody2D�� �ִٸ� AddForce�� �߻��ϰų� velocity�� ������ �� ������,
		// ���⼭�� ������ transform.Translate�� �̵��մϴ�.
		// Rigidbody2D�� �ִٸ� �߷� ���� �����ϵ��� Kinematic���� �����ϴ� ���� �����ϴ�.
		Destroy(gameObject, lifeTime); // ���� �ð� �� �ڵ����� �ı�
	}

	void Update()
	{
		// ���������� �̵�
		transform.Translate(Vector2.right * speed * Time.deltaTime);
	}

	void OnTriggerEnter2D(Collider2D other)
	{
		// ���񿡰� ���ظ� �ְų� ��ֹ��� ������ �ı�
		if (other.CompareTag("Enemy"))
		{
			Zombie zombie = other.GetComponent<Zombie>();
			if (zombie != null)
			{
				zombie.TakeDamage(damage);
			}
			Destroy(gameObject); // �Ѿ� �ı�
		}
		else if (other.CompareTag("Obstacle")) // ��ֹ��� ������ �ı�
		{
			// �� �������� ��ֹ��� ���ظ� ���� ������ Obstacle ��ũ��Ʈ�� ���ǰ� �ʿ�
			// ����� �׳� �Ѿ��� �ı���
			Destroy(gameObject); // �Ѿ� �ı�
		}
		// (TODO) �߻� ȿ����, ��ƼŬ �� �߰�
	}
}
