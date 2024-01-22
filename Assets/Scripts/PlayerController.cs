using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    [Header("Horizontal Movement Settings")]
    [SerializeField] float walkSpeed = 1.0f;
    [Space(5)]

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
    [Space(5)]
    
    [Header("Ground Check Settings")]
    [SerializeField] private Transform groundCheckPoint;
    [SerializeField] private float groundCheckY = 0.2f;
    [SerializeField] private float groundCheckX = 0.5f;
    [SerializeField] private LayerMask whatIsGround;

    private bool canDash = true;
    private bool dashed = false;

    private bool attack = false;

    private float timeBetweenAttack = 2.0f;
    private float timeSinceAttack;
    private float gravity;
    [Space(5)]

    [Header("Dash Settings")]
    [SerializeField] private float dashSpeed;
    [SerializeField] private float dashTime;
    [SerializeField] private float dashCooldown;
    [SerializeField] private GameObject dashEffect;

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
        // keep normal gravity stored in variable
        gravity = rb.gravityScale;
    }

    // Update is called once per frame
    void Update()
    {
        GetInputs();
        UpdateJumpVariables();
        if (pState.dashing)
            return;
        Flip();     
        Move();
        Jump();
        StartDash();
        Attack();       // Dash was using the left mouse and left shift button - changed it to c only to match Jose.
    }

    void GetInputs(){
        xAxis = Input.GetAxisRaw("Horizontal"); //  moving character left and right
        attack = Input.GetMouseButtonDown(0);   // left mouse button calls Attack
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

    void StartDash(){
        if (Input.GetButtonDown("Dash") && canDash && !dashed){
            StartCoroutine(Dash());
            dashed = true;
        }

        if (Grounded()){
            dashed = false;
        }
    }

    IEnumerator Dash(){
        canDash = false;
        pState.dashing = true;
        anim.SetTrigger("Dashing");
        rb.gravityScale = 0;
        rb.velocity = new Vector2(transform.localScale.x * dashSpeed, 0);
        if (Grounded()){
           Instantiate(dashEffect, transform);  // play dash effect if player is on the ground
        }
        yield return new WaitForSeconds(dashTime);
        // reset to non-dashing
        rb.gravityScale = gravity;
        pState.dashing = false;
        yield return new WaitForSeconds(dashCooldown);
        canDash = true;
/*        if (Grounded()){
            dashed = false;
        }*/
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

    void Attack(){
        timeSinceAttack += Time.deltaTime;
        if (attack && timeSinceAttack > timeBetweenAttack){
            timeSinceAttack = 0;
            anim.SetTrigger("Attacking");
        }
    }
}
