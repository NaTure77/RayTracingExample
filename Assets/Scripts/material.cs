
public abstract class material
{
    public int id;
    public abstract vec3 GetAlbedo();
    public abstract bool scatter(Ray r_in, hit_record rec, out vec3 attenuation, out Ray scattered);
}

public class lambertian : material
{
    vec3 albedo;
    public lambertian(vec3 a) { albedo = a; id = 1; }

    public override vec3 GetAlbedo()
    {
        return albedo;
    }

    public override bool scatter(Ray r_in, hit_record rec, out vec3 attenuation, out Ray scattered)
    {
        vec3 scatter_direction = rec.normal + vec3.random_unit_vector();
        // Catch degenerate scatter direction
        if (scatter_direction.near_zero())
            scatter_direction = rec.normal;
        scattered = new Ray(rec.p, scatter_direction);
        attenuation = albedo;

        return true;
    }


}

public class metal : material 
{
    vec3 albedo;
    float fuzz;
    public metal(vec3 a, float f) { albedo = a; fuzz = f < 1 ? f : 1; id = 0; }
    public override vec3 GetAlbedo()
    {
        return albedo;
    }
    public override bool scatter(Ray r_in, hit_record rec, out vec3 attenuation, out Ray scattered)
    {
        vec3 reflected = vec3.reflect(r_in.direction.Unit(), rec.normal);
        scattered = new Ray(rec.p, reflected + fuzz * vec3.random_in_unit_sphere());
        attenuation = albedo;
        return (vec3.dot(scattered.direction, rec.normal) > 0); //no more than degree 90
    }
}

public class dielectric : material 
{
    float ir;
    public dielectric(float index_of_refraction) { ir = index_of_refraction; id = 2; }
    public override vec3 GetAlbedo()
    {
        return new vec3(1, 1, 1);
    }

    public override bool scatter(Ray r_in, hit_record rec, out vec3 attenuation, out Ray scattered)
    {
        attenuation = new vec3(1.0f, 1.0f, 1.0f);
        float refraction_ratio = rec.front_face ? (1.0f / ir) : ir;

        vec3 unit_direction = r_in.direction.Unit();

        float cos_theta = (float)System.Math.Min(vec3.dot(unit_direction * -1, rec.normal), 1.0);
        float sin_theta = utils.sqrt(1.0f - cos_theta * cos_theta);

        bool cannot_refract = refraction_ratio * sin_theta > 1.0;
        vec3 direction;

        if (cannot_refract || reflectance(cos_theta, refraction_ratio) > utils.rand())
            direction = vec3.reflect(unit_direction, rec.normal);
        else
            direction = vec3.refract(unit_direction, rec.normal, refraction_ratio);

        scattered = new Ray(rec.p, direction);
        return true;
    }

    float reflectance(float cosine, float ref_idx)
    {
        // Use Schlick's approximation for reflectance.
        float r0 = (1 - ref_idx) / (1 + ref_idx);
        r0 = r0 * r0;
        return r0 + (1 - r0) * (float)System.Math.Pow((1 - cosine), 5);
    }
}