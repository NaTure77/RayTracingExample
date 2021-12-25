using color = vec3;
using point3 = vec3;
public struct vec3
{
    public float x, y, z;
    public vec3(float x, float y, float z) 
    { 
        this.x = x; 
        this.y = y; 
        this.z = z; 
    }

    public static vec3 operator +(vec3 a, vec3 b)
    {
        return new vec3(a.x + b.x, a.y + b.y, a.z + b.z);
    }
    public static vec3 operator -(vec3 a, vec3 b)
    {
        return new vec3(a.x - b.x, a.y - b.y, a.z - b.z);
    }

    public static vec3 operator *(vec3 a, float f)
    {
        return new vec3(a.x * f, a.y * f, a.z * f);
    }
    public static vec3 operator *(float f, vec3 a)
    {
        return new vec3(a.x * f, a.y * f, a.z * f);
    }
    public static vec3 operator *(vec3 a, vec3 b)
    {
        return new vec3(a.x * b.x, a.y * b.y, a.z * b.z);
    }
    public static vec3 operator /(vec3 a, float f)
    {
        return new vec3(a.x / f, a.y / f, a.z / f);
    }

    public static bool operator ==(vec3 a, vec3 b)
    {
        return a.x == b.x && a.y == b.y && a.z == b.z;
    }
    public static bool operator !=(vec3 a, vec3 b)
    {
        return a.x != b.x || a.y != b.y || a.z != b.z;
    }
    public static float dot(vec3 a, vec3 b)
    {
        return a.x * b.x + a.y * b.y + a.z * b.z;
    }
    public static vec3 cross(vec3 a, vec3 b)
    {
        return new vec3(a.y * b.z - a.z * b.y,
                        a.z * b.x - a.x * b.z,
                        a.x * b.y - a.y * b.x);
    }

    public static vec3 random()
    {
        return new vec3(utils.rand(), utils.rand(), utils.rand());
    }

    public static vec3 random(float min, float max)
    {
        return new vec3(utils.rand(min,max),
                        utils.rand(min, max), 
                        utils.rand(min, max));
    }
    public float length()
    {
        return utils.sqrt(length_squared());
    }
    public float length_squared()
    {
        return x * x + y * y + z * z;
    }

    public vec3 Unit()
    {
        return this / length();
    }
    public static vec3 refract(vec3 uv, vec3 n, float etai_over_etat)
    {
        float cos_theta = (float)System.Math.Min(dot(uv * -1, n), 1.0);
        vec3 r_out_perp = etai_over_etat * (uv + cos_theta * n);
        vec3 r_out_parallel = -utils.sqrt((float)System.Math.Abs(1.0 - r_out_perp.length_squared())) * n;
        return r_out_perp + r_out_parallel;
    }

    public static vec3 random_in_unit_sphere()
    {
        //return vec3.random(-1, 1).Unit();
        while (true)
        {
            vec3 p = random(-1, 1);


            if (p.length_squared() >= 1) continue;
            return p;
        }
    }
    public static vec3 random_unit_vector()
    {
        return random_in_unit_sphere().Unit();
    }

    public static vec3 random_in_hemisphere(vec3 normal)
    {
        vec3 in_unit_sphere = random_in_unit_sphere();
        if (dot(in_unit_sphere, normal) > 0.0) // In the same hemisphere as the normal
            return in_unit_sphere;
        else
            return in_unit_sphere * -1;
    }

    public bool near_zero()
    {
        // Return true if the vector is close to zero in all dimensions.
        const float s = 1e-4f;
        return (System.Math.Abs(x) < s) && (System.Math.Abs(y) < s) && (System.Math.Abs(z) < s);
    }

    public static vec3 reflect (vec3 v, vec3 n) 
    {
        return v - 2*dot(v, n)*n;
    }


    public static vec3 random_in_unit_disk()
    {
        while (true)
        {
            var p = new vec3(utils.rand(-1, 1), utils.rand(-1, 1), 0);
            if (p.length_squared() >= 1) continue;
            return p;
        }
    }
}