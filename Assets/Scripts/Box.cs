using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Box : MonoBehaviour
{
    public Transform red;
    public Transform green;
    public int col;
    public int row;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private bool _selected;
    public bool selected {
        get
        {
            return _selected;
        }
        set
        {
            _selected = value;
            changeColor();
        }
    }

    private void changeColor()
    {
        red.localScale = _selected ? Vector3.zero : Vector3.one;
        green.localScale = !_selected ? Vector3.zero : Vector3.one;
    }
}
