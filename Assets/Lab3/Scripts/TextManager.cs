using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class TextManager : MonoBehaviour
{

    Text[] texts;

    public int textAmount = 10;
    public int textsDistance = 1;
    public Vector2 textPos;
    public int inputBoxDistance = 20;

    public GameObject textLine;
    public GameObject inputBox;
    public GameObject canvas;

    InputField inputField;

    Stack<string> toSay;

    // Start is called before the first frame update
    void Start()
    {
        toSay = new Stack<string>();

        texts = new Text[textAmount];
        
        int i = 0;
        for (i =0; i<textAmount;i++)
        {
            GameObject a = Instantiate(textLine);
            a.transform.SetParent(canvas.transform);
            a.GetComponent<RectTransform>().anchoredPosition =new Vector3(textPos.x,textPos.y - textsDistance*i,0);
            texts[i] = a.GetComponent<Text>();
        }

        //GameObject box = Instantiate(inputBox);
        //box.transform.SetParent(canvas.transform);
        inputBox.GetComponent<RectTransform>().anchoredPosition = new Vector3(textPos.x, textPos.y - textsDistance * i - inputBoxDistance, 0);
        inputField = inputBox.GetComponent<InputField>();
    }

    // Update is called once per frame
    void Update()
    {
        int n = toSay.Count;

        for(int y = 0; y < n; y++)
        {
            for (int i = 0; i < texts.Length -1; i++)
            {
                texts[i].text = texts[i + 1].text;
            }
            
            texts[texts.Length - 1].text = toSay.Pop();
        }


    }

    public void SubmitText()
    {
        
    }

    public void Say(string _string)
    {
        toSay.Push(_string);
    }
}
