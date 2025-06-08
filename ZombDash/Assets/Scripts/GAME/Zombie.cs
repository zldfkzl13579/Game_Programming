using UnityEngine;

public class Zombie : MonoBehaviour
{
	[Header("Zombie Stats")]
	[SerializeField] protected int zombieHealth = 1; // ���� ü�� (protected�� �ڽ� Ŭ�������� ���� ����)
	[SerializeField] protected float moveSpeed = 1f; // ���� �̵� �ӵ� (�Ϲ� �����)

	// �÷��̾� ��Ʈ�ѷ����� ���� �����ϵ��� public �Ӽ� �߰�
	[field: SerializeField] public int DamageOnCollisionToPlayer { get; protected set; } = 1; // �÷��̾�� �浹 �� �ִ� ����

	protected Rigidbody2D rb; // ������ Rigidbody2D
	protected Animator anim; // ������ Animator (���� ����)

	protected virtual void Awake() // Start ���� ������Ʈ ����
	{
		rb = GetComponent<Rigidbody2D>();
		anim = GetComponent<Animator>();

		if (rb == null) Debug.LogWarning($"Zombie {name} missing Rigidbody2D!");
	}

	protected virtual void FixedUpdate()
	{
		// �Ϲ� ����� �����̹Ƿ� �⺻ �̵� ���� �߰�
		if (rb != null)
		{
			rb.linearVelocity = new Vector2(-moveSpeed, rb.linearVelocity.y); // �������� �̵�
		}
	}

	// �÷��̾ �Ѿ˰��� �浹 ó��
	protected virtual void OnTriggerEnter2D(Collider2D other)
	{
		// �÷��̾� �Ѿ˿� �¾��� ��
		if (other.CompareTag("PlayerBullet"))
		{
			PlayerBullet bullet = other.GetComponent<PlayerBullet>();
			if (bullet != null)
			{
				TakeDamage(bullet.damage); // �Ѿ��� ���ط���ŭ ������
				Destroy(other.gameObject); // �Ѿ� �ı�
				Debug.Log($"Zombie hit by PlayerBullet. Took {bullet.damage} damage.");
			}
		}
		// �÷��̾���� ���� �浹�� PlayerController���� �����ϰ� TakeDamage�� ȣ���մϴ�.
		// ���� ������ PlayerController���� OverlapCircle�� �����ϹǷ� ���⼭�� ó������ �ʽ��ϴ�.
	}

	// ���ظ� �Դ� �޼���
	public virtual void TakeDamage(int damage)
	{
		zombieHealth -= damage;
		Debug.Log($"Zombie took {damage} damage. Remaining Health: {zombieHealth}");
		// (TODO) ���� �ǰ� �ִϸ��̼�/ȿ��

		if (zombieHealth <= 0)
		{
			Die();
		}
	}

	// ���� ��� ó��
	protected virtual void Die()
	{
		Debug.Log("Zombie Died!");
		// (TODO) ���� ��� �ִϸ��̼�/ȿ�� (��: ��ƼŬ, ����)
		Destroy(gameObject); // ���� ������Ʈ �ı�
	}
}
