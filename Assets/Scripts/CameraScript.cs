using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class CameraScript : MonoBehaviour
{
    public Transform playerTransform;
    public GameObject LH;
    public GameObject RH;

    private bool handStartLerp = false;
    private float handStartLerpTime = 0f;
    private float handStartGrabTime = 0f;
    
    private Vector3 LH_StartPos;
    private Vector3 RH_StartPos;
    private Quaternion LH_StartRot;
    private Quaternion RH_StartRot;
    
    private Vector3 LH_ToPos_Crouch;
    private Vector3 RH_ToPos_Crouch;
    private Quaternion LH_ToRot_Crouch;
    private Quaternion RH_ToRot_Crouch;

    private Vector3 RH_ToPos_Grab;
    private Quaternion RH_ToRot_Grab;

    private Vector3 LH_FromPos;
    private Vector3 RH_FromPos;
    private Quaternion LH_FromRot;
    private Quaternion RH_FromRot;
    
    private Vector3 LH_ToPos;
    private Vector3 RH_ToPos;
    private Quaternion LH_ToRot;
    private Quaternion RH_ToRot;
    
    public float handLerpTime;

    private bool revertedGrab = false;

    // Start is called before the first frame update
    void Start()
    {
        LH_StartPos = LH.transform.localPosition;
        RH_StartPos = RH.transform.localPosition;
        LH_StartRot = LH.transform.localRotation;
        RH_StartRot = RH.transform.localRotation;
            
        LH_ToPos = LH_StartPos;
        RH_ToPos = RH_StartPos;
        LH_ToRot = LH_StartRot;
        RH_ToRot = RH_StartRot;

        LH_ToPos_Crouch = new Vector3(-1f, -0.35f, 1f);
        RH_ToPos_Crouch = new Vector3(1f, -0.35f, 1f);
        LH_ToRot_Crouch = Quaternion.Euler(30f, 90f, -90f);
        RH_ToRot_Crouch = Quaternion.Euler(-30f, 90f, -90f);

        RH_ToPos_Grab = new Vector3(1f, -0.51f, 1.5f);
        RH_ToRot_Grab = Quaternion.Euler(-75f, 90f, -90f);
    }

    // Update is called once per frame
    void Update()
    {
        if (handStartLerp) {
            if (Time.time - handStartGrabTime < handLerpTime * 3f) {
                //Grab animations
                if (Time.time - handStartGrabTime > handLerpTime * 2f) {
                    //Reverting back
                    if (!revertedGrab) {
                        revertedGrab = true;

                        //Reset from position
                        LH_FromPos = LH.transform.localPosition;
                        RH_FromPos = RH.transform.localPosition;
                        LH_FromRot = LH.transform.localRotation;
                        RH_FromRot = RH.transform.localRotation;
                    }
                    LH.transform.localPosition = Vector3.Lerp(LH_FromPos, LH_ToPos, (Time.time - handStartLerpTime - handLerpTime*2f)/handLerpTime);
                    RH.transform.localPosition = Vector3.Lerp(RH_FromPos, RH_ToPos, (Time.time - handStartLerpTime - handLerpTime*2f)/handLerpTime);
                    LH.transform.localRotation = Quaternion.Lerp(LH_FromRot, LH_ToRot, (Time.time - handStartLerpTime - handLerpTime*2f)/handLerpTime);
                    RH.transform.localRotation = Quaternion.Lerp(RH_FromRot, RH_ToRot, (Time.time - handStartLerpTime - handLerpTime*2f)/handLerpTime);
                }
                else {
                    if (Time.time - handStartGrabTime < handLerpTime) {
                        //Reaching out
                        LH.transform.localPosition = Vector3.Lerp(LH_FromPos, LH_ToPos, (Time.time - handStartLerpTime)/handLerpTime);
                        RH.transform.localPosition = Vector3.Lerp(RH_FromPos, RH_ToPos_Grab, (Time.time - handStartLerpTime)/handLerpTime);
                        LH.transform.localRotation = Quaternion.Lerp(LH_FromRot, LH_ToRot, (Time.time - handStartLerpTime)/handLerpTime);
                        RH.transform.localRotation = Quaternion.Lerp(RH_FromRot, RH_ToRot_Grab, (Time.time - handStartLerpTime)/handLerpTime);
                    }
                }
            }
            else {
                if (Time.time - handStartLerpTime > handLerpTime) {
                    handStartLerp = false;
                    LH.transform.localPosition = LH_ToPos;
                    RH.transform.localPosition = RH_ToPos;
                    LH.transform.localRotation = LH_ToRot;
                    RH.transform.localRotation = RH_ToRot;

                    LH_FromPos = LH_ToPos;
                    RH_FromPos = RH_ToPos;
                    LH_FromRot = LH_ToRot;
                    RH_FromRot = RH_ToRot;
                }
                else {
                    LH.transform.localPosition = Vector3.Lerp(LH_FromPos, LH_ToPos, (Time.time - handStartLerpTime)/handLerpTime);
                    RH.transform.localPosition = Vector3.Lerp(RH_FromPos, RH_ToPos, (Time.time - handStartLerpTime)/handLerpTime);
                    LH.transform.localRotation = Quaternion.Lerp(LH_FromRot, LH_ToRot, (Time.time - handStartLerpTime)/handLerpTime);
                    RH.transform.localRotation = Quaternion.Lerp(RH_FromRot, RH_ToRot, (Time.time - handStartLerpTime)/handLerpTime);
                }
            }
        }
    }

    void FixedUpdate()
    {
        transform.position = playerTransform.transform.position + Vector3.up * 0.8f;
    }

    public void LerpHands(int handType)
    {
        handStartLerpTime = Time.time;
        handStartLerp = true;

        LH_FromPos = LH.transform.localPosition;
        RH_FromPos = RH.transform.localPosition;
        LH_FromRot = LH.transform.localRotation;
        RH_FromRot = RH.transform.localRotation;

        if (handType == 0) {
            LH_ToPos = LH_StartPos;
            RH_ToPos = RH_StartPos;
            LH_ToRot = LH_StartRot;
            RH_ToRot = RH_StartRot;
        }
        else if (handType == 1) {
            LH_ToPos = LH_ToPos_Crouch;
            RH_ToPos = RH_ToPos_Crouch;
            LH_ToRot = LH_ToRot_Crouch;
            RH_ToRot = RH_ToRot_Crouch;
        }
    }
    

    public void HandsGrab()
    {
        handStartLerpTime = Time.time;
        handStartLerp = true;
        handStartGrabTime = Time.time;

        LH_FromPos = LH.transform.localPosition;
        RH_FromPos = RH.transform.localPosition;
        LH_FromRot = LH.transform.localRotation;
        RH_FromRot = RH.transform.localRotation;
        
        revertedGrab = false;
    }
}
