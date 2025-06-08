using UnityEngine;
using UnityEngine.SceneManagement; // 씬 관리를 위해 필요
using UnityEngine.UI; // UI 버튼을 위해 필요
using TMPro; // TextMeshPro 버튼을 위해 필요

public class MainMenuManager : MonoBehaviour
{
	[Header("Buttons")]
	[SerializeField] private Button playButton; // Play 버튼 (Inspector에서 연결)
	[SerializeField] private Button quitButton; // Quit 버튼 (Inspector에서 연결)

	[Header("Scene Names")]
	// 게임 플레이 씬의 정확한 이름 (Build Settings에 추가된 이름)
	[SerializeField] private string gamePlaySceneName = "GAME_PLAY";

	void Awake()
	{
		// 필수 버튼들이 할당되었는지 확인
		if (playButton == null) Debug.LogError("Play Button not assigned in MainMenuManager!");
		if (quitButton == null) Debug.LogError("Quit Button not assigned in MainMenuManager!");
		// OptionsButton 관련 경고 제거
	}

	void Start()
	{
		// 버튼 클릭 이벤트 리스너 추가
		if (playButton != null)
		{
			playButton.onClick.AddListener(OnPlayButtonClicked);
		}
		// OptionsButton 클릭 이벤트 리스너 제거
		if (quitButton != null)
		{
			quitButton.onClick.AddListener(OnQuitButtonClicked);
		}
	}

	// Play 버튼 클릭 시 호출될 메서드
	private void OnPlayButtonClicked()
	{
		Debug.Log("Play Button Clicked! Loading Game Play Scene...");
		// 게임 플레이 씬을 로드합니다.
		SceneManager.LoadScene(gamePlaySceneName);
	}

	// Options 버튼 클릭 시 호출될 메서드 제거
	// private void OnOptionsButtonClicked() { /* ... */ }

	// Quit 버튼 클릭 시 호출될 메서드
	private void OnQuitButtonClicked()
	{
		Debug.Log("Quit Button Clicked! Quitting Game...");
		// 유니티 에디터에서 게임을 종료할 때
#if UNITY_EDITOR
		UnityEditor.EditorApplication.isPlaying = false;
#else
        // 빌드된 게임에서 게임을 종료할 때
        Application.Quit();
#endif
	}
}
