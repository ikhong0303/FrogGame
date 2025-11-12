using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class BeeController : MonoBehaviour
{
    public float moveSpeed = 6f;
    private Rigidbody2D rb;
    private Vector2 input;
    private BeeHealth beeHealth;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        rb.gravityScale = 0f;
        rb.constraints = RigidbodyConstraints2D.FreezeRotation;

        beeHealth = GetComponent<BeeHealth>();
    }

    private void Update()
    {
        if (!GameFlow.I || !GameFlow.I.IsRunning || GameFlow.I.IsGameOver)
        {
            input = Vector2.zero;
            return;
        }

        float x = Input.GetAxisRaw("Horizontal");
        float y = Input.GetAxisRaw("Vertical");
        input = new Vector2(x, y).normalized;
    }

    private void FixedUpdate()
    {
        rb.linearVelocity = input * moveSpeed;
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!GameFlow.I || GameFlow.I.IsGameOver) return;

        // 혀(EdgeCollider2D, isTrigger=On)에 닿으면 데미지
        if (other.CompareTag("Tongue"))
        {
            if (beeHealth != null)
            {
                // 무적 상태가 아니면 데미지 받음
                if (!beeHealth.IsInvincible)
                {
                    beeHealth.TakeDamage(1);
                }
            }
            else
            {
                // BeeHealth가 없다면 기존 방식대로 즉시 게임오버
                GameFlow.I.GameOver();
            }
        }
    }
}