using UnityEngine;

// FirstPersonController.cs
// A minimal, dependency-free stand-in for what Unreal's Manny + Third Person template gives
// you for free. No animation, no imported character asset - just WASD + mouse look on a
// CharacterController. This is enough for weapon switching / homing missile / save-load,
// none of which need a visible animated body (first-person, so you never see yourself anyway).
//
// SETUP:
//  1. Create an empty GameObject "Player". Add a CharacterController component to it
//     (Component > Physics > Character Controller - it has its own built-in capsule shape,
//     you don't need a separate mesh/collider unless you want one visible for testing).
//  2. Add this script to the same object.
//  3. Add a Camera as a CHILD of Player, positioned at roughly (0, 1.6, 0) for eye height.
//     Drag that Camera into this script's `playerCamera` field.
//  4. Also add WeaponSwitcher, SaveLoadManager, and AssessmentHUD to the Player object.
//     WeaponSwitcher's `aimCamera` should point at the same Camera; give it a `muzzle` empty
//     child positioned slightly in front of the camera.
//  5. Press Play - mouse look is captured by default. Press Escape to release the cursor
//     (useful while testing in the editor).
[RequireComponent(typeof(CharacterController))]
public class FirstPersonController : MonoBehaviour
{
    [Header("Movement")]
    public float moveSpeed = 6f;
    public float gravity = -20f;
    public float jumpHeight = 1.2f;

    [Header("Look")]
    public Camera playerCamera;
    public float mouseSensitivity = 2f;
    public float minPitch = -85f;
    public float maxPitch = 85f;

    CharacterController controller;
    Vector3 velocity;
    float pitch;

    void Awake()
    {
        controller = GetComponent<CharacterController>();
        Cursor.lockState = CursorLockMode.Locked;
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            Cursor.lockState = Cursor.lockState == CursorLockMode.Locked
                ? CursorLockMode.None
                : CursorLockMode.Locked;
        }

        HandleLook();
        HandleMove();
    }

    void HandleLook()
    {
        if (Cursor.lockState != CursorLockMode.Locked) return;

        float mouseX = Input.GetAxis("Mouse X") * mouseSensitivity;
        float mouseY = Input.GetAxis("Mouse Y") * mouseSensitivity;

        transform.Rotate(Vector3.up * mouseX);

        pitch = Mathf.Clamp(pitch - mouseY, minPitch, maxPitch);
        if (playerCamera) playerCamera.transform.localEulerAngles = new Vector3(pitch, 0f, 0f);
    }

    void HandleMove()
    {
        bool grounded = controller.isGrounded;
        if (grounded && velocity.y < 0f) velocity.y = -2f;

        float x = Input.GetAxis("Horizontal");
        float z = Input.GetAxis("Vertical");
        Vector3 move = transform.right * x + transform.forward * z;
        controller.Move(move * moveSpeed * Time.deltaTime);

        if (grounded && Input.GetButtonDown("Jump"))
        {
            velocity.y = Mathf.Sqrt(jumpHeight * -2f * gravity);
        }

        velocity.y += gravity * Time.deltaTime;
        controller.Move(velocity * Time.deltaTime);
    }
}
