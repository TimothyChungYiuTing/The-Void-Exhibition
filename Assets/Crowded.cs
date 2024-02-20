using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;


public class Crowded : MonoBehaviour
{
    public Camera cam;
    public Transform cameraPos1;
    public Transform cameraPos2;
    public TextMeshProUGUI Text_Level;
    public Image Screen_Dimmer;
    private List<String> textToShow = new List<String>{
        "Illusions seem Real,",
        "Reality seems Fake.",
        "The moment I realized",
        "I'm all alone in this Gallery",
    };

    // Start is called before the first frame update
    void Start()
    {
        StartCoroutine(Text(0f, 4f));
        StartCoroutine(ScreenDarken(15f, 1.5f, Color.clear, Color.black));
        Invoke("ToGallery", 19.5f);
    }

    // Update is called once per frame
    void Update()
    {
        cam.transform.position += cam.transform.right * Time.deltaTime;
    }

    void SwitchToPos1()
    {
        cam.transform.position = cameraPos1.position;
        cam.transform.rotation = cameraPos1.rotation;
    }
    void SwitchToPos2()
    {
        cam.transform.position = cameraPos2.position;
        cam.transform.rotation = cameraPos2.rotation;
    }

    private void ToGallery()
    {
        SceneManager.LoadScene("Gallery", LoadSceneMode.Single);
    }

    private IEnumerator Text(float delay, float duration) {
        yield return new WaitForSeconds(delay);

        float timer = 0f;
        //float t;
        float sineWave;

        Color whiteAlphaColor = new Color(0.84f, 0.84f, 0.84f, 0f);
        Color whiteColor = new Color(0.84f, 0.84f, 0.84f, 1f);

        Text_Level.color = Color.clear;
        Text_Level.text = textToShow[0];
        //yield return new WaitForSeconds(glitchDuration);
        while (timer < duration)
        {
            sineWave = 3f * Mathf.Sin(Mathf.PI * timer / duration);

            Text_Level.color = Color.Lerp(whiteAlphaColor, whiteColor, sineWave);

            timer += Time.deltaTime;
            yield return null;
        }
        timer = 0f;
        Text_Level.text = textToShow[1];
        SwitchToPos1();
        while (timer < duration)
        {
            sineWave = 3f * Mathf.Sin(Mathf.PI * timer / duration);
            
            Text_Level.color = Color.Lerp(whiteAlphaColor, whiteColor, sineWave);

            timer += Time.deltaTime;
            yield return null;
        }
        timer = 0f;
        Text_Level.text = textToShow[2];
        SwitchToPos2();
        while (timer < duration)
        {
            sineWave = 3f * Mathf.Sin(Mathf.PI * timer / duration);
            
            Text_Level.color = Color.Lerp(whiteAlphaColor, whiteColor, sineWave);

            timer += Time.deltaTime;
            yield return null;
        }
        timer = 0f;
        Text_Level.text = textToShow[3];
        AudioManager.Instance.ChangeSong(1);
        while (timer < duration)
        {
            sineWave = 3f * Mathf.Sin(Mathf.PI * timer / duration);
            
            Text_Level.color = Color.Lerp(whiteAlphaColor, whiteColor, sineWave);

            timer += Time.deltaTime;
            yield return null;
        }

        Text_Level.color = Color.clear;
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
