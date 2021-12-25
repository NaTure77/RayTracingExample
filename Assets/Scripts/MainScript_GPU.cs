using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;

public class MainScript_GPU : MonoBehaviour
{
    public RawImage rawImage;

    [Range(0, 180)]
    public float vfov = 60;

    [Range(0, 20)]
    public float dist_to_focus = 10;

    [Range(0, 1)]
    public float aperture = 0.1f;

    [Range(0, 10)]
    public float moveSpeed = 0.5f;

    [Range(0, 5)]
    public float fuzz;

    public RenderTexture renderTexture;
    public ComputeShader shader;

    ComputeBuffer objects;
    ComputeBuffer color_sum;
    ComputeBuffer traced_cnt;
    ComputeBuffer rays;
    ComputeBuffer seeds;

    int kernelID;
    bool changed = true;
    void Start()
    {
        Application.targetFrameRate = 60;
        StartCoroutine(Render3());
    }

    private void OnValidate()
    {
        changed = true;
    }
    private void OnDestroy()
    {
        objects.Release();
        color_sum.Release();
        traced_cnt.Release();
        rays.Release();
        seeds.Release();
    }

    void InitComputeShader(int image_width, int image_height)
    {
        hittable_struct[] world = hittableList.Generate_random_scene().toStruct();
        objects = new ComputeBuffer(world.Length, sizeof(float) * 8);
        objects.SetData(world);

        color_sum = new ComputeBuffer(image_width * image_height, sizeof(float) * 3);
        traced_cnt = new ComputeBuffer(image_width * image_height, sizeof(int) * 2);
        rays = new ComputeBuffer(image_width * image_height, sizeof(float) * 10);
        seeds = new ComputeBuffer(image_width * image_height, sizeof(int));

        renderTexture = new RenderTexture(image_width, image_height, 32);
        renderTexture.enableRandomWrite = true;
        renderTexture.Create();
        rawImage.texture = renderTexture;

        kernelID = shader.FindKernel("CSMain");
        shader.SetBuffer(kernelID, "objects", objects);
        shader.SetBuffer(kernelID, "color_sum", color_sum);
        shader.SetBuffer(kernelID, "traced_cnt", traced_cnt);
        shader.SetBuffer(kernelID, "rays", rays);
        shader.SetTexture(kernelID, "Result", renderTexture);

        shader.SetVector("resolution", new Vector2(image_width, image_height));
        shader.SetInt("object_count", world.Length);

        int max_depth = 50;
        int depth_per_frame = 1;
        shader.SetInt("max_depth", max_depth);
        shader.SetInt("depth_per_frame", depth_per_frame);

        uint[] s = new uint[image_width * image_height];
        System.Random rand = new System.Random();
        for (int i = 0; i < s.Length; i++)
            s[i] = (uint)rand.Next(-int.MaxValue, int.MaxValue);

        seeds.SetData(s);
        shader.SetBuffer(kernelID, "seeds", seeds);
    }

    // Vector3 <=> vec3 Converter
    Vector3 vec3ToVector3(vec3 v) => new Vector3(v.x, v.y, v.z);
    vec3 Vector3Tovec3(Vector3 v) => new vec3(v.x, v.y, v.z);
    IEnumerator Render3()
    {
        //Image
        float aspect_ratio = 16.0f / 9.0f;
        int image_width = 1920;
        int image_height = (int)(image_width / aspect_ratio);

        //Camera
        vec3 lookfrom = new vec3(13, 2, 3);
        vec3 lookat = new vec3(0, 0, 0);
        vec3 vup = new vec3(0, 1, 0);
        vec3 direction = lookat - lookfrom;
        transform.position = vec3ToVector3(lookfrom);
        transform.forward = vec3ToVector3(direction);
        camera cam = new camera(lookfrom, lookat, vup, vfov, aspect_ratio, aperture, dist_to_focus);
        Vector2 cursorPos = transform.rotation.eulerAngles;

        //Control
        float sensitivity = 2f;

        //shader
        InitComputeShader(image_width, image_height);

        //Loop
        while (true)
        {
            //Rotate camera When Mouse0 clicked
            if (Input.GetKey(KeyCode.Mouse0))
            {
                cursorPos.x -= Input.GetAxisRaw("Mouse Y") * sensitivity;
                cursorPos.y -= Input.GetAxisRaw("Mouse X") * sensitivity;
                cursorPos.x = Mathf.Clamp(cursorPos.x, -90, 90);

                Quaternion rot = Quaternion.Euler(new Vector3(cursorPos.x, cursorPos.y, 0));
                transform.rotation = Quaternion.Lerp(transform.rotation, rot, 0.3f);

                direction = Vector3Tovec3(transform.forward);
                changed = true;
            }

            //Move camera
            Vector3 movement = new Vector3();
            if (Input.GetKey(KeyCode.Space))
            {
                movement += Vector3.up;
            }
            if (Input.GetKey(KeyCode.LeftShift))
            {
                movement += Vector3.down;
            }
            if (Input.GetKeyDown(KeyCode.C))
            {
                utils.CaptureImage(renderTexture);
            }
            movement.x -= Input.GetAxisRaw("Horizontal");
            movement.z += Input.GetAxisRaw("Vertical");

            changed |= movement.sqrMagnitude > 0;

            Vector3 forward = Quaternion.Euler(0, cursorPos.y, 0) * Vector3.forward * movement.z;
            Vector3 nextPosition = transform.position + (forward + transform.right * movement.x + Vector3.up * movement.y) * moveSpeed * Time.deltaTime;
            transform.position = nextPosition;

            if (changed)
            {
                lookfrom = Vector3Tovec3(nextPosition);
                lookat = lookfrom + direction;
                cam.Set(lookfrom, lookat, vup, vfov, aspect_ratio, aperture, dist_to_focus);
                
                shader.SetVector("origin", vec3ToVector3(cam.origin));
                shader.SetVector("lower_left_corner", vec3ToVector3(cam.lower_left_corner));
                shader.SetVector("horizontal", vec3ToVector3(cam.horizontal));
                shader.SetVector("vertical", vec3ToVector3(cam.vertical));
                shader.SetVector("w", vec3ToVector3(cam.w));
                shader.SetVector("u", vec3ToVector3(cam.u));
                shader.SetVector("v", vec3ToVector3(cam.v));
                shader.SetFloat("lens_radius", cam.lens_radius);
            }

            Vector3 rand = new Vector3(Random.value, Random.value, Random.value);
            shader.SetVector("randNum", rand);
            shader.SetFloat("fuzz", fuzz);
            shader.SetBool("updated", changed);
            shader.Dispatch(kernelID, Mathf.CeilToInt(image_width / 8f), Mathf.CeilToInt(image_height / 8f), 1);
            changed = false;
            yield return null;
        }
    }

}
