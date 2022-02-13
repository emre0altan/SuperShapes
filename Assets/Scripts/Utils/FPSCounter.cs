using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class FPSCounter : MonoBehaviour
{
    [SerializeField, Range(0,1f)] private float RefreshRate = 1f;
    private Text text;
    private float timer;

    private void Start()
    {
        text = GetComponent<Text>();
    }

    private void Update()
    {
        if (Time.unscaledTime > timer)
        {
            int fps = (int)(1f / Time.unscaledDeltaTime);
            text.text = fps.ToString();
            timer = Time.unscaledTime + RefreshRate;
        }
    }
}
