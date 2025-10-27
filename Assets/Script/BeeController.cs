using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class BeeController : MonoBehaviour
{
    public float moveSpeed = 6f;
    private Rigidbody2D rb;
    private Vector2 input;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        rb.gravityScale = 0f;
        rb.constraints = RigidbodyConstraints2D.FreezeRotation;
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
        // ��(EdgeCollider2D, isTrigger=On)�� ������ �ٷ� GameOver
        if (!GameFlow.I || GameFlow.I.IsGameOver) return;
        if (other.CompareTag("Tongue"))
        {
            GameFlow.I.GameOver();
        }
    }
}
