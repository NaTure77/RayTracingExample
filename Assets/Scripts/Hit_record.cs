
public struct hittable_struct
{
    public float radius;
    public float x;
    public float y;
    public float z;
    public float cx;
    public float cy;
    public float cz;
    public int mat;
}

public class hit_record
{
    public vec3 p;
    public vec3 normal;
    public material mat;
    public float t;
    public bool front_face;

    public void set_face_normal(Ray r, vec3 outward_normal) 
    {
        front_face = vec3.dot(r.direction, outward_normal) < 0;
        normal = front_face ? outward_normal : outward_normal * -1;
    }
}

public abstract class hittable
{
    public abstract bool hit(Ray r, float t_min, float t_max, hit_record rec);

    public abstract hittable_struct toStruct();

}

class sphere : hittable
{
    public vec3 center;
    public float radius;
    public material mat;
    public sphere() { }

    public sphere(vec3 cen, float r, material m)
    {
        center = cen;
        radius = r;
        mat = m;
    }
    //(A + tB - C)(A + tB - C) = rr
    //(A - C + tB)(A - C + tB) = rr
    //(oc + tB)(oc + tB) = rr
    //ttBB + 2 * tB(oc) + (oc)(oc) - rr = 0
    //���⼭ t�� ���� 2���� �� ���� ǥ���� �ι� ���.
    public override bool hit(Ray r, float t_min, float t_max, hit_record rec)
    {
        vec3 oc = r.origin - center;
        var a = r.direction.length_squared();
        var half_b = vec3.dot(oc, r.direction);
        var c = oc.length_squared() - radius * radius;

        var discriminant = half_b * half_b - a * c;
        if (discriminant < 0) return false;
        var sqrtd = utils.sqrt(discriminant);

        // Find the nearest root that lies in the acceptable range.
        //���� ���Ŀ� ���� �ذ� �ΰ��� ��� �ϳ��� ���ؾ� ��.
        var root = (-half_b - sqrtd) / a;
        if (root < t_min || t_max < root)
        {
            root = (-half_b + sqrtd) / a;
            if (root < t_min || t_max < root)
                return false;
        }

        //ǥ����� �Ÿ�
        rec.t = root;

        //ǥ���� ��ǥ
        rec.p = r.At(rec.t);

        //ǥ���� �븻
        vec3 outward_normal = (rec.p - center) / radius;

        //��ü ���ʿ� ���� ���� �븻 ������ �ݴ�� �ٲ��� ��.
        rec.set_face_normal(r, outward_normal);
        rec.mat = mat;

        return true;
    }

    public override hittable_struct toStruct()
    {
        hittable_struct s = new hittable_struct
        {
            x = (float)center.x,
            y = (float)center.y,
            z = (float)center.z,
            radius = (float)radius,
            mat = mat.id,
            cx = (float)mat.GetAlbedo().x,
            cy = (float)mat.GetAlbedo().y,
            cz = (float)mat.GetAlbedo().z
        };
        return s;
    }
}
