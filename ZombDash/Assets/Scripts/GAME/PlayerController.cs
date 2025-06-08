using UnityEngine;
using System.Collections; // Coroutine을 사용하기 위해 필요

// 스크립트가 추가될 때 Rigidbody2D와 CapsuleCollider2D 컴포넌트가 자동으로 추가되도록 합니다.
[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(CapsuleCollider2D))]
[RequireComponent(typeof(AudioSource))] // AudioSource 컴포넌트 추가
public class PlayerController : MonoBehaviour
{
	// === 컴포넌트 참조 (유니티 에디터에서 연결) ===
	[SerializeField] private Animator playerAnimator; // 플레이어 Animator 컴포넌트
	[SerializeField] private Rigidbody2D playerRigidbody; // 플레이어 Rigidbody2D 컴포넌트
	[SerializeField] private Transform groundCheck; // 땅 체크를 위한 오브젝트의 위치
	[SerializeField] private LayerMask groundLayer; // 땅으로 인식할 레이어 (Ground, Platform 등)
	[SerializeField] private CapsuleCollider2D playerCollider; // 플레이어의 메인 CapsuleCollider2D 컴포넌트 (슬라이딩 시 크기 조절용)
	private AudioSource playerAudioSource; // 플레이어의 AudioSource 컴포넌트

	[Header("Player Settings")]
	[SerializeField] private float baseMoveSpeed = 5f; // 플레이어의 기본 이동 속도 (게임 시작 시)
	[SerializeField] private float accelerationRate = 0.1f; // 초당 이동 속도 증가량
	[SerializeField] private float maxMoveSpeed = 15f; // 플레이어의 최대 이동 속도

	[SerializeField] private float jumpForce = 10f; // 점프 시 가해질 힘
	[SerializeField] private float groundCheckRadius = 0.2f; // 땅 체크 원의 반지름
	[SerializeField] private float shootCooldownTime = 0.5f; // 사격 쿨다운 시간
	[SerializeField] private float meleeAttackDuration = 0.3f; // 근접 공격 애니메이션 지속 시간 또는 판정 시간
	[SerializeField] private Transform bulletSpawnPoint; // 총알이 생성될 위치
	[SerializeField] private GameObject playerBulletPrefab; // 플레이어 총알 프리팹 (Inspector에서 연결)
	[SerializeField] private Transform meleeAttackPoint; // 근접 공격 판정 시작점 (빈 오브젝트로 설정)
	[SerializeField] private float meleeAttackRadius = 0.5f; // 근접 공격 판정 범위
	[SerializeField] private LayerMask attackableLayer; // 공격 가능한 오브젝트 레이어 (Enemy, EnemyProjectile 등)

	[Header("Collider Sizes")]
	[SerializeField] private Vector2 standingColliderSize = new Vector2(0.8f, 1.8f); // 서 있을 때 콜라이더 크기
	[SerializeField] private Vector2 slidingColliderSize = new Vector2(1.6f, 0.9f); // 슬라이딩 시 콜라이더 크기
	[SerializeField] private Vector2 standingColliderOffset = new Vector2(0f, 0f); // 서 있을 때 콜라이더 오프셋
	[SerializeField] private Vector2 slidingColliderOffset = new Vector2(0f, -0.45f); // 슬라이딩 시 콜라이더 오프셋 (조절 필요)

	[Header("Health and Damage Settings")]
	[SerializeField] private int maxHealth = 3; // 최대 체력
	[SerializeField] private int maxGuardHealth = 1; // 최대 가드 체력
	[SerializeField] private float invincibilityDuration = 1.0f; // 피격 후 무적 시간

	[Header("Ammo Settings")]
	[SerializeField] private int maxAmmo = 4; // 최대 탄약 수
	[SerializeField] private int initialAmmo = 2; // 게임 시작 시 지급될 초기 탄약 수

	[Header("Audio Clips")] // 오디오 클립 추가
	[SerializeField] private AudioClip jumpSound; // 점프 사운드
	[SerializeField] private AudioClip landSound; // 착지 사운드
	[SerializeField] private AudioClip hurtSound; // 피격 사운드

	// === 내부 상태 변수 ===
	private int currentHealth;
	private int currentGuardHealth;
	private int currentAmmo;
	private bool isGrounded; // 플레이어가 땅에 닿아 있는지 여부
	private bool wasGroundedLastFrame; // 이전 프레임에 땅에 닿아 있었는지 여부 (착지 감지용)
	private bool isSliding; // 슬라이딩 중인지 여부
	private bool isJumping; // 점프 중인지 여부 (공중 상태 유지용)
	private float lastShootTime; // 마지막 사격 시간
	private bool isInvincible; // 무적 상태 여부
	private float invincibilityEndTime; // 무적 종료 시간
	private bool isMeleeAttacking; // 근접 공격 중인지 여부

	// 게임 오버 상태를 관리하는 public 속성
	public bool IsDead { get; private set; }
	public int CurrentHealth => currentHealth; // 외부에서 체력 읽기용
	public int CurrentGuardHealth => currentGuardHealth; // 외부에서 가드체력 읽기용
	public int CurrentAmmo => currentAmmo; // 외부에서 탄약 읽기용
	public float CurrentMoveSpeed => currentMoveSpeed; // 현재 이동 속도를 외부에서 읽기용

	private float currentMoveSpeed; // 플레이어의 현재 이동 속도 (내부 사용)

	// === 유니티 생명주기 메서드 ===

	void Awake()
	{
		if (playerAnimator == null) playerAnimator = GetComponent<Animator>();
		if (playerRigidbody == null) playerRigidbody = GetComponent<Rigidbody2D>();
		if (playerCollider == null) playerCollider = GetComponent<CapsuleCollider2D>();
		if (playerAudioSource == null) playerAudioSource = GetComponent<AudioSource>(); // AudioSource 컴포넌트 참조

		if (playerAnimator == null) Debug.LogError("PlayerAnimator is not found on this GameObject!");
		if (playerRigidbody == null) Debug.LogError("PlayerRigidbody2D is not found on this GameObject!");
		if (playerCollider == null) Debug.LogError("PlayerCollider (CapsuleCollider2D) is not found on this GameObject!");
		if (groundCheck == null) Debug.LogError("GroundCheck Transform is not assigned in the Inspector! Please assign it.");
		if (bulletSpawnPoint == null) Debug.LogError("Bullet Spawn Point Transform is not assigned in the Inspector!");
		if (playerBulletPrefab == null) Debug.LogError("Player Bullet Prefab is not assigned in the Inspector!");
		if (meleeAttackPoint == null) Debug.LogError("Melee Attack Point Transform is not assigned in the Inspector!");
		if (playerAudioSource == null) Debug.LogError("Player AudioSource is not found on this GameObject!"); // AudioSource 확인
	}

	void Start()
	{
		// 초기 스탯 설정
		currentHealth = maxHealth;
		currentGuardHealth = 0;
		currentAmmo = initialAmmo;
		currentMoveSpeed = baseMoveSpeed; // 게임 시작 시 기본 이동 속도로 설정

		wasGroundedLastFrame = Physics2D.OverlapCircle(groundCheck.position, groundCheckRadius, groundLayer);
		isGrounded = wasGroundedLastFrame;
		lastShootTime = -shootCooldownTime;
		isJumping = false;
		IsDead = false;
		isInvincible = false;
		isMeleeAttacking = false;

		// 게임 시작 시 초기 UI 업데이트 (UIManager가 준비되었을 때만)
		if (UIManager.Instance != null)
		{
			UIManager.Instance.UpdateAllGameplayUI();
		}
	}

	void Update()
	{
		if (IsDead) return;

		// 게임이 정지 상태가 아닐 때만 속도 증가 (인트로 중에는 가속 안 되도록)
		// UIManager.Instance.IsGameStarted가 true일 때만 속도 증가
		if (Time.timeScale > 0f && UIManager.Instance != null && UIManager.Instance.IsGameStarted)
		{
			currentMoveSpeed = Mathf.Min(currentMoveSpeed + accelerationRate * Time.deltaTime, maxMoveSpeed);
		}

		CheckGroundStatus();
		playerAnimator.SetBool("IsGrounded", isGrounded);
		playerAnimator.SetBool("IsJumping", isJumping);

		// 무적 시간 관리
		if (isInvincible && Time.time >= invincibilityEndTime)
		{
			isInvincible = false;
			// GetComponent<SpriteRenderer>().color = Color.white; // 무적 종료 시 스프라이트 색상 복구 (선택 사항)
		}

		// 인트로 화면이 끝나고 게임이 시작되었을 때만 일반 입력 처리
		if (UIManager.Instance != null && UIManager.Instance.IsGameStarted)
		{
			HandleInput();
		}
	}

	void FixedUpdate()
	{
		if (IsDead)
		{
			playerRigidbody.linearVelocity = Vector2.zero;
			return;
		}
		// 현재 증가된 이동 속도를 적용
		playerRigidbody.linearVelocity = new Vector2(currentMoveSpeed, playerRigidbody.linearVelocity.y);
	}

	// === 입력 처리 메서드 ===
	private void HandleInput()
	{
		// 게임이 일시 정지 상태가 아닐 때만 플레이어 조작 입력 처리
		// Time.timeScale == 0f (일시 정지)일 때는 입력 무시
		if (Time.timeScale > 0f)
		{
			HandleJumpInput();
			HandleSlideInput();
			HandleShootInput();
			HandleSlashInput();
		}
	}

	private void HandleJumpInput()
	{
		if (Input.GetKeyDown(KeyCode.Space))
		{
			if (isGrounded && !isSliding)
			{
				playerRigidbody.linearVelocity = new Vector2(playerRigidbody.linearVelocity.x, jumpForce);
				playerAnimator.SetTrigger("JumpTrigger");
				isJumping = true;

				// 점프 사운드 재생
				if (jumpSound != null && playerAudioSource != null)
				{
					playerAudioSource.PlayOneShot(jumpSound);
				}
			}
		}
	}

	private void HandleSlideInput()
	{
		bool currentSlideInput = Input.GetKey(KeyCode.S);

		if (currentSlideInput && isGrounded && !isJumping)
		{
			if (!isSliding)
			{
				playerAnimator.SetBool("IsSliding", true);
				isSliding = true;
				playerCollider.size = slidingColliderSize;
				playerCollider.offset = slidingColliderOffset;
			}
		}
		else
		{
			if (isSliding)
			{
				playerAnimator.SetBool("IsSliding", false);
				isSliding = false;
				playerCollider.size = standingColliderSize;
				playerCollider.offset = standingColliderOffset;
			}
		}
	}

	private void HandleShootInput()
	{
		if (Input.GetKeyDown(KeyCode.P))
		{
			if (!isSliding && Time.time >= lastShootTime + shootCooldownTime && currentAmmo > 0)
			{
				playerAnimator.SetTrigger("Shoot");
				FireBullet();
				lastShootTime = Time.time;
			}
			else if (currentAmmo <= 0)
			{
				Debug.Log("Not enough ammo to shoot!");
			}
		}
	}

	private void HandleSlashInput()
	{
		if (Input.GetKey(KeyCode.O))
		{
			if (!isMeleeAttacking)
			{
				playerAnimator.SetBool("IsSlashing", true);
				isMeleeAttacking = true;
				StartCoroutine(PerformMeleeAttackRoutine());
			}
		}
		else
		{
			playerAnimator.SetBool("IsSlashing", false);
		}
	}

	// === 총알 발사 로직 ===
	private void FireBullet()
	{
		if (playerBulletPrefab != null && bulletSpawnPoint != null)
		{
			Instantiate(playerBulletPrefab, bulletSpawnPoint.position, Quaternion.identity);
			currentAmmo--;
			Debug.Log($"Player Fired. Current Ammo: {currentAmmo}");
			if (UIManager.Instance != null)
			{
				UIManager.Instance.UpdatePlayerAmmo(currentAmmo);
			}
		}
	}

	// === 근접 공격 로직 ===
	private IEnumerator PerformMeleeAttackRoutine()
	{
		yield return new WaitForSeconds(meleeAttackDuration);

		Collider2D[] hitObjects = Physics2D.OverlapCircleAll(meleeAttackPoint.position, meleeAttackRadius, attackableLayer);

		foreach (Collider2D hit in hitObjects)
		{
			if (hit.CompareTag("Enemy"))
			{
				Zombie zombie = hit.GetComponent<Zombie>();
				if (zombie != null)
				{
					zombie.TakeDamage(1); // 좀비 체력 -1 (일반/발사 좀비 공통)
					Debug.Log($"Melee hit Enemy: {hit.name}");
				}
			}
			else if (hit.CompareTag("EnemyProjectile")) // 발사 좀비의 발사체 파괴
			{
				ZombieProjectile zombieProj = hit.GetComponent<ZombieProjectile>();
				if (zombieProj != null)
				{
					// 발사체는 근접 공격으로만 파괴 가능
					zombieProj.DestroyProjectile();
					Debug.Log($"Melee destroyed Enemy Projectile: {hit.name}");
				}
			}
		}

		isMeleeAttacking = false;
		playerAnimator.SetBool("IsSlashing", false);
	}


	// === 땅 체크 및 공중 상태 관리 메서드 ===
	private void CheckGroundStatus()
	{
		bool currentlyGrounded = Physics2D.OverlapCircle(groundCheck.position, groundCheckRadius, groundLayer);

		if (currentlyGrounded && !wasGroundedLastFrame)
		{
			playerAnimator.SetTrigger("LandTrigger");
			isJumping = false;
			playerAnimator.SetBool("IsJumping", false);

			// 착지 사운드 재생
			if (landSound != null && playerAudioSource != null)
			{
				playerAudioSource.PlayOneShot(landSound);
			}
		}

		isGrounded = currentlyGrounded;
		wasGroundedLastFrame = currentlyGrounded;
	}

	// === 충돌 감지 ===
	void OnTriggerEnter2D(Collider2D other)
	{
		if (other.CompareTag("Obstacle"))
		{
			Obstacle obstacle = other.GetComponent<Obstacle>();
			if (obstacle != null)
			{
				int damageAmount = obstacle.DamageAmount;

				if (obstacle.Type == Obstacle.ObstacleType.FallenStone)
				{
					if (!isSliding) // PlayerController 내의 isSliding 변수에 직접 접근
					{
						TakeDamage(damageAmount);
						Debug.Log("Hit Fallen Stone while not sliding!");
					}
					else
					{
						Debug.Log("Slid over Fallen Stone successfully!");
					}
				}
				else // 선인장 등 다른 장애물은 항상 피해
				{
					TakeDamage(damageAmount);
					Debug.Log("Hit Cactus!"); // 또는 다른 장애물 이름
				}
			}
		}
		else if (other.CompareTag("Collectable"))
		{
			CollectableItem item = other.GetComponent<CollectableItem>();
			if (item != null)
			{
				item.ApplyEffect(this);
				Destroy(other.gameObject);
			}
		}
		else if (other.CompareTag("Enemy"))
		{
			Zombie zombie = other.GetComponent<Zombie>();
			if (zombie != null)
			{
				TakeDamage(zombie.DamageOnCollisionToPlayer);
			}
		}
		else if (other.CompareTag("EnemyProjectile"))
		{
			ZombieProjectile zombieProj = other.GetComponent<ZombieProjectile>();
			if (zombieProj != null)
			{
				TakeDamage(zombieProj.damageToPlayer);
				zombieProj.DestroyProjectile();
			}
		}
	}

	// === 피격 처리 ===
	public void TakeDamage(int damage)
	{
		if (IsDead || isInvincible) return;

		int finalDamage = damage;

		if (currentGuardHealth > 0)
		{
			int guardAbsorb = Mathf.Min(finalDamage, currentGuardHealth);
			currentGuardHealth -= guardAbsorb;
			finalDamage -= guardAbsorb;
		}

		currentHealth -= finalDamage;

		playerAnimator.SetTrigger("HurtTrigger");

		// 피격 사운드 재생
		if (hurtSound != null && playerAudioSource != null)
		{
			playerAudioSource.PlayOneShot(hurtSound);
		}

		isInvincible = true;
		invincibilityEndTime = Time.time + invincibilityDuration;

		Debug.Log($"Player Health: {currentHealth}, Guard Health: {currentGuardHealth}");

		if (currentHealth <= 0)
		{
			Die();
		}

		if (UIManager.Instance != null)
		{
			UIManager.Instance.UpdatePlayerHealth(currentHealth);
			UIManager.Instance.UpdatePlayerGuardHealth(currentGuardHealth);
		}
	}

	// === 플레이어 사망 처리 ===
	private void Die()
	{
		IsDead = true;
		playerAnimator.SetBool("IsDead", true);

		currentMoveSpeed = 0; // 사망 시 이동 속도 0
		playerRigidbody.linearVelocity = Vector2.zero;
		playerRigidbody.gravityScale = 0;
		playerCollider.enabled = false;

		Debug.Log("Player has died!");

		if (UIManager.Instance != null)
		{
			UIManager.Instance.ShowGameOverUI();
		}
		else
		{
			Debug.LogError("UIManager.Instance is null! Cannot show game over UI.");
		}
	}

	// === 아이템 획득 메서드 ===
	public void AddScore(float amount)
	{
		if (ScoreManager.Instance != null)
		{
			ScoreManager.Instance.AddScore(amount);
		}
		else
		{
			Debug.LogWarning("ScoreManager.Instance is null. Score not added to manager.");
		}
		Debug.Log($"Score Added: {amount}");
	}

	public void GainHealth(int amount)
	{
		currentHealth = Mathf.Min(currentHealth + amount, maxHealth);
		Debug.Log($"Player gained Health. Current Health: {currentHealth}");
		if (UIManager.Instance != null)
		{
			UIManager.Instance.UpdatePlayerHealth(currentHealth);
		}
	}

	public void GainGuardHealth(int amount)
	{
		currentGuardHealth = Mathf.Min(currentGuardHealth + amount, maxGuardHealth);
		Debug.Log($"Player gained Guard Health. Current Guard Health: {currentGuardHealth}");
		if (UIManager.Instance != null)
		{
			UIManager.Instance.UpdatePlayerGuardHealth(currentGuardHealth);
		}
	}

	public void AddAmmo(int amount)
	{
		currentAmmo = Mathf.Min(currentAmmo + amount, maxAmmo);
		Debug.Log($"Player gained Ammo. Current Ammo: {currentAmmo}");
		if (UIManager.Instance != null)
		{
			UIManager.Instance.UpdatePlayerAmmo(currentAmmo);
		}
	}

	// === 시각화를 위한 기즈모 (Gizmos) 그리기 ===
	void OnDrawGizmos()
	{
		if (groundCheck != null)
		{
			Gizmos.color = isGrounded ? Color.green : Color.red;
			Gizmos.DrawWireSphere(groundCheck.position, groundCheckRadius);
		}
		if (meleeAttackPoint != null)
		{
			Gizmos.color = Color.red;
			Gizmos.DrawWireSphere(meleeAttackPoint.position, meleeAttackRadius);
		}
	}
}
