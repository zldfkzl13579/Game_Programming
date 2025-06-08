// ScoreManager.cs 파일에서 Update() 및 AddScore() 메서드 수정
// UIManager 인스턴스를 사용하도록 변경

using UnityEngine;

public class ScoreManager : MonoBehaviour
{
	public static ScoreManager Instance { get; private set; } // 싱글톤 패턴

	[Header("Score Settings")]
	[SerializeField] private Transform playerTransform; // 플레이어 Transform 참조 (이동 거리 점수용)
	[SerializeField] private float distanceScoreMultiplier = 0.1f; // 이동 거리 1유닛당 얻는 점수
	[SerializeField] private float scoreUpdateInterval = 0.1f; // 점수 업데이트 간격 (초)

	private float currentScore;
	private float lastPlayerXPosition;
	private float lastScoreUpdateTime;

	public float CurrentScore => currentScore; // 외부에서 점수 읽기용

	void Awake()
	{
		// 싱글톤 인스턴스 설정
		if (Instance != null && Instance != this)
		{
			Destroy(gameObject);
		}
		else
		{
			Instance = this;
			// DontDestroyOnLoad(gameObject); // 씬 전환 시 파괴되지 않도록 (선택 사항)
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
		// 초기 UI 업데이트는 UIManager의 StartGamePlay()에서 한 번만 호출되도록 변경
	}

	void Update()
	{
		// 플레이어 이동 거리에 따른 점수 추가
		// 게임이 시작된 후에만 점수 업데이트 (UIManager.StartGamePlay() 이후)
		if (Time.timeScale > 0f && playerTransform != null && Time.time >= lastScoreUpdateTime + scoreUpdateInterval)
		{
			float distanceMoved = playerTransform.position.x - lastPlayerXPosition;
			AddScore(distanceMoved * distanceScoreMultiplier); // 이 메서드가 UI 업데이트도 호출
			lastPlayerXPosition = playerTransform.position.x;
			lastScoreUpdateTime = Time.time;
		}
	}

	public void AddScore(float amount)
	{
		currentScore += amount;
		Debug.Log($"Total Score: {currentScore}");
		// UI 업데이트 호출
		if (UIManager.Instance != null)
		{
			UIManager.Instance.UpdateScore(currentScore);
		}
	}

	// (TODO) 최고 점수 저장 및 불러오기 로직 추가
}
