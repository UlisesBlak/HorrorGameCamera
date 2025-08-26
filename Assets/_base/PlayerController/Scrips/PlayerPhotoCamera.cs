using UnityEngine;
using System.Collections;

// Controlador para tomar fotos con cámara fantasma y flash
public class PlayerPhotoCamera : MonoBehaviour
{
    [Header("Cámara fantasma que toma la foto (no se verá en pantalla)")]
    public Camera photoCamera;          // Cámara invisible que captura la foto
    public RenderTexture renderTexture; // RenderTexture donde se guarda la imagen

    [Header("Quad donde se muestra la foto")]
    public MeshRenderer photoQuad;      // Quad donde se mostrará la foto

    [Header("Tiempo visible de la foto")]
    public float photoDuration = 5f;    // Tiempo que se muestra la foto en pantalla

    [Header("Flash")]
    public Light flashLight;            // Luz tipo flash
    public float flashDuration = 0.2f;  // Duración del flash
    public float holdTime = 1.5f;       // Tiempo de mantener clic derecho para tomar foto

    private Texture2D lastPhoto;        // Última foto tomada
    private Coroutine photoRoutine;     // Rutina para mostrar la foto
    private Material photoMaterial;     // Material del quad para asignar la foto

    private bool isHolding = false;     // Indica si se está manteniendo clic derecho
    private float holdTimer = 0f;       // Temporizador de hold

    void Start()
    {
        // Validar referencias
        if (photoCamera == null || renderTexture == null)
        {
            Debug.LogError("❌ Faltan referencias en PlayerPhotoCamera.");
            enabled = false;
            return;
        }

        photoCamera.enabled = false; // Desactivar cámara al inicio

        // Configurar quad de foto
        if (photoQuad != null)
        {
            photoQuad.gameObject.SetActive(false);
            photoMaterial = photoQuad.material;
        }

        // Apagar flash al inicio
        if (flashLight != null)
            flashLight.enabled = false;
    }

    void Update()
    {
        // Detectar inicio de clic derecho
        if (Input.GetMouseButtonDown(1))
        {
            isHolding = true;
            holdTimer = 0f;
        }

        // Manteniendo clic derecho
        if (isHolding && Input.GetMouseButton(1))
        {
            holdTimer += Time.deltaTime;

            // Si se mantiene el tiempo suficiente, tomar foto
            if (holdTimer >= holdTime)
            {
                StartCoroutine(FlashAndTakePhoto());
                isHolding = false;
            }
        }

        // Soltar clic derecho antes del holdTime → solo flash
        if (Input.GetMouseButtonUp(1))
        {
            if (isHolding && holdTimer < holdTime)
            {
                StartCoroutine(FlashOnly());
            }
            isHolding = false;
        }
    }

    // Flash sin tomar foto
    IEnumerator FlashOnly()
    {
        if (flashLight != null)
        {
            flashLight.enabled = true;
            yield return new WaitForSeconds(flashDuration);
            flashLight.enabled = false;
        }
    }

    // Flash + tomar foto
    IEnumerator FlashAndTakePhoto()
    {
        if (flashLight != null)
        {
            flashLight.enabled = true;

            // Esperar un frame para que la luz ilumine la escena antes de capturar
            yield return null;
        }

        TakePhoto();

        if (flashLight != null)
        {
            // Mantener flash visible un breve momento
            yield return new WaitForSeconds(flashDuration);
            flashLight.enabled = false;
        }
    }

    // Captura la foto desde la cámara fantasma
    void TakePhoto()
    {
        photoCamera.targetTexture = renderTexture;
        RenderTexture.active = renderTexture;
        photoCamera.Render();

        // Crear o ajustar textura para guardar la foto
        if (lastPhoto == null || lastPhoto.width != renderTexture.width || lastPhoto.height != renderTexture.height)
            lastPhoto = new Texture2D(renderTexture.width, renderTexture.height, TextureFormat.RGB24, false);

        lastPhoto.ReadPixels(new Rect(0, 0, renderTexture.width, renderTexture.height), 0, 0);
        lastPhoto.Apply();

        photoCamera.targetTexture = null;
        RenderTexture.active = null;

        // Mostrar foto en el quad
        if (photoQuad != null && photoMaterial != null)
        {
            photoMaterial.mainTexture = lastPhoto;

            // Detener rutina anterior si existe
            if (photoRoutine != null)
                StopCoroutine(photoRoutine);

            photoRoutine = StartCoroutine(ShowPhotoRoutine());
        }
    }

    // Rutina para mostrar la foto en pantalla temporalmente
    IEnumerator ShowPhotoRoutine()
    {
        photoQuad.gameObject.SetActive(true);
        yield return new WaitForSeconds(photoDuration);
        photoQuad.gameObject.SetActive(false);
    }
}
