using System.IO;
using PlateCheck.Interaction;
using PlateCheck.UI;
using TMPro;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem.UI;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace PlateCheck.EditorTools
{
    /// <summary>
    /// Generates Assets/Scenes/Clinic.unity from primitives and wires every reference, so the
    /// scene can be rebuilt from code. Rebuilding overwrites manual changes to the scene; content
    /// (foods, plates, options) lives in Assets/Content and is not touched.
    /// Batch mode: -executeMethod PlateCheck.EditorTools.SceneBuilder.BuildAll
    /// </summary>
    public static class SceneBuilder
    {
        public const string ScenePath = "Assets/Scenes/Clinic.unity";
        const string MaterialsDir = "Assets/Art/Materials";
        const string TmpEssentials = "Packages/com.unity.ugui/Package Resources/TMP Essential Resources.unitypackage";
        const float TableTop = 0.80f;
        const float SurfaceTop = 0.82f; // plate and tray top, where food sits

        static readonly Color PanelColor = new Color(0.10f, 0.12f, 0.16f, 0.88f);
        static readonly Color ButtonColor = new Color(0.20f, 0.45f, 0.75f);
        static readonly Color BubbleColor = new Color(1f, 1f, 1f, 0.95f);
        static readonly Color DarkText = new Color(0.10f, 0.10f, 0.14f);

        /// <summary>Everything needed for a fresh checkout: TMP resources, content, scene, settings.</summary>
        public static void BuildAll()
        {
            ImportTmpEssentials();
            ContentBuilder.BuildAll();
            BuildClinicScene();
            ConfigureProject(); // after the scene exists, so build settings record its GUID
        }

        /// <summary>
        /// The TMP resources are committed under Assets/TextMesh Pro. Package import is
        /// asynchronous, so batch mode would quit before it finished; fail loudly instead.
        /// </summary>
        public static void ImportTmpEssentials()
        {
            if (File.Exists("Assets/TextMesh Pro/Resources/TMP Settings.asset")) return;
            throw new FileNotFoundException(
                "TextMeshPro resources are missing. Use Window > TextMeshPro > Import TMP Essential Resources, " +
                "or import " + TmpEssentials);
        }

        [MenuItem("Plate Check/Configure Project Settings")]
        public static void ConfigureProject()
        {
            PlayerSettings.companyName = "Plate Check";
            PlayerSettings.productName = "Plate Check";
            // python3 -m http.server sends no Content-Encoding header, so compressed builds fail there.
            PlayerSettings.WebGL.compressionFormat = WebGLCompressionFormat.Disabled;
            PlayerSettings.WebGL.decompressionFallback = true;
            EditorBuildSettings.scenes = new[] { new EditorBuildSettingsScene(AssetDatabase.GUIDFromAssetPath(ScenePath), true) };
            AssetDatabase.SaveAssets();
        }

        [MenuItem("Plate Check/Rebuild Clinic Scene")]
        public static void BuildClinicScene()
        {
            Directory.CreateDirectory(Path.GetDirectoryName(ScenePath));
            Directory.CreateDirectory(MaterialsDir);
            var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);

            var camera = BuildCameraAndLight();
            var room = BuildRoom(out var plateCenter, out var cupSpot, out var traySurface, out var clientBody, out var clientHead);

            var systems = new GameObject("Systems");
            var eventSystem = new GameObject("EventSystem", typeof(EventSystem));
            eventSystem.transform.SetParent(systems.transform);
            eventSystem.AddComponent<InputSystemUIInputModule>().AssignDefaultActions();

            var controller = new GameObject("ScenarioController").AddComponent<ScenarioController>();
            controller.transform.SetParent(systems.transform);
            var drag = new GameObject("DragController").AddComponent<DragController>();
            drag.transform.SetParent(systems.transform);
            var foodParent = new GameObject("Foods").transform;

            var plateArea = plateCenter.parent.gameObject.AddComponent<PlateArea>();
            var trayArea = traySurface.parent.gameObject.AddComponent<TrayArea>();

            var canvas = BuildCanvas();
            BuildUi(canvas, out var intro, out var hud, out var counseling, out var feedback, out var debrief,
                out var inspect, out var totalsRoot, out var totalsText);

            Set(drag, "viewCamera", camera);
            Set(drag, "controller", controller);
            SetFloat(drag, "tableHeight", SurfaceTop);

            Set(plateArea, "plateCenter", plateCenter);
            Set(plateArea, "cupSpot", cupSpot);
            Set(plateArea, "totalsText", totalsText);
            Set(plateArea, "totalsRoot", totalsRoot);
            Set(trayArea, "surface", traySurface);

            Set(feedback, "clientBody", clientBody);
            Set(feedback, "clientHead", clientHead);

            var so = new SerializedObject(controller);
            var variantsProperty = so.FindProperty("variants");
            var variants = ContentBuilder.LoadVariants();
            variantsProperty.arraySize = variants.Count;
            for (int i = 0; i < variants.Count; i++)
                variantsProperty.GetArrayElementAtIndex(i).objectReferenceValue = variants[i];
            so.ApplyModifiedPropertiesWithoutUndo();
            Set(controller, "foodParent", foodParent);
            Set(controller, "plateArea", plateArea);
            Set(controller, "trayArea", trayArea);
            Set(controller, "dragController", drag);
            Set(controller, "inspectPanel", inspect);
            Set(controller, "labelMaterial", LabelMaterial());
            Set(controller, "introScreen", intro);
            Set(controller, "hud", hud);
            Set(controller, "counselingPanel", counseling);
            Set(controller, "feedbackView", feedback);
            Set(controller, "debriefScreen", debrief);

            EditorSceneManager.SaveScene(scene, ScenePath);
            AssetDatabase.SaveAssets();
            Debug.Log("[PlateCheck] Built " + ScenePath);
        }

        // ---------- 3D ----------

        static Camera BuildCameraAndLight()
        {
            var cameraObject = new GameObject("Main Camera") { tag = "MainCamera" };
            var camera = cameraObject.AddComponent<Camera>();
            camera.clearFlags = CameraClearFlags.SolidColor;
            camera.backgroundColor = new Color(0.78f, 0.84f, 0.88f);
            camera.fieldOfView = 52f;
            camera.nearClipPlane = 0.05f;
            cameraObject.AddComponent<AudioListener>();
            cameraObject.AddComponent<PhysicsRaycaster>();
            cameraObject.transform.position = new Vector3(0.22f, 1.62f, -0.85f);
            cameraObject.transform.LookAt(new Vector3(0.22f, 0.86f, 0.7f));

            var light = new GameObject("Directional Light").AddComponent<Light>();
            light.type = LightType.Directional;
            light.intensity = 1.1f;
            light.shadows = LightShadows.Soft;
            light.transform.rotation = Quaternion.Euler(50f, -30f, 0f);
            RenderSettings.ambientMode = UnityEngine.Rendering.AmbientMode.Flat;
            RenderSettings.ambientLight = new Color(0.55f, 0.57f, 0.62f);
            return camera;
        }

        static GameObject BuildRoom(out Transform plateCenter, out Transform cupSpot, out Transform traySurface,
            out Renderer clientBody, out Transform clientHead)
        {
            var room = new GameObject("Clinic Room");
            Box("Floor", room.transform, new Vector3(0f, -0.05f, 1f), new Vector3(8f, 0.1f, 8f), Mat("Floor", new Color(0.55f, 0.60f, 0.65f)));
            var wall = Mat("Wall", new Color(0.93f, 0.91f, 0.85f));
            Box("Back Wall", room.transform, new Vector3(0f, 1.5f, 3f), new Vector3(8f, 3f, 0.1f), wall);
            Box("Left Wall", room.transform, new Vector3(-2.5f, 1.5f, 1f), new Vector3(0.1f, 3f, 8f), wall);
            Box("Right Wall", room.transform, new Vector3(2.9f, 1.5f, 1f), new Vector3(0.1f, 3f, 8f), wall);
            Box("Poster", room.transform, new Vector3(-0.9f, 1.7f, 2.94f), new Vector3(0.9f, 0.6f, 0.02f), Mat("Poster", new Color(0.45f, 0.70f, 0.55f)));

            var wood = Mat("Table", new Color(0.72f, 0.56f, 0.40f));
            Box("Table Top", room.transform, new Vector3(0.2f, TableTop - 0.03f, 0.6f), new Vector3(2.0f, 0.06f, 1.1f), wood);
            foreach (var x in new[] { -0.7f, 1.1f })
            foreach (var z in new[] { 0.15f, 1.05f })
                Box("Table Leg", room.transform, new Vector3(x, (TableTop - 0.06f) / 2f, z), new Vector3(0.06f, TableTop - 0.06f, 0.06f), wood);

            var plateRoot = new GameObject("Plate Area");
            plateRoot.transform.SetParent(room.transform);
            var plate = Shape(PrimitiveType.Cylinder, "Plate", plateRoot.transform, new Vector3(-0.3f, TableTop + 0.01f, 0.55f),
                new Vector3(0.9f, 0.01f, 0.9f), Mat("Plate", Color.white));
            Object.DestroyImmediate(plate.GetComponent<Collider>()); // drops are found by position, not raycast
            var cup = Shape(PrimitiveType.Cylinder, "Cup Spot", plateRoot.transform, new Vector3(0.27f, TableTop + 0.005f, 0.85f),
                new Vector3(0.2f, 0.005f, 0.2f), Mat("Cup Spot", new Color(0.80f, 0.88f, 0.95f)));
            Object.DestroyImmediate(cup.GetComponent<Collider>());
            plateCenter = new GameObject("Plate Center").transform;
            plateCenter.SetParent(plateRoot.transform);
            plateCenter.position = new Vector3(-0.3f, SurfaceTop, 0.55f);
            cupSpot = new GameObject("Cup Spot Center").transform;
            cupSpot.SetParent(plateRoot.transform);
            cupSpot.position = new Vector3(0.27f, TableTop + 0.01f, 0.85f);

            var trayRoot = new GameObject("Tray Area");
            trayRoot.transform.SetParent(room.transform);
            var tray = Box("Tray", trayRoot.transform, new Vector3(0.82f, TableTop + 0.01f, 0.55f), new Vector3(0.68f, 0.02f, 0.8f),
                Mat("Tray", new Color(0.35f, 0.40f, 0.48f)));
            Object.DestroyImmediate(tray.GetComponent<Collider>());
            traySurface = tray.transform;

            var client = new GameObject("Client");
            client.transform.SetParent(room.transform);
            client.transform.position = new Vector3(0.2f, 0f, 1.55f);
            var body = Shape(PrimitiveType.Capsule, "Body", client.transform, Vector3.zero, new Vector3(0.55f, 0.5f, 0.4f),
                Mat("Client Shirt", new Color(0.35f, 0.50f, 0.75f)));
            body.transform.localPosition = new Vector3(0f, 0.72f, 0f); // seated
            var head = Shape(PrimitiveType.Sphere, "Head", client.transform, Vector3.zero, Vector3.one * 0.32f,
                Mat("Client Skin", new Color(0.62f, 0.45f, 0.34f)));
            head.transform.localPosition = new Vector3(0f, 1.37f, 0f);
            clientBody = body.GetComponent<Renderer>();
            clientHead = head.transform;
            return room;
        }

        static GameObject Box(string name, Transform parent, Vector3 position, Vector3 scale, Material material) =>
            Shape(PrimitiveType.Cube, name, parent, position, scale, material);

        static GameObject Shape(PrimitiveType type, string name, Transform parent, Vector3 position, Vector3 scale, Material material)
        {
            var go = GameObject.CreatePrimitive(type);
            go.name = name;
            go.transform.SetParent(parent);
            go.transform.position = position;
            go.transform.localScale = scale;
            go.GetComponent<Renderer>().sharedMaterial = material;
            return go;
        }

        static Material Mat(string name, Color color)
        {
            string path = $"{MaterialsDir}/{name}.mat";
            var material = AssetDatabase.LoadAssetAtPath<Material>(path);
            if (material == null)
            {
                material = new Material(Shader.Find("Standard"));
                AssetDatabase.CreateAsset(material, path);
            }
            material.color = color;
            material.SetFloat("_Glossiness", 0.15f);
            EditorUtility.SetDirty(material);
            return material;
        }

        static Material LabelMaterial()
        {
            string path = $"{MaterialsDir}/Food Label Overlay.mat";
            var material = AssetDatabase.LoadAssetAtPath<Material>(path);
            if (material == null)
            {
                material = new Material(TMP_Settings.defaultFontAsset.material);
                AssetDatabase.CreateAsset(material, path);
            }
            material.shader = Shader.Find("TextMeshPro/Distance Field Overlay");
            material.EnableKeyword(ShaderUtilities.Keyword_Outline);
            material.SetFloat(ShaderUtilities.ID_FaceDilate, 0.15f);
            material.SetFloat(ShaderUtilities.ID_OutlineWidth, 0.3f);
            material.SetColor(ShaderUtilities.ID_OutlineColor, new Color32(20, 20, 30, 255));
            // A soft dark glow behind the letters keeps labels legible on light and dark surfaces.
            material.EnableKeyword(ShaderUtilities.Keyword_Underlay);
            material.SetColor(ShaderUtilities.ID_UnderlayColor, new Color(0f, 0f, 0f, 0.75f));
            material.SetFloat(ShaderUtilities.ID_UnderlayDilate, 0.6f);
            material.SetFloat(ShaderUtilities.ID_UnderlaySoftness, 0.6f);
            EditorUtility.SetDirty(material);
            return material;
        }

        // ---------- UI ----------

        static RectTransform BuildCanvas()
        {
            var canvasObject = new GameObject("Canvas", typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
            canvasObject.GetComponent<Canvas>().renderMode = RenderMode.ScreenSpaceOverlay;
            var scaler = canvasObject.GetComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1280, 720);
            scaler.matchWidthOrHeight = 0.5f;
            return (RectTransform)canvasObject.transform;
        }

        static void BuildUi(RectTransform canvas, out IntroScreen intro, out HudView hud, out CounselingPanel counseling,
            out FeedbackView feedback, out DebriefScreen debrief, out InspectPanel inspect,
            out GameObject totalsRoot, out TMP_Text totalsText)
        {
            // HUD: instruction bar, client speech bubble, Ready button.
            var hudRoot = MakeRect("HUD", canvas, Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero);
            var instructionBar = MakePanel("Instruction", hudRoot, new Vector2(0, 1), new Vector2(1, 1), new Vector2(0, -80), new Vector2(0, -30), PanelColor);
            var instruction = MakeText("Text", instructionBar, "", 19, TextAlignmentOptions.MidlineLeft, Color.white, 20);
            var speech = MakePanel("Client Speech", hudRoot, new Vector2(0.55f, 1), new Vector2(0.97f, 1), new Vector2(0, -172), new Vector2(0, -92), BubbleColor);
            var speechText = MakeText("Text", speech, "", 18, TextAlignmentOptions.Center, DarkText, 14);
            var ready = MakeButton("Ready Button", hudRoot, "Ready to help", new Vector2(1, 0), new Vector2(1, 0), new Vector2(-260, 24), new Vector2(-24, 84));
            hud = hudRoot.gameObject.AddComponent<HudView>();
            Set(hud, "root", hudRoot.gameObject);
            Set(hud, "instruction", instruction);
            Set(hud, "speechRoot", speech.gameObject);
            Set(hud, "speech", speechText);
            hud.readyButton = ready;

            // Live plate totals (shown by PlateArea in Act).
            var totals = MakePanel("Plate Totals", canvas, new Vector2(0, 0.5f), new Vector2(0, 0.5f), new Vector2(16, -30), new Vector2(286, 150), PanelColor);
            totalsText = MakeText("Text", totals, "", 16, TextAlignmentOptions.TopLeft, Color.white, 12);
            totalsRoot = totals.gameObject;

            // Inspect panel.
            var inspectRect = MakePanel("Inspect Panel", canvas, new Vector2(1, 0.5f), new Vector2(1, 0.5f), new Vector2(-286, -10), new Vector2(-16, 150), PanelColor);
            var inspectTitle = MakeText("Title", inspectRect, "", 22, TextAlignmentOptions.TopLeft, Color.white, 12);
            inspectTitle.fontStyle = FontStyles.Bold;
            var inspectDetails = MakeText("Details", inspectRect, "", 17, TextAlignmentOptions.TopLeft, Color.white, 12);
            inspectDetails.rectTransform.offsetMax = new Vector2(-12, -46);
            inspect = inspectRect.gameObject.AddComponent<InspectPanel>();
            Set(inspect, "root", inspectRect.gameObject);
            Set(inspect, "title", inspectTitle);
            Set(inspect, "details", inspectDetails);

            // Counseling options.
            var counselRect = MakePanel("Counseling Panel", canvas, new Vector2(0, 0), new Vector2(1, 0), new Vector2(0, 0), new Vector2(0, 168), PanelColor);
            var counselTitle = MakeText("Title", counselRect, "<b>What do you say?</b> (this locks the plate)", 18, TextAlignmentOptions.TopLeft, Color.white, 12);
            counselTitle.rectTransform.offsetMin = new Vector2(16, 0);
            counseling = counselRect.gameObject.AddComponent<CounselingPanel>();
            var buttons = new Button[3];
            var labels = new TMP_Text[3];
            for (int i = 0; i < 3; i++)
            {
                buttons[i] = MakeButton("Option " + (i + 1), counselRect, "", new Vector2(i / 3f, 0), new Vector2((i + 1) / 3f, 1),
                    new Vector2(12, 12), new Vector2(-12, -42));
                labels[i] = buttons[i].GetComponentInChildren<TMP_Text>();
                labels[i].fontSize = 16;
                labels[i].enableAutoSizing = true;
                labels[i].fontSizeMin = 12;
                labels[i].fontSizeMax = 17;
            }
            Set(counseling, "root", counselRect.gameObject);
            SetArray(counseling, "optionButtons", buttons);
            SetArray(counseling, "optionLabels", labels);

            // Feedback: client reaction bubble and Continue.
            var feedbackRoot = MakeRect("Feedback", canvas, Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero);
            var reactionBubble = MakePanel("Reaction", feedbackRoot, new Vector2(0.55f, 1), new Vector2(0.97f, 1), new Vector2(0, -172), new Vector2(0, -92), BubbleColor);
            var reaction = MakeText("Text", reactionBubble, "", 20, TextAlignmentOptions.Center, DarkText, 14);
            var continueButton = MakeButton("Continue Button", feedbackRoot, "Continue", new Vector2(0.5f, 0), new Vector2(0.5f, 0), new Vector2(-120, 30), new Vector2(120, 90));
            feedback = feedbackRoot.gameObject.AddComponent<FeedbackView>();
            Set(feedback, "root", feedbackRoot.gameObject);
            Set(feedback, "reaction", reaction);
            feedback.continueButton = continueButton;

            // Demo banner, always visible.
            var banner = MakePanel("Demo Banner", canvas, new Vector2(0, 1), new Vector2(1, 1), new Vector2(0, -30), Vector2.zero, new Color(0.55f, 0.15f, 0.15f, 0.95f));
            MakeText("Text", banner, "Demo content only. Not clinical guidance. Portions follow the public USDA MyPlate idea.", 15, TextAlignmentOptions.Center, Color.white, 4);

            // Intro screen.
            var introRoot = MakePanel("Intro Screen", canvas, Vector2.zero, Vector2.one, Vector2.zero, new Vector2(0, -30), new Color(0.08f, 0.10f, 0.14f, 0.92f));
            var introCard = MakePanel("Card", introRoot, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(-380, -210), new Vector2(380, 210), new Color(0.16f, 0.19f, 0.25f));
            var introTitle = MakeText("Title", introCard, "Plate Check", 40, TextAlignmentOptions.Top, Color.white, 24);
            introTitle.fontStyle = FontStyles.Bold;
            var introBody = MakeText("Body", introCard,
                "You are a community health worker. A client has brought in a typical meal.\n\n" +
                "<b>Assess:</b> click each food on the plate to see its food group and portion.\n" +
                "<b>Act:</b> drag foods between the plate and the side tray to balance it, then choose what to say.\n\n" +
                "A balanced plate is about half vegetables and fruit, a quarter grains and a quarter protein.",
                19, TextAlignmentOptions.TopLeft, Color.white, 28);
            introBody.rectTransform.offsetMax = new Vector2(-28, -90);
            introBody.rectTransform.offsetMin = new Vector2(28, 90);
            var start = MakeButton("Start Button", introCard, "Start", new Vector2(0.5f, 0), new Vector2(0.5f, 0), new Vector2(-110, 24), new Vector2(110, 80));
            intro = introRoot.gameObject.AddComponent<IntroScreen>();
            Set(intro, "root", introRoot.gameObject);
            intro.startButton = start;

            // Debrief screen.
            var debriefRoot = MakePanel("Debrief Screen", canvas, Vector2.zero, Vector2.one, Vector2.zero, new Vector2(0, -30), new Color(0.08f, 0.10f, 0.14f, 0.92f));
            var debriefCard = MakePanel("Card", debriefRoot, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(-470, -330), new Vector2(470, 320), new Color(0.16f, 0.19f, 0.25f));
            var debriefTitle = MakeText("Title", debriefCard, "", 28, TextAlignmentOptions.Top, Color.white, 18);
            debriefTitle.fontStyle = FontStyles.Bold;
            var debriefBody = MakeText("Body", debriefCard, "", 17, TextAlignmentOptions.TopLeft, Color.white, 28);
            debriefBody.rectTransform.offsetMax = new Vector2(-28, -66);
            debriefBody.rectTransform.offsetMin = new Vector2(28, 86);
            debriefBody.enableAutoSizing = true;
            debriefBody.fontSizeMin = 11;
            debriefBody.fontSizeMax = 18;
            var tryAgain = MakeButton("Try Again Button", debriefCard, "Try Again (next plate)", new Vector2(0.5f, 0), new Vector2(0.5f, 0), new Vector2(-250, 20), new Vector2(-10, 76));
            var quit = MakeButton("Quit Button", debriefCard, "Quit", new Vector2(0.5f, 0), new Vector2(0.5f, 0), new Vector2(10, 20), new Vector2(250, 76));
            debrief = debriefRoot.gameObject.AddComponent<DebriefScreen>();
            Set(debrief, "root", debriefRoot.gameObject);
            Set(debrief, "title", debriefTitle);
            Set(debrief, "body", debriefBody);
            debrief.tryAgainButton = tryAgain;
            debrief.quitButton = quit;
        }

        static RectTransform MakeRect(string name, Transform parent, Vector2 anchorMin, Vector2 anchorMax, Vector2 offsetMin, Vector2 offsetMax)
        {
            var go = new GameObject(name, typeof(RectTransform));
            var rect = (RectTransform)go.transform;
            rect.SetParent(parent, false);
            rect.anchorMin = anchorMin;
            rect.anchorMax = anchorMax;
            rect.offsetMin = offsetMin;
            rect.offsetMax = offsetMax;
            return rect;
        }

        static RectTransform MakePanel(string name, Transform parent, Vector2 anchorMin, Vector2 anchorMax, Vector2 offsetMin, Vector2 offsetMax, Color color)
        {
            var rect = MakeRect(name, parent, anchorMin, anchorMax, offsetMin, offsetMax);
            rect.gameObject.AddComponent<Image>().color = color;
            return rect;
        }

        static TextMeshProUGUI MakeText(string name, Transform parent, string text, float size, TextAlignmentOptions alignment, Color color, float padding)
        {
            var rect = MakeRect(name, parent, Vector2.zero, Vector2.one, new Vector2(padding, padding * 0.5f), new Vector2(-padding, -padding * 0.5f));
            var tmp = rect.gameObject.AddComponent<TextMeshProUGUI>();
            tmp.text = text;
            tmp.fontSize = size;
            tmp.alignment = alignment;
            tmp.color = color;
            tmp.textWrappingMode = TextWrappingModes.Normal;
            tmp.raycastTarget = false;
            return tmp;
        }

        static Button MakeButton(string name, Transform parent, string label, Vector2 anchorMin, Vector2 anchorMax, Vector2 offsetMin, Vector2 offsetMax)
        {
            var rect = MakePanel(name, parent, anchorMin, anchorMax, offsetMin, offsetMax, ButtonColor);
            var button = rect.gameObject.AddComponent<Button>();
            var colors = button.colors;
            colors.highlightedColor = new Color(0.85f, 0.92f, 1f);
            colors.pressedColor = new Color(0.7f, 0.8f, 0.9f);
            button.colors = colors;
            MakeText("Label", rect, label, 20, TextAlignmentOptions.Center, Color.white, 10);
            return button;
        }

        // ---------- Serialized field helpers ----------

        static void Set(Object target, string field, Object value)
        {
            var so = new SerializedObject(target);
            var property = so.FindProperty(field);
            if (property == null) throw new System.ArgumentException($"{target.GetType().Name} has no field '{field}'");
            property.objectReferenceValue = value;
            so.ApplyModifiedPropertiesWithoutUndo();
        }

        static void SetFloat(Object target, string field, float value)
        {
            var so = new SerializedObject(target);
            so.FindProperty(field).floatValue = value;
            so.ApplyModifiedPropertiesWithoutUndo();
        }

        static void SetArray(Object target, string field, Object[] values)
        {
            var so = new SerializedObject(target);
            var property = so.FindProperty(field);
            property.arraySize = values.Length;
            for (int i = 0; i < values.Length; i++)
                property.GetArrayElementAtIndex(i).objectReferenceValue = values[i];
            so.ApplyModifiedPropertiesWithoutUndo();
        }
    }
}
