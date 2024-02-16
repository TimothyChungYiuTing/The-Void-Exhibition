using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Mirror : MonoBehaviour
{
    public GameObject LH;
    public GameObject RH;
    public GameObject Mirror_LH;
    public GameObject Mirror_RH;

    public int mirrorID;

    private Vector3 enterPos;
    private Vector3 exitPos;

    private Level_Mirror level_Mirror;

    // Start is called before the first frame update
    void Start()
    {
        level_Mirror = FindObjectOfType<Level_Mirror>();
    }

    // Update is called once per frame
    void Update()
    {
        if (level_Mirror.currentMirrorID == mirrorID) {
            ReflectTransform(Mirror_LH.transform, LH.transform);
            ReflectTransform(Mirror_RH.transform, RH.transform);
        }
    }
    
    public void ReflectTransform(Transform objectTransform, Transform targetTransform)
    {
        // Get the normal and position of the plane
        Vector3 planeNormal = transform.forward;
        Vector3 planePosition = transform.position;

        // Calculate the vector from the plane's position to the object's position
        Vector3 planeToObject = targetTransform.position - planePosition;

        // Calculate the reflected position using vector reflection formula
        Vector3 reflectedPosition = targetTransform.position - 2f * Vector3.Dot(planeToObject, planeNormal) * planeNormal;

        // Calculate the reflected rotation
        Quaternion rotationDifference = Quaternion.FromToRotation(planeNormal, -planeNormal);
        Quaternion reflectedRotation = rotationDifference * targetTransform.rotation;

        // Set the reflected position and rotation to the object
        objectTransform.position = reflectedPosition;
        objectTransform.rotation = reflectedRotation;

        objectTransform.localScale = new Vector3(-targetTransform.localScale.x, targetTransform.localScale.y, targetTransform.localScale.z);
    }

    private void OnTriggerEnter(Collider other) {
        if (other.gameObject.layer == LayerMask.NameToLayer("Player")) {
            enterPos = other.transform.position;
        }
    }

    private void OnTriggerExit(Collider other) {
        if (other.gameObject.layer == LayerMask.NameToLayer("Player")) {
            exitPos = other.transform.position;
            if (PassedThru(enterPos, exitPos)) {
                if (level_Mirror.currentMirrorID == mirrorID) {
                    level_Mirror.currentMirrorID++;
                }
                else {
                    level_Mirror.currentMirrorID = 0;
                }
            }
        }
    }

    private bool PassedThru(Vector3 enterPos, Vector3 exitPos)
    {
        //Calculate the signed distances from the positions to the plane
        float distance1 = SignedDistanceToPlane(enterPos, transform.forward, transform.position);
        float distance2 = SignedDistanceToPlane(exitPos, transform.forward, transform.position);

        //Check if the signed distances have opposite signs
        return Mathf.Sign(distance1) != Mathf.Sign(distance2);
    }

    //Calculate the signed distance from a point to a plane
    private float SignedDistanceToPlane(Vector3 point, Vector3 planeNormal, Vector3 planePosition)
    {
        return Vector3.Dot(planeNormal, point - planePosition);
    }
}
