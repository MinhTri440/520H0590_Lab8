using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    [SerializeField] private float moveSpeed = 1f;
    [SerializeField] private float runSpeedMultiplier = 2f; // Tốc độ di chuyển khi chạy
    [SerializeField] private float turnSpeed = 100f;
    [SerializeField] private Animator animator;
    [SerializeField] private string gameControllerTag = "GameController";

    private GameObject gameController;
    private bool isRunning = false; // Biến bool để xác định liệu phím Shift có được nhấn hay không

    private void Awake()
    {
        gameController = GameObject.FindGameObjectWithTag(gameControllerTag);
    }

    private void Update()
    {
        // Capture input
        float verticalInput = Input.GetAxis("Vertical");
        float horizontalInput = Input.GetAxis("Horizontal");
        isRunning = Input.GetKey(KeyCode.LeftShift); // Kiểm tra xem phím Shift có được nhấn hay không

        // Set speed variable
        float speed = Mathf.Abs(verticalInput) * moveSpeed;
        if (isRunning)
        {
            speed *= runSpeedMultiplier; // Tăng tốc độ di chuyển nếu đang chạy
        }
        animator.SetFloat(AnimatorIDs.Speed, speed);

        // Movement
        Vector3 movement = transform.forward * verticalInput * speed * Time.deltaTime;
        transform.position += movement;

        // Rotation
        if (verticalInput == 0)
        {
            if (horizontalInput < 0)
            {
                animator.Play(AnimatorIDs.TurnOnSpotLeftAState);
                transform.Rotate(Vector3.down * turnSpeed * Time.deltaTime);
            }
            else if (horizontalInput > 0)
            {
                animator.Play(AnimatorIDs.TurnOnSpotRightAState);
                transform.Rotate(Vector3.up * turnSpeed * Time.deltaTime);
            }
        }
        else
        {
            // Reset to idle state
            if (animator.GetCurrentAnimatorStateInfo(0).fullPathHash != AnimatorIDs.IdleState)
            {
                animator.Play("Walk");
            }
            transform.Rotate(Vector3.up * horizontalInput * turnSpeed * Time.deltaTime);
        }
    }
}
