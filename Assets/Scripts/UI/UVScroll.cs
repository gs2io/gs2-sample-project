using System;
using UnityEngine;
using UnityEngine.UI;

public class UVScroll : MonoBehaviour
{
    [SerializeField]
    private RawImage _targetRawImage;
    
    float scrollSpeed = 0.5f;
    
    void Awake()
    {
        var _targetRawImage = GetComponent<RawImage>();
    }

    void OnDisable()
    {
        if (_targetRawImage != null)
            _targetRawImage.material.mainTextureOffset = new Vector2(0, 0);
    }

    // Update is called once per frame
    void Update()
    {
       if (_targetRawImage != null)
            _targetRawImage.material.mainTextureOffset = new Vector2(0, Time.time * scrollSpeed);
    }
}
