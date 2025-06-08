using UnityEngine;

public class ZombieProjectile : MonoBehaviour
{
	[Header("Projectile Settings")]
	[SerializeField] public int damageToPlayer = 2; // 플레이어에게 주는 피해량
	[SerializeField] private float speed = 5f; // 발사체 속도
	[SerializeField] private float lifeTime = 4f; // 발사체가 자동으로 사라질 시간

	private Transform target; // 플레이어의 Transform 참조

	void Start()
	{
		Destroy(gameObject, lifeTime); // 일정 시간 후 자동으로 파괴
	}

	public void SetTarget(Transform playerTransform)
	{
		target = playerTransform;
		// (선택 사항) 발사 시 플레이어 방향으로 즉시 회전
		// if (target != null)
		// {
		//     Vector2 direction = (target.position - transform.position).normalized;
		//     float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
		//     transform.rotation = Quaternion.Euler(new Vector3(0, 0, angle));
		// }
	}

	void Update()
	{
		if (target != null && target.GetComponent<PlayerController>().IsDead == false) // 플레이어가 살아있다면 추적
		{
			// 플레이어를 향해 서서히 이동 (간단한 추적)
			transform.position = Vector2.MoveTowards(transform.position, target.position, speed * Time.deltaTime);
		}
		else // 플레이어가 죽었거나 타겟이 없으면 그냥 직진하거나 멈춤
		{
			// 중력 영향 없음, 그냥 현재 방향으로 진행 (또는 멈춤)
			// transform.Translate(Vector2.left * speed * Time.deltaTime); // 예시: 왼쪽으로 직진
		}
	}

	// 플레이어와의 충돌 (OnTriggerEnter2D는 PlayerController에서 감지)
	void OnTriggerEnter2D(Collider2D other)
	{
		// 발사체는 근접 공격으로만 파괴되므로, 일반 충돌 시 자신을 파괴하지 않음
		// PlayerController에서 이 발사체와 충돌 시 TakeDamage 호출 후 DestroyerProjectile()을 호출해야 함
	}

	// 근접 공격으로 파괴될 때 호출될 메서드 (PlayerController에서 호출)
	public void DestroyProjectile()
	{
		Debug.Log("Zombie Projectile destroyed by melee attack.");
		// (TODO) 파괴 효과음, 파티클 등
		Destroy(gameObject);
	}
}
