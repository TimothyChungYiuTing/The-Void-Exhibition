using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FirstPerson : MonoBehaviour
{
    public Camera cam;

    public float sensX;
    public float sensY;

    float xRotation;
    float yRotation;

    public enum MovementState
    {
        walking,
        crouching,
        air
    }
    
    [Header("Movement")]
    private float moveSpeed = 7f;
    public float walkSpeed = 7f;
    public float groundDrag;
    public MovementState state;

    [Header("Jumping")]

    public float jumpForce;
    public float jumpCooldown;
    public float airMultiplier;
    bool readyToJump;

    [Header("Crouching")]
    public float crouchSpeed = 3f;
    public float crouchYScale;
    private float startYScale;

    [Header("Keybinds")]
    public KeyCode jumpKey = KeyCode.Space;
    public KeyCode crouchKey = KeyCode.LeftControl;
    public KeyCode grabKey = KeyCode.E;

    [Header("Ground Check")]
    public float playerHeight;
    public LayerMask groundLayer;
    bool grounded;

    public Transform orientation;

    private Vector3 forward;
    private Vector3 right;

    float horizontalInput;
    float verticalInput;

    Vector3 moveDirection;

    Rigidbody rb;


    [Header("Gallery")]
    private GameObject collidedRedFrame = null;
    private int heldItemID = -1;
    public GameObject HeldRedFrame;
    public GameObject RedFramePrefab;
    private GameObject InstantiatedRedFrame = null;
    public GameObject PerfectRedFrame;
    public GameObject PlayerSnap0;
    public bool redFrameSnapped = false;
    public GameObject wall0;
    public GameObject newWall0;

    // Start is called before the first frame update
    void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
        
        rb = GetComponent<Rigidbody>();
        rb.freezeRotation = true;

        readyToJump = true;

        startYScale = transform.localScale.y;
    }

    // Update is called once per frame
    void Update()
    {
        //Mouse input
        float mouseX = Input.GetAxisRaw("Mouse X") * Time.deltaTime * sensX;
        float mouseY = Input.GetAxisRaw("Mouse Y") * Time.deltaTime * sensY;

        yRotation += mouseX;
        xRotation -= mouseY;
        xRotation = Mathf.Clamp(xRotation, -90f, 90f);
        
        transform.rotation = Quaternion.Euler(0f, yRotation, 0f);
        cam.transform.rotation = Quaternion.Euler(xRotation, yRotation, 0f);
        
        forward = orientation.forward;
        right = orientation.right;

        //Ground check
        grounded = Physics.Raycast(transform.position, Vector3.down, playerHeight * 0.5f + 0.05f, groundLayer);

        CharacterInputs();
        SpeedControl();
        StateHandler();

        //Drag
        if (grounded)
            rb.drag = groundDrag;
        else
            rb.drag = 0;
        
        //Snaps
        ObjectSnaps();
    }

    private void FixedUpdate()
    {
        Movement();
    }

    private void CharacterInputs()
    {
        horizontalInput = Input.GetAxisRaw("Horizontal");
        verticalInput = Input.GetAxisRaw("Vertical");

        //Jump
        if(Input.GetKey(jumpKey) && readyToJump && grounded)
        {
            readyToJump = false;

            Jump();

            Invoke(nameof(ResetJump), jumpCooldown);
        }

        //Crouch
        if (Input.GetKeyDown(crouchKey)) {
            transform.localScale = new Vector3(transform.localScale.x, crouchYScale, transform.localScale.z);
            rb.AddForce(Vector3.down * 5f, ForceMode.Impulse);
            cam.GetComponent<CameraScript>().LerpHands(1);
        }
        if (Input.GetKeyUp(crouchKey)) {
            transform.localScale = new Vector3(transform.localScale.x, startYScale, transform.localScale.z);
            cam.GetComponent<CameraScript>().LerpHands(0);
        }
        if (Input.GetKeyDown(grabKey)) {
            cam.GetComponent<CameraScript>().HandsGrab();
            
            if (heldItemID == 0) {
                TryPutDownRedFrame();
            }
            
            if (heldItemID == -1) {
                if (collidedRedFrame != null) {
                    heldItemID = 0;
                    Destroy(collidedRedFrame);
                    collidedRedFrame = null;
                    InstantiatedRedFrame = null;
                    HeldRedFrame.SetActive(true);
                }
            }
        }
    }

    private void TryPutDownRedFrame()
    {
        if (HeldRedFrame.GetComponent<HeldRedFrame>().canPlace) {
            heldItemID = -1;
            InstantiatedRedFrame = Instantiate(RedFramePrefab, HeldRedFrame.transform.position, HeldRedFrame.transform.rotation);
            HeldRedFrame.SetActive(false);
        }
    }

    private void ObjectSnaps()
    {
        redFrameSnapped = PositionSnap(InstantiatedRedFrame, 0.5f, 4f, 0, PerfectRedFrame);
        if (redFrameSnapped) {
            //Player pos and camera angle check
            bool player0_Snapped = PositionSnap(gameObject, 0.8f, 7f, 2, PlayerSnap0);
            bool camera0_Snapped = PositionSnap(cam.gameObject, 0.5f, 7f, 7, PlayerSnap0, true);

            if (player0_Snapped && camera0_Snapped) {
                Destroy(InstantiatedRedFrame);
                PositionSnap(gameObject, 0.8f, 7f, 0, PlayerSnap0);
                Destroy(wall0);
                newWall0.SetActive(true);
            }
        }
    }

    private bool PositionSnap(GameObject originalGameObject, float distThres, float angleThres, int snapMode = 0, GameObject targetGameObject = null, bool ignorePos = false) //snapMode 0 replace, snapMode 1 reposition
    {
        if (InstantiatedRedFrame != null) {
            if (ignorePos || Vector3.Distance(originalGameObject.transform.position, targetGameObject.transform.position) < distThres) {
                if (Mathf.Abs(originalGameObject.transform.eulerAngles.x - targetGameObject.transform.eulerAngles.x) < angleThres) {
                    if (Mathf.Abs(originalGameObject.transform.eulerAngles.y - targetGameObject.transform.eulerAngles.y) < angleThres) {
                        if (Mathf.Abs(originalGameObject.transform.eulerAngles.z - targetGameObject.transform.eulerAngles.z) < angleThres) {
                                
                                if (snapMode == 0) {
                                    originalGameObject.transform.position = targetGameObject.transform.position;
                                    originalGameObject.transform.rotation = targetGameObject.transform.rotation;
                                }

                                if (snapMode == 1) {
                                    targetGameObject.SetActive(true);
                                    Destroy(originalGameObject);
                                }
                                return true;
                        }
                    }
                }
            }
        }
        return false;
    }

    private void Movement()
    {
        //Movement direction
        moveDirection = forward * verticalInput + right * horizontalInput;

        //On ground
        if(grounded) {
            Debug.Log("isGrounded");
            rb.AddForce(moveDirection.normalized * moveSpeed * 10f, ForceMode.Force);
        }

        //In air
        else if(!grounded) {
            Debug.Log("NOT Grounded");
            rb.AddForce(moveDirection.normalized * moveSpeed * 10f * airMultiplier, ForceMode.Force);
        }
    }

    private void SpeedControl()
    {
        Vector3 flatVel = new Vector3(rb.velocity.x, 0f, rb.velocity.z);

        //Limit velocity
        if(flatVel.magnitude > moveSpeed) {
            Vector3 limitedVel = flatVel.normalized * moveSpeed;
            rb.velocity = new Vector3(limitedVel.x, rb.velocity.y, limitedVel.z);
        }
    }

    private void Jump()
    {
        // reset y velocity
        rb.velocity = new Vector3(rb.velocity.x, 0f, rb.velocity.z);

        rb.AddForce(transform.up * jumpForce, ForceMode.Impulse);
    }
    private void ResetJump()
    {
        readyToJump = true;
    }

    private void StateHandler()
    {
        if (Input.GetKey(crouchKey)) {
            state = MovementState.crouching;
            moveSpeed = crouchSpeed;
        }
        else if (grounded) {
            state = MovementState.walking;
            moveSpeed = walkSpeed;
        }
        else {
            state = MovementState.air;
        }
    }

    private void OnTriggerEnter(Collider other) {
        if (other.gameObject.layer == LayerMask.NameToLayer("RedFrame")) {
            collidedRedFrame = other.gameObject;
        }
    }

    private void OnTriggerExit(Collider other) {
        if (other.gameObject.layer == LayerMask.NameToLayer("RedFrame")) {
            collidedRedFrame = null;
        }
    }
}
