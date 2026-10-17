using System.Collections.Generic;
using PlateCheck.Core;
using PlateCheck.Data;
using UnityEngine;

namespace PlateCheck.Interaction
{
    /// <summary>
    /// Builds a small low-poly model for each <see cref="FoodShape"/> out of primitives and
    /// procedural meshes. Models sit on y = 0 and are about 10 to 16 cm across.
    /// Meshes and materials are cached, so spawning the same food again costs little.
    /// </summary>
    public static class FoodModels
    {
        static readonly Dictionary<string, Mesh> Meshes = new Dictionary<string, Mesh>();
        static readonly Dictionary<(Color, float), Material> Materials = new Dictionary<(Color, float), Material>();
        static Material template;

        // Palette
        static readonly Color Ceramic = new Color(0.93f, 0.90f, 0.83f);
        static readonly Color BowlTeal = new Color(0.30f, 0.62f, 0.66f);
        static readonly Color LeafGreen = new Color(0.36f, 0.68f, 0.25f);
        static readonly Color DarkGreen = new Color(0.13f, 0.42f, 0.16f);

        public static void Build(FoodItemData food, Transform parent)
        {
            // Seeded by name, so a food looks the same every time it is spawned.
            var random = new System.Random(StableHash(food.displayName));
            switch (food.shape)
            {
                case FoodShape.Broccoli: Broccoli(parent, random); break;
                case FoodShape.Salad: Salad(parent, random); break;
                case FoodShape.Carrots: Carrots(parent); break;
                case FoodShape.Apple: Apple(parent); break;
                case FoodShape.Rice: Rice(parent, random); break;
                case FoodShape.BreadRoll: BreadRoll(parent); break;
                case FoodShape.GrilledChicken: GrilledChicken(parent); break;
                case FoodShape.Beans: Beans(parent, random); break;
                case FoodShape.Milk: Milk(parent); break;
                case FoodShape.CakeSlice: CakeSlice(parent); break;
                case FoodShape.Soda: Soda(parent); break;
                case FoodShape.Fries: Fries(parent, random); break;
                default: Simple(food, parent); break;
            }
        }

        // ---------- Foods ----------

        static void Broccoli(Transform parent, System.Random random)
        {
            var florets = new[] { new Vector3(-0.03f, 0f, 0.012f), new Vector3(0.035f, 0f, -0.008f), new Vector3(0.002f, 0f, -0.035f) };
            foreach (var at in florets)
            {
                var floret = Group("Floret", parent, at, new Vector3(Range(random, -12, 12), Range(random, 0, 360), Range(random, -12, 12)));
                Part(PrimitiveType.Cylinder, floret, new Vector3(0f, 0.03f, 0f), new Vector3(0.022f, 0.03f, 0.022f), new Color(0.58f, 0.75f, 0.38f), 0.1f);
                Part(PrimitiveType.Sphere, floret, new Vector3(0f, 0.078f, 0f), Vector3.one * 0.05f, DarkGreen, 0.1f);
                for (int i = 0; i < 5; i++)
                {
                    float angle = i * Mathf.PI * 2f / 5f;
                    var offset = new Vector3(Mathf.Cos(angle) * 0.022f, 0.068f, Mathf.Sin(angle) * 0.022f);
                    var shade = i % 2 == 0 ? new Color(0.18f, 0.50f, 0.20f) : DarkGreen;
                    Part(PrimitiveType.Sphere, floret, offset, Vector3.one * 0.038f, shade, 0.1f);
                }
            }
        }

        static void Salad(Transform parent, System.Random random)
        {
            Bowl(parent, 0.085f, 0.045f, BowlTeal);
            var greens = new[] { LeafGreen, new Color(0.52f, 0.80f, 0.32f), new Color(0.28f, 0.58f, 0.22f) };
            for (int i = 0; i < 10; i++)
            {
                var p = InDisk(random, 0.05f);
                var leaf = Part(PrimitiveType.Sphere, parent, new Vector3(p.x, Range(random, 0.034f, 0.052f), p.y),
                    new Vector3(0.055f, 0.012f, 0.035f), greens[i % greens.Length], 0.15f);
                leaf.localRotation = Quaternion.Euler(Range(random, -25, 25), Range(random, 0, 360), Range(random, -25, 25));
            }
            for (int i = 0; i < 3; i++)
            {
                float angle = i * 2.1f + 0.4f;
                Part(PrimitiveType.Sphere, parent, new Vector3(Mathf.Cos(angle) * 0.035f, 0.056f, Mathf.Sin(angle) * 0.035f),
                    Vector3.one * 0.022f, new Color(0.90f, 0.18f, 0.14f), 0.6f);
            }
            for (int i = 0; i < 2; i++)
            {
                var slice = Part(PrimitiveType.Cylinder, parent, new Vector3(-0.02f + 0.04f * i, 0.058f, 0.015f - 0.03f * i),
                    new Vector3(0.03f, 0.003f, 0.03f), new Color(0.78f, 0.90f, 0.60f), 0.3f);
                slice.localRotation = Quaternion.Euler(15f, 0f, -10f + 20f * i);
            }
        }

        static void Carrots(Transform parent)
        {
            var cone = CachedMesh("carrot", () => MeshFactory.Lathe(new[]
            {
                new Vector2(0f, 0f), new Vector2(0.013f, 0f), new Vector2(0.013f, 0f),
                new Vector2(0.011f, 0.04f), new Vector2(0f, 0.11f),
            }, 12));
            var orange = new Color(0.98f, 0.52f, 0.10f);
            for (int i = 0; i < 3; i++)
            {
                // The cone grows along +Y; lay it down along +X.
                var stick = Group("Carrot", parent, new Vector3(-0.055f + 0.01f * (i % 2), 0.013f, -0.03f + 0.03f * i), new Vector3(0f, -12f + 12f * i, -90f));
                MeshPart(cone, stick, Vector3.zero, Vector3.one, orange, 0.35f);
                Part(PrimitiveType.Cylinder, stick, new Vector3(0f, -0.006f, 0f), new Vector3(0.012f, 0.006f, 0.012f), new Color(0.35f, 0.62f, 0.22f), 0.1f);
            }
        }

        static void Apple(Transform parent)
        {
            Part(PrimitiveType.Sphere, parent, new Vector3(0f, 0.034f, 0f), new Vector3(0.075f, 0.068f, 0.075f), new Color(0.82f, 0.10f, 0.12f), 0.75f);
            Part(PrimitiveType.Sphere, parent, new Vector3(0.012f, 0.05f, -0.016f), new Vector3(0.04f, 0.03f, 0.04f), new Color(0.90f, 0.30f, 0.18f), 0.75f);
            var stem = Part(PrimitiveType.Cylinder, parent, new Vector3(0f, 0.074f, 0f), new Vector3(0.006f, 0.012f, 0.006f), new Color(0.35f, 0.22f, 0.10f), 0.1f);
            stem.localRotation = Quaternion.Euler(0f, 0f, 12f);
            var leaf = Part(PrimitiveType.Sphere, parent, new Vector3(0.012f, 0.08f, 0f), new Vector3(0.026f, 0.004f, 0.013f), new Color(0.30f, 0.62f, 0.20f), 0.3f);
            leaf.localRotation = Quaternion.Euler(0f, 30f, 20f);
        }

        static void Rice(Transform parent, System.Random random)
        {
            var white = new Color(0.97f, 0.96f, 0.91f);
            // Half sunk into the plate, so it reads as a mound.
            Part(PrimitiveType.Sphere, parent, new Vector3(0f, 0.012f, 0f), new Vector3(0.13f, 0.06f, 0.12f), white, 0.05f);
            for (int i = 0; i < 22; i++)
            {
                float angle = Range(random, 0f, Mathf.PI * 2f);
                float up = Range(random, 0.25f, 1f);
                float flat = Mathf.Sqrt(1f - up * up);
                var grain = Part(PrimitiveType.Capsule, parent,
                    new Vector3(Mathf.Cos(angle) * flat * 0.064f, 0.012f + up * 0.03f, Mathf.Sin(angle) * flat * 0.059f),
                    new Vector3(0.006f, 0.007f, 0.006f), i % 3 == 0 ? new Color(1f, 1f, 0.97f) : new Color(0.92f, 0.90f, 0.84f), 0.2f);
                grain.localRotation = Quaternion.Euler(Range(random, 0, 180), Range(random, 0, 180), 90f);
            }
            Part(PrimitiveType.Sphere, parent, new Vector3(0.02f, 0.044f, -0.01f), new Vector3(0.012f, 0.004f, 0.008f), new Color(0.30f, 0.60f, 0.22f), 0.2f);
        }

        static void BreadRoll(Transform parent)
        {
            Part(PrimitiveType.Sphere, parent, new Vector3(0f, 0.026f, 0f), new Vector3(0.10f, 0.06f, 0.075f), new Color(0.80f, 0.52f, 0.22f), 0.3f);
            Part(PrimitiveType.Sphere, parent, new Vector3(0f, 0.04f, 0f), new Vector3(0.085f, 0.035f, 0.06f), new Color(0.86f, 0.60f, 0.28f), 0.3f);
            foreach (var x in new[] { -0.018f, 0.012f })
            {
                var score = Part(PrimitiveType.Capsule, parent, new Vector3(x, 0.054f, 0f), new Vector3(0.008f, 0.022f, 0.008f), new Color(0.96f, 0.84f, 0.60f), 0.1f);
                score.localRotation = Quaternion.Euler(90f, 15f, 0f);
            }
        }

        static void GrilledChicken(Transform parent)
        {
            Part(PrimitiveType.Sphere, parent, new Vector3(0f, 0.014f, 0f), new Vector3(0.13f, 0.036f, 0.085f), new Color(0.86f, 0.60f, 0.32f), 0.35f);
            Part(PrimitiveType.Sphere, parent, new Vector3(-0.012f, 0.02f, 0f), new Vector3(0.09f, 0.03f, 0.06f), new Color(0.80f, 0.52f, 0.26f), 0.35f);
            var marks = Group("Grill Marks", parent, Vector3.zero, new Vector3(0f, 35f, 0f));
            for (int i = 0; i < 4; i++)
            {
                float x = -0.033f + 0.022f * i;
                float length = 0.075f * Mathf.Sqrt(Mathf.Max(0.05f, 1f - (x / 0.06f) * (x / 0.06f)));
                Part(PrimitiveType.Cube, marks, new Vector3(x, 0.031f, 0f), new Vector3(0.007f, 0.004f, length), new Color(0.33f, 0.19f, 0.09f), 0.1f);
            }
            foreach (var at in new[] { new Vector3(0.03f, 0.033f, 0.02f), new Vector3(-0.025f, 0.034f, -0.02f), new Vector3(0.005f, 0.035f, 0.025f) })
                Part(PrimitiveType.Sphere, parent, at, new Vector3(0.008f, 0.003f, 0.006f), new Color(0.25f, 0.55f, 0.18f), 0.2f);
        }

        static void Beans(Transform parent, System.Random random)
        {
            Bowl(parent, 0.068f, 0.036f, Ceramic);
            Part(PrimitiveType.Cylinder, parent, new Vector3(0f, 0.026f, 0f), new Vector3(0.11f, 0.002f, 0.11f), new Color(0.48f, 0.20f, 0.12f), 0.5f);
            var colors = new[] { new Color(0.55f, 0.18f, 0.14f), new Color(0.65f, 0.25f, 0.17f), new Color(0.45f, 0.14f, 0.10f) };
            for (int i = 0; i < 16; i++)
            {
                var p = InDisk(random, 0.042f);
                var bean = Part(PrimitiveType.Sphere, parent, new Vector3(p.x, Range(random, 0.029f, 0.034f), p.y),
                    new Vector3(0.018f, 0.010f, 0.011f), colors[i % colors.Length], 0.55f);
                bean.localRotation = Quaternion.Euler(0f, Range(random, 0, 360), Range(random, -15, 15));
            }
        }

        static void Milk(Transform parent)
        {
            var white = new Color(0.98f, 0.98f, 0.96f);
            Part(PrimitiveType.Cylinder, parent, new Vector3(0f, 0.002f, 0f), new Vector3(0.064f, 0.002f, 0.064f), new Color(0.72f, 0.82f, 0.90f), 0.9f);
            Part(PrimitiveType.Cylinder, parent, new Vector3(0f, 0.045f, 0f), new Vector3(0.058f, 0.043f, 0.058f), white, 0.25f);
            var rim = CachedMesh("glass rim", () => MeshFactory.Lathe(new[]
            {
                new Vector2(0.0295f, 0.086f), new Vector2(0.031f, 0.104f), new Vector2(0.031f, 0.104f),
                new Vector2(0.0285f, 0.104f), new Vector2(0.0285f, 0.104f), new Vector2(0.027f, 0.086f),
            }));
            MeshPart(rim, parent, Vector3.zero, Vector3.one, new Color(0.72f, 0.82f, 0.90f), 0.9f);
        }

        static void CakeSlice(Transform parent)
        {
            var sponge = new Color(0.96f, 0.84f, 0.58f);
            var cream = new Color(1f, 0.94f, 0.95f);
            var frosting = new Color(0.95f, 0.55f, 0.70f);
            // Tip toward the viewer and to one side; the slice is centered on the origin.
            var slice = Group("Slice", parent, Vector3.zero, new Vector3(0f, 230f, 0f));
            var offset = new Vector3(-0.055f, 0f, 0f);
            var layers = new[] { (0f, 0.022f, sponge), (0.022f, 0.030f, cream), (0.030f, 0.052f, sponge), (0.052f, 0.062f, frosting) };
            foreach (var (bottom, top, color) in layers)
            {
                var mesh = CachedMesh($"wedge {bottom}", () => MeshFactory.Wedge(0.11f, 42f, bottom, top));
                MeshPart(mesh, slice, offset, Vector3.one, color, color == frosting ? 0.45f : 0.1f);
            }
            Part(PrimitiveType.Sphere, slice, offset + new Vector3(0.08f, 0.072f, 0f), Vector3.one * 0.022f, new Color(0.80f, 0.05f, 0.12f), 0.85f);
            for (int i = 0; i < 3; i++)
                Part(PrimitiveType.Sphere, slice, offset + new Vector3(0.1f, 0.064f, -0.025f + 0.025f * i), Vector3.one * 0.014f, cream, 0.3f);
        }

        static void Soda(Transform parent)
        {
            float RadiusAt(float y) => 0.03f + 0.013f * (y / 0.15f);
            var red = new Color(0.85f, 0.12f, 0.13f);
            var cup = CachedMesh("soda cup", () => MeshFactory.Lathe(new[]
            {
                new Vector2(0f, 0f), new Vector2(0.03f, 0f), new Vector2(0.03f, 0f),
                new Vector2(RadiusAt(0.15f), 0.15f), new Vector2(RadiusAt(0.15f), 0.15f), new Vector2(0f, 0.15f),
            }));
            MeshPart(cup, parent, Vector3.zero, Vector3.one, red, 0.35f);
            var band = CachedMesh("soda band", () => MeshFactory.Lathe(new[]
            {
                new Vector2(RadiusAt(0.06f) + 0.0015f, 0.06f), new Vector2(RadiusAt(0.09f) + 0.0015f, 0.09f),
            }));
            MeshPart(band, parent, Vector3.zero, Vector3.one, Color.white, 0.35f);
            Part(PrimitiveType.Cylinder, parent, new Vector3(0f, 0.152f, 0f), new Vector3(0.092f, 0.003f, 0.092f), new Color(0.90f, 0.90f, 0.92f), 0.5f);
            Part(PrimitiveType.Sphere, parent, new Vector3(0f, 0.154f, 0f), new Vector3(0.07f, 0.014f, 0.07f), new Color(0.90f, 0.90f, 0.92f), 0.5f);
            var straw = Part(PrimitiveType.Cylinder, parent, new Vector3(0.008f, 0.19f, 0f), new Vector3(0.008f, 0.045f, 0.008f), new Color(0.98f, 0.82f, 0.20f), 0.4f);
            straw.localRotation = Quaternion.Euler(0f, 0f, -12f);
        }

        static void Fries(Transform parent, System.Random random)
        {
            // Four segments with flat shading give a square, tapered carton.
            var carton = CachedMesh("fries carton", () => MeshFactory.Lathe(new[]
            {
                new Vector2(0f, 0f), new Vector2(0.045f, 0f), new Vector2(0.045f, 0f),
                new Vector2(0.062f, 0.075f), new Vector2(0.062f, 0.075f),
                new Vector2(0.057f, 0.075f), new Vector2(0.057f, 0.075f),
                new Vector2(0.04f, 0.008f), new Vector2(0.04f, 0.008f), new Vector2(0f, 0.008f),
            }, 4, flat: true, startAngle: Mathf.PI / 4f));
            MeshPart(carton, parent, Vector3.zero, Vector3.one, new Color(0.86f, 0.15f, 0.13f), 0.3f);
            var yellow = new[] { new Color(0.98f, 0.82f, 0.30f), new Color(0.95f, 0.74f, 0.24f), new Color(1f, 0.88f, 0.42f) };
            for (int i = 0; i < 14; i++)
            {
                var fry = Part(PrimitiveType.Cube, parent,
                    new Vector3(Range(random, -0.026f, 0.026f), Range(random, 0.065f, 0.08f), Range(random, -0.026f, 0.026f)),
                    new Vector3(0.011f, Range(random, 0.07f, 0.09f), 0.011f), yellow[i % yellow.Length], 0.15f);
                fry.localRotation = Quaternion.Euler(Range(random, -14, 14), Range(random, 0, 90), Range(random, -14, 14));
            }
        }

        /// <summary>Fallback for foods without a model: a colored shape by food group.</summary>
        static void Simple(FoodItemData food, Transform parent)
        {
            var type = food.isDrink ? PrimitiveType.Cylinder
                : food.foodGroup == FoodGroup.Vegetable || food.foodGroup == FoodGroup.Fruit ? PrimitiveType.Sphere
                : PrimitiveType.Cube;
            float size = 0.09f + 0.03f * food.portionUnits;
            var scale = food.isDrink ? new Vector3(0.08f, 0.04f + 0.02f * food.portionUnits, 0.08f)
                : type == PrimitiveType.Cube ? new Vector3(size, size * 0.5f, size)
                : Vector3.one * size;
            float height = type == PrimitiveType.Cylinder ? scale.y * 2f : scale.y; // cylinders are 2 units tall
            Part(type, parent, new Vector3(0f, height / 2f, 0f), scale, food.color, 0.2f);
        }

        // ---------- Helpers ----------

        static void Bowl(Transform parent, float radius, float height, Color color)
        {
            var mesh = CachedMesh($"bowl {radius} {height}", () => MeshFactory.Lathe(new[]
            {
                new Vector2(0f, 0f), new Vector2(radius * 0.55f, 0f), new Vector2(radius * 0.55f, 0f),
                new Vector2(radius * 0.9f, height * 0.55f), new Vector2(radius, height), new Vector2(radius, height),
                new Vector2(radius - 0.005f, height), new Vector2(radius - 0.005f, height),
                new Vector2(radius * 0.82f, height * 0.5f), new Vector2(radius * 0.5f, 0.006f), new Vector2(0f, 0.006f),
            }, 28));
            MeshPart(mesh, parent, Vector3.zero, Vector3.one, color, 0.6f);
        }

        static Transform Group(string name, Transform parent, Vector3 position, Vector3 euler)
        {
            var group = new GameObject(name).transform;
            group.SetParent(parent, false);
            group.localPosition = position;
            group.localRotation = Quaternion.Euler(euler);
            return group;
        }

        static Transform Part(PrimitiveType type, Transform parent, Vector3 position, Vector3 scale, Color color, float gloss)
        {
            var go = GameObject.CreatePrimitive(type);
            if (template == null) template = go.GetComponent<Renderer>().sharedMaterial;
            // The food root has one collider for the whole model.
            if (Application.isPlaying) Object.Destroy(go.GetComponent<Collider>());
            else Object.DestroyImmediate(go.GetComponent<Collider>());
            go.transform.SetParent(parent, false);
            go.transform.localPosition = position;
            go.transform.localScale = scale;
            go.GetComponent<Renderer>().sharedMaterial = MaterialFor(color, gloss);
            return go.transform;
        }

        static Transform MeshPart(Mesh mesh, Transform parent, Vector3 position, Vector3 scale, Color color, float gloss)
        {
            var go = new GameObject(mesh.name, typeof(MeshFilter), typeof(MeshRenderer));
            go.transform.SetParent(parent, false);
            go.transform.localPosition = position;
            go.transform.localScale = scale;
            go.GetComponent<MeshFilter>().sharedMesh = mesh;
            go.GetComponent<MeshRenderer>().sharedMaterial = MaterialFor(color, gloss);
            return go.transform;
        }

        static Material MaterialFor(Color color, float gloss)
        {
            if (Materials.TryGetValue((color, gloss), out var material) && material != null) return material;
            if (template == null)
            {
                var probe = GameObject.CreatePrimitive(PrimitiveType.Cube);
                template = probe.GetComponent<Renderer>().sharedMaterial;
                Object.DestroyImmediate(probe);
            }
            material = new Material(template) { color = color };
            material.SetFloat("_Glossiness", gloss);
            Materials[(color, gloss)] = material;
            return material;
        }

        static Mesh CachedMesh(string key, System.Func<Mesh> build)
        {
            if (!Meshes.TryGetValue(key, out var mesh) || mesh == null) Meshes[key] = mesh = build();
            return mesh;
        }

        static int StableHash(string text)
        {
            unchecked
            {
                int hash = (int)2166136261;
                foreach (char c in text) hash = (hash ^ c) * 16777619;
                return hash;
            }
        }

        static float Range(System.Random random, float min, float max) => min + (float)random.NextDouble() * (max - min);

        static Vector2 InDisk(System.Random random, float radius)
        {
            float angle = Range(random, 0f, Mathf.PI * 2f);
            float r = radius * Mathf.Sqrt((float)random.NextDouble());
            return new Vector2(Mathf.Cos(angle) * r, Mathf.Sin(angle) * r);
        }
    }
}
