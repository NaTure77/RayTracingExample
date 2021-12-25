
public class camera
{
    public vec3 origin;
    public vec3 lower_left_corner;
    public vec3 horizontal;
    public vec3 vertical;

    public float aperture;
    public float focus_dist;
    public float lens_radius;

    public vec3 w, u, v;
    public camera(vec3 lookfrom, vec3 lookat, vec3 vup, float vfov, float aspect_ratio, float aperture, float focus_dist)
    {
        Set(lookfrom, lookat, vup, vfov, aspect_ratio, aperture, focus_dist);
    }

    public void Set(vec3 lookfrom, vec3 lookat, vec3 vup, float vfov, float aspect_ratio, float aperture, float focus_dist)
    {
        var theta = utils.degrees_to_radians(vfov);
        var h = (float)System.Math.Tan(theta / 2);
        var viewport_height = 2.0f * h;
        var viewport_width = aspect_ratio * viewport_height;

        w = (lookfrom - lookat).Unit();
        u = vec3.cross(vup, w).Unit();
        v = vec3.cross(w, u);

        //camera position
        origin = lookfrom;

        horizontal = focus_dist * viewport_width * u;
        vertical = focus_dist * viewport_height * v;
        lower_left_corner = origin - (horizontal * 0.5f) - (vertical * 0.5f) - focus_dist * w;

        lens_radius = aperture / 2;
    }

    public Ray get_ray(float s, float t)
    {
        vec3 rd = lens_radius * vec3.random_in_unit_disk();
        vec3 offset = u * rd.x + v * rd.y;

        return new Ray(origin + offset, lower_left_corner + s * horizontal + t * vertical - (origin + offset));
    }
}