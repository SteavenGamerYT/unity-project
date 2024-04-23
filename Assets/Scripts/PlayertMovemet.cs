using UnityEngine;

public class PlayertMovemet : MonoBehaviour {

    #region Variables
    public CharacterController controller;

    public float speed = 12f;
    public float jumpHeight = 3f;

    public Transform groundCheck;
    public float groundDistance = 0.4f;
    public LayerMask groundMask;
    
    public int maxHealth = 100;
    public int currentHealth;

    public HealthBar healthBar;

    Vector3 Velocity;
    bool isGrounded;
    #endregion

    #region Unity Methods
    void Start()
    {
        currentHealth = maxHealth;
        healthBar.SetMaxHealth(maxHealth);
    }

    void Update()
    {
        isGrounded = Physics.CheckSphere(groundCheck.position, groundDistance, groundMask);

        if(isGrounded && Velocity.y < 0)
		{
            Velocity.y = -2f;
		}
        float x = Input.GetAxis("Horizontal");
        float z = Input.GetAxis("Vertical");

        Vector3 move = transform.right * x + transform.forward * z;

        controller.Move(move * speed *Time.deltaTime);

        if(Input.GetButtonDown("Jump") && isGrounded)
		{
            Velocity.y = Mathf.Sqrt(jumpHeight * -2f * Physics.gravity.y);
		}

        Velocity.y += Physics.gravity.y * Time.deltaTime;

        controller.Move(Velocity * Time.deltaTime);
        if (Input.GetKeyDown(KeyCode.V))
        {
            TakeDamage(5);
        }
        if (Input.GetKeyDown(KeyCode.B))
        {
            TakeDamage(-5);
        }
        if (Input.GetKeyDown(KeyCode.H))
        {
            TakeDamage(-100);
        }
    }
    void TakeDamage(int damage)
    {
        currentHealth -= damage;
        currentHealth = Mathf.Clamp(currentHealth, 0, maxHealth);


        healthBar.SetHealth(currentHealth);
    }
    #endregion
}
