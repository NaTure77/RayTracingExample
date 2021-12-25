using System.Collections;
using System.Collections.Generic;

public class hittableList
{
    public List<hittable> objects = new List<hittable>();
    public hittableList() { }
    public hittableList(hittable obj)
    {
        add(obj);
    }

    public void clear() { objects.Clear(); }
    public void add(hittable obj) { objects.Add(obj); }

    public bool hit(Ray r, float t_min, float t_max, hit_record rec)
    {
        bool hit_anything = false;
        float closest_so_far = t_max;

        for(int i = 0; i < objects.Count; i++)
        {
            if(objects[i].hit(r,t_min,closest_so_far,rec))
            {
                hit_anything = true;
                closest_so_far = rec.t;
            }
        }

        return hit_anything;
    }

    public hittable_struct[] toStruct()
    {
        List <hittable_struct> list = new List<hittable_struct>();
        for (int i = 0; i < objects.Count; i++)
        {
            list.Add(objects[i].toStruct());
        }
        return list.ToArray();
    }

    public static hittableList Generate_random_scene()
    {
        hittableList world = new hittableList();
        var ground_material = new lambertian(new vec3(0.5f, 0.5f, 0.5f));
        world.add(new sphere(new vec3(0, -1000, 0), 1000, ground_material));

        for (int i = -11; i < 11; i++)
            for (int j = -11; j < 11; j++)
            {
                var choose_mat = utils.rand();

                vec3 center = new vec3(i + 0.9f * utils.rand(), 0.2f, j + 0.9f * utils.rand());

                if ((center - new vec3(4, 0.2f, 0)).length() > 0.9)
                {
                    material sphere_material;

                    if (choose_mat < 0.8)
                    {
                        //diffuse
                        var albedo = vec3.random() * vec3.random();

                        sphere_material = new lambertian(albedo);

                        world.add(new sphere(center, 0.2f, sphere_material));
                    }
                    else if (choose_mat < 0.95)
                    {
                        //metal
                        var albedo = vec3.random(0.5f, 1);
                        var fuzz = utils.rand(0, 0.5f);
                        sphere_material = new metal(albedo, fuzz);

                        world.add(new sphere(center, 0.2f, sphere_material));
                    }
                    else
                    {
                        //glass
                        sphere_material = new dielectric(1.5f);
                        world.add(new sphere(center, 0.2f, sphere_material));
                    }
                }
            }
        var material1 = new dielectric(1.5f);
        world.add(new sphere(new vec3(0, 1, 0), 1.0f, material1));

        var material2 = new lambertian(new vec3(0.4f, 0.2f, 0.1f));
        world.add(new sphere(new vec3(-4, 1, 0), 1.0f, material2));

        var material3 = new metal(new vec3(0.7f, 0.6f, 0.5f), 0.0f);
        world.add(new sphere(new vec3(4, 1, 0), 1.0f, material3));
        return world;
    }
}
