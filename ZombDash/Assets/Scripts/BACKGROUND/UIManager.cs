using UnityEngine;
using UnityEngine.UI; // UI 관련 기능을 사용하기 위해 필요
using UnityEngine.SceneManagement; // 씬 관리 기능을 사용하기 위해 필요
using TMPro; // TextMeshPro를 사용하기 위해 필요
using System.Collections; // 코루틴을 사용하기 위해 필요

public class UIManager : MonoBehaviour
{
	public static UIManager Instance { get; private set; } // 싱글톤 패턴

	[Header("UI Panels")]
	[SerializeField] private GameObject gameOverPanel; // 게임 오버 패널 GameObject
	[SerializeField] private GameObject introPanel; // 인트로 패널 GameObject
	[SerializeField] private GameObject pausePanel; // 일시 정지 패널 GameObject

	[Header("Game Over Buttons")]
	[SerializeField] private Button gameOverRestartButton; // 재시작 버튼 (게임 오버 패널)
	[SerializeField] private Button gameOverMainMenuButton; // 메인 화면 버튼 (게임 오버 패널)

	[Header("Intro Screen Elements")]
	[SerializeField] private TextMeshProUGUI pressAnyKeyText; // "아무키나 누르세요" 텍스트
	[SerializeField] private float blinkSpeed = 0.5f; // 텍스트 깜빡이는 속도 (초)

	[Header("Pause Menu Buttons")] // 일시 정지 메뉴 버튼
	[SerializeField] private Button pauseToggleButton; // 게임 중앙 상단 일시정지 토글 버튼
	[SerializeField] private Button pauseMenuRestartButton; // 일시정지 메뉴의 재시작 버튼
	[SerializeField] private Button resumeButton; // 재개 버튼 (일시 정지 패널)
	[SerializeField] private Button inGameMainMenuButton; // 인게임 메인 메뉴 버튼 (일시 정지 패널)

	// UI 텍스트 참조
	[Header("UI References")]
	[SerializeField] private TextMeshProUGUI scoreText;
	[SerializeField] private TextMeshProUGUI healthText;
	[SerializeField] private TextMeshProUGUI guardHealthText;
	[SerializeField] private TextMeshProUGUI ammoText;
	[SerializeField] private TextMeshProUGUI finalScoreText; // 게임 오버 패널의 최종 점수 텍스트

	[Header("Scene Names")]
	[SerializeField] private string gamePlaySceneName = "GAME_PLAY";
	[SerializeField] private string mainMenuSceneName = "MAIN_MENU_SCENE_NAME";

	// 매니저 참조
	[SerializeField] private PlayerController playerController;
	[SerializeField] private ScoreManager scoreManager;

	[Header("Audio Clips")] // 오디오 클립 추가
	[SerializeField] private AudioClip gameStartSound; // 게임 시작 사운드
	private AudioSource uiAudioSource; // UI AudioSource 컴포넌트

	public bool IsGameStarted { get; private set; } = false; // 외부에서 게임 시작 여부 확인 가능
	private bool isGamePaused = false; // 게임이 일시 정지 상태인지

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

		// AudioSource 컴포넌트 참조 (UI Manager 오브젝트에 AudioSource 추가)
		if (uiAudioSource == null) uiAudioSource = GetComponent<AudioSource>();
		// 모든 할당이 Inspector에서 이루어지므로, Awake에서는 필수 참조만 확인
		// 만약 AudioSource가 없다면 오류 메시지
		if (uiAudioSource == null) Debug.LogError("UIManager: AudioSource component not found on UIManager GameObject!");


		// 모든 Inspector 참조 필드 확인 (null 여부)
		// 패널
		if (gameOverPanel == null) Debug.LogError("UIManager: GameOverPanel is not assigned!");
		if (introPanel == null) Debug.LogError("UIManager: IntroPanel is not assigned!");
		if (pausePanel == null) Debug.LogError("UIManager: PausePanel is not assigned!");

		// 게임 오버 버튼 (필수)
		if (gameOverRestartButton == null) Debug.LogError("UIManager: GameOverRestartButton is not assigned!");
		if (gameOverMainMenuButton == null) Debug.LogError("UIManager: GameOverMainMenuButton is not assigned!");

		// 인트로 텍스트 (필수)
		if (pressAnyKeyText == null) Debug.LogError("UIManager: PressAnyKeyText is not assigned!");

		// 일시정지 메뉴 버튼 (필수)
		if (pauseToggleButton == null) Debug.LogError("UIManager: PauseToggleButton is not assigned!");
		if (pauseMenuRestartButton == null) Debug.LogError("UIManager: PauseMenuRestartButton is not assigned!");
		if (resumeButton == null) Debug.LogError("UIManager: ResumeButton is not assigned!");
		if (inGameMainMenuButton == null) Debug.LogError("UIManager: InGameMainMenuButton is not assigned!");

		// UI 텍스트 (Score, Health, Guard, Ammo, FinalScore) (필수)
		if (scoreText == null) Debug.LogError("UIManager: Score Text not assigned in UI References!");
		if (healthText == null) Debug.LogError("UIManager: Health Text not assigned in UI References!");
		if (guardHealthText == null) Debug.LogError("UIManager: Guard Health Text not assigned in UI References!");
		if (ammoText == null) Debug.LogError("UIManager: Ammo Text not assigned in UI References!");
		if (finalScoreText == null) Debug.LogError("UIManager: Final Score Text not assigned in UI References!");

		// 매니저/컨트롤러 참조 (필수)
		if (playerController == null) Debug.LogError("Player Controller not assigned in UIManager!");
		if (scoreManager == null) Debug.LogError("Score Manager not assigned in UIManager!");
	}

	void Start()
	{
		// 모든 UI 패널은 비활성화 상태로 시작 (인트로 패널은 StartGamePlay() 전까지 활성화됨)
		if (gameOverPanel != null) gameOverPanel.SetActive(false);
		if (pausePanel != null) pausePanel.SetActive(false); // 초기 비활성화

		// === 게임 오버 버튼 리스너 추가 ===
		// 이 부분이 중요합니다! 할당된 버튼에만 리스너를 추가
		if (gameOverRestartButton != null)
		{
			gameOverRestartButton.onClick.AddListener(OnRestartButtonClicked);
			Debug.Log("Game Over Restart Button listener added."); // 디버그 로그 추가
		}
		if (gameOverMainMenuButton != null)
		{
			gameOverMainMenuButton.onClick.AddListener(OnMainMenuButtonClicked);
			Debug.Log("Game Over Main Menu Button listener added."); // 디버그 로그 추가
		}

		// === 인트로 화면 시작 ===
		if (introPanel != null)
		{
			introPanel.SetActive(true); // 인트로 패널 활성화
		}
		Time.timeScale = 0f; // 게임 시간 정지
		IsGameStarted = false; // 게임은 아직 시작 안 된 상태
		isGamePaused = false; // 게임은 아직 일시 정지 안 된 상태
		StartCoroutine(BlinkTextRoutine()); // 텍스트 깜빡임 시작

		// === 일시 정지 메뉴 버튼 리스너 추가 ===
		if (pauseToggleButton != null) pauseToggleButton.onClick.AddListener(TogglePauseMenu);
		if (pauseMenuRestartButton != null) pauseMenuRestartButton.onClick.AddListener(OnRestartButtonClicked); // 일시정지 메뉴 재시작 버튼 연결
		if (resumeButton != null) resumeButton.onClick.AddListener(HidePauseMenu);
		if (inGameMainMenuButton != null) inGameMainMenuButton.onClick.AddListener(OnMainMenuButtonClicked);
	}

	void Update()
	{
		// 게임 오버 상태 감지 (플레이어가 죽었고 게임 오버 패널이 아직 활성화되지 않았을 때)
		if (playerController != null && playerController.IsDead && !gameOverPanel.activeSelf)
		{
			ShowGameOverUI();
		}

		// 인트로 화면이 활성화되어 있고 게임이 아직 시작 안 된 상태에서 아무 키나 누르면 게임 시작
		if (!IsGameStarted && introPanel != null && introPanel.activeSelf)
		{
			if (Input.anyKeyDown)
			{
				StartGamePlay();
			}
		}
	}

	// === 인트로 화면에서 실제 게임 플레이 시작 메서드 ===
	public void StartGamePlay()
	{
		if (IsGameStarted) return; // 이미 시작했다면 중복 호출 방지

		IsGameStarted = true;
		Time.timeScale = 1f; // 게임 시간 스케일 복구 (게임 시작)
		StopCoroutine(BlinkTextRoutine()); // 텍스트 깜빡임 중지
		if (introPanel != null)
		{
			introPanel.SetActive(false); // 인트로 패널 비활성화
		}
		Debug.Log("Game Play Started!");
		UpdateAllGameplayUI(); // 게임 플레이 관련 UI 초기 업데이트

		// 게임 시작 사운드 재생
		if (gameStartSound != null && uiAudioSource != null)
		{
			uiAudioSource.PlayOneShot(gameStartSound);
		}
	}

	// === 게임 오버 UI 표시 메서드 ===
	public void ShowGameOverUI()
	{
		if (gameOverPanel != null)
		{
			gameOverPanel.SetActive(true); // 게임 오버 패널 활성화
			Time.timeScale = 0f; // 게임 일시 정지 (모든 게임 내 시간이 멈춥니다)
								 // 최종 점수 표시
			if (finalScoreText != null && scoreManager != null)
			{
				finalScoreText.text = $"Final Score: {Mathf.FloorToInt(scoreManager.CurrentScore)}";
			}
		}
	}

	// === 일시 정지 메뉴 표시/숨기기 (토글) ===
	public void TogglePauseMenu()
	{
		// 게임 오버 상태가 아니며, 인트로 화면이 활성화되어 있지 않고, 게임이 시작된 상태일 때만 작동
		if (playerController != null && (playerController.IsDead || !IsGameStarted)) return;

		isGamePaused = !isGamePaused; // 상태 토글

		if (pausePanel != null)
		{
			pausePanel.SetActive(isGamePaused); // 패널 활성화/비활성화
		}

		if (isGamePaused)
		{
			Time.timeScale = 0f; // 게임 시간 정지
			Debug.Log("Game Paused.");
		}
		else
		{
			Time.timeScale = 1f; // 게임 시간 다시 시작
			Debug.Log("Game Resumed.");
		}
	}

	// === 재개 버튼에서 직접 호출될 메서드 ===
	public void HidePauseMenu()
	{
		if (isGamePaused) // 일시 정지 상태일 때만 재개
		{
			TogglePauseMenu(); // 상태를 다시 토글하여 게임 재개
		}
	}

	// === 버튼 클릭 핸들러 (재사용 가능) ===
	// 게임 오버 패널과 일시정지 패널의 재시작 버튼 모두 이 함수를 호출
	private void OnRestartButtonClicked()
	{
		Debug.Log("Restart Button Clicked!");
		Time.timeScale = 1f; // 게임 시간 스케일 복구 (필수!)
		SceneManager.LoadScene(gamePlaySceneName);
	}

	// 게임 오버 패널 및 일시정지 패널의 메인 메뉴 버튼 모두 이 함수를 호출
	private void OnMainMenuButtonClicked()
	{
		Debug.Log("Main Menu Button Clicked!");
		Time.timeScale = 1f; // 게임 시간 스케일 복구 (필수!)
		SceneManager.LoadScene(mainMenuSceneName);
	}

	// === 텍스트 깜빡임 코루틴 ===
	private IEnumerator BlinkTextRoutine()
	{
		// Realtime 사용으로 Time.timeScale=0에서도 작동
		while (!IsGameStarted)
		{
			if (pressAnyKeyText != null)
			{
				pressAnyKeyText.enabled = !pressAnyKeyText.enabled;
			}
			yield return new WaitForSecondsRealtime(blinkSpeed);
		}
		// 게임 시작 후 텍스트가 완전히 보이도록 설정
		if (pressAnyKeyText != null)
		{
			pressAnyKeyText.enabled = true;
		}
	}

	// === 게임 플레이 중 UI 업데이트 ===
	public void UpdateAllGameplayUI()
	{
		if (scoreManager != null) UpdateScore(scoreManager.CurrentScore);
		if (playerController != null)
		{
			UpdatePlayerHealth(playerController.CurrentHealth);
			UpdatePlayerGuardHealth(playerController.CurrentGuardHealth);
			UpdatePlayerAmmo(playerController.CurrentAmmo);
		}
	}

	public void UpdateScore(float score)
	{
		if (scoreText != null)
		{
			scoreText.text = $"Score: {Mathf.FloorToInt(score)}";
		}
	}

	public void UpdatePlayerHealth(int health)
	{
		if (healthText != null)
		{
			healthText.text = $"HP: {health}";
		}
	}

	public void UpdatePlayerGuardHealth(int guardHealth)
	{
		if (guardHealthText != null)
		{
			guardHealthText.text = $"Guard: {guardHealth}";
		}
	}

	public void UpdatePlayerAmmo(int ammo)
	{
		if (ammoText != null)
		{
			ammoText.text = $"Ammo: {ammo}";
		}
	}
}
