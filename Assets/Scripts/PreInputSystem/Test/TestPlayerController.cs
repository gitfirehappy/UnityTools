using UnityEngine;

public class TestPlayerController : MonoBehaviour
{
    [Header("Ground Check")]
    public LayerMask groundLayer;
    public Transform groundCheck;
    private Rigidbody2D rb;
    public float groundCheckRadius = 0.2f;
    public float jumpForce = 5f;
    public float dashForce = 4f;

    public bool IsGrounded => Physics2D.OverlapCircle(groundCheck.position, groundCheckRadius, groundLayer);

    private bool canDash = true;
    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    public void Jump()
    {
        Debug.Log(">>> Jump executed!");
        rb.velocity = new Vector2(rb.velocity.x, 0f);
        rb.AddForce(Vector2.up * jumpForce,ForceMode2D.Impulse);
        // 示例：可以添加 rigidbody 上跳跃力
    }

    public void Dash()
    {
        if (!canDash)
        {
            Debug.Log("Dash on cooldown");
            return;
        }

        Debug.Log(">>> Dash executed!");
        rb.AddForce(Vector2.right * dashForce, ForceMode2D.Impulse);
        canDash = false;
        Invoke(nameof(ResetDash), 2f); // 模拟冷却
    }

    void ResetDash() => canDash = true;
    public bool CanDash() => canDash;

    private void OnDrawGizmosSelected()
    {
        if (groundCheck == null) return;
        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(groundCheck.position, groundCheckRadius);
    }
}
