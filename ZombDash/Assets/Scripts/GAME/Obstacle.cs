using UnityEngine;

public class Obstacle : MonoBehaviour
{
	// 장애물 종류를 구분하기 위한 Enum
	public enum ObstacleType
	{
		Cactus, // 선인장
		FallenStone // 쓰러진 돌
	}

	[Header("Obstacle Settings")]
	[SerializeField] private ObstacleType obstacleType; // 장애물의 종류
	[SerializeField] private int baseDamageToPlayer = 1; // 이 장애물이 플레이어에게 주는 기본 피해량 (Inspector에서 설정)

	// 플레이어 컨트롤러에서 접근 가능하도록 public 속성 추가
	public int DamageAmount
	{
		get
		{
			// 장애물 타입에 따른 기본 피해량 반환. 슬라이딩 여부는 PlayerController에서 판단
			return baseDamageToPlayer;
		}
	}

	// 장애물 타입을 외부에서 읽을 수 있도록 public 속성 추가
	public ObstacleType Type => obstacleType;


	void OnTriggerEnter2D(Collider2D other)
	{
		// 장애물이 플레이어에게 피해를 주는 로직은 PlayerController의 OnTriggerEnter2D에서 호출
		// 여기서는 직접 TakeDamage를 호출하지 않고, PlayerController가 Obstacle 컴포넌트를 통해 정보를 가져가도록 함
		// (선택 사항) 장애물이 플레이어에게 피해를 준 후 파괴되거나 사라지는 로직 추가
		// if (other.CompareTag("Player")) {
		//     Destroy(gameObject); // 예시: 한 번 피해를 준 후 사라짐
		// }
	}
}
