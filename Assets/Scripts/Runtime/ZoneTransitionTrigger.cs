using Tidepool.Domain;
using UnityEngine;
using UnityEngine.Events;

namespace Tidepool.Runtime
{
    [RequireComponent(typeof(Collider2D))]
    public class ZoneTransitionTrigger : MonoBehaviour
    {
        [SerializeField] private ZoneId destinationZone = ZoneId.KelpCurtain;
        [SerializeField] private Transform playerRoot;
        [SerializeField] private PlayerGridMover playerMover;
        [SerializeField] private Grid grid;
        [SerializeField] private Transform destinationSpawn;
        [SerializeField] private UnityEvent enteredZone;

        private bool transitionInProgress;

        public ZoneId DestinationZone => destinationZone;

        public void EnterZone()
        {
            ApplyTransition(playerRoot);
        }

        private void Reset()
        {
            Collider2D trigger = GetComponent<Collider2D>();
            if (trigger != null)
            {
                trigger.isTrigger = true;
            }
        }

        private void OnValidate()
        {
            Collider2D trigger = GetComponent<Collider2D>();
            if (trigger != null)
            {
                trigger.isTrigger = true;
            }
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            if (other == null || !ShouldTransition(other.transform))
            {
                return;
            }

            ApplyTransition(other.transform);
        }

        private bool ShouldTransition(Transform candidate)
        {
            if (transitionInProgress || candidate == null)
            {
                return false;
            }

            if (playerRoot != null)
            {
                return candidate == playerRoot || candidate.IsChildOf(playerRoot);
            }

            PlayerGridMover candidateMover = candidate.GetComponentInParent<PlayerGridMover>();
            return playerMover == null ? candidateMover != null : candidateMover == playerMover;
        }

        private void ApplyTransition(Transform candidate)
        {
            Transform target = ResolveTarget(candidate);
            if (target == null)
            {
                return;
            }

            transitionInProgress = true;
            try
            {
                if (playerMover != null)
                {
                    playerMover.SetInputEnabled(false);
                }

                if (destinationSpawn != null)
                {
                    target.position = destinationSpawn.position;
                }

                SaveTransition(target);
                enteredZone?.Invoke();
            }
            finally
            {
                if (playerMover != null)
                {
                    playerMover.SetInputEnabled(true);
                }

                transitionInProgress = false;
            }
        }

        private Transform ResolveTarget(Transform candidate)
        {
            if (playerRoot != null)
            {
                return playerRoot;
            }

            return playerMover != null ? playerMover.transform : candidate;
        }

        private void SaveTransition(Transform target)
        {
            GameSaveService saveService = GameSaveService.Instance;
            if (saveService == null)
            {
                return;
            }

            saveService.SetCurrentZone(destinationZone);
            if (grid != null && target != null)
            {
                Vector3Int cell = grid.WorldToCell(target.position);
                saveService.SetPlayerTile(new Vector2Int(cell.x, cell.y));
            }
        }
    }
}
