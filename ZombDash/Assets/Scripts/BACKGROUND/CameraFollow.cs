using UnityEngine;

public class CameraFollow : MonoBehaviour
{
	public Transform target; // ī�޶� ���� ��� (�÷��̾�)
	public float smoothSpeed = 0.125f; // ī�޶� �������� �ε巯�� ����
	public Vector3 offset; // �÷��̾�κ����� ī�޶� ������ (x, y, z)

	// FixedUpdate ��� LateUpdate�� ����ϸ� ��� ������Ʈ�� ������Ʈ�� ���� �� ī�޶� ��ġ�� �����ϹǷ� �ε巴���ϴ�.
	void LateUpdate()
	{
		if (target == null)
		{
			Debug.LogWarning("Camera target is not assigned!");
			return;
		}

		// ��ǥ ���� (�÷��̾��� X ��ġ�� �������� ����)
		// ī�޶��� Y���� ������ ���� ����մϴ�. (���� ī�޶��� Y��)
		// Z�൵ ������ ���� ����մϴ�. (���� ī�޶��� Z��, ���� -10)
		Vector3 desiredPosition = new Vector3(target.position.x + offset.x, transform.position.y + offset.y, transform.position.z + offset.z);

		// Lerp�� ����Ͽ� ī�޶� ��ǥ �������� �ε巴�� �̵���ŵ�ϴ�.
		Vector3 smoothedPosition = Vector3.Lerp(transform.position, desiredPosition, smoothSpeed);
		transform.position = smoothedPosition;

		// (���� ����) �÷��̾ �ٶ󺸰� �Ϸ���
		// transform.LookAt(target);
	}
}