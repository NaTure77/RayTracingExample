public struct Ray
{
    public vec3 dir;
    public vec3 orig;

    public Ray(vec3 origin, vec3 direction)
    {
        this.orig = origin;
        this.dir = direction;
    }

    public void Set(vec3 origin, vec3 direction)
    {
        this.orig = origin;
        this.dir = direction;
    }
    public vec3 origin => orig;
    public vec3 direction => dir;
    public vec3 At(float t) => orig + t * dir;
}
