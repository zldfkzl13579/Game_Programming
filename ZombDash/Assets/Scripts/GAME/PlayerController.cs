using UnityEngine;

public class PlayerController : MonoBehaviour
{
    public float moveSpeed = 5f;
    public float jumpForce = 10f;
    public float lineChangeDelay = 0.2f; // ���� ���� ������

    // �÷��̾��� ���� Y�� ���� ��ġ (0: ��, 1: ���, 2: �Ʒ�)
    public float[] laneYPositions = { 2f, 0f, -2f }; // ����: Z=1 (�� ����)�� Y=2, Z=2 (��� ����)�� Y=0, Z=3 (�Ʒ� ����)�� Y=-2

    // �÷��̾��� Z�� ���� �� (Z=1, Z=2, Z=3)
    public float[] laneZDepths = { 1f, 2f, 3f }; // Z�� �� 1, 2, 3 (���� Z�� depth)

    // ���̾� ����ũ�� �ν����Ϳ��� �Ҵ�
    public LayerMask laneZ1Layer; // LaneZ1 ���̾� ����ũ
    public LayerMask laneZ2Layer; // LaneZ2 ���̾� ����ũ
    public LayerMask laneZ3Layer; // LaneZ3 ���̾� ����ũ

    public float horizontalMoveSpeed = 3f; // A/D Ű�� �յ� �̵� �ӵ�
    public Transform playerCamera; // ī�޶� ���� (������ ����)
    public float cameraMinX = -5f; // ī�޶� ���� �÷��̾� �ּ� X ��ġ (ȭ�� ���� ��)
    public float cameraMaxX = 5f; // ī�޶� ���� �÷��̾� �ּ� X ��ġ (ȭ�� ������ ��)

    public float groundTolerance = 0.1f; // �÷��̾ ���� ���� Y�� ��ġ�� �����ߴٰ� �Ǵ��� ���� ����

    private Rigidbody2D rb;
    private Animator animator;
    private int currentLaneIndex = 1; // 0: �� ���� (Z=1, Y=2), 1: ��� ���� (Z=2, Y=0), 2: �Ʒ� ���� (Z=3, Y=-2)
    private float lastLaneChangeTime;
    private bool isHit = false; // �ǰ� ���� ����
    private bool isDead = false; // ��� ���� ����

    private bool isCurrentlyInAir = false; // �÷��̾ ���� ���̰ų� ���� ������ ��ũ��Ʈ���� ����
    private float jumpStartLaneYPosition; // ������ ������ ������ ��Ȯ�� Y�� ��ġ ����

    // �÷��̾��� �ʱ� �ݶ��̴� ũ��� �������� ������ ����
    private CapsuleCollider2D playerCollider;
    private Vector2 originalColliderSize;
    private Vector2 originalColliderOffset;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();
        playerCollider = GetComponent<CapsuleCollider2D>(); // CapsuleCollider2D ��� ����
        if (playerCollider != null)
        {
            originalColliderSize = playerCollider.size;
            originalColliderOffset = playerCollider.offset;
        }

        lastLaneChangeTime = Time.time;

        // ���� ���� �ô� ���� �ִٰ� ����
        animator.SetBool("IsGrounded", true);
        isCurrentlyInAir = false;

        // ���� ���� �� �÷��̾ ��� ����(Z=2, Y=0)���� ����
        transform.position = new Vector3(transform.position.x, laneYPositions[currentLaneIndex], laneZDepths[currentLaneIndex]);
        UpdatePlayerLayer(); // �ʱ� ���̾� ����
    }

    void Update()
    {
        // �ǰ� �Ǵ� ��� �� ���� �Ҵ�
        if (isHit || isDead)
        {
            transform.Translate(Vector2.right * moveSpeed * Time.deltaTime);
            return;
        }

        // 1. �ڵ� �޸��� (X�� �̵�) - �׻� ����
        transform.Translate(Vector2.right * moveSpeed * Time.deltaTime);

        if (Input.GetKeyDown(KeyCode.W))
        {
            Debug.Log("W key pressed!"); // 이 줄 추가
            HandleLaneChange();
        }
        else if (Input.GetKeyDown(KeyCode.S))
        {
            Debug.Log("S key pressed!"); // 이 줄 추가
            HandleLaneChange();
        }

        // 3. 전후 이동 (A/D) - (이건 된다고 하셨으니 일단 스킵)
        // HandleHorizontalMovement();

        // 4. 점프 (Space)
        if (Input.GetButtonDown("Jump")) // GetButtonDown("Jump")는 Spacebar에 매핑됩니다.
        {
            Debug.Log("Spacebar pressed (Jump)!"); // 이 줄 추가
            HandleJump();
        }

        // 5. 슬라이딩 (Ctrl)
        if (Input.GetKey(KeyCode.LeftControl))
        {
            Debug.Log("LeftControl key held (Slide)!"); // 이 줄 추가
            HandleSlide();
        }
        else if (Input.GetKeyUp(KeyCode.LeftControl)) // Ctrl 키를 떼는 것도 로그로 확인
        {
            Debug.Log("LeftControl key released (Slide)!"); // 이 줄 추가
            HandleSlide(); // 슬라이드 종료를 위해 다시 호출 (else 블록이 실행됨)
        }

        // 6. 사격 (P)
        if (Input.GetKeyDown(KeyCode.P))
        {
            Debug.Log("P key pressed (Shoot)!"); // 이 줄 추가
            HandleShoot();
        }
    }

    void FixedUpdate()
    {
        // ���� ������Ʈ�� FixedUpdate����
        UpdateGroundStatusBasedOnYPosition();
    }

    void LateUpdate()
    {
        if (playerCamera != null)
        {
            Vector3 cameraTargetPos = new Vector3(transform.position.x, playerCamera.position.y, playerCamera.position.z);
            playerCamera.position = Vector3.Lerp(playerCamera.position, cameraTargetPos, Time.deltaTime * 5f);
        }
    }

    void HandleLaneChange()
    {
        // ���߿����� ���� ���� ������ �� ���� ������ ����
        if (Time.time - lastLaneChangeTime > lineChangeDelay && !isCurrentlyInAir)
        {
            int previousLaneIndex = currentLaneIndex;

            if (Input.GetKeyDown(KeyCode.W)) // �� ���� �̵� (Z���� �۾���, Y���� Ŀ��)
            {
                if (currentLaneIndex > 0)
                {
                    currentLaneIndex--;
                    lastLaneChangeTime = Time.time;
                }
            }
            else if (Input.GetKeyDown(KeyCode.S)) // �Ʒ� ���� �̵� (Z���� Ŀ��, Y���� �۾���)
            {
                if (currentLaneIndex < laneYPositions.Length - 1)
                {
                    currentLaneIndex++;
                    lastLaneChangeTime = Time.time;
                }
            }

            // ���� �ε����� ����Ǿ��ٸ� Y��, Z�� ��ġ�� ��� ������Ʈ�ϰ� �÷��̾� ���̾� ������Ʈ
            if (previousLaneIndex != currentLaneIndex)
            {
                transform.position = new Vector3(transform.position.x, laneYPositions[currentLaneIndex], laneZDepths[currentLaneIndex]);
                UpdatePlayerLayer(); // �÷��̾� ���̾� ������Ʈ
            }
        }
    }

    void UpdatePlayerLayer()
    {
        string layerName = "";
        switch (currentLaneIndex)
        {
            case 0: layerName = "LaneZ1"; break; // Z = 1 (�� ����)
            case 1: layerName = "LaneZ2"; break; // Z = 2 (��� ����)
            case 2: layerName = "LaneZ3"; break; // Z = 3 (�Ʒ� ����)
        }
        if (!string.IsNullOrEmpty(layerName))
        {
            gameObject.layer = LayerMask.NameToLayer(layerName);
            // Debug.Log($"Player's layer changed to: {layerName} ({gameObject.layer})"); // ����׿�
        }
    }

    void HandleHorizontalMovement()
    {
        float currentX = transform.position.x;
        float cameraX = playerCamera.position.x;
        float minX = cameraX + cameraMinX;
        float maxX = cameraX + cameraMaxX;

        float horizontalInput = 0f;
        if (Input.GetKey(KeyCode.A))
        {
            horizontalInput = -1f;
        }
        else if (Input.GetKey(KeyCode.D))
        {
            horizontalInput = 1f;
        }

        currentX += horizontalInput * horizontalMoveSpeed * Time.deltaTime;
        transform.position = new Vector3(Mathf.Clamp(currentX, minX, maxX), transform.position.y, transform.position.z);
    }

    void HandleJump()
    {
        // Space �ٸ� ������ ���� ���� ���� ���� ���� ���
        if (Input.GetButtonDown("Jump") && animator.GetBool("IsGrounded"))
        {
            rb.linearVelocity = new Vector2(rb.linearVelocity.x, jumpForce);
            animator.SetBool("IsJumping", true);
            animator.SetBool("IsGrounded", false);

            isCurrentlyInAir = true;
            // ���� ���� Y ��ġ�� ���� ������ ���� Y ��ġ�� �����մϴ�.
            jumpStartLaneYPosition = laneYPositions[currentLaneIndex];
        }
    }

    void HandleSlide()
    {
        if (Input.GetKey(KeyCode.LeftControl))
        {
            animator.SetBool("IsSliding", true);
            if (playerCollider != null)
            {
                playerCollider.size = new Vector2(originalColliderSize.x, originalColliderSize.y / 2);
                playerCollider.offset = new Vector2(originalColliderOffset.x, originalColliderOffset.y - originalColliderSize.y / 4);
            }
        }
        else
        {
            animator.SetBool("IsSliding", false);
            if (playerCollider != null)
            {
                playerCollider.size = originalColliderSize;
                playerCollider.offset = originalColliderOffset;
            }
        }
    }

    void HandleShoot()
    {
        if (Input.GetKeyDown(KeyCode.P))
        {
            animator.SetTrigger("ShootTrigger");
            // TODO: �Ѿ� ���� �� �߻� ���� �߰�
        }
    }

    void UpdateAnimatorParameters()
    {
        // IsGrounded �Ķ���ʹ� FixedUpdate�� UpdateGroundStatusBasedOnYPosition()���� ������Ʈ
    }

    // �÷��̾ ���� ���� ������ Y�࿡ �����ߴ��� Ȯ���ϴ� �Լ�
    void UpdateGroundStatusBasedOnYPosition()
    {
        // �÷��̾ ���߿� �� �ִ� ������ ���� ���� ���� üũ
        if (isCurrentlyInAir)
        {
            // Debug.Log($"Player Y: {transform.position.y}, Target Y: {laneYPositions[currentLaneIndex]}, Velocity Y: {rb.velocity.y}");

            // �÷��̾ ���� ���̰� (rb.velocity.y <= 0)
            // ���� Y ��ġ�� ��ǥ ���� Y ��ġ + ��� ���� ���� ���� ������ �����ߴٰ� �Ǵ�
            if (rb.linearVelocity.y <= 0 && transform.position.y <= laneYPositions[currentLaneIndex] + groundTolerance)
            {
                // ���� ó��
                isCurrentlyInAir = false;
                animator.SetBool("IsJumping", false);
                animator.SetBool("IsGrounded", true);

                rb.linearVelocity = new Vector2(rb.linearVelocity.x, 0); // Y�� �ӵ��� 0���� �����Ͽ� ���� �ٵ��� ��
                // Y�� ��ġ�� ���� ������ ��ǥ Y ��ġ�� ��Ȯ�� �����Ͽ� �̼��� ���� ���� (Z���� ����)
                transform.position = new Vector3(transform.position.x, laneYPositions[currentLaneIndex], transform.position.z);
            }
        }
        else // ���� ���� ���� Y�� ��ġ�� ��Ȯ�� �����ϵ��� �����մϴ�. (�̼��� ���� ���� ����)
        {
            if (Mathf.Abs(transform.position.y - laneYPositions[currentLaneIndex]) > groundTolerance)
            {
                transform.position = new Vector3(transform.position.x, laneYPositions[currentLaneIndex], transform.position.z);
                rb.linearVelocity = new Vector2(rb.linearVelocity.x, 0); // Y�� �ӵ��� 0���� ����
            }
        }
    }

    public void TakeHit()
    {
        if (isHit || isDead) return;

        isHit = true;
        animator.SetTrigger("HitTrigger");
        Invoke("ResetHitState", 0.5f);
    }

    void ResetHitState()
    {
        isHit = false;
    }

    public void Die()
    {
        if (isDead) return;

        isDead = true;
        animator.SetTrigger("DeathTrigger");
        // TODO: ���� ���� ���� �߰�
    }
}