using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering.Universal;
using UnityEngine.UI;

public class MatchResolution : MonoBehaviour
{
    // Start is called before the first frame update

    private float lastAspect = -1f;
    public int targetWidth = 240;

    public RawImage image;

    private Camera cam;
    private Vector2 currRes = Vector2.zero;

    void Start()
    {
        cam = GetComponent<Camera>();
    }

    // Update is called once per frame
    void Update()
    {
        if (Camera.main.aspect != lastAspect)
        {
            lastAspect = Camera.main.aspect;
            updateRendTexture();
        }

    }

    void updateRendTexture()
    {
        int height = Mathf.RoundToInt(targetWidth / Camera.main.aspect);

        if (new Vector2(targetWidth, height) == currRes)
        {
            return;
        }

        if (cam.targetTexture != null)
        {
            cam.targetTexture.Release();
            cam.targetTexture = null;
        }

        RenderTexture rendTex = new RenderTexture(targetWidth, height, 24, RenderTextureFormat.ARGB32)
        {
            name = "resizedTexture",
            useMipMap = false,
            autoGenerateMips = false,
            antiAliasing = 1,
            filterMode = FilterMode.Point,

        };

        rendTex.Create();

        cam.targetTexture = rendTex;
        image.texture = rendTex;
    }
}

