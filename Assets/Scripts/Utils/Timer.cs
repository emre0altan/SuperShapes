using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Timer : MonoBehaviour
{
    private Text text;
    private float timer;

    private void Start()
    {
        text = GetComponent<Text>();
    }

    private void Update()
    {
        text.text = Time.time.ToString(".0");
    }
}
