using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// Movimiento con WASD / flechas relativo a la cámara.
/// El jugador gira hacia donde camina. Usa CharacterController.
/// </summary>
[RequireComponent(typeof(CharacterController))]
public class PlayerMovement : MonoBehaviour
{
    public float moveSpeed = 5f;
    public float rotationSpeed = 720f; // grados por segundo
    public float gravity = -20f;

    private CharacterController controller;
    private Transform cam;
    private float verticalVelocity;

    void Awake()
    {
        controller = GetComponent<CharacterController>();
        if (Camera.main != null) cam = Camera.main.transform;
    }

    void Update()
    {
        // No moverse en pausa o al terminar el nivel
        if (GameManager.Instance != null && !GameManager.Instance.IsPlaying) return;

        Vector2 input = ReadInput();

        // Dirección relativa a la cámara (ignorando la inclinación)
        Vector3 forward = cam != null ? cam.forward : Vector3.forward;
        Vector3 right = cam != null ? cam.right : Vector3.right;
        forward.y = 0f; right.y = 0f;
        forward.Normalize(); right.Normalize();

        Vector3 move = forward * input.y + right * input.x;
        if (move.sqrMagnitude > 1f) move.Normalize();

        // Girar hacia donde camina
        if (move.sqrMagnitude > 0.01f)
        {
            Quaternion target = Quaternion.LookRotation(move);
            transform.rotation = Quaternion.RotateTowards(transform.rotation, target, rotationSpeed * Time.deltaTime);
        }

        // Gravedad simple
        if (controller.isGrounded && verticalVelocity < 0f) verticalVelocity = -2f;
        verticalVelocity += gravity * Time.deltaTime;

        Vector3 velocity = move * moveSpeed + Vector3.up * verticalVelocity;
        controller.Move(velocity * Time.deltaTime);
    }

    /// <summary>Lee WASD y flechas directamente del teclado (Input System nuevo).</summary>
    private Vector2 ReadInput()
    {
        Keyboard kb = Keyboard.current;
        if (kb == null) return Vector2.zero;

        float x = 0f, y = 0f;
        if (kb.aKey.isPressed || kb.leftArrowKey.isPressed) x -= 1f;
        if (kb.dKey.isPressed || kb.rightArrowKey.isPressed) x += 1f;
        if (kb.sKey.isPressed || kb.downArrowKey.isPressed) y -= 1f;
        if (kb.wKey.isPressed || kb.upArrowKey.isPressed) y += 1f;
        return new Vector2(x, y);
    }
}
