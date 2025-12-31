using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Events;

namespace Streets.House
{
    /// <summary>
    /// An object that can be pushed/pulled to reveal hidden items or paths.
    /// Great for moving furniture, crates, or bookcases.
    /// </summary>
    public class Moveable : Interactable
    {
        [Header("Movement Settings")]
        [SerializeField] private Vector3 moveDirection = Vector3.forward;
        [SerializeField] private float moveDistance = 1f;
        [SerializeField] private float moveDuration = 0.5f;
        [SerializeField] private bool useLocalDirection = true;

        [Header("Movement Options")]
        [SerializeField] private bool canMoveBack = false;
        [SerializeField] private bool snapToGrid = false;
        [SerializeField] private float gridSize = 1f;

        [Header("Reveal on Move")]
        [Tooltip("Objects to reveal (enable) when moved")]
        [SerializeField] private GameObject[] revealObjects;
        [Tooltip("Objects to hide (disable) when moved")]
        [SerializeField] private GameObject[] hideObjects;

        [Header("Movement Audio")]
        [SerializeField] private AudioClip moveSound;
        [SerializeField] private AudioClip moveBackSound;
        [SerializeField] private AudioClip impactSound;

        [Header("Events")]
        [SerializeField] private UnityEvent OnMoveStarted;
        [SerializeField] private UnityEvent OnMoveComplete;
        [SerializeField] private UnityEvent OnMoveBackComplete;

        // State
        private bool hasMoved = false;
        private bool isMoving = false;
        private Vector3 originalPosition;
        private Vector3 movedPosition;

        // Events
        public event Action OnMoved;
        public event Action OnMovedBack;

        // Properties
        public bool HasMoved => hasMoved;
        public bool IsMoving => isMoving;

        protected override void Start()
        {
            base.Start();

            originalPosition = transform.position;

            // Calculate moved position
            Vector3 direction = useLocalDirection ? transform.TransformDirection(moveDirection.normalized) : moveDirection.normalized;
            movedPosition = originalPosition + direction * moveDistance;

            if (snapToGrid)
            {
                movedPosition = SnapToGrid(movedPosition);
            }

            UpdatePromptText();

            // Initially hide reveal objects
            foreach (var obj in revealObjects)
            {
                if (obj != null) obj.SetActive(false);
            }
        }

        protected override void OnInteract()
        {
            if (!hasMoved)
            {
                Move();
            }
            else if (canMoveBack)
            {
                MoveBack();
            }
        }

        protected override bool CanInteract()
        {
            if (isMoving) return false;
            if (hasMoved && !canMoveBack) return false;

            return base.CanInteract();
        }

        private void UpdatePromptText()
        {
            if (!hasMoved)
            {
                promptText = "Press E to push";
            }
            else if (canMoveBack)
            {
                promptText = "Press E to push back";
            }
            else
            {
                promptText = "";
            }
        }

        #region Movement

        public void Move()
        {
            if (hasMoved || isMoving) return;

            StartCoroutine(AnimateMove(movedPosition, true));
            PlaySound(moveSound);
            OnMoveStarted?.Invoke();
        }

        public void MoveBack()
        {
            if (!hasMoved || isMoving || !canMoveBack) return;

            StartCoroutine(AnimateMove(originalPosition, false));
            PlaySound(moveBackSound ?? moveSound);
        }

        private IEnumerator AnimateMove(Vector3 targetPosition, bool movingForward)
        {
            isMoving = true;

            Vector3 startPosition = transform.position;
            float elapsed = 0f;

            while (elapsed < moveDuration)
            {
                elapsed += Time.deltaTime;
                float t = elapsed / moveDuration;

                // Ease in-out for heavy furniture feel
                t = t < 0.5f ? 2 * t * t : 1 - Mathf.Pow(-2 * t + 2, 2) / 2;

                transform.position = Vector3.Lerp(startPosition, targetPosition, t);
                yield return null;
            }

            transform.position = targetPosition;
            isMoving = false;

            // Play impact sound
            if (impactSound != null)
            {
                PlaySound(impactSound);
            }

            if (movingForward)
            {
                hasMoved = true;
                RevealHiddenObjects();
                OnMoveComplete?.Invoke();
                OnMoved?.Invoke();
            }
            else
            {
                hasMoved = false;
                HideRevealedObjects();
                OnMoveBackComplete?.Invoke();
                OnMovedBack?.Invoke();
            }

            UpdatePromptText();
        }

        private void RevealHiddenObjects()
        {
            foreach (var obj in revealObjects)
            {
                if (obj != null)
                {
                    obj.SetActive(true);
                    Debug.Log($"[Moveable] Revealed: {obj.name}");
                }
            }

            foreach (var obj in hideObjects)
            {
                if (obj != null) obj.SetActive(false);
            }
        }

        private void HideRevealedObjects()
        {
            foreach (var obj in revealObjects)
            {
                if (obj != null) obj.SetActive(false);
            }

            foreach (var obj in hideObjects)
            {
                if (obj != null) obj.SetActive(true);
            }
        }

        #endregion

        private Vector3 SnapToGrid(Vector3 position)
        {
            return new Vector3(
                Mathf.Round(position.x / gridSize) * gridSize,
                position.y,
                Mathf.Round(position.z / gridSize) * gridSize
            );
        }

        /// <summary>
        /// Force set position (for save/load)
        /// </summary>
        public void SetMoved(bool moved)
        {
            hasMoved = moved;
            transform.position = moved ? movedPosition : originalPosition;

            if (moved)
            {
                foreach (var obj in revealObjects)
                {
                    if (obj != null) obj.SetActive(true);
                }
            }

            UpdatePromptText();
        }

        /// <summary>
        /// Reset to original position
        /// </summary>
        public override void ResetInteraction()
        {
            base.ResetInteraction();
            hasMoved = false;
            transform.position = originalPosition;

            foreach (var obj in revealObjects)
            {
                if (obj != null) obj.SetActive(false);
            }

            UpdatePromptText();
        }

        protected override void OnDrawGizmosSelected()
        {
            base.OnDrawGizmosSelected();

            // Draw movement path
            Vector3 startPos = Application.isPlaying ? originalPosition : transform.position;
            Vector3 direction = useLocalDirection ? transform.TransformDirection(moveDirection.normalized) : moveDirection.normalized;
            Vector3 endPos = startPos + direction * moveDistance;

            Gizmos.color = Color.cyan;
            Gizmos.DrawLine(startPos, endPos);
            Gizmos.DrawWireCube(endPos, Vector3.one * 0.3f);

            // Draw arrow
            Vector3 arrowLeft = Quaternion.Euler(0, 150, 0) * direction * 0.3f;
            Vector3 arrowRight = Quaternion.Euler(0, -150, 0) * direction * 0.3f;
            Gizmos.DrawLine(endPos, endPos + arrowLeft);
            Gizmos.DrawLine(endPos, endPos + arrowRight);
        }
    }
}
