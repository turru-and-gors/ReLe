using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class TextOnSpot : MonoBehaviour
{
    public string text = "Training car";

    private void Start()
    {
        SetText(text);
    }

    public void SetText(string text)
    {
        this.text = text;
        Text label = GetComponentInChildren<Text>();
        label.text = this.text;
    }
}
