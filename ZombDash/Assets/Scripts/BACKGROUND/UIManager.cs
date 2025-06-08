using UnityEngine;
using UnityEngine.UI; // UI ���� ����� ����ϱ� ���� �ʿ�
using UnityEngine.SceneManagement; // �� ���� ����� ����ϱ� ���� �ʿ�
using TMPro; // TextMeshPro�� ����ϱ� ���� �ʿ�
using System.Collections; // �ڷ�ƾ�� ����ϱ� ���� �ʿ�

public class UIManager : MonoBehaviour
{
	public static UIManager Instance { get; private set; } // �̱��� ����

	[Header("UI Panels")]
	[SerializeField] private GameObject gameOverPanel; // ���� ���� �г� GameObject
	[SerializeField] private GameObject introPanel; // ��Ʈ�� �г� GameObject
	[SerializeField] private GameObject pausePanel; // �Ͻ� ���� �г� GameObject

	[Header("Game Over Buttons")]
	[SerializeField] private Button gameOverRestartButton; // ����� ��ư (���� ���� �г�)
	[SerializeField] private Button gameOverMainMenuButton; // ���� ȭ�� ��ư (���� ���� �г�)

	[Header("Intro Screen Elements")]
	[SerializeField] private TextMeshProUGUI pressAnyKeyText; // "�ƹ�Ű�� ��������" �ؽ�Ʈ
	[SerializeField] private float blinkSpeed = 0.5f; // �ؽ�Ʈ �����̴� �ӵ� (��)

	[Header("Pause Menu Buttons")] // �Ͻ� ���� �޴� ��ư
	[SerializeField] private Button pauseToggleButton; // ���� �߾� ��� �Ͻ����� ��� ��ư
	[SerializeField] private Button pauseMenuRestartButton; // �Ͻ����� �޴��� ����� ��ư
	[SerializeField] private Button resumeButton; // �簳 ��ư (�Ͻ� ���� �г�)
	[SerializeField] private Button inGameMainMenuButton; // �ΰ��� ���� �޴� ��ư (�Ͻ� ���� �г�)

	// UI �ؽ�Ʈ ����
	[Header("UI References")]
	[SerializeField] private TextMeshProUGUI scoreText;
	[SerializeField] private TextMeshProUGUI healthText;
	[SerializeField] private TextMeshProUGUI guardHealthText;
	[SerializeField] private TextMeshProUGUI ammoText;
	[SerializeField] private TextMeshProUGUI finalScoreText; // ���� ���� �г��� ���� ���� �ؽ�Ʈ

	[Header("Scene Names")]
	[SerializeField] private string gamePlaySceneName = "GAME_PLAY";
	[SerializeField] private string mainMenuSceneName = "MAIN_MENU_SCENE_NAME";

	// �Ŵ��� ����
	[SerializeField] private PlayerController playerController;
	[SerializeField] private ScoreManager scoreManager;

	[Header("Audio Clips")] // ����� Ŭ�� �߰�
	[SerializeField] private AudioClip gameStartSound; // ���� ���� ����
	private AudioSource uiAudioSource; // UI AudioSource ������Ʈ

	public bool IsGameStarted { get; private set; } = false; // �ܺο��� ���� ���� ���� Ȯ�� ����
	private bool isGamePaused = false; // ������ �Ͻ� ���� ��������

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

		// AudioSource ������Ʈ ���� (UI Manager ������Ʈ�� AudioSource �߰�)
		if (uiAudioSource == null) uiAudioSource = GetComponent<AudioSource>();
		// ��� �Ҵ��� Inspector���� �̷�����Ƿ�, Awake������ �ʼ� ������ Ȯ��
		// ���� AudioSource�� ���ٸ� ���� �޽���
		if (uiAudioSource == null) Debug.LogError("UIManager: AudioSource component not found on UIManager GameObject!");


		// ��� Inspector ���� �ʵ� Ȯ�� (null ����)
		// �г�
		if (gameOverPanel == null) Debug.LogError("UIManager: GameOverPanel is not assigned!");
		if (introPanel == null) Debug.LogError("UIManager: IntroPanel is not assigned!");
		if (pausePanel == null) Debug.LogError("UIManager: PausePanel is not assigned!");

		// ���� ���� ��ư (�ʼ�)
		if (gameOverRestartButton == null) Debug.LogError("UIManager: GameOverRestartButton is not assigned!");
		if (gameOverMainMenuButton == null) Debug.LogError("UIManager: GameOverMainMenuButton is not assigned!");

		// ��Ʈ�� �ؽ�Ʈ (�ʼ�)
		if (pressAnyKeyText == null) Debug.LogError("UIManager: PressAnyKeyText is not assigned!");

		// �Ͻ����� �޴� ��ư (�ʼ�)
		if (pauseToggleButton == null) Debug.LogError("UIManager: PauseToggleButton is not assigned!");
		if (pauseMenuRestartButton == null) Debug.LogError("UIManager: PauseMenuRestartButton is not assigned!");
		if (resumeButton == null) Debug.LogError("UIManager: ResumeButton is not assigned!");
		if (inGameMainMenuButton == null) Debug.LogError("UIManager: InGameMainMenuButton is not assigned!");

		// UI �ؽ�Ʈ (Score, Health, Guard, Ammo, FinalScore) (�ʼ�)
		if (scoreText == null) Debug.LogError("UIManager: Score Text not assigned in UI References!");
		if (healthText == null) Debug.LogError("UIManager: Health Text not assigned in UI References!");
		if (guardHealthText == null) Debug.LogError("UIManager: Guard Health Text not assigned in UI References!");
		if (ammoText == null) Debug.LogError("UIManager: Ammo Text not assigned in UI References!");
		if (finalScoreText == null) Debug.LogError("UIManager: Final Score Text not assigned in UI References!");

		// �Ŵ���/��Ʈ�ѷ� ���� (�ʼ�)
		if (playerController == null) Debug.LogError("Player Controller not assigned in UIManager!");
		if (scoreManager == null) Debug.LogError("Score Manager not assigned in UIManager!");
	}

	void Start()
	{
		// ��� UI �г��� ��Ȱ��ȭ ���·� ���� (��Ʈ�� �г��� StartGamePlay() ������ Ȱ��ȭ��)
		if (gameOverPanel != null) gameOverPanel.SetActive(false);
		if (pausePanel != null) pausePanel.SetActive(false); // �ʱ� ��Ȱ��ȭ

		// === ���� ���� ��ư ������ �߰� ===
		// �� �κ��� �߿��մϴ�! �Ҵ�� ��ư���� �����ʸ� �߰�
		if (gameOverRestartButton != null)
		{
			gameOverRestartButton.onClick.AddListener(OnRestartButtonClicked);
			Debug.Log("Game Over Restart Button listener added."); // ����� �α� �߰�
		}
		if (gameOverMainMenuButton != null)
		{
			gameOverMainMenuButton.onClick.AddListener(OnMainMenuButtonClicked);
			Debug.Log("Game Over Main Menu Button listener added."); // ����� �α� �߰�
		}

		// === ��Ʈ�� ȭ�� ���� ===
		if (introPanel != null)
		{
			introPanel.SetActive(true); // ��Ʈ�� �г� Ȱ��ȭ
		}
		Time.timeScale = 0f; // ���� �ð� ����
		IsGameStarted = false; // ������ ���� ���� �� �� ����
		isGamePaused = false; // ������ ���� �Ͻ� ���� �� �� ����
		StartCoroutine(BlinkTextRoutine()); // �ؽ�Ʈ ������ ����

		// === �Ͻ� ���� �޴� ��ư ������ �߰� ===
		if (pauseToggleButton != null) pauseToggleButton.onClick.AddListener(TogglePauseMenu);
		if (pauseMenuRestartButton != null) pauseMenuRestartButton.onClick.AddListener(OnRestartButtonClicked); // �Ͻ����� �޴� ����� ��ư ����
		if (resumeButton != null) resumeButton.onClick.AddListener(HidePauseMenu);
		if (inGameMainMenuButton != null) inGameMainMenuButton.onClick.AddListener(OnMainMenuButtonClicked);
	}

	void Update()
	{
		// ���� ���� ���� ���� (�÷��̾ �׾��� ���� ���� �г��� ���� Ȱ��ȭ���� �ʾ��� ��)
		if (playerController != null && playerController.IsDead && !gameOverPanel.activeSelf)
		{
			ShowGameOverUI();
		}

		// ��Ʈ�� ȭ���� Ȱ��ȭ�Ǿ� �ְ� ������ ���� ���� �� �� ���¿��� �ƹ� Ű�� ������ ���� ����
		if (!IsGameStarted && introPanel != null && introPanel.activeSelf)
		{
			if (Input.anyKeyDown)
			{
				StartGamePlay();
			}
		}
	}

	// === ��Ʈ�� ȭ�鿡�� ���� ���� �÷��� ���� �޼��� ===
	public void StartGamePlay()
	{
		if (IsGameStarted) return; // �̹� �����ߴٸ� �ߺ� ȣ�� ����

		IsGameStarted = true;
		Time.timeScale = 1f; // ���� �ð� ������ ���� (���� ����)
		StopCoroutine(BlinkTextRoutine()); // �ؽ�Ʈ ������ ����
		if (introPanel != null)
		{
			introPanel.SetActive(false); // ��Ʈ�� �г� ��Ȱ��ȭ
		}
		Debug.Log("Game Play Started!");
		UpdateAllGameplayUI(); // ���� �÷��� ���� UI �ʱ� ������Ʈ

		// ���� ���� ���� ���
		if (gameStartSound != null && uiAudioSource != null)
		{
			uiAudioSource.PlayOneShot(gameStartSound);
		}
	}

	// === ���� ���� UI ǥ�� �޼��� ===
	public void ShowGameOverUI()
	{
		if (gameOverPanel != null)
		{
			gameOverPanel.SetActive(true); // ���� ���� �г� Ȱ��ȭ
			Time.timeScale = 0f; // ���� �Ͻ� ���� (��� ���� �� �ð��� ����ϴ�)
								 // ���� ���� ǥ��
			if (finalScoreText != null && scoreManager != null)
			{
				finalScoreText.text = $"Final Score: {Mathf.FloorToInt(scoreManager.CurrentScore)}";
			}
		}
	}

	// === �Ͻ� ���� �޴� ǥ��/����� (���) ===
	public void TogglePauseMenu()
	{
		// ���� ���� ���°� �ƴϸ�, ��Ʈ�� ȭ���� Ȱ��ȭ�Ǿ� ���� �ʰ�, ������ ���۵� ������ ���� �۵�
		if (playerController != null && (playerController.IsDead || !IsGameStarted)) return;

		isGamePaused = !isGamePaused; // ���� ���

		if (pausePanel != null)
		{
			pausePanel.SetActive(isGamePaused); // �г� Ȱ��ȭ/��Ȱ��ȭ
		}

		if (isGamePaused)
		{
			Time.timeScale = 0f; // ���� �ð� ����
			Debug.Log("Game Paused.");
		}
		else
		{
			Time.timeScale = 1f; // ���� �ð� �ٽ� ����
			Debug.Log("Game Resumed.");
		}
	}

	// === �簳 ��ư���� ���� ȣ��� �޼��� ===
	public void HidePauseMenu()
	{
		if (isGamePaused) // �Ͻ� ���� ������ ���� �簳
		{
			TogglePauseMenu(); // ���¸� �ٽ� ����Ͽ� ���� �簳
		}
	}

	// === ��ư Ŭ�� �ڵ鷯 (���� ����) ===
	// ���� ���� �гΰ� �Ͻ����� �г��� ����� ��ư ��� �� �Լ��� ȣ��
	private void OnRestartButtonClicked()
	{
		Debug.Log("Restart Button Clicked!");
		Time.timeScale = 1f; // ���� �ð� ������ ���� (�ʼ�!)
		SceneManager.LoadScene(gamePlaySceneName);
	}

	// ���� ���� �г� �� �Ͻ����� �г��� ���� �޴� ��ư ��� �� �Լ��� ȣ��
	private void OnMainMenuButtonClicked()
	{
		Debug.Log("Main Menu Button Clicked!");
		Time.timeScale = 1f; // ���� �ð� ������ ���� (�ʼ�!)
		SceneManager.LoadScene(mainMenuSceneName);
	}

	// === �ؽ�Ʈ ������ �ڷ�ƾ ===
	private IEnumerator BlinkTextRoutine()
	{
		// Realtime ������� Time.timeScale=0������ �۵�
		while (!IsGameStarted)
		{
			if (pressAnyKeyText != null)
			{
				pressAnyKeyText.enabled = !pressAnyKeyText.enabled;
			}
			yield return new WaitForSecondsRealtime(blinkSpeed);
		}
		// ���� ���� �� �ؽ�Ʈ�� ������ ���̵��� ����
		if (pressAnyKeyText != null)
		{
			pressAnyKeyText.enabled = true;
		}
	}

	// === ���� �÷��� �� UI ������Ʈ ===
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
