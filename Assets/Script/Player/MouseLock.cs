using UnityEngine;

public class MouseLook : MonoBehaviour
{
    [Header("마우스 감도")]
    public float mouseSensitivity = 100f;

    [Header("연결할 오브젝트")]
    public Transform firstPersonCamera; // 카메라 Transform
    public Transform playerBody;        // 플레이어 몸통 (회전 기준)

    private float xRotation = 0f; // 상하 회전 값

    void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    void Update()
    {
        float mouseX = Input.GetAxis("Mouse X") * mouseSensitivity * Time.deltaTime;
        float mouseY = Input.GetAxis("Mouse Y") * mouseSensitivity * Time.deltaTime;

        // 카메라 상하 회전
        xRotation -= mouseY;
        xRotation = Mathf.Clamp(xRotation, -90f, 90f);
        firstPersonCamera.localRotation = Quaternion.Euler(xRotation, 0f, 0f);

        // 플레이어 좌우 회전
        playerBody.Rotate(Vector3.up * mouseX);
    }
}
