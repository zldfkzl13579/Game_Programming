// ScoreManager.cs ���Ͽ��� Update() �� AddScore() �޼��� ����
// UIManager �ν��Ͻ��� ����ϵ��� ����

using UnityEngine;

public class ScoreManager : MonoBehaviour
{
	public static ScoreManager Instance { get; private set; } // �̱��� ����

	[Header("Score Settings")]
	[SerializeField] private Transform playerTransform; // �÷��̾� Transform ���� (�̵� �Ÿ� ������)
	[SerializeField] private float distanceScoreMultiplier = 0.1f; // �̵� �Ÿ� 1���ִ� ��� ����
	[SerializeField] private float scoreUpdateInterval = 0.1f; // ���� ������Ʈ ���� (��)

	private float currentScore;
	private float lastPlayerXPosition;
	private float lastScoreUpdateTime;

	public float CurrentScore => currentScore; // �ܺο��� ���� �б��

	void Awake()
	{
		// �̱��� �ν��Ͻ� ����
		if (Instance != null && Instance != this)
		{
			Destroy(gameObject);
		}
		else
		{
			Instance = this;
			// DontDestroyOnLoad(gameObject); // �� ��ȯ �� �ı����� �ʵ��� (���� ����)
		}

		if (playerTransform == null) Debug.LogError("ScoreManager: Player Transform is not assigned!");
	}

	void Start()
	{
		currentScore = 0f;
		if (playerTransform != null)
		{
			lastPlayerXPosition = playerTransform.position.x;
		}
		lastScoreUpdateTime = Time.time;
		// �ʱ� UI ������Ʈ�� UIManager�� StartGamePlay()���� �� ���� ȣ��ǵ��� ����
	}

	void Update()
	{
		// �÷��̾� �̵� �Ÿ��� ���� ���� �߰�
		// ������ ���۵� �Ŀ��� ���� ������Ʈ (UIManager.StartGamePlay() ����)
		if (Time.timeScale > 0f && playerTransform != null && Time.time >= lastScoreUpdateTime + scoreUpdateInterval)
		{
			float distanceMoved = playerTransform.position.x - lastPlayerXPosition;
			AddScore(distanceMoved * distanceScoreMultiplier); // �� �޼��尡 UI ������Ʈ�� ȣ��
			lastPlayerXPosition = playerTransform.position.x;
			lastScoreUpdateTime = Time.time;
		}
	}

	public void AddScore(float amount)
	{
		currentScore += amount;
		Debug.Log($"Total Score: {currentScore}");
		// UI ������Ʈ ȣ��
		if (UIManager.Instance != null)
		{
			UIManager.Instance.UpdateScore(currentScore);
		}
	}

	// (TODO) �ְ� ���� ���� �� �ҷ����� ���� �߰�
}
