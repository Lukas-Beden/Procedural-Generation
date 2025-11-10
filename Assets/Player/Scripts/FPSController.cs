using UnityEditor;
using UnityEngine;

public class FPSController : MonoBehaviour
{
    [Header("Script")]
    [SerializeField] private InputHandler _inputHandler;    

    [Header("Movement")]
    private int _moveSpeed = 5;
    [SerializeField] private LayerMask groundLayer;
    [SerializeField] private int jumpForce;

    [Header("Camera")]
    [SerializeField] private Camera _mainCamera;
    private float _xRotation = 0f;
    private float _mouseSensitivity = 25f;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Locked;
    }

    // Update is called once per frame
    void Update()
    {
        Move();
        Jump();
        Look();
    }

    void Move()
    {
        Vector2 move = _inputHandler.Move;
        Vector3 moveDirection = transform.forward * move.y + transform.right * move.x;
        transform.position += moveDirection * (_moveSpeed * Time.deltaTime);
    }

    void Look()
    {
        Vector2 look = _inputHandler.Look;

        transform.Rotate(Vector3.up * look.x * (_mouseSensitivity * Time.deltaTime));

        _xRotation -= look.y * (_mouseSensitivity * Time.deltaTime);
        _xRotation = Mathf.Clamp(_xRotation, -90f, 90f);
        _mainCamera.transform.localRotation = Quaternion.Euler(_xRotation, 0f, 0f);
    }
    bool IsGrounded()
    {
        return Physics.Raycast(transform.position, Vector3.down, 1.1f, groundLayer);
    }

    void Jump()
    {
        Debug.Log("feur");
        if (_inputHandler.Jump && IsGrounded())
        {
            Debug.Log("apagnan");
            Rigidbody rb = gameObject.GetComponent<Rigidbody>();
            rb.linearVelocity = new Vector3(0, jumpForce, 0);
        }
    }
}
 