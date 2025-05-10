using UnityEngine;

public class SmoothMovingCloudController : MonoBehaviour
{
	[Header("Movement Parameters")]
	[Tooltip("최대 이동 속도")]
	public float moveSpeed = 2.0f;

	[Tooltip("방향 전환 시 속도를 부드럽게 만드는데 걸리는 시간. 값이 클수록 전환이 느리고 부드러워집니다.")]
	public float smoothTime = 0.5f;

	[Tooltip("시작 위치로부터의 이동 거리 (Local Position 기준)")]
	public Vector3 localOffsetA = new Vector3(-5, 0, 0);

	[Tooltip("시작 위치로부터의 이동 거리 (Local Position 기준)")]
	public Vector3 localOffsetB = new Vector3(5, 0, 0);

	[Tooltip("체크하면 시작 시 우측(localOffsetB 방향)으로 먼저 이동합니다. 체크 해제 시 좌측(localOffsetA 방향)으로 먼저 이동합니다.")]
	public bool startMovingRight = true; // 이 변수로 시작 방향을 제어합니다.

	private Vector3 globalPointA;
	private Vector3 globalPointB;
	private Vector3 targetPoint; // 현재 목표 지점

	private Rigidbody2D rigid2D;

	// Vector3.SmoothDamp 함수 내부에서 사용되는 속도 (참조 변수)
	// 이 변수를 통해 SmoothDamp가 현재 속도를 추적하고 계산합니다.
	private Vector3 smoothDampVelocity = Vector3.zero;

	void Start()
	{
		rigid2D = GetComponent<Rigidbody2D>();
		if (rigid2D == null)
		{
			Debug.LogError("MovingCloud 스크립트는 Rigidbody2D 컴포넌트가 필요합니다!");
			enabled = false;
			return;
		}

		// Rigidbody2D의 Body Type 설정 확인 (Unity 6에서는 bodyType 사용 권장)
		// moving cloud는 Body Type이 'Kinematic'으로 설정되어야 합니다.
		if (rigid2D.bodyType != RigidbodyType2D.Kinematic)
		{
			Debug.LogWarning("MovingCloud의 Rigidbody2D는 Body Type이 'Kinematic'으로 설정되어야 합니다. 에디터에서 직접 설정해주세요.");
			// 필요하다면 여기서 코드로 Body Type을 설정할 수도 있습니다:
			// rigid2D.bodyType = RigidbodyType2D.Kinematic; 
		}

		// 게임 오브젝트의 초기 위치를 기준으로 이동할 전역(Global) 위치 계산
		Vector3 initialPosition = transform.position;
		globalPointA = initialPosition + localOffsetA;
		globalPointB = initialPosition + localOffsetB;

		// **수정된 부분: Inspector에서 설정한 startMovingRight 값에 따라 초기 목표 지점 설정**
		if (startMovingRight)
		{
			targetPoint = globalPointB; // 우측으로 먼저 이동
		}
		else
		{
			targetPoint = globalPointA; // 좌측으로 먼저 이동
		}
		// 이전의 거리 기반 초기 목표 설정 로직은 제거되었습니다.

		// SmoothDamp velocity 초기화 (시작 시 갑자기 움직이는 것 방지)
		smoothDampVelocity = Vector3.zero;
	}

	void FixedUpdate()
	{
		// Vector3.SmoothDamp를 사용하여 현재 위치를 목표 지점으로 부드럽게 이동
		// 이 함수가 내부적으로 smoothDampVelocity를 사용하여 현재 속도를 관리하며 보간합니다.
		Vector3 newPosition = Vector3.SmoothDamp(
			transform.position,     // 현재 위치
			targetPoint,            // 목표 위치
			ref smoothDampVelocity, // SmoothDamp가 사용하는 속도 (참조 전달)
			smoothTime,             // 목표 속도에 도달하는 데 걸리는 시간 (값이 작을수록 빠르게 도달 -> 덜 부드러움)
			moveSpeed,              // 최대 이동 속도 제한 (SmoothDamp가 이 속도를 넘지 않도록 보장)
			Time.fixedDeltaTime     // 물리 업데이트 시간 간격 (FixedUpdate에서는 Time.fixedDeltaTime 사용)
		);

		// 계산된 새 위치로 Rigidbody2D 이동
		// Kinematic Rigidbody는 MovePosition을 사용하여 이동시키는 것이 물리 충돌 처리에 좋습니다.
		rigid2D.MovePosition(newPosition);

		// 현재 위치가 목표 지점에 거의 도달했는지 확인 (다음 목표로 전환 로직)
		// SmoothDamp 특성상 정확히 targetPoint에 도달하기 어렵기에 작은 임계값 사용
		// 또는 smoothDampVelocity.magnitude가 매우 작아졌는지 (멈췄는지) 확인할 수도 있습니다.
		if (Vector3.Distance(transform.position, targetPoint) < 0.2f) // 임계값은 테스트하며 조절 (예: 0.1f ~ 0.3f)
		{
			// 목표 지점에 도달했으면 다음 목표 지점으로 전환
			if (targetPoint == globalPointB)
			{
				targetPoint = globalPointA;
			}
			else
			{
				targetPoint = globalPointB;
			}
			// smoothDampVelocity를 0으로 리셋하지 않습니다.
			// SmoothDamp가 새로운 targetPoint를 향해 속도를 자연스럽게 전환합니다.
		}
	}

	// OnCollisionEnter2D, OnCollisionExit2D, OnDrawGizmosSelected 함수는 동일합니다.

	void OnCollisionEnter2D(Collision2D collision)
	{
		if (collision.gameObject.name == "cat" || collision.gameObject.CompareTag("Player"))
		{
			ContactPoint2D[] contacts = new ContactPoint2D[collision.contactCount];
			collision.GetContacts(contacts);

			bool landedOnTop = false;
			foreach (ContactPoint2D contact in contacts)
			{
				if (contact.normal.y > 0.7f) // 위쪽에서 충돌했는지 확인
				{
					landedOnTop = true;
					break;
				}
			}

			if (landedOnTop)
			{
				// 플레이어를 구름의 자식으로 설정하여 함께 이동하도록 함
				collision.transform.SetParent(transform);
			}
		}
	}

	void OnCollisionExit2D(Collision2D collision)
	{
		// 플레이어가 구름에서 벗어났을 때 자식 해제
		if (collision.gameObject.name == "cat" || collision.gameObject.CompareTag("Player"))
		{
			if (collision.transform.parent == transform)
			{
				collision.transform.SetParent(null);
			}
		}
	}

	void OnDrawGizmosSelected()
	{
		// 에디터에서 구름의 시작 위치와 목표 지점 A, B를 시각적으로 표시
		Vector3 startPos = transform.position;
		Vector3 editorGlobalPointA = startPos + localOffsetA;
		Vector3 editorGlobalPointB = startPos + localOffsetB;

		Gizmos.color = Color.yellow;
		Gizmos.DrawLine(editorGlobalPointA, editorGlobalPointB);
		Gizmos.DrawWireSphere(editorGlobalPointA, 0.2f);
		Gizmos.DrawWireSphere(editorGlobalPointB, 0.2f);
	}
}