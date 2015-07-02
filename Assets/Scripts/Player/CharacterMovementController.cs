using UnityEngine;
using System.Collections;
/*
[RequireComponent(typeof(Rigidbody))]
[RequireComponent(typeof(CapsuleCollider))]  */

//[RequireComponent(typeof(SuperCharacterController))]
public class CharacterMovementController : MonoBehaviour
{
    public float jumpHeight = 10.0f;
    public float gravityFactor = 10.0f;
    public float groundFriction = 8.0f;
    public float airStrafeFactor = 0.5f;
 
    public float maxVelocityAir = 2.0f;
    public float airAccelerate = 100.0f;
    public float groundAccelerate = 50.0f;
    public float maxVelocityGround = 4.0f;  


    public float speed = 0.0f;

    public Transform cameraView;
    public float playerViewOffset = 0.6f;


    private Vector3 previousVelocity = Vector3.zero;
    private Vector3 finalVelocity = Vector3.zero;
    private CharacterController charController;
 
    private bool isGrounded = false;
    private bool isWantingToJump = false;

    private float rotX = 0.0f;
    private float rotY = 0.0f;

    private Rigidbody rigidBody;

    void Start()
    {       
        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Locked;

        cameraView.position = new Vector3(this.transform.position.x, this.transform.position.y + playerViewOffset, this.transform.position.z);
        
    }


    void Awake()
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        charController = GetComponent<CharacterController>();
    }


    void Update()
    {

        /* Camera rotation stuff, mouse controls this shit */
        rotX -= Input.GetAxis("Mouse Y");
        rotY += Input.GetAxis("Mouse X");



        this.transform.rotation = Quaternion.Euler(0, rotY, 0);
        cameraView.rotation = Quaternion.Euler(rotX, rotY, 0);
        cameraView.position = new Vector3(this.transform.position.x, this.transform.position. y + playerViewOffset, this.transform.position.z);

        
        //TODO:fix diagonal stuff
        Vector3 targetInputVelocity = new Vector3(Input.GetAxis("Horizontal"), 0, Input.GetAxis("Vertical"));    
        targetInputVelocity = transform.TransformDirection(targetInputVelocity);

        isGrounded = charController.isGrounded;      

        QueueJump();
        if (isGrounded)
        {
            finalVelocity = MoveGround(targetInputVelocity, previousVelocity);
        }
        else
        {
           finalVelocity =  MoveAir(targetInputVelocity, previousVelocity);
        }

        Debug.DrawLine(transform.position, transform.position +  finalVelocity, Color.red);
        ApplyVelocity(finalVelocity);
        previousVelocity = finalVelocity;

        speed = finalVelocity.magnitude;
    

    
    }


    private Vector3 Accelerate(Vector3 accelerationDir, Vector3 playerVelocity, float accelerate, float maxVelocity)
    {
        float projectedVel = Vector3.Dot(playerVelocity, accelerationDir) * airStrafeFactor ; // Vector projection of Current velocity onto accelDir.
        float accelerationVel = accelerate * Time.fixedDeltaTime; // Accelerated velocity in direction of movement

        // If necessary, truncate the accelerated velocity so the vector projection does not exceed maxVelocity
        if (projectedVel + accelerationVel > maxVelocity)
            accelerationVel = maxVelocity - projectedVel;

        return playerVelocity + accelerationDir * accelerationVel;
    }  


    private Vector3 MoveGround(Vector3 accelDir, Vector3 playerVelocity)
    {
        // Apply Friction
        float speed = playerVelocity.magnitude;
        if (speed != 0) // To avoid divide by zero errors
        {
            float drop = speed * groundFriction * Time.fixedDeltaTime;
            playerVelocity *= Mathf.Max(speed - drop, 0) / speed; // Scale the velocity based on friction.
        }

        if (isWantingToJump)
        {
            playerVelocity.y = CalculateJumpHeight();            
            isWantingToJump= false;           
        }
                
        return Accelerate(accelDir, playerVelocity, groundAccelerate, maxVelocityGround);
    }


    private Vector3 MoveAir(Vector3 accelDir, Vector3 playerVelocity)
    {
        
        playerVelocity = ApplyGravity(playerVelocity);   
        return Accelerate(accelDir, playerVelocity, airAccelerate, maxVelocityAir);
        
    }


    private Vector3 ApplyGravity(Vector3 playerVelocity)
    {        
        Vector3 newVelocity = playerVelocity;
        newVelocity.y -= gravityFactor * Time.fixedDeltaTime;
        return newVelocity;
    }

    private float CalculateJumpHeight()
    {
        return Mathf.Sqrt(2 * jumpHeight * gravityFactor);
    }

    private void QueueJump()
    {
        if (Input.GetKeyDown(KeyCode.Space) && !isWantingToJump )
            isWantingToJump = true;        
        if (Input.GetKeyUp(KeyCode.Space))
            isWantingToJump = false;
    }


    private void ApplyVelocity(Vector3 velocity)
    {       
        charController.Move(velocity * Time.fixedDeltaTime);
    }
    
}