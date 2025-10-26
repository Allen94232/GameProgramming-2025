using UnityEngine;
using System.Collections.Generic;
using Unity.VectorGraphics;
using Newtonsoft.Json;

[System.Serializable]
public class GeoJSON
{
    public Feature[] features;
}

[System.Serializable]
public class Feature
{
    public string type;
    public Properties properties;
    public Geometry geometry;
}

[System.Serializable]
public class Properties
{
    public string name;
    public string building;
}

[System.Serializable]
public class Geometry
{
    public string type;
    public float[][][][] coordinates; // MultiPolygon: [ [ [ [x,y], ... ] ], ... ]
}

public class GeoJsonLoader : MonoBehaviour
{
    public TextAsset geoJsonFile;
    public Material lineMaterial;    // 畫輪廓用
    public Material fillMaterial;    // 填色用
    public float scale = 1f;

    void Start()
    {
        if (geoJsonFile == null)
        {
            Debug.LogError("請先拖入 GeoJSON (TextAsset)!");
            return;
        }

        GeoJSON geo = JsonConvert.DeserializeObject<GeoJSON>(geoJsonFile.text);
        if (geo.features == null || geo.features.Length == 0)
        {
            Debug.LogWarning("GeoJSON 沒有 features!");
            return;
        }

        // 計算中心點
        Vector2 center = Vector2.zero;
        int count = 0;
        foreach (var f in geo.features)
        {
            if (f.geometry == null || f.geometry.coordinates == null) continue;
            foreach (var poly in f.geometry.coordinates)
                foreach (var ring in poly)
                    foreach (var c in ring)
                    {
                        center.x += c[0];
                        center.y += c[1];
                        count++;
                    }
        }
        if (count > 0) center /= count;

        int totalBuildings = 0;

        foreach (var feature in geo.features)
        {
            if (feature.geometry == null || feature.geometry.coordinates == null) continue;

            foreach (var polygon in feature.geometry.coordinates)
            {
                if (polygon.Length == 0) continue;

                var outerRing = polygon[0];
                List<Vector2> verts2D = new List<Vector2>();
                List<Vector3> verts3D = new List<Vector3>();

                foreach (var c in outerRing)
                {
                    Vector3 v3 = new Vector3((c[0] - center.x) * scale * 1500 + 1080, (c[1] - center.y) * scale * 1500 - 370, 0);
                    verts3D.Add(v3);
                    verts2D.Add(new Vector2(v3.x, v3.y));
                }

                if (verts3D.Count < 3) continue;

                GameObject bld = new GameObject("Building_" + totalBuildings);
                bld.transform.parent = transform;

                // -----------------------
                // LineRenderer 畫輪廓
                // -----------------------
                LineRenderer lr = bld.AddComponent<LineRenderer>();
                lr.positionCount = verts3D.Count + 1;
                lr.loop = true;
                lr.widthMultiplier = 1f;
                lr.material = lineMaterial;
                lr.startColor = lr.endColor = Color.black;
                lr.sortingLayerName = "Default";
                lr.sortingOrder = 1;
                for (int i = 0; i < verts3D.Count; i++)
                    lr.SetPosition(i, verts3D[i]);
                lr.SetPosition(verts3D.Count, verts3D[0]);

                // -----------------------
                // EdgeCollider2D
                // -----------------------
                EdgeCollider2D edge = bld.AddComponent<EdgeCollider2D>();
                List<Vector2> edgePoints = new List<Vector2>(verts2D);
                edgePoints.Add(verts2D[0]);
                edge.points = edgePoints.ToArray();

                // -----------------------
                // Vector Graphics 填色
                // -----------------------
                BezierContour contour = new BezierContour()
                {
                    Segments = new BezierPathSegment[verts2D.Count],
                    Closed = true
                };

                for (int i = 0; i < verts2D.Count; i++)
                {
                    Vector2 p = verts2D[i];
                    contour.Segments[i] = new BezierPathSegment() { P0 = p, P1 = p, P2 = p };
                }

                Shape shape = new Shape()
                {
                    Contours = new BezierContour[] { contour },
                    Fill = new SolidFill() { Color = Random.ColorHSV() },
                    PathProps = new PathProperties()
                };

                Scene scene = new Scene()
                {
                    Root = new SceneNode()
                    {
                        Shapes = new List<Shape>() { shape }
                    }
                };

                Mesh mesh = new Mesh();
                VectorUtils.TessellationOptions options = new VectorUtils.TessellationOptions()
                {
                    StepDistance = 1f,
                    MaxCordDeviation = 0.5f,
                    SamplingStepSize = 0.01f
                };

                List<VectorUtils.Geometry> geometry = VectorUtils.TessellateScene(scene, options);
                VectorUtils.FillMesh(mesh, geometry, 1f);
                MeshRenderer mr = bld.AddComponent<MeshRenderer>();
                MeshFilter mf = bld.AddComponent<MeshFilter>();
                mf.mesh = mesh;
                mr.material = lineMaterial; // 這個材質可以用來顯示顏色
                mr.sortingLayerName = "Default";
                mr.sortingOrder = 0;

                totalBuildings++;
            }
        }

        Debug.Log($"總共生成 {totalBuildings} 個建築物輪廓並填色");
    }
}
