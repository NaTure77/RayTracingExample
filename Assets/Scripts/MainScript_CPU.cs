using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;

public class MainScript_CPU : MonoBehaviour
{
    public RawImage rawImage;
    void Start()
    {
        StartCoroutine(Render2());
    }
    vec3 ray_color(Ray r, hittableList world, int depth)
    {
        hit_record rec = new hit_record();
        Ray scattered;
        vec3 attenuation;
        vec3 result = new vec3(1, 1, 1);
        while (depth-- > 0)
        {
            if(world.hit(r, 0.001f, utils.infinity, rec))
            {
                if(rec.mat.scatter(r, rec, out attenuation, out scattered))
                {
                    result *= attenuation;
                    r = scattered;
                    continue;
                }
                result = new vec3(0, 0, 0);
                break;

            }

            //make background color if nothing hits
            vec3 unitDirection = r.direction.Unit();
            //change -1 ~ 1 into  0 ~ 1
            float t = 0.5f * (unitDirection.y + 1.0f);
            //¼±Çü È¥ÇÕ.
            result *= (1.0f - t) * new vec3(1.0f, 1.0f, 1.0f) + t * new vec3(0.5f, 0.7f, 1.0f);
            break;
        }
        return result;
    }

    [Range(0, 180)]
    public float vfov = 60;

    [Range(0, 20)]
    public float dist_to_focus = 10;

    [Range(0, 1)]
    public float aperture = 0.1f;

    [Range(0,10)]
    public float moveSpeed = 0.5f;
    bool changed = false;
    private void OnValidate()
    {
        changed = true;
    }
    // Vector3 <=> vec3 Converter
    Vector3 vec3ToVector3(vec3 v) => new Vector3(v.x, v.y, v.z);
    vec3 Vector3Tovec3(Vector3 v) => new vec3(v.x, v.y, v.z);
    IEnumerator Render2()
    {
        //Image
        float aspect_ratio = 16.0f / 9.0f;
        int image_width = 400;
        int image_height = (int)(image_width / aspect_ratio);
        int max_depth = 50;
        //int samples_per_pixel = 100;
        Texture2D texture = new Texture2D(image_width, image_height);
        //World
        hittableList world = hittableList.Generate_random_scene();

        //Camera
        vec3 lookfrom = new vec3(13, 2, 3);
        vec3 lookat = new vec3(0, 0, 0);
        vec3 vup = new vec3(0, 1, 0);
        vec3 direction = lookat - lookfrom;
        transform.position = vec3ToVector3(lookfrom);
        transform.forward = vec3ToVector3(direction);
        camera cam = new camera(lookfrom, lookat, vup, vfov, aspect_ratio, aperture, dist_to_focus);

        Color[] vecImage = new Color[image_width * image_height];
        vec3[,] vecImage2 = new vec3[image_width, image_height];
        int[,] hitCount = new int[image_width, image_height];

        float sensitivity = 2f;

        Vector2 cursorPos = transform.rotation.eulerAngles;


        System.Diagnostics.Stopwatch sw = new System.Diagnostics.Stopwatch();
        sw.Start();
        int ms = 60;

        while (true)
        {
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
            Vector3 movement = new Vector3();
            if (Input.GetKey(KeyCode.Space))
            {
                movement += Vector3.up;
            }
            if (Input.GetKey(KeyCode.LeftShift))
            {
                movement += Vector3.down;
            }
            if (Input.GetKey(KeyCode.C))
            {
                RenderTexture renderTexture = new RenderTexture(image_width, image_height, 32);
                renderTexture.Create();
                Graphics.Blit(texture, renderTexture);
                utils.CaptureImage(renderTexture);
            }
            movement.x -= Input.GetAxis("Horizontal");
            movement.z += Input.GetAxis("Vertical");
            changed |= movement.sqrMagnitude > 0;

            Vector3 forward = Quaternion.Euler(0, cursorPos.y, 0) * Vector3.forward * movement.z;
            Vector3 nextPosition = transform.position + (forward + transform.right * movement.x + Vector3.up * movement.y) * moveSpeed * Time.deltaTime;
            transform.position = nextPosition;

            //reset
            if (changed)
            {
                lookfrom = Vector3Tovec3(nextPosition);
                lookat = lookfrom + direction;
                texture = new Texture2D(image_width, image_height);
                cam.Set(lookfrom, lookat, vup, vfov, aspect_ratio, aperture, dist_to_focus);
                vecImage = new Color[image_width * image_height];
                vecImage2 = new vec3[image_width, image_height];
                hitCount = new int[image_width, image_height];
                changed = false;
                //show contour while updating camera
                for (int j = 0; j < image_height; j+= 3)
                    for (int i = 0; i < image_width; i+= 3)
                    {
                        float u = (i) / (image_width - 1f);
                        float v = (j) / (image_height - 1f);
                        Ray r = cam.get_ray(u, v);
                        vec3 pixel_color = ray_color(r, world,1);
                        vecImage[j * image_width + i] = new Color(pixel_color.x, pixel_color.y, pixel_color.z);
                    }
            }
            while (true)
            {
                //Pick a random pixel
                int randX = Random.Range(0, image_width);
                int randY = Random.Range(0, image_height);

                //make u,v
                float u = (randX + utils.rand()) / (image_width - 1f);
                float v = (randY + utils.rand()) / (image_height - 1f);

                Ray r = cam.get_ray(u, v);
                vecImage2[randX, randY] += ray_color(r, world, max_depth);
                hitCount[randX, randY]++;

                float color_scale = 1.0f / hitCount[randX, randY];
                vec3 pixel_color = vecImage2[randX, randY] * color_scale;
                pixel_color.x = utils.sqrt(pixel_color.x);
                pixel_color.y = utils.sqrt(pixel_color.y);
                pixel_color.z = utils.sqrt(pixel_color.z);

                vecImage[randY * image_width + randX] = new Color(pixel_color.x, pixel_color.y, pixel_color.z);
                if (sw.ElapsedMilliseconds > ms)
                {
                    yield return null;
                    sw.Restart();
                    break;
                }
            }
            texture.SetPixels(vecImage);
            texture.Apply();
            rawImage.texture = texture;
        }
    }
}
