using UnityEngine;
using UnityEngine.UI;

//[ExecuteInEditMode]
public class PrngTester : MonoBehaviour
{
    [SerializeField] ComputeShader _compute;

    RenderTexture _buffer;
    public RawImage rawImage;

    ComputeBuffer seeds;
    int kernel;
    void OnDestroy()
    {
        if (_buffer != null)
            if (Application.isPlaying)
                Destroy(_buffer);
            else
                DestroyImmediate(_buffer);

        seeds.Release();
    }

    private void Start()
    {
        seeds = new ComputeBuffer(1920 * 1080, sizeof(int));

        uint[] s = new uint[1920 * 1080];
        System.Random rand = new System.Random();
        for (int i = 0; i < s.Length; i++)
            s[i] = (uint) rand.Next(-int.MaxValue, int.MaxValue);

        seeds.SetData(s);
        
        _buffer = new RenderTexture(1920, 1080, 0);
        _buffer.enableRandomWrite = true;
        _buffer.filterMode = FilterMode.Point;
        _buffer.hideFlags = HideFlags.DontSave;
        _buffer.Create();
        rawImage.texture = _buffer;
        kernel = _compute.FindKernel("TesterKernel");
        _compute.SetTexture(kernel, "Result", _buffer);
        _compute.SetBuffer(kernel, "seed", seeds);

    }
    private void Update()
    {


        _compute.SetFloat("r", Random.Range(-10000,10000));
        _compute.Dispatch(kernel, Mathf.CeilToInt(1920 / 8f), Mathf.CeilToInt(1080 / 8f), 1);
    }
}