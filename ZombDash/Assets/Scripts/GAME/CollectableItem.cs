using UnityEngine;

public class CollectableItem : MonoBehaviour
{
	// ������ ������ �����ϱ� ���� Enum
	public enum ItemType
	{
		Score,
		Health,
		GuardHealth,
		Ammo,
		ScoreAndGuardHealth,
		ScoreAndHealth,
		ScoreAndGuardHealthAndHealth // ���� ������
	}

	[Header("Item Settings")]
	[SerializeField] private ItemType itemType; // �������� ����
	[SerializeField] private float value; // ����, ü��, ź�෮ �� (�Ҽ��� ���� ������ float)

	// ����Ʈ �� ���� (���� ����)
	[SerializeField] private GameObject collectEffectPrefab; // ȹ�� �� ��ƼŬ �� ������
	[SerializeField] private AudioClip collectSound; // ȹ�� �� ���� Ŭ��

	void OnTriggerEnter2D(Collider2D other)
	{
		// �÷��̾�� �浹�ߴ��� Ȯ�� (�±׸� "Collectable"�� ����)
		if (other.CompareTag("Player"))
		{
			PlayerController player = other.GetComponent<PlayerController>();
			if (player != null)
			{
				ApplyEffect(player); // �÷��̾�� ȿ�� ����
				PlayCollectEffectAndSound(); // �ð�/û�� ȿ�� ���
				Destroy(gameObject); // ������ �ı�
			}
		}
	}

	// PlayerController���� ���� �����ϵ��� public���� ����
	public void ApplyEffect(PlayerController player)
	{
		switch (itemType)
		{
			case ItemType.Score:
				player.AddScore(value);
				break;
			case ItemType.Health:
				player.GainHealth((int)value); // ü���� ������
				break;
			case ItemType.GuardHealth:
				player.GainGuardHealth((int)value); // ����ü���� ������
				break;
			case ItemType.Ammo:
				player.AddAmmo((int)value); // ź���� ������
				break;
			case ItemType.ScoreAndGuardHealth:
				player.AddScore(value);
				player.GainGuardHealth(1); // ����: ����ü���� ���� �����ۿ��� 1 �ο�
				break;
			case ItemType.ScoreAndHealth:
				player.AddScore(value);
				player.GainHealth(1); // ����: ü���� ���� �����ۿ��� 1 �ο�
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
