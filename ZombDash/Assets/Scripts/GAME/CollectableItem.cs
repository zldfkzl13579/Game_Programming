using UnityEngine;

public class CollectableItem : MonoBehaviour
{
	// 아이템 종류를 구분하기 위한 Enum
	public enum ItemType
	{
		Score,
		Health,
		GuardHealth,
		Ammo,
		ScoreAndGuardHealth,
		ScoreAndHealth,
		ScoreAndGuardHealthAndHealth // 복합 아이템
	}

	[Header("Item Settings")]
	[SerializeField] private ItemType itemType; // 아이템의 종류
	[SerializeField] private float value; // 점수, 체력, 탄약량 등 (소수점 점수 때문에 float)

	// 이펙트 및 사운드 (선택 사항)
	[SerializeField] private GameObject collectEffectPrefab; // 획득 시 파티클 등 프리팹
	[SerializeField] private AudioClip collectSound; // 획득 시 사운드 클립

	void OnTriggerEnter2D(Collider2D other)
	{
		// 플레이어와 충돌했는지 확인 (태그를 "Collectable"로 설정)
		if (other.CompareTag("Player"))
		{
			PlayerController player = other.GetComponent<PlayerController>();
			if (player != null)
			{
				ApplyEffect(player); // 플레이어에게 효과 적용
				PlayCollectEffectAndSound(); // 시각/청각 효과 재생
				Destroy(gameObject); // 아이템 파괴
			}
		}
	}

	// PlayerController에서 접근 가능하도록 public으로 변경
	public void ApplyEffect(PlayerController player)
	{
		switch (itemType)
		{
			case ItemType.Score:
				player.AddScore(value);
				break;
			case ItemType.Health:
				player.GainHealth((int)value); // 체력은 정수형
				break;
			case ItemType.GuardHealth:
				player.GainGuardHealth((int)value); // 가드체력은 정수형
				break;
			case ItemType.Ammo:
				player.AddAmmo((int)value); // 탄약은 정수형
				break;
			case ItemType.ScoreAndGuardHealth:
				player.AddScore(value);
				player.GainGuardHealth(1); // 예시: 가드체력은 복합 아이템에서 1 부여
				break;
			case ItemType.ScoreAndHealth:
				player.AddScore(value);
				player.GainHealth(1); // 예시: 체력은 복합 아이템에서 1 부여
				break;
			case ItemType.ScoreAndGuardHealthAndHealth:
				player.AddScore(value);
				player.GainGuardHealth(1);
				player.GainHealth(1);
				break;
		}
	}

	private void PlayCollectEffectAndSound()
	{
		if (collectEffectPrefab != null)
		{
			Instantiate(collectEffectPrefab, transform.position, Quaternion.identity);
		}
		if (collectSound != null)
		{
			AudioSource.PlayClipAtPoint(collectSound, transform.position);
		}
	}
}
