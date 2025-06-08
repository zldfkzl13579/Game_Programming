using UnityEngine;

public class ZombieProjectile : MonoBehaviour
{
	[Header("Projectile Settings")]
	[SerializeField] public int damageToPlayer = 2; // �÷��̾�� �ִ� ���ط�
	[SerializeField] private float speed = 5f; // �߻�ü �ӵ�
	[SerializeField] private float lifeTime = 4f; // �߻�ü�� �ڵ����� ����� �ð�

	private Transform target; // �÷��̾��� Transform ����

	void Start()
	{
		Destroy(gameObject, lifeTime); // ���� �ð� �� �ڵ����� �ı�
	}

	public void SetTarget(Transform playerTransform)
	{
		target = playerTransform;
		// (���� ����) �߻� �� �÷��̾� �������� ��� ȸ��
		// if (target != null)
		// {
		//     Vector2 direction = (target.position - transform.position).normalized;
		//     float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
		//     transform.rotation = Quaternion.Euler(new Vector3(0, 0, angle));
		// }
	}

	void Update()
	{
		if (target != null && target.GetComponent<PlayerController>().IsDead == false) // �÷��̾ ����ִٸ� ����
		{
			// �÷��̾ ���� ������ �̵� (������ ����)
			transform.position = Vector2.MoveTowards(transform.position, target.position, speed * Time.deltaTime);
		}
		else // �÷��̾ �׾��ų� Ÿ���� ������ �׳� �����ϰų� ����
		{
			// �߷� ���� ����, �׳� ���� �������� ���� (�Ǵ� ����)
			// transform.Translate(Vector2.left * speed * Time.deltaTime); // ����: �������� ����
		}
	}

	// �÷��̾���� �浹 (OnTriggerEnter2D�� PlayerController���� ����)
	void OnTriggerEnter2D(Collider2D other)
	{
		// �߻�ü�� ���� �������θ� �ı��ǹǷ�, �Ϲ� �浹 �� �ڽ��� �ı����� ����
		// PlayerController���� �� �߻�ü�� �浹 �� TakeDamage ȣ�� �� DestroyerProjectile()�� ȣ���ؾ� ��
	}

	// ���� �������� �ı��� �� ȣ��� �޼��� (PlayerController���� ȣ��)
	public void DestroyProjectile()
	{
		Debug.Log("Zombie Projectile destroyed by melee attack.");
		// (TODO) �ı� ȿ����, ��ƼŬ ��
		Destroy(gameObject);
	}
}
