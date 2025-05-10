using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class PCCC : MonoBehaviour
{
	Rigidbody2D rigid2D;
	Animator animator;
	float jumpForce = 680.0f;
	float walkForce = 30.0f;
	float maxWalkSpeed = 2.0f;

	private int jumpCount = 0;
	public int maxJumpCount = 2;

	// private int horizontalInput = 0; // 이 변수는 사용되지 않아 주석처리하거나 삭제해도 됩니다.

	void Start()
	{
		this.rigid2D = GetComponent<Rigidbody2D>();
		this.animator = GetComponent<Animator>();
	}

	void Update()
	{
		// 점프
		// GetKeyDown을 사용하여 키를 누르는 순간에만 점프하도록 합니다.
		// jumpCount < maxJumpCount 조건으로 최대 점프 횟수를 제한합니다.
		if (Input.GetKeyDown(KeyCode.Space) && jumpCount < maxJumpCount)
		{
			this.animator.SetTrigger("JumpTrigger");
			// 부모 객체로부터 분리하여 독립적인 물리 작용을 하도록 합니다.
			if (transform.parent != null)
			{
				transform.SetParent(null);
			}
			// 이미 위로 올라가는 속도가 있더라도, 점프 힘을 다시 적용하여 상승시키도록 AddForce를 사용합니다.
			// Velocity를 직접 설정하면 기존 y 속도가 무시됩니다.
			// 필요에 따라 rigid2D.velocity = new Vector2(rigid2D.velocity.x, 0); 후 AddForce를 고려할 수도 있습니다.
			this.rigid2D.AddForce(transform.up * this.jumpForce);
			jumpCount++;
		}

		// 좌우이동 입력 감지
		int key = 0;
		if (Input.GetKey(KeyCode.RightArrow)) key = 1;
		if (Input.GetKey(KeyCode.LeftArrow)) key = -1;

		// 플레이어 x축 현재 속도 (상대속도 또는 절대속도 계산)
		// SmoothMovingCloudController 컴포넌트를 가진 부모가 있을 경우 상대 속도를 계산합니다.
		float currentHorizontalVelocity;
		Rigidbody2D parentRigidbody = null;
		if (transform.parent != null)
		{
			// 부모가 이동하는 발판인지 확인 (SmoothMovingCloudController 컴포넌트가 있다고 가정)
			if (transform.parent.GetComponent<SmoothMovingCloudController>() != null)
			{
				parentRigidbody = transform.parent.GetComponent<Rigidbody2D>();
			}
		}

		if (parentRigidbody != null) // 상대속도 계산
		{
			// 플레이어의 절대 속도에서 부모의 속도를 빼서 상대 속도를 얻습니다.
			Vector2 parentVelocity = parentRigidbody.linearVelocity;
			currentHorizontalVelocity = this.rigid2D.linearVelocity.x - parentVelocity.x;
		}
		else // 절대속도 사용
		{
			currentHorizontalVelocity = this.rigid2D.linearVelocity.x;
		}

		// 이동속도제한 및 공중 방향 전환 허용 로직 수정
		// key != 0 (좌우 입력이 있을 때)
		// 그리고 다음 두 조건 중 하나를 만족할 때 힘을 적용:
		// 1. key == 1 (오른쪽 입력) 이고 현재 수평 속도(currentHorizontalVelocity)가 maxWalkSpeed 미만일 때
		// 2. key == -1 (왼쪽 입력) 이고 현재 수평 속도(currentHorizontalVelocity)가 -maxWalkSpeed 초과일 때
		if (key != 0 && ((key == 1 && currentHorizontalVelocity < this.maxWalkSpeed) || (key == -1 && currentHorizontalVelocity > -this.maxWalkSpeed)))
		{
			// 원하는 방향으로 힘을 적용합니다.
			this.rigid2D.AddForce(transform.right * key * this.walkForce);
		}
		// 주의: 이 로직은 속도 제한 내에서만 힘을 적용하므로,
		// 최대 속도를 넘어서 가속되지는 않습니다.
		// 만약 플레이어가 반대 방향으로 빠르게 움직일 때 즉시 감속시키고 싶다면
		// rigid2D.velocity.x를 직접 조절하는 방식을 고려해 볼 수 있습니다.
		// 예: if (key == 1 && currentHorizontalVelocity < 0) { rigid2D.velocity = new Vector2(rigid2D.velocity.x * 0.9f, rigid2D.velocity.y); }

		// 이동방향에 따라 플레이어 스프라이트 반전
		// 좌우 입력이 있을 때 즉시 스프라이트를 반전합니다.
		if (key != 0)
		{
			transform.localScale = new Vector3(key, 1, 1);
		}

		// 플레이어 속도에 따라 애니메이션 속도 조절
		// 플레이어의 수직 속도가 거의 0에 가까울 때 (땅에 있을 때)
		if (Mathf.Abs(this.rigid2D.linearVelocity.y) < 0.1f)
		{
			// 좌우 입력이 있을 때
			if (key != 0)
			{
				// 수평 속도의 절대값 (magnitue)을 사용하여 걷기 애니메이션 속도 조절
				// speedx는 기존 로직에서 사용하던 수평 속도의 크기입니다.
				// 애니메이션에는 방향이 중요하지 않으므로 speedx를 그대로 사용합니다.
				float speedx = Mathf.Abs(currentHorizontalVelocity); // 상대 속도 크기 사용
				float normalizedSpeed = Mathf.Clamp01(speedx / this.maxWalkSpeed);
				this.animator.speed = normalizedSpeed; // 0에서 1 사이 값으로 애니메이션 속도 설정
			}
			else // 좌우 입력이 없을 때
			{
				this.animator.speed = 0f; // 애니메이션 정지
			}
		}
		else // 공중에 있을 때
		{
			this.animator.speed = 1.0f; // 애니메이션 속도 1로 설정 (점프/낙하 애니메이션 등)
		}

		// 플레이어가 특정 높이 아래로 떨어지면 처음부터 다시 시작
		if (transform.position.y < -10)
		{
			// 부모 객체로부터 분리
			if (transform.parent != null)
			{
				transform.SetParent(null);
			}
			// 현재 씬을 다시 로드합니다.
			SceneManager.LoadScene("ClimbCloud"); // 씬 이름 확인 필요 (예: SceneManager.GetActiveScene().name)
		}
	}

	// 착지 감지 및 점프 횟수 초기화 로직
	void OnCollisionEnter2D(Collision2D collision)
	{
		// 충돌한 오브젝트의 태그가 "Ground"인지 확인
		if (collision.gameObject.CompareTag("Ground"))
		{
			ContactPoint2D[] contacts = new ContactPoint2D[collision.contactCount];
			collision.GetContacts(contacts);
			bool landedOnTop = false;
			// 충돌 지점들의 정보를 확인하여 위쪽에서 충돌했는지 판단
			foreach (ContactPoint2D contact in contacts)
			{
				// 충돌 지점의 법선 벡터의 y 값이 0.7보다 크면 위쪽에서 충돌한 것으로 간주 (바닥에 착지)
				if (contact.normal.y > 0.7f) // 0.7f는 대략 45도 각도에 해당
				{
					landedOnTop = true;
					break;
				}
			}
			// 위에서 착지했다면 점프 횟수를 초기화
			if (landedOnTop)
			{
				jumpCount = 0;
			}
		}
		// 만약 moving cloud나 일반 cloud도 "Ground" 태그를 사용한다면,
		// 해당 발판들에 착지했을 때도 점프 횟수가 초기화됩니다.
		// 특정 발판에서만 초기화하고 싶다면 태그나 다른 방식으로 구분해야 합니다.
	}

	// 골 지점 도착(flag 엔티티 접촉) 시 실행
	void OnTriggerEnter2D(Collider2D other)
	{
		// 충돌한 트리거 오브젝트가 Flag인지 확인하는 로직이 필요합니다.
		// 예를 들어, Flag 엔티티에 "Flag" 태그를 적용하고 여기서 확인합니다.
		if (other.gameObject.CompareTag("Flag")) // "Flag" 태그가 적용되어 있다고 가정
		{
			// 부모 객체로부터 분리
			if (transform.parent != null)
			{
				transform.SetParent(null);
			}
			// 골에 도착했음을 로그로 출력
			Debug.Log("골");
			// 다음 씬 또는 메인 씬으로 이동
			SceneManager.LoadScene("ClimbCloud_Main"); // 씬 이름 확인 필요
		}
	}
}