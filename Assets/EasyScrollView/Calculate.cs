using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Calculate : MonoBehaviour
{
    public ComputeShader computeShader;


    private Renderer _renderer;
    private RenderTexture _renderTexture;

    public int TexResolution;
    
    // Start is called before the first frame update
    void Start()
    {
        _renderTexture = RenderTexture.GetTemporary(TexResolution,TexResolution,24);
        _renderTexture.enableRandomWrite = true;
        _renderer = GetComponent<Renderer>();
        
        
    }

    private void UpdateTexFromComputer()
    {
        int kernelHandle = computeShader.FindKernel("CSMain");
        computeShader.SetInt("RandomOffset",(int)Time.timeSinceLevelLoad*100);
        computeShader.SetTexture(kernelHandle,"Result",_renderTexture);
        computeShader.Dispatch(kernelHandle,TexResolution/32,TexResolution/32,1);
        _renderer.material.SetTexture("_MainTex",_renderTexture);
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetMouseButtonDown(0))
            UpdateTexFromComputer();

    }
}
