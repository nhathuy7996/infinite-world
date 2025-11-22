using UnityEngine;

namespace InfiniteWorld
{
    [RequireComponent(typeof(CharacterController))]
    public class PlayerController : MonoBehaviour
    {
        [Header("Movement Settings")]
        [SerializeField] private float moveSpeed = 10f;
        [SerializeField] private float sprintMultiplier = 2f;
        [SerializeField] private float rotationSpeed = 5f;

        [Header("References")]
        [SerializeField] private FloatingOrigin floatingOrigin;
        [SerializeField] private ChunkManager chunkManager;

        [Header("Debug")]
        [SerializeField] private bool showDebugUI = true;

        private CharacterController characterController;
        private Vector3 moveDirection;

        private void Awake()
        {
            characterController = GetComponent<CharacterController>();

            if (floatingOrigin == null)
                floatingOrigin = FindObjectOfType<FloatingOrigin>();

            if (chunkManager == null)
                chunkManager = FindObjectOfType<ChunkManager>();
        }

        private void Update()
        {
            HandleMovement();
            HandleRotation();
        }

        private void HandleMovement()
        {
            float horizontal = Input.GetAxis("Horizontal");
            float vertical = Input.GetAxis("Vertical");

            Vector3 forward = transform.forward;
            Vector3 right = transform.right;

            forward.y = 0;
            right.y = 0;
            forward.Normalize();
            right.Normalize();

            moveDirection = (forward * vertical + right * horizontal).normalized;

            float speed = moveSpeed;
            if (Input.GetKey(KeyCode.LeftShift))
            {
                speed *= sprintMultiplier;
            }

            if (moveDirection.magnitude > 0.1f)
            {
                characterController.Move(moveDirection * speed * Time.deltaTime);
            }

            characterController.Move(Vector3.down * 9.81f * Time.deltaTime);
        }

        private void HandleRotation()
        {
            if (Input.GetMouseButton(1))
            {
                float mouseX = Input.GetAxis("Mouse X");
                transform.Rotate(Vector3.up, mouseX * rotationSpeed);
            }

            if (Input.GetKey(KeyCode.Q))
            {
                transform.Rotate(Vector3.up, -rotationSpeed * Time.deltaTime * 50f);
            }
            if (Input.GetKey(KeyCode.E))
            {
                transform.Rotate(Vector3.up, rotationSpeed * Time.deltaTime * 50f);
            }
        }

        private void OnGUI()
        {
            if (!showDebugUI)
                return;

            float startY = 220;
            GUILayout.BeginArea(new Rect(10, startY, 400, 300));

            GUILayout.Label($"<b>Player Info</b>", new GUIStyle(GUI.skin.label) { fontSize = 16, richText = true });
            GUILayout.Label($"Unity Position: {transform.position.ToString("F2")}");

            if (floatingOrigin != null)
            {
                Vector3 worldPos = floatingOrigin.GetPlayerWorldPosition();
                GUILayout.Label($"World Position: {worldPos.ToString("F2")}");

                WorldCoordinate coord = WorldCoordinate.FromWorldPosition(worldPos, 50f);
                GUILayout.Label($"Chunk: ({coord.chunkIndex.x}, {coord.chunkIndex.y})");
                GUILayout.Label($"Local in Chunk: ({coord.localPosition.x:F2}, {coord.localPosition.y:F2})");
            }

            if (chunkManager != null)
            {
                Vector2Int chunkIndex = chunkManager.GetPlayerChunkIndex();
                int loadedChunks = chunkManager.GetLoadedChunks().Count;
                GUILayout.Label($"Current Chunk: ({chunkIndex.x}, {chunkIndex.y})");
                GUILayout.Label($"Loaded Chunks: {loadedChunks}");
            }

            GUILayout.Space(10);
            GUILayout.Label("Controls:");
            GUILayout.Label("WASD - Move");
            GUILayout.Label("Shift - Sprint");
            GUILayout.Label("Q/E or Right Mouse - Rotate");

            GUILayout.EndArea();
        }
    }
}
