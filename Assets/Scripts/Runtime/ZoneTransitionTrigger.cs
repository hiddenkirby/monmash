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
        [SerializeField] private bool requireDestinationUnlocked;
        [SerializeField] private ZoneId requiredCaughtZone = ZoneId.SeagrassMeadow;
        [SerializeField, Min(0)] private int requiredCaughtSpeciesCount;
        [SerializeField] private GameObject lockedVisualRoot;
        [SerializeField] private GameObject unlockedVisualRoot;
        [SerializeField] private UnityEvent gateLocked = new UnityEvent();
        [SerializeField] private UnityEvent enteredZone = new UnityEvent();

        private bool transitionInProgress;
        private GameSaveService subscribedSaveService;

        public ZoneId DestinationZone => destinationZone;
        public UnityEvent GateLocked => gateLocked;
        public UnityEvent EnteredZone => enteredZone;

        private void OnEnable()
        {
            SubscribeToSaveService();
            RefreshGateVisualState();
        }

        private void Start()
        {
            SubscribeToSaveService();
            RefreshGateVisualState();
        }

        private void OnDisable()
        {
            if (subscribedSaveService != null)
            {
                subscribedSaveService.ZoneUnlocked -= HandleZoneUnlocked;
                subscribedSaveService = null;
            }
        }

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
            SubscribeToSaveService();
            RefreshGateVisualState();

            Transform target = ResolveTarget(candidate);
            if (target == null)
            {
                return;
            }

            if (!CanEnterDestination())
            {
                RefreshGateVisualState();
                gateLocked?.Invoke();
                return;
            }

            RefreshGateVisualState();
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

        private void SubscribeToSaveService()
        {
            GameSaveService saveService = GameSaveService.Instance;
            if (saveService == null || subscribedSaveService == saveService)
            {
                return;
            }

            if (subscribedSaveService != null)
            {
                subscribedSaveService.ZoneUnlocked -= HandleZoneUnlocked;
            }

            subscribedSaveService = saveService;
            subscribedSaveService.ZoneUnlocked += HandleZoneUnlocked;
        }

        private void HandleZoneUnlocked(ZoneId zone)
        {
            if (zone == destinationZone)
            {
                RefreshGateVisualState();
            }
        }

        private void RefreshGateVisualState()
        {
            bool open = IsDestinationOpen();
            if (lockedVisualRoot != null)
            {
                lockedVisualRoot.SetActive(requireDestinationUnlocked && !open);
            }

            if (unlockedVisualRoot != null)
            {
                unlockedVisualRoot.SetActive(open);
            }
        }

        private bool IsDestinationOpen()
        {
            if (!requireDestinationUnlocked)
            {
                return true;
            }

            GameSaveService saveService = GameSaveService.Instance;
            return saveService == null || saveService.IsZoneUnlocked(destinationZone);
        }

        private bool CanEnterDestination()
        {
            if (!requireDestinationUnlocked)
            {
                return true;
            }

            GameSaveService saveService = GameSaveService.Instance;
            if (saveService == null)
            {
                return true;
            }

            if (saveService.IsZoneUnlocked(destinationZone))
            {
                return true;
            }

            if (requiredCaughtSpeciesCount > 0
                && saveService.CountCaughtSpeciesInZone(requiredCaughtZone) >= requiredCaughtSpeciesCount)
            {
                saveService.UnlockZone(destinationZone);
                return true;
            }

            return false;
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
