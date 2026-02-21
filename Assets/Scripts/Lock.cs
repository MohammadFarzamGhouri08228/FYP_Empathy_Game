using UnityEngine;
using System.Collections;

public enum LockRetractDirection
{
    Up,
    Down,
    Left,
    Right
}

public enum LockMode
{
    TiledSprite,
    ScaleTransform
}

public class Lock : MonoBehaviour
{
    [Header("Retract Settings")]
    [SerializeField] private LockMode mode = LockMode.TiledSprite;
    [SerializeField] private LockRetractDirection retractDirection = LockRetractDirection.Up;

    [Tooltip("How fast the lock retracts (world-units per second).")]
    [SerializeField] private float retractSpeed = 4f;

    [Header("Interaction")]
    [SerializeField] private KeyCode interactKey = KeyCode.E;

    [Tooltip("How close the player must be to interact.")]
    [SerializeField] private float interactRange = 2.5f;

    [Tooltip("Optional child object (text / sprite) shown when the player can interact.")]
    [SerializeField] private GameObject interactPrompt;

    [Header("Collider (TiledSprite mode only)")]
    [SerializeField] private bool autoSizeCollider = true;

    private SpriteRenderer sr;
    private BoxCollider2D col;
    private float currentSize;
    private Vector3 anchorWorld;
    private Vector3 initialScale;
    private Vector3 initialPosition;
    private bool isRetracting = false;
    private bool isAxisX;
    private Transform playerTransform;

    public LockRetractDirection RetractDir => retractDirection;

    void Awake()
    {
        sr = GetComponent<SpriteRenderer>();
        col = GetComponent<BoxCollider2D>();

        if (mode == LockMode.TiledSprite)
        {
            if (sr == null)
                Debug.LogError("Lock set to TiledSprite but no SpriteRenderer found!", this);
            if (col == null && autoSizeCollider)
                col = gameObject.AddComponent<BoxCollider2D>();
        }

        isAxisX = (retractDirection == LockRetractDirection.Left ||
                   retractDirection == LockRetractDirection.Right);

        initialScale = transform.localScale;
        initialPosition = transform.position;

        if (mode == LockMode.TiledSprite && sr != null)
            currentSize = isAxisX ? sr.size.x : sr.size.y;
        else
            currentSize = 1f;

        anchorWorld = CalculateAnchor();

        if (interactPrompt != null)
            interactPrompt.SetActive(false);
    }

    void Start()
    {
        GameObject player = GameObject.FindWithTag("Player");
        if (player != null)
            playerTransform = player.transform;
    }

    void Update()
    {
        if (isRetracting)
        {
            if (mode == LockMode.TiledSprite)
            {
                currentSize = Mathf.MoveTowards(currentSize, 0f, retractSpeed * Time.deltaTime);
                ApplySizeTiled(currentSize);
            }
            else
            {
                float scaleSpeed = retractSpeed * 0.5f;
                currentSize = Mathf.MoveTowards(currentSize, 0f, scaleSpeed * Time.deltaTime);
                ApplySizeScaled(currentSize);
            }

            if (currentSize <= 0.001f)
                Destroy(gameObject);

            return;
        }

        if (playerTransform == null) return;

        float dist = Vector2.Distance(anchorWorld, playerTransform.position);
        bool inRange = dist <= interactRange;

        if (interactPrompt != null)
            interactPrompt.SetActive(inRange && Key.hasKey);

        if (inRange && Key.hasKey && Input.GetKeyDown(interactKey))
            UnlockAll();
    }

    private Vector3 CalculateAnchor()
    {
        Vector3 pos = transform.position;
        Vector3 size = Vector3.one;

        if (mode == LockMode.TiledSprite && sr != null)
        {
            size = new Vector3(sr.size.x, sr.size.y, 0);
        }
        else
        {
            Renderer r = GetComponentInChildren<Renderer>();
            if (r != null) size = r.bounds.size;
        }

        return retractDirection switch
        {
            LockRetractDirection.Up    => pos + Vector3.up    * (size.y * 0.5f),
            LockRetractDirection.Down  => pos - Vector3.up    * (size.y * 0.5f),
            LockRetractDirection.Left  => pos - Vector3.right * (size.x * 0.5f),
            LockRetractDirection.Right => pos + Vector3.right * (size.x * 0.5f),
            _ => pos
        };
    }

    private void ApplySizeTiled(float size)
    {
        float safeSize = Mathf.Max(size, 0.01f);
        Vector2 spriteSize = sr.size;

        if (isAxisX)
        {
            spriteSize.x = safeSize;
            sr.size = spriteSize;

            Vector3 pos = anchorWorld;
            if (retractDirection == LockRetractDirection.Left)
                pos += Vector3.right * (size * 0.5f);
            else
                pos -= Vector3.right * (size * 0.5f);

            pos.y = transform.position.y;
            pos.z = transform.position.z;
            transform.position = pos;
        }
        else
        {
            spriteSize.y = safeSize;
            sr.size = spriteSize;

            Vector3 pos = anchorWorld;
            if (retractDirection == LockRetractDirection.Up)
                pos -= Vector3.up * (size * 0.5f);
            else
                pos += Vector3.up * (size * 0.5f);

            pos.x = transform.position.x;
            pos.z = transform.position.z;
            transform.position = pos;
        }

        if (col != null && autoSizeCollider)
        {
            col.size = spriteSize;
            col.offset = Vector2.zero;
        }
    }

    private void ApplySizeScaled(float scalePercent)
    {
        Vector3 newScale = initialScale;
        if (isAxisX) newScale.x *= scalePercent;
        else         newScale.y *= scalePercent;

        transform.localScale = newScale;

        Renderer r = GetComponentInChildren<Renderer>();
        if (r == null) return;

        Vector3 currentWorldSize = r.bounds.size;
        Vector3 pos = anchorWorld;

        if (retractDirection == LockRetractDirection.Up)
            pos -= Vector3.up * (currentWorldSize.y * 0.5f);
        else if (retractDirection == LockRetractDirection.Down)
            pos += Vector3.up * (currentWorldSize.y * 0.5f);
        else if (retractDirection == LockRetractDirection.Left)
            pos += Vector3.right * (currentWorldSize.x * 0.5f);
        else
            pos -= Vector3.right * (currentWorldSize.x * 0.5f);

        if (isAxisX) pos.y = transform.position.y;
        else         pos.x = transform.position.x;
        pos.z = transform.position.z;

        transform.position = pos;
    }

    public void StartRetracting()
    {
        isRetracting = true;
        if (interactPrompt != null)
            interactPrompt.SetActive(false);
    }

    public static void UnlockAll()
    {
        Key.hasKey = false;

        if (Key.keyIndicatorInstance != null)
        {
            Destroy(Key.keyIndicatorInstance);
            Key.keyIndicatorInstance = null;
        }

        Debug.Log("All locks unlocking (sequenced: Up -> Left -> Down -> Right)");

        Lock[] allLocks = FindObjectsByType<Lock>(FindObjectsSortMode.None);
        foreach (Lock lk in allLocks)
        {
            if (lk.interactPrompt != null)
                lk.interactPrompt.SetActive(false);
        }

        GameObject runner = new GameObject("_LockSequencer");
        LockSequencer seq = runner.AddComponent<LockSequencer>();
        seq.Run(allLocks);
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = new Color(1f, 0.9f, 0f, 0.35f);
        Vector3 gizmoCenter = Application.isPlaying ? anchorWorld : transform.position;
        Gizmos.DrawWireSphere(gizmoCenter, interactRange);

        Gizmos.color = Color.red;
        Vector3 dir = retractDirection switch
        {
            LockRetractDirection.Up    => Vector3.up,
            LockRetractDirection.Down  => Vector3.down,
            LockRetractDirection.Left  => Vector3.left,
            LockRetractDirection.Right => Vector3.right,
            _ => Vector3.up
        };
        Gizmos.DrawRay(transform.position, dir * 1.5f);
    }
}

public class LockSequencer : MonoBehaviour
{
    private static readonly LockRetractDirection[] order =
    {
        LockRetractDirection.Up,
        LockRetractDirection.Left,
        LockRetractDirection.Down,
        LockRetractDirection.Right
    };

    public void Run(Lock[] locks)
    {
        StartCoroutine(UnlockSequence(locks));
    }

    private IEnumerator UnlockSequence(Lock[] locks)
    {
        foreach (LockRetractDirection dir in order)
        {
            bool anyInGroup = false;
            foreach (Lock lk in locks)
            {
                if (lk != null && lk.RetractDir == dir)
                {
                    lk.StartRetracting();
                    anyInGroup = true;
                }
            }

            if (!anyInGroup) continue;

            bool groupDone = false;
            while (!groupDone)
            {
                groupDone = true;
                foreach (Lock lk in locks)
                {
                    if (lk != null && lk.RetractDir == dir)
                    {
                        groupDone = false;
                        break;
                    }
                }
                yield return null;
            }
        }

        Destroy(gameObject);
    }
}
