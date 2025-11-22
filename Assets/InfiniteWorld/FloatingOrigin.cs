using UnityEngine;
using System.Collections.Generic;

namespace InfiniteWorld
{
    public class FloatingOrigin : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private Transform player;
        [SerializeField] private ChunkManager chunkManager;

        [Header("Floating Origin Settings")]
        [SerializeField] private float threshold = 1000f;
        [SerializeField] private bool autoShift = true;

        [Header("What to Shift")]
        [SerializeField] private bool shiftParticles = true;
        [SerializeField] private bool shiftRigidbodies = true;
        [SerializeField] private bool shiftTrailRenderers = true;

        [Header("Debug")]
        [SerializeField] private bool showDebugInfo = true;

        private Vector3 totalOffset = Vector3.zero;
        private int shiftCount = 0;

        private void Update()
        {
            if (!autoShift || player == null)
                return;

            float distanceFromOrigin = player.position.magnitude;

            if (distanceFromOrigin > threshold)
            {
                PerformOriginShift();
            }
        }

        public void PerformOriginShift()
        {
            if (player == null)
            {
                Debug.LogWarning("Cannot perform origin shift: Player reference is null");
                return;
            }

            Vector3 offset = player.position;

            totalOffset += offset;
            shiftCount++;

            ShiftAllObjects(offset);

            player.position = Vector3.zero;

            if (showDebugInfo)
            {
                Debug.Log($"Origin Shift #{shiftCount}: Offset = {offset}, Total World Position = {totalOffset}");
            }
        }

        private void ShiftAllObjects(Vector3 offset)
        {
            GameObject[] allObjects = FindObjectsOfType<GameObject>();

            foreach (GameObject obj in allObjects)
            {
                if (obj.transform == player || obj.transform.IsChildOf(player))
                    continue;

                if (obj.layer == LayerMask.NameToLayer("UI"))
                    continue;

                if (obj.transform.parent == null)
                {
                    obj.transform.position -= offset;
                }
            }

            if (shiftParticles)
            {
                ShiftParticleSystems(offset);
            }

            if (shiftRigidbodies)
            {
                ShiftRigidbodies(offset);
            }

            if (shiftTrailRenderers)
            {
                ShiftTrailRenderers(offset);
            }
        }

        private void ShiftParticleSystems(Vector3 offset)
        {
            ParticleSystem[] particleSystems = FindObjectsOfType<ParticleSystem>();

            foreach (ParticleSystem ps in particleSystems)
            {
                if (ps.main.simulationSpace != ParticleSystemSimulationSpace.World)
                    continue;

                if (ps.transform.IsChildOf(player))
                    continue;

                ParticleSystem.Particle[] particles = new ParticleSystem.Particle[ps.main.maxParticles];
                int particleCount = ps.GetParticles(particles);

                for (int i = 0; i < particleCount; i++)
                {
                    particles[i].position -= offset;
                }

                ps.SetParticles(particles, particleCount);
            }
        }

        private void ShiftRigidbodies(Vector3 offset)
        {
            Rigidbody[] rigidbodies = FindObjectsOfType<Rigidbody>();

            foreach (Rigidbody rb in rigidbodies)
            {
                if (rb.transform == player || rb.transform.IsChildOf(player))
                    continue;
            }
        }

        private void ShiftTrailRenderers(Vector3 offset)
        {
            TrailRenderer[] trailRenderers = FindObjectsOfType<TrailRenderer>();

            foreach (TrailRenderer trail in trailRenderers)
            {
                if (trail.transform.IsChildOf(player))
                    continue;

                trail.Clear();
            }
        }

        public Vector3 GetPlayerWorldPosition()
        {
            if (player == null)
                return totalOffset;

            return totalOffset + player.position;
        }

        public Vector3 UnityToWorldPosition(Vector3 unityPos)
        {
            return totalOffset + unityPos;
        }

        public Vector3 WorldToUnityPosition(Vector3 worldPos)
        {
            return worldPos - totalOffset;
        }

        public Vector3 GetTotalOffset()
        {
            return totalOffset;
        }

        public void ResetOrigin()
        {
            totalOffset = Vector3.zero;
            shiftCount = 0;
            Debug.Log("Floating Origin Reset");
        }

        [ContextMenu("Force Origin Shift")]
        public void ForceOriginShift()
        {
            PerformOriginShift();
        }

        private void OnGUI()
        {
            if (!showDebugInfo)
                return;

            GUILayout.BeginArea(new Rect(10, 10, 400, 200));
            GUILayout.Label($"<b>Floating Origin Debug</b>", new GUIStyle(GUI.skin.label) { fontSize = 16, richText = true });
            GUILayout.Label($"Shift Count: {shiftCount}");
            GUILayout.Label($"Player Unity Pos: {(player != null ? player.position.ToString("F2") : "null")}");
            GUILayout.Label($"Player World Pos: {GetPlayerWorldPosition().ToString("F2")}");
            GUILayout.Label($"Total Offset: {totalOffset.ToString("F2")}");
            GUILayout.Label($"Distance from Origin: {(player != null ? player.position.magnitude.ToString("F2") : "N/A")}");
            GUILayout.Label($"Threshold: {threshold}");
            GUILayout.EndArea();
        }
    }
}
