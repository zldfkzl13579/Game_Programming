using UnityEngine;

// Zombie.cs를 상속하여 기본 좀비 기능을 가져옴
public class ProjectileZombie : Zombie
{
	[Header("Projectile Zombie Settings")]
	[SerializeField] private GameObject zombieProjectilePrefab; // 발사할 발사체 프리팹
	[SerializeField] private Transform projectileSpawnPoint; // 발사체가 생성될 위치
	[SerializeField] private float fireRate = 2f; // 발사 간격 (초)
	[SerializeField] private float detectionRange = 10f; // 플레이어를 감지하는 거리

	private Transform playerTarget; // 플레이어의 Transform 참조
	private float nextFireTime;

	protected override void Awake()
	{
		base.Awake(); // 부모 클래스의 Awake 호출

		// 발사 좀비는 멈춰있으므로 이동 속도를 0으로 설정
		moveSpeed = 0f;

		// Rigidbody2D가 있다면 물리 영향을 받지 않도록 Kinematic으로 설정 (정지 상태 유지)
		if (rb != null)
		{
			rb.bodyType = RigidbodyType2D.Kinematic;
			rb.linearVelocity = Vector2.zero; // 혹시 모를 초기 속도 제거
		}

		// 플레이어 Transform을 GameManager 등에서 가져올 수도 있지만, 여기서는 FindFirstObjectByType으로 찾음
		// 경고 메시지에 따라 FindObjectOfType 대신 FindFirstObjectByType 사용
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

	void Update() // 발사 로직은 Update에서 시간 기반으로 처리
	{
		if (playerTarget == null || playerTarget.GetComponent<PlayerController>().IsDead) return;

		// 플레이어가 감지 범위 내에 있고, 다음 발사 시간이 되면 발사
		float distanceToPlayer = Vector2.Distance(transform.position, playerTarget.position);
		if (distanceToPlayer <= detectionRange && Time.time >= nextFireTime)
		{
			FireProjectile();
			nextFireTime = Time.time + fireRate;
		}
	}

	// 발사체 발사 로직
	private void FireProjectile()
	{
		if (zombieProjectilePrefab != null && projectileSpawnPoint != null && playerTarget != null)
		{
			GameObject projectile = Instantiate(zombieProjectilePrefab, projectileSpawnPoint.position, Quaternion.identity);
			ZombieProjectile projScript = projectile.GetComponent<ZombieProjectile>();
			if (projScript != null)
			{
				// 발사체에게 플레이어 목표를 전달하여 추적하도록 설정
				projScript.SetTarget(playerTarget);
			}
			Debug.Log("Projectile Zombie fired a projectile!");
			// (TODO) 발사 애니메이션, 효과음 등 추가
		}
	}

	// 좀비 발사체는 충돌 시 플레이어에게 피해를 주므로, 기본 OnTriggerEnter2D는 유지
	// 플레이어 총알에 대한 TakeDamage는 Zombie.cs에서 처리됨
}
