using UnityEngine;

public class CameraFollow : MonoBehaviour
{
	public Transform target; // 카메라가 따라갈 대상 (플레이어)
	public float smoothSpeed = 0.125f; // 카메라 움직임의 부드러움 정도
	public Vector3 offset; // 플레이어로부터의 카메라 오프셋 (x, y, z)

	// FixedUpdate 대신 LateUpdate를 사용하면 모든 오브젝트의 업데이트가 끝난 후 카메라 위치를 조정하므로 부드럽습니다.
	void LateUpdate()
	{
		if (target == null)
		{
			Debug.LogWarning("Camera target is not assigned!");
			return;
		}

		// 목표 지점 (플레이어의 X 위치에 오프셋을 더함)
		// 카메라의 Y축은 고정된 값을 사용합니다. (현재 카메라의 Y값)
		// Z축도 고정된 값을 사용합니다. (현재 카메라의 Z값, 보통 -10)
		Vector3 desiredPosition = new Vector3(target.position.x + offset.x, transform.position.y + offset.y, transform.position.z + offset.z);

		// Lerp를 사용하여 카메라를 목표 지점으로 부드럽게 이동시킵니다.
		Vector3 smoothedPosition = Vector3.Lerp(transform.position, desiredPosition, smoothSpeed);
		transform.position = smoothedPosition;

		// (선택 사항) 플레이어를 바라보게 하려면
		// transform.LookAt(target);
	}
}