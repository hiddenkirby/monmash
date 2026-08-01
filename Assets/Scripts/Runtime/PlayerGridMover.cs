using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

namespace Tidepool.Runtime
{
    public class PlayerGridMover : MonoBehaviour
    {
        [SerializeField] private Grid grid;
        [SerializeField] private Transform actor;
        [SerializeField] private Tilemap groundTilemap;
        [SerializeField] private Tilemap obstacleTilemap;
        [SerializeField] private float tilesPerSecond = 4f;

        private Coroutine walkRoutine;
        private Camera mainCamera;
        private bool inputEnabled = true;

        public event Action<Vector3Int> StepCompleted;

        public Vector3Int CurrentCell => grid.WorldToCell(actor.position);

        private void Awake()
        {
            mainCamera = Camera.main;
            if (actor == null)
            {
                actor = transform;
            }
        }

        private void Update()
        {
            if (!inputEnabled || !WasPrimaryTapPressed(out Vector3 screenPosition))
            {
                return;
            }

            Vector3 worldPosition = mainCamera.ScreenToWorldPoint(screenPosition);
            Vector3Int targetCell = grid.WorldToCell(worldPosition);
            MoveTo(targetCell);
        }

        public void SetInputEnabled(bool enabled)
        {
            inputEnabled = enabled;
        }

        public void MoveTo(Vector3Int targetCell)
        {
            Vector2Int start = ToVector2Int(CurrentCell);
            Vector2Int goal = ToVector2Int(targetCell);
            List<Vector2Int> path = GridPathfinder.FindPath(start, goal, IsWalkable);

            if (path.Count <= 1)
            {
                return;
            }

            if (walkRoutine != null)
            {
                StopCoroutine(walkRoutine);
            }

            walkRoutine = StartCoroutine(WalkPath(path));
        }

        private IEnumerator WalkPath(List<Vector2Int> path)
        {
            for (int i = 1; i < path.Count; i++)
            {
                Vector3 targetWorld = grid.GetCellCenterWorld(new Vector3Int(path[i].x, path[i].y, 0));
                Vector3 startWorld = actor.position;
                float distance = Vector3.Distance(startWorld, targetWorld);
                float duration = distance / tilesPerSecond;
                float elapsed = 0f;

                while (elapsed < duration)
                {
                    elapsed += Time.deltaTime;
                    float t = Mathf.Clamp01(elapsed / duration);
                    actor.position = Vector3.Lerp(startWorld, targetWorld, t);
                    yield return null;
                }

                actor.position = targetWorld;
                StepCompleted?.Invoke(new Vector3Int(path[i].x, path[i].y, 0));
            }

            walkRoutine = null;
        }

        private bool IsWalkable(Vector2Int cell)
        {
            Vector3Int tileCell = new Vector3Int(cell.x, cell.y, 0);
            return groundTilemap != null
                && groundTilemap.HasTile(tileCell)
                && (obstacleTilemap == null || !obstacleTilemap.HasTile(tileCell));
        }

        private static Vector2Int ToVector2Int(Vector3Int cell)
        {
            return new Vector2Int(cell.x, cell.y);
        }

        private static bool WasPrimaryTapPressed(out Vector3 screenPosition)
        {
            if (Input.touchCount > 0 && Input.GetTouch(0).phase == TouchPhase.Began)
            {
                screenPosition = Input.GetTouch(0).position;
                return true;
            }

            if (Input.GetMouseButtonDown(0))
            {
                screenPosition = Input.mousePosition;
                return true;
            }

            screenPosition = Vector3.zero;
            return false;
        }
    }
}

