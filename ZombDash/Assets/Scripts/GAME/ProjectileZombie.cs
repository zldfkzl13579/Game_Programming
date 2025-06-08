using UnityEngine;

// Zombie.cs�� ����Ͽ� �⺻ ���� ����� ������
public class ProjectileZombie : Zombie
{
	[Header("Projectile Zombie Settings")]
	[SerializeField] private GameObject zombieProjectilePrefab; // �߻��� �߻�ü ������
	[SerializeField] private Transform projectileSpawnPoint; // �߻�ü�� ������ ��ġ
	[SerializeField] private float fireRate = 2f; // �߻� ���� (��)
	[SerializeField] private float detectionRange = 10f; // �÷��̾ �����ϴ� �Ÿ�

	private Transform playerTarget; // �÷��̾��� Transform ����
	private float nextFireTime;

	protected override void Awake()
	{
		base.Awake(); // �θ� Ŭ������ Awake ȣ��

		// �߻� ����� ���������Ƿ� �̵� �ӵ��� 0���� ����
		moveSpeed = 0f;

		// Rigidbody2D�� �ִٸ� ���� ������ ���� �ʵ��� Kinematic���� ���� (���� ���� ����)
		if (rb != null)
		{
			rb.bodyType = RigidbodyType2D.Kinematic;
			rb.linearVelocity = Vector2.zero; // Ȥ�� �� �ʱ� �ӵ� ����
		}

		// �÷��̾� Transform�� GameManager ��� ������ ���� ������, ���⼭�� FindFirstObjectByType���� ã��
		// ��� �޽����� ���� FindObjectOfType ��� FindFirstObjectByType ���
		PlayerController playerController = FindFirstObjectByType<PlayerController>(); // 
		if (playerController != null)
		{
			playerTarget = playerController.transform;
		}
		else
		{
			Debug.LogWarning("PlayerController not found for Projectile Zombie targeting!");
		}

		if (zombieProjectilePrefab == null) Debug.LogError("Projectile Zombie: Projectile Prefab not assigned!");
		if (projectileSpawnPoint == null) Debug.LogError("Projectile Zombie: Projectile Spawn Point not assigned!");
	}

	void Update() // �߻� ������ Update���� �ð� ������� ó��
	{
		if (playerTarget == null || playerTarget.GetComponent<PlayerController>().IsDead) return;

		// �÷��̾ ���� ���� ���� �ְ�, ���� �߻� �ð��� �Ǹ� �߻�
		float distanceToPlayer = Vector2.Distance(transform.position, playerTarget.position);
		if (distanceToPlayer <= detectionRange && Time.time >= nextFireTime)
		{
			FireProjectile();
			nextFireTime = Time.time + fireRate;
		}
	}

	// �߻�ü �߻� ����
	private void FireProjectile()
	{
		if (zombieProjectilePrefab != null && projectileSpawnPoint != null && playerTarget != null)
		{
			GameObject projectile = Instantiate(zombieProjectilePrefab, projectileSpawnPoint.position, Quaternion.identity);
			ZombieProjectile projScript = projectile.GetComponent<ZombieProjectile>();
			if (projScript != null)
			{
				// �߻�ü���� �÷��̾� ��ǥ�� �����Ͽ� �����ϵ��� ����
				projScript.SetTarget(playerTarget);
			}
			Debug.Log("Projectile Zombie fired a projectile!");
			// (TODO) �߻� �ִϸ��̼�, ȿ���� �� �߰�
		}
	}

	// ���� �߻�ü�� �浹 �� �÷��̾�� ���ظ� �ֹǷ�, �⺻ OnTriggerEnter2D�� ����
	// �÷��̾� �Ѿ˿� ���� TakeDamage�� Zombie.cs���� ó����
}
