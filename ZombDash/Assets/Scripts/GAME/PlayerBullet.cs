using UnityEngine;

public class PlayerBullet : MonoBehaviour
{
	[Header("Bullet Settings")]
	[SerializeField] private float speed = 15f; // 총알 속도
	[SerializeField] public int damage = 1; // 총알 피해량 (public으로 PlayerController에서 접근 가능)
	[SerializeField] private float lifeTime = 2f; // 총알이 자동으로 사라질 시간

	void Start()
	{
		// Rigidbody2D가 있다면 AddForce로 발사하거나 velocity를 설정할 수 있지만,
		// 여기서는 간단히 transform.Translate로 이동합니다.
		// Rigidbody2D가 있다면 중력 등을 무시하도록 Kinematic으로 설정하는 것이 좋습니다.
		Destroy(gameObject, lifeTime); // 일정 시간 후 자동으로 파괴
	}

	void Update()
	{
		// 오른쪽으로 이동
		transform.Translate(Vector2.right * speed * Time.deltaTime);
	}

	void OnTriggerEnter2D(Collider2D other)
	{
		// 좀비에게 피해를 주거나 장애물에 맞으면 파괴
		if (other.CompareTag("Enemy"))
		{
			Zombie zombie = other.GetComponent<Zombie>();
			if (zombie != null)
			{
				zombie.TakeDamage(damage);
			}
			Destroy(gameObject); // 총알 파괴
		}
		else if (other.CompareTag("Obstacle")) // 장애물에 맞으면 파괴
		{
			// 이 시점에서 장애물에 피해를 줄지 말지는 Obstacle 스크립트와 논의가 필요
			// 현재는 그냥 총알이 파괴됨
			Destroy(gameObject); // 총알 파괴
		}
		// (TODO) 발사 효과음, 파티클 등 추가
	}
}
