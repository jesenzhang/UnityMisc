using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class listTest : MonoBehaviour
{
    public EasyScrollView.ScrollView ScrollView;
    // Start is called before the first frame update
    void Start()
    {
        ScrollView.OnCellUpdate(UpdateCell);
        ScrollView.UpdateContents(new int[100]);
      
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    Vector2 UpdateCell(GameObject cell, int index)
    {
        cell.transform.GetChild(0).GetComponent<Text>().text = "" + index +"  "+ cell.name;
        return Random.Range(100f,150f)*(0+1)*2*Vector2.one;
    }
}
