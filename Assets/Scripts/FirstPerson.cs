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

    private float horizontalInput;
    private float verticalInput;

    private Vector3 moveDirection;

    private Rigidbody rb;

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

    //Level 2
    public List<MeshRenderer> whiteToYellow;
    public Material yellowMat;
    public bool pictureMatched = false;
    public GameObject PlayerSnap1;
    public GameObject oldRoom;
    public GameObject newRoom;
    
    private GameObject collidedBlueFrame = null;
    public GameObject HeldBlueFrame;
    public GameObject BlueFramePrefab;
    private GameObject InstantiatedBlueFrame = null;

    //Level 2.5
    public bool loopExited = false;
    public bool inLoop = false;
    public bool outOfLoopTrigger = false;
    public GameObject OriginalRooms;
    public GameObject roomsOut;

    //Level 3
    public bool barsAligned = false;
    public GameObject PlayerSnap2;
    public GameObject startWall;
    public GameObject oldWall;
    public GameObject newWall;

    public GameObject endlessMirror;
    public GameObject BlackFrame;

    //Level 4
    private GameObject collidedBlackFrame = null;
    public GameObject HeldBlackFrame;
    public GameObject BlackFramePrefab;
    private GameObject InstantiatedBlackFrame = null;

    //Fall
    public GameObject BlackFloor;


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
        if (!loopExited && inLoop) {
            Loop();
        }

        //Debug from Level 4
        if (Input.GetKeyDown(KeyCode.Tab)) {
            //2.5 done
            roomsOut.SetActive(true);
            Destroy(OriginalRooms);

            //3 done
            Destroy(startWall);
            Destroy(oldWall);
            newWall.SetActive(true);
            BlackFrame.SetActive(true);

            barsAligned = true;
            loopExited = true;
            
            inLoop = false;
            
            Destroy(oldRoom);
            newRoom.SetActive(true);
            pictureMatched = true;
        }
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
            HeldRedFrame.transform.localScale = new Vector3(1f, 1f/crouchYScale, 1f);
            HeldBlueFrame.transform.localScale = new Vector3(1f, 1f/crouchYScale, 1f);
            rb.AddForce(Vector3.down * 5f, ForceMode.Impulse);
            cam.GetComponent<CameraScript>().LerpHands(1);
        }
        if (Input.GetKeyUp(crouchKey)) {
            transform.localScale = new Vector3(transform.localScale.x, startYScale, transform.localScale.z);
            HeldRedFrame.transform.localScale = Vector3.one;
            HeldBlueFrame.transform.localScale = Vector3.one;
            cam.GetComponent<CameraScript>().LerpHands(0);
        }
        if (Input.GetKeyDown(grabKey)) {
            //Press E
            cam.GetComponent<CameraScript>().HandsGrab();
            
            if (state != MovementState.crouching) {
                if (heldItemID == 0) {
                    TryPutDownRedFrame();
                }
                if (heldItemID == 1) {
                    TryPutDownBlueFrame();
                }
                if (heldItemID == 2) {
                    TryPutDownBlackFrame();
                }
            }
            
            if (heldItemID == -1) {
                if (collidedRedFrame != null) {
                    heldItemID = 0;
                    Destroy(collidedRedFrame);
                    collidedRedFrame = null;
                    InstantiatedRedFrame = null;
                    HeldRedFrame.SetActive(true);
                }
                else if (collidedBlueFrame != null) {
                    heldItemID = 1;
                    Destroy(collidedBlueFrame);
                    collidedBlueFrame = null;
                    InstantiatedBlueFrame = null;
                    HeldBlueFrame.SetActive(true);
                }
                else if (collidedBlackFrame != null) {
                    heldItemID = 2;
                    Destroy(collidedBlackFrame);
                    collidedBlackFrame = null;
                    InstantiatedBlackFrame = null;
                    HeldBlackFrame.SetActive(true);
                }
            }
        }
    }

    private void TryPutDownBlackFrame()
    {
        if (HeldBlackFrame.GetComponent<HeldBlackFrame>().canPlace) {
            heldItemID = -1;
            InstantiatedBlackFrame = Instantiate(BlackFramePrefab, HeldBlackFrame.transform.position, HeldBlackFrame.transform.rotation);
            HeldBlackFrame.SetActive(false);
        }
    }

    private void TryPutDownBlueFrame()
    {
        if (HeldBlueFrame.GetComponent<HeldBlueFrame>().canPlace) {
            heldItemID = -1;
            InstantiatedBlueFrame = Instantiate(BlueFramePrefab, HeldBlueFrame.transform.position, HeldBlueFrame.transform.rotation);
            HeldBlueFrame.SetActive(false);
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
            bool camera0_Snapped = PositionSnap(cam.gameObject, 0.8f, 7f, 2, PlayerSnap0, true);

            if (player0_Snapped && camera0_Snapped) {
                Destroy(InstantiatedRedFrame);
                PositionSnap(gameObject, 0.8f, 7f, 0, PlayerSnap0);
                Destroy(wall0);
                newWall0.SetActive(true);
            }
        }
        
        if (!pictureMatched) {
            bool player1_Snapped = PositionSnap(gameObject, 1.5f, 45f, 2, PlayerSnap1);
            bool camera1_Snapped = PositionSnap(cam.gameObject, 1.5f, 45f, 2, PlayerSnap1, true);
            
            if (player1_Snapped && camera1_Snapped) {
                PositionSnap(gameObject, 1.5f, 45f, 0, PlayerSnap1);
                foreach (MeshRenderer mr in whiteToYellow) {
                    mr.material = yellowMat;
                }
                Destroy(oldRoom);
                newRoom.SetActive(true);
                pictureMatched = true;
            }
        }
        
        if (loopExited && !barsAligned) {
            bool player2_Snapped = PositionSnap(gameObject, 0.6f, 25f, 2, PlayerSnap2);
            bool camera2_Snapped = PositionSnap(cam.gameObject, 0.6f, 25f, 2, PlayerSnap2, true);
            
            if (player2_Snapped && camera2_Snapped) {
                PositionSnap(gameObject, 0.6f, 45f, 0, PlayerSnap2);
                Destroy(startWall);
                Destroy(oldWall);
                newWall.SetActive(true);
                BlackFrame.SetActive(true);

                barsAligned = true;
            }
        }
    }

    private bool PositionSnap(GameObject originalGameObject, float distThres, float angleThres, int snapMode = 0, GameObject targetGameObject = null, bool ignorePos = false) //snapMode 0 replace, snapMode 1 reposition
    {
        if (originalGameObject == null)
            return false;
        
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

        return false;
    }

    private void Loop()
    {
        if (transform.position.x < -72f) {
            if (Quaternion.Angle(cam.transform.rotation, Quaternion.Euler(0, 90, 0)) > 70f)
                transform.position = new Vector3(transform.position.x + 80f, transform.position.y, transform.position.z);
        }

        if (outOfLoopTrigger) {
            if (Quaternion.Angle(cam.transform.rotation, Quaternion.Euler(0, 180, 0)) < 70f) {
                inLoop = false;
                loopExited = true;
                
                oldWall.SetActive(true);
                roomsOut.SetActive(true);
                Destroy(OriginalRooms);
            }
        }
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
        if (other.gameObject.layer == LayerMask.NameToLayer("BlueFrame")) {
            collidedBlueFrame = other.gameObject;
        }
        if (other.gameObject.layer == LayerMask.NameToLayer("BlackFrame")) {
            collidedBlackFrame = other.gameObject;
        }
        if (!loopExited) {
            if (other.gameObject.layer == LayerMask.NameToLayer("LoopTrigger")) {
                inLoop = true;
                endlessMirror.SetActive(true);
            }
            if (other.gameObject.layer == LayerMask.NameToLayer("ExitLoopTrigger")) {
                outOfLoopTrigger = true;
            }
        }
        if (other.gameObject.layer == LayerMask.NameToLayer("FallTrigger")) {
            Destroy(BlackFloor);
        }
    }

    private void OnTriggerExit(Collider other) {
        if (other.gameObject.layer == LayerMask.NameToLayer("RedFrame")) {
            collidedRedFrame = null;
        }
        if (other.gameObject.layer == LayerMask.NameToLayer("BlueFrame")) {
            collidedBlueFrame = null;
        }
        if (other.gameObject.layer == LayerMask.NameToLayer("BlackFrame")) {
            collidedBlackFrame = null;
        }
        if (!loopExited) {
            if (other.gameObject.layer == LayerMask.NameToLayer("LoopTrigger")) {
                inLoop = false;
                endlessMirror.SetActive(false);
            }
            if (other.gameObject.layer == LayerMask.NameToLayer("ExitLoopTrigger")) {
                outOfLoopTrigger = false;
            }
        }
    }
}
