using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Float : MonoBehaviour
{
    private Vector3 startPos;
    // Start is called before the first frame update
    void Start()
    {
        startPos = transform.position;
    }

    // Update is called once per frame
    void Update()
    {
        transform.position = startPos + Vector3.up * Mathf.Sin(Time.time);
        transform.rotation = Quaternion.Euler(transform.eulerAngles.x, transform.eulerAngles.y + Time.deltaTime * 60f, transform.eulerAngles.z);
    }
}
