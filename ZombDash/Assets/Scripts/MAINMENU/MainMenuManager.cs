using UnityEngine;
using UnityEngine.SceneManagement; // �� ������ ���� �ʿ�
using UnityEngine.UI; // UI ��ư�� ���� �ʿ�
using TMPro; // TextMeshPro ��ư�� ���� �ʿ�

public class MainMenuManager : MonoBehaviour
{
	[Header("Buttons")]
	[SerializeField] private Button playButton; // Play ��ư (Inspector���� ����)
	[SerializeField] private Button quitButton; // Quit ��ư (Inspector���� ����)

	[Header("Scene Names")]
	// ���� �÷��� ���� ��Ȯ�� �̸� (Build Settings�� �߰��� �̸�)
	[SerializeField] private string gamePlaySceneName = "GAME_PLAY";

	void Awake()
	{
		// �ʼ� ��ư���� �Ҵ�Ǿ����� Ȯ��
		if (playButton == null) Debug.LogError("Play Button not assigned in MainMenuManager!");
		if (quitButton == null) Debug.LogError("Quit Button not assigned in MainMenuManager!");
		// OptionsButton ���� ��� ����
	}

	void Start()
	{
		// ��ư Ŭ�� �̺�Ʈ ������ �߰�
		if (playButton != null)
		{
			playButton.onClick.AddListener(OnPlayButtonClicked);
		}
		// OptionsButton Ŭ�� �̺�Ʈ ������ ����
		if (quitButton != null)
		{
			quitButton.onClick.AddListener(OnQuitButtonClicked);
		}
	}

	// Play ��ư Ŭ�� �� ȣ��� �޼���
	private void OnPlayButtonClicked()
	{
		Debug.Log("Play Button Clicked! Loading Game Play Scene...");
		// ���� �÷��� ���� �ε��մϴ�.
		SceneManager.LoadScene(gamePlaySceneName);
	}

	// Options ��ư Ŭ�� �� ȣ��� �޼��� ����
	// private void OnOptionsButtonClicked() { /* ... */ }

	// Quit ��ư Ŭ�� �� ȣ��� �޼���
	private void OnQuitButtonClicked()
	{
		Debug.Log("Quit Button Clicked! Quitting Game...");
		// ����Ƽ �����Ϳ��� ������ ������ ��
#if UNITY_EDITOR
		UnityEditor.EditorApplication.isPlaying = false;
#else
        // ����� ���ӿ��� ������ ������ ��
        Application.Quit();
#endif
	}
}
