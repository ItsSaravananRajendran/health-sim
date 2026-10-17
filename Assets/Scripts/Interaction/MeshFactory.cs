using System.Collections.Generic;
using UnityEngine;

namespace PlateCheck.Interaction
{
    /// <summary>Small procedural meshes for food models: revolved profiles and extruded outlines.</summary>
    public static class MeshFactory
    {
        /// <summary>
        /// Revolves a profile (x = radius, y = height) around the Y axis. Order the profile so the
        /// surface is on its right going forward: start at the axis on the bottom, go out, up the
        /// outside, then (for an open vessel) over the rim and down the inside. With
        /// <paramref name="flat"/> every face gets its own vertices, for a faceted look.
        /// </summary>
        public static Mesh Lathe(IReadOnlyList<Vector2> profile, int segments = 24, bool flat = false, float startAngle = 0f)
        {
            var vertices = new List<Vector3>();
            var triangles = new List<int>();
            int n = profile.Count;

            Vector3 Point(int ring, int i)
            {
                float angle = startAngle + Mathf.PI * 2f * ring / segments;
                return new Vector3(profile[i].x * Mathf.Cos(angle), profile[i].y, profile[i].x * Mathf.Sin(angle));
            }

            if (!flat)
            {
                for (int ring = 0; ring <= segments; ring++)
                for (int i = 0; i < n; i++)
                    vertices.Add(Point(ring, i));
            }

            for (int ring = 0; ring < segments; ring++)
            for (int i = 0; i < n - 1; i++)
            {
                if (profile[i] == profile[i + 1]) continue; // duplicate point: a hard edge, no face
                int a, b, c, d; // a = this ring/low, c = this ring/high, b and d on the next ring
                if (flat)
                {
                    a = vertices.Count;
                    vertices.Add(Point(ring, i));
                    vertices.Add(Point(ring, i + 1));
                    vertices.Add(Point(ring + 1, i));
                    vertices.Add(Point(ring + 1, i + 1));
                    c = a + 1; b = a + 2; d = a + 3;
                }
                else
                {
                    a = ring * n + i; c = a + 1; b = (ring + 1) * n + i; d = b + 1;
                }
                triangles.AddRange(new[] { a, c, d, a, d, b });
            }
            return Finish("Lathe", vertices, triangles);
        }

        /// <summary>A pie wedge (cake slice) with its tip at the origin, pointing along +X.</summary>
        public static Mesh Wedge(float radius, float angleDegrees, float yBottom, float yTop, int arcSegments = 8)
        {
            var outline = new List<Vector2> { Vector2.zero };
            float half = angleDegrees * Mathf.Deg2Rad / 2f;
            for (int i = 0; i <= arcSegments; i++)
            {
                float angle = -half + 2f * half * i / arcSegments;
                outline.Add(new Vector2(Mathf.Cos(angle), Mathf.Sin(angle)) * radius);
            }
            return Extrude(outline, yBottom, yTop);
        }

        /// <summary>Extrudes a convex outline given counter-clockwise in (x, z). Faceted.</summary>
        public static Mesh Extrude(IReadOnlyList<Vector2> outline, float yBottom, float yTop)
        {
            var vertices = new List<Vector3>();
            var triangles = new List<int>();
            int n = outline.Count;

            int top = vertices.Count;
            foreach (var p in outline) vertices.Add(new Vector3(p.x, yTop, p.y));
            for (int i = 1; i < n - 1; i++) triangles.AddRange(new[] { top, top + i + 1, top + i });

            int bottom = vertices.Count;
            foreach (var p in outline) vertices.Add(new Vector3(p.x, yBottom, p.y));
            for (int i = 1; i < n - 1; i++) triangles.AddRange(new[] { bottom, bottom + i, bottom + i + 1 });

            for (int i = 0; i < n; i++)
            {
                var p = outline[i];
                var q = outline[(i + 1) % n];
                int s = vertices.Count;
                vertices.Add(new Vector3(p.x, yBottom, p.y));
                vertices.Add(new Vector3(p.x, yTop, p.y));
                vertices.Add(new Vector3(q.x, yTop, q.y));
                vertices.Add(new Vector3(q.x, yBottom, q.y));
                triangles.AddRange(new[] { s, s + 1, s + 2, s, s + 2, s + 3 });
            }
            return Finish("Extrude", vertices, triangles);
        }

        static Mesh Finish(string name, List<Vector3> vertices, List<int> triangles)
        {
            var mesh = new Mesh { name = name };
            mesh.SetVertices(vertices);
            mesh.SetTriangles(triangles, 0);
            mesh.RecalculateNormals();
            mesh.RecalculateBounds();
            return mesh;
        }
    }
}
