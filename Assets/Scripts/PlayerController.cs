using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    [Header("Horizontal Movement Settings")]
    [SerializeField] float walkSpeed = 1.0f;
    [Header("Vertical Movement Settings")]
    [SerializeField] float jumpForce = 45.0f;
    private int jumpBufferCounter = 0;
    [SerializeField] private int jumpBufferFrames;
    // Coyote time allows the user a small margin of error when 
    // jumping off of a platform
    private float coyoteTimeCounter = 0;
    [SerializeField] private float coyoteTime;
    private int airJumpCounter = 0;
    [SerializeField] private int maxAirJumps; 
    private float xAxis = 0f;
    private Rigidbody2D rb;
    private Animator anim;
    private PlayerStateList pState;
    [Header("Ground Check Settings")]
    [SerializeField] private Transform groundCheckPoint;
    [SerializeField] private float groundCheckY = 0.2f;
    [SerializeField] private float groundCheckX = 0.5f;
    [SerializeField] private LayerMask whatIsGround;

    // Singleton Pattern created here
    public static PlayerController Instance;

    void Awake(){
        if (Instance != null && Instance != this){
            Destroy(gameObject);
        }
        else{
            Instance = this;
        }
        
    }

    // Start is called before the first frame update
    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        anim = GetComponent<Animator>();
        pState = GetComponent<PlayerStateList>();
    }

    // Update is called once per frame
    void Update()
    {
        GetInputs();
        UpdateJumpVariables();
        Flip();    // Original Video has Flip() call after Jump(). I prefer to change the player's direction before he starts moving. 
        Move();
        Jump();

    }

    void GetInputs(){
        xAxis = Input.GetAxisRaw("Horizontal");
    }

    // Flip the player's direction when the user changes direction
    void Flip(){
        if (xAxis < 0){
            transform.localScale = new Vector2(-1, transform.localScale.y);
        }
        else if (xAxis > 0){
            transform.localScale = new Vector2(1, transform.localScale.y);            
        }
    }

    private void Move(){
        rb.velocity = new Vector2(walkSpeed * xAxis, rb.velocity.y);
        anim.SetBool("Walking", rb.velocity.x != 0 && Grounded());
    }

    public bool Grounded(){
        if (Physics2D.Raycast(groundCheckPoint.position, Vector2.down, groundCheckY, whatIsGround) || Physics2D.Raycast(groundCheckPoint.position + new Vector3(groundCheckX, 0, 0), Vector2.down, groundCheckY, whatIsGround)
            || Physics2D.Raycast(groundCheckPoint.position - new Vector3(groundCheckX, 0, 0), Vector2.down, groundCheckY, whatIsGround)){
            return true;
        }
        return false;
    }

    public void Jump(){
        // Allow the player to stop the jump by releasing the jump button (spacebar)
        if (Input.GetButtonUp("Jump") && (rb.velocity.y > 0)){
            rb.velocity = new Vector2(rb.velocity.x, 0);
            pState.jumping = false;
        }
        if (!pState.jumping){

            // Making sure the player is able to jump when the jump button (spacebar) is pressed
            if (jumpBufferCounter > 0 && coyoteTimeCounter > 0){
                rb.velocity = new Vector2(rb.velocity.x, jumpForce);   
                pState.jumping = true;
            }
            else if (!Grounded() && airJumpCounter < maxAirJumps && Input.GetButtonDown("Jump")){
                pState.jumping = true;
                airJumpCounter++;
                rb.velocity = new Vector2(rb.velocity.x, jumpForce);   
            }
        }
        anim.SetBool("Jumping", !Grounded());
    }

    void UpdateJumpVariables(){
        if (Grounded()){
            pState.jumping = false;
            coyoteTimeCounter = coyoteTime;
            airJumpCounter = 0;
        }
        else{
            coyoteTimeCounter -= Time.deltaTime;
        }

        if (Input.GetButtonDown("Jump")){
            jumpBufferCounter = jumpBufferFrames;
        }
        else{
            jumpBufferCounter--;
        }
    }
}
