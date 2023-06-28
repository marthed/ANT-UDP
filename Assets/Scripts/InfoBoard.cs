using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Singleton;
using TMPro;


public class InfoBoard : Singleton<InfoBoard>
{
    // Start is called before the first frame update

    public TMP_Text _text;

    public TMP_Text _antText;
    void Start()
    {
    }


    public void SetInfoBoardText(string text) {
        _text.text = text;
    }

    public void SetInforBoardAntText(string text) {
        _antText.text = text;
    }
}
