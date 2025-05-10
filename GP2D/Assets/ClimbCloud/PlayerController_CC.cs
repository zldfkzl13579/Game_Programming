using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class PlayerController_CC : MonoBehaviour
{
    Rigidbody2D rigid2D;
	Animator animator;
    float jumpForce = 680.0f;
	float walkForce = 30.0f;
	float maxWalkSpeed = 2.0f;

	private int jumpCount = 0;
	public int maxJumpCount = 2;

	private int horizontalInput = 0;

	void Start()
	{
		this.rigid2D = GetComponent<Rigidbody2D>();
		this.animator = GetComponent<Animator>();
	}

	void Update()
	{
		// 점프
		if (Input.GetKeyDown(KeyCode.Space) && jumpCount < maxJumpCount)
		{
			this.animator.SetTrigger("JumpTrigger");
			if (transform.parent != null)
			{
				transform.SetParent(null);
			}
			this.rigid2D.AddForce(transform.up * this.jumpForce);
			jumpCount++;
		}

		// 좌우이동
		int key = 0;
		if (Input.GetKey(KeyCode.RightArrow)) key = 1;
		if (Input.GetKey(KeyCode.LeftArrow)) key = -1;

		// 플레이어 x축 속도
		float speedx;
		Rigidbody2D parentRigidbody = null;
		if (transform.parent != null)
		{
			if (transform.parent.GetComponent<SmoothMovingCloudController>() != null)
			{
				parentRigidbody = transform.parent.GetComponent<Rigidbody2D>();
			}
		}

		if (parentRigidbody != null) // 상대속도
		{
			Vector2 parentVelocity = parentRigidbody.linearVelocity;
			Vector2 relativeVelocity = this.rigid2D.linearVelocity - parentVelocity;
			speedx = Mathf.Abs(relativeVelocity.x);
		}
		else // 절대속도
		{
			speedx = Mathf.Abs(this.rigid2D.linearVelocity.x);
		}

		// 이동속도제한
		if (key != 0 && speedx < this.maxWalkSpeed) //key != 0 &&추가됨.
		{
			this.rigid2D.AddForce(transform.right * key * this.walkForce);
		}

		// 이동방향에 따라 반전
		if (key != 0)
		{
			transform.localScale = new Vector3(key, 1, 1);
		}

		// 플레이어 속도에 따라 애니메이션 속도 조절
		if (Mathf.Abs(this.rigid2D.linearVelocity.y) < 0.1f)
		{
			if (key != 0)
			{
				float normalizedSpeed = Mathf.Clamp01(speedx / this.maxWalkSpeed);
				this.animator.speed = normalizedSpeed;
			}
			else
			{
				this.animator.speed = 0f;
			}
		}
		else
		{
			this.animator.speed = 1.0f;
		}

		// 플레이어가 떨어지면 처음부터
		if (transform.position.y < -10)
		{
			if (transform.parent != null)
			{
				transform.SetParent(null);
			}
			SceneManager.LoadScene("ClimbCloud");
		}
	}

	void OnCollisionEnter2D(Collision2D collision) //착지 감지 및 점프 횟수 초기화 로직
	{
		if (collision.gameObject.CompareTag("Ground"))
		{
			ContactPoint2D[] contacts = new ContactPoint2D[collision.contactCount];
			collision.GetContacts(contacts);
			bool landedOnTop = false;
			foreach (ContactPoint2D contact in contacts)
			{
				if (contact.normal.y > 0.7f)
				{
					landedOnTop = true;
					break;
				}
			}
			if (landedOnTop)
			{
				jumpCount = 0;
			}
		}
	}

	void OnTriggerEnter2D(Collider2D other) //골 지점 도착(flag 엔티티 접촉) 시 실행 
	{
		if (transform.parent != null)
		{
			transform.SetParent(null);
		}
		Debug.Log("골");
		SceneManager.LoadScene("ClimbCloud_Main");
	}
}