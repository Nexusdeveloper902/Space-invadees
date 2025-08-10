using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class UIManager : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI livesText;
    [SerializeField] private Player player;

    void Update()
    {
        livesText.text = player.lives.ToString() + "/3";
    }
}
