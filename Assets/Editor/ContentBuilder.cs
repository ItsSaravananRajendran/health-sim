using System.Collections.Generic;
using System.IO;
using PlateCheck.Core;
using PlateCheck.Data;
using UnityEditor;
using UnityEngine;

namespace PlateCheck.EditorTools
{
    /// <summary>
    /// Creates the baseline content from docs/01-design.md as ScriptableObject assets.
    /// Assets that already exist are left alone, so edits made in the Inspector are kept.
    /// Batch mode: -executeMethod PlateCheck.EditorTools.ContentBuilder.BuildAll
    /// </summary>
    public static class ContentBuilder
    {
        public const string Root = "Assets/Content";
        const string FoodsDir = Root + "/Foods";
        const string PlatesDir = Root + "/Plates";
        const string OptionsDir = Root + "/Counseling";

        [MenuItem("Plate Check/Create Baseline Content")]
        public static void BuildAll()
        {
            Directory.CreateDirectory(FoodsDir);
            Directory.CreateDirectory(PlatesDir);
            Directory.CreateDirectory(OptionsDir);

            var foods = new Dictionary<string, FoodItemData>();
            void Food(string name, FoodGroup group, int units, Color color, bool drink = false) =>
                foods[name] = GetOrCreate<FoodItemData>($"{FoodsDir}/{name.Replace(" ", "")}.asset", f =>
                {
                    f.displayName = name;
                    f.foodGroup = group;
                    f.portionUnits = units;
                    f.color = color;
                    f.isDrink = drink;
                });

            Food("Broccoli", FoodGroup.Vegetable, 2, new Color(0.18f, 0.55f, 0.20f));
            Food("Salad", FoodGroup.Vegetable, 2, new Color(0.55f, 0.82f, 0.32f));
            Food("Carrots", FoodGroup.Vegetable, 1, new Color(0.98f, 0.55f, 0.12f));
            Food("Apple", FoodGroup.Fruit, 1, new Color(0.85f, 0.15f, 0.15f));
            Food("Rice", FoodGroup.Grain, 2, new Color(0.96f, 0.94f, 0.86f));
            Food("Bread roll", FoodGroup.Grain, 1, new Color(0.80f, 0.60f, 0.35f));
            Food("Grilled chicken", FoodGroup.Protein, 2, new Color(0.70f, 0.45f, 0.25f));
            Food("Beans", FoodGroup.Protein, 1, new Color(0.50f, 0.20f, 0.15f));
            Food("Milk", FoodGroup.Dairy, 1, new Color(0.97f, 0.97f, 1.00f), drink: true);
            Food("Cake slice", FoodGroup.Sweet, 2, new Color(0.95f, 0.60f, 0.75f));
            Food("Large soda", FoodGroup.Sweet, 2, new Color(0.30f, 0.15f, 0.10f), drink: true);
            Food("Fries", FoodGroup.Fried, 2, new Color(0.95f, 0.80f, 0.20f));

            Plate(foods, "A", "Plate A: Mostly grains",
                plate: new[] { "Rice", "Bread roll", "Carrots", "Beans" },
                tray: new[] { "Broccoli", "Salad", "Apple", "Grilled chicken" },
                clientLine: "It's been a busy week, so it's mostly rice and bread. It fills me up and it's cheap.",
                teachingPoint: "When grains crowd the plate, keep them to about a quarter and fill half with " +
                               "vegetables and fruit. Start from foods the client already likes and can afford.",
                new Option(Quality.Neutral,
                    "MyPlate guidance says half of the plate should be vegetables and fruit.",
                    "Okay... I guess I've heard that before.",
                    "Accurate, but it is a lecture. It does not connect to the client's situation or ask what they could change."),
                new Option(Quality.Good,
                    "Rice and bread are filling and affordable. Would you be open to swapping the roll for a vegetable you already like?",
                    "Hmm, I do like broccoli. I could try that.",
                    "Acknowledges the client's reasons (cost, feeling full), then invites one small, specific change and asks permission."),
                new Option(Quality.Judgmental,
                    "This is way too many carbs. You really need to eat better than this.",
                    "I'm doing the best I can with what I have.",
                    "Labels the meal as bad and puts the blame on the client. Shame tends to end the conversation rather than change habits."));

            Plate(foods, "B", "Plate B: No protein, big dessert",
                plate: new[] { "Salad", "Apple", "Rice", "Cake slice" },
                tray: new[] { "Grilled chicken", "Beans", "Carrots", "Broccoli" },
                clientLine: "I skipped the meat today, but I saved room for a big slice of cake.",
                teachingPoint: "A quarter of the plate should be protein, such as beans, chicken, fish or eggs. " +
                               "Sweets are fine sometimes in small portions, not in place of a food group.",
                new Option(Quality.Good,
                    "It sounds like you enjoy a sweet treat. What if you kept a smaller piece for later and added beans or chicken so the meal keeps you full longer?",
                    "A smaller piece later works. I didn't think about protein keeping me full.",
                    "Respects that the client enjoys dessert, suggests a trade instead of a ban, and links protein to something the client cares about."),
                new Option(Quality.Judgmental,
                    "That much cake is really unhealthy. You should know better.",
                    "I don't think I want to talk about my food anymore.",
                    "Moralizes about the client's choice. Feeling judged makes people less likely to be honest or to come back."),
                new Option(Quality.Neutral,
                    "Try to include a protein food with each meal.",
                    "Sure, I'll keep that in mind.",
                    "Correct but generic. It ignores the dessert and gives the client nothing concrete to try."));

            Plate(foods, "C", "Plate C: Already balanced",
                plate: new[] { "Broccoli", "Salad", "Rice", "Grilled chicken" },
                tray: new[] { "Bread roll", "Cake slice", "Apple" },
                clientLine: "I've been trying to follow what we talked about last time. Is this okay?",
                teachingPoint: "Not every plate needs fixing. When a plate already matches the guide (half vegetables " +
                               "and fruit, a quarter grains, a quarter protein), say so and build on it.",
                new Option(Quality.Judgmental,
                    "It's okay, but I'm sure you don't eat like this every day.",
                    "Why would you assume that?",
                    "Doubts the client's honesty. Suspicion damages trust even when the plate is good."),
                new Option(Quality.Neutral,
                    "It's fine. Maybe cut back on the rice a little.",
                    "Oh. I thought I had it right.",
                    "Suggests a change the plate does not need. Over-correcting a balanced plate can discourage a client who is doing well."),
                new Option(Quality.Good,
                    "This looks well balanced: vegetables, rice and chicken in good proportions. What helped you put it together?",
                    "My daughter helped me plan the shopping. That's good to hear, thanks!",
                    "Affirms what is working and asks what helped, which reinforces the habit. Nothing needed to change."));

            Plate(foods, "D", "Plate D: Sugary drink and fries",
                plate: new[] { "Salad", "Broccoli", "Grilled chicken", "Fries", "Large soda" },
                tray: new[] { "Rice", "Milk", "Apple" },
                clientLine: "I always get fries and a big soda with lunch. It's just what I'm used to.",
                teachingPoint: "Large sugary drinks and fried sides add a lot without filling a food group. Swapping " +
                               "one at a time (water or milk, a grain or vegetable side) is easier than changing everything at once.",
                new Option(Quality.Neutral,
                    "Sugary drinks and fried foods are linked to health problems.",
                    "Yeah, everyone says that.",
                    "True, but it is a warning, not a plan. The client hears nagging and has no next step."),
                new Option(Quality.Judgmental,
                    "Soda and fries every day? No wonder you're having health problems.",
                    "Wow. Okay.",
                    "Treats the client's health as a personal failing. Blame makes people defensive and less likely to change."),
                new Option(Quality.Good,
                    "Lots of people have that habit, and you already have salad and chicken here. Would you try milk or water instead of soda, and rice instead of fries, a few days a week?",
                    "Milk a few days a week sounds doable. I'll try it.",
                    "Normalizes the habit, points out what is already good, and offers specific, gradual swaps the client can choose."));

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            Debug.Log("[PlateCheck] Baseline content ready in " + Root);
        }

        readonly struct Option
        {
            public readonly Quality quality;
            public readonly string text, reaction, explanation;

            public Option(Quality quality, string text, string reaction, string explanation)
            {
                this.quality = quality;
                this.text = text;
                this.reaction = reaction;
                this.explanation = explanation;
            }
        }

        static void Plate(Dictionary<string, FoodItemData> foods, string key, string title,
            string[] plate, string[] tray, string clientLine, string teachingPoint, params Option[] options)
        {
            var optionAssets = new List<CounselingOptionData>();
            foreach (var option in options)
            {
                optionAssets.Add(GetOrCreate<CounselingOptionData>($"{OptionsDir}/{key}_{option.quality}.asset", o =>
                {
                    o.text = option.text;
                    o.quality = option.quality;
                    o.clientReaction = option.reaction;
                    o.explanation = option.explanation;
                }));
            }

            GetOrCreate<PlateVariantData>($"{PlatesDir}/Plate{key}.asset", p =>
            {
                p.title = title;
                p.plateItems = Lookup(foods, plate);
                p.trayItems = Lookup(foods, tray);
                p.clientLine = clientLine;
                p.counselingOptions = optionAssets;
                p.teachingPoint = teachingPoint;
            });
        }

        static List<FoodItemData> Lookup(Dictionary<string, FoodItemData> foods, string[] names)
        {
            var list = new List<FoodItemData>();
            foreach (var name in names) list.Add(foods[name]);
            return list;
        }

        static T GetOrCreate<T>(string path, System.Action<T> fill) where T : ScriptableObject
        {
            var existing = AssetDatabase.LoadAssetAtPath<T>(path);
            if (existing != null) return existing;
            var asset = ScriptableObject.CreateInstance<T>();
            fill(asset);
            AssetDatabase.CreateAsset(asset, path);
            return asset;
        }

        /// <summary>The plate variants in play order (PlateA, PlateB, ...).</summary>
        public static List<PlateVariantData> LoadVariants()
        {
            var variants = new List<PlateVariantData>();
            foreach (var guid in AssetDatabase.FindAssets("t:PlateVariantData", new[] { Root }))
                variants.Add(AssetDatabase.LoadAssetAtPath<PlateVariantData>(AssetDatabase.GUIDToAssetPath(guid)));
            variants.Sort((a, b) => string.CompareOrdinal(a.name, b.name));
            return variants;
        }
    }
}
