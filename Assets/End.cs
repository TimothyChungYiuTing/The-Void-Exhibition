using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;


public class End : MonoBehaviour
{
    public Camera cam;
    public List<Transform> cameraPositions;
    private int currentCamPos = 0;
    public Image Screen_Dimmer;
    // Start is called before the first frame update
    void Start()
    {
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = false;
        Screen_Dimmer.color = Color.black;
        StartCoroutine(ScreenDarken(1f, 1.2f, Color.black, new Color(0f, 0f, 0f, 0.1f)));
        AudioManager.Instance.ChangeSong(6);
        InvokeRepeating("NextShowcase", 0.6375f, 1.85f);
    }

    // Update is called once per frame
    void Update()
    {
        cam.transform.position += cam.transform.right * Time.deltaTime;
    }

    void NextShowcase()
    {
        currentCamPos++;
        currentCamPos %= cameraPositions.Count;
        cam.transform.position = cameraPositions[currentCamPos].position;
        cam.transform.rotation = cameraPositions[currentCamPos].rotation;
    }

    private IEnumerator ScreenDarken(float delay, float duration, Color fromColor, Color toColor) {
        yield return new WaitForSeconds(delay);

        float timer = 0f;
        float t;

        while (timer < duration)
        {
            t = Mathf.Clamp01(timer/duration);
            Screen_Dimmer.color = Color.Lerp(fromColor, toColor, t);

            timer += Time.deltaTime;
            yield return null;
        }
        //Todo: Exit back to original settings
        Screen_Dimmer.color = toColor;
    }
}
