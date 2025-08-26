using UnityEngine;

// Controlador del jugador para movimiento y cámara
public class PlayerController : MonoBehaviour
{
    [Header("Movimiento")]
    public float speed = 5f; // Velocidad de movimiento del jugador

    [Header("Mouse Look")]
    public float sensitivity = 2f; // Sensibilidad del mouse
    public Transform playerCamera; // Referencia a la cámara del jugador
    private float xRotation = 0f;  // Rotación acumulada de la cámara en X

    private CharacterController controller; // Componente CharacterController

    void Start()
    {
        // Obtener el CharacterController adjunto
        controller = GetComponent<CharacterController>();
        // Bloquear el cursor en el centro de la pantalla
        Cursor.lockState = CursorLockMode.Locked;
    }

    void Update()
    {
        Mover(); // Llamada a la función de movimiento
        Mirar(); // Llamada a la función de control de cámara
    }

    // Maneja el movimiento del jugador
    void Mover()
    {
        // Obtener input del teclado
        float x = Input.GetAxis("Horizontal"); // A/D o izquierda/derecha
        float z = Input.GetAxis("Vertical");   // W/S o adelante/atrás

        // Calcular vector de movimiento relativo a la rotación del jugador
        Vector3 move = transform.right * x + transform.forward * z;

        // Mover el CharacterController
        controller.Move(move * speed * Time.deltaTime);
    }

    // Maneja la rotación de la cámara y del jugador con el mouse
    void Mirar()
    {
        // Obtener movimiento del mouse
        float mouseX = Input.GetAxis("Mouse X") * sensitivity;
        float mouseY = Input.GetAxis("Mouse Y") * sensitivity;

        // Ajustar rotación vertical de la cámara
        xRotation -= mouseY;
        xRotation = Mathf.Clamp(xRotation, -90f, 90f); // Limitar rotación vertical

        // Aplicar rotación a la cámara
        playerCamera.localRotation = Quaternion.Euler(xRotation, 0f, 0f);
        // Rotar el jugador horizontalmente
        transform.Rotate(Vector3.up * mouseX);
    }
}
