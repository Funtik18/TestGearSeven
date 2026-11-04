using TMPro;
using System;
using System.IO;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

namespace RosiesCornerShop.Core.Editor
{
    /// <summary>Authors real, editable Unity prefabs. Only runs on an explicit menu command.</summary>
    public static class ShopPrefabBuilder
    {
        const string Base = "Assets/Game/Assets/Prefabs";
        static ShopAssets assets;
        static TMP_FontAsset font;
        static readonly Color Cream = ShopColors.Cream, Ink = ShopColors.Ink, Wood = ShopColors.Wood,
            Dark = ShopColors.Dark, Berry = ShopColors.Berry, Sage = ShopColors.Sage, Green = ShopColors.Green;

        [MenuItem("Tools/Rosie's Corner Shop/Rebuild prefabs")]
        public static void Rebuild()
        {
            try
            {
                Directory.CreateDirectory(Base + "/Resources/Rosie");
                foreach (string folder in new[] { "Gameplay", "Screens", "Windows", "Common", "Effects" }) Directory.CreateDirectory(Base + "/" + folder);
                AssetDatabase.Refresh();
                font = ShopFontUtility.GetFont();
                const string assetPath = Base + "/Resources/Rosie/ShopAssets.asset";
                assets = AssetDatabase.LoadAssetAtPath<ShopAssets>(assetPath);
                if (assets == null) { assets = ScriptableObject.CreateInstance<ShopAssets>(); AssetDatabase.CreateAsset(assets,assetPath); }
                assets.pixel = LoadSprite("Pixel.jpg"); assets.circle = LoadSprite("T_Circle.png"); assets.triangle = LoadSprite("Triangle.png");
                assets.button = BuildButton(); assets.item = BuildItem(); assets.shelf = BuildShelf(); assets.basket = BuildBasket();
                assets.counter = BuildCounter(); assets.cabinet = BuildCabinet(); assets.toast = BuildToast();
                assets.home = BuildHome(); assets.game = BuildGame(); assets.collection = BuildCollection();
                assets.settingsWindow = BuildWindow("SettingsWindow"); assets.winWindow = BuildWindow("WinWindow");
                assets.loseWindow = BuildWindow("LoseWindow"); assets.levelsWindow = BuildWindow("LevelsWindow",true);
                assets.purchaseWindow = BuildWindow("PurchaseWindow"); assets.tutorialWindow = BuildWindow("TutorialWindow");
                assets.confetti = BuildConfetti(); assets.root = BuildRoot();
                EditorUtility.SetDirty(assets); AssetDatabase.SaveAssets(); AssetDatabase.Refresh();
                ValidateReferences();
                ShopTextMigration.Validate();
                ShopModelValidation.Validate();
                Debug.Log("Rosie: prefab library built and validated at " + Base);
            }
            catch (Exception exception)
            { Debug.LogException(exception); if (Application.isBatchMode) EditorApplication.Exit(1); else throw; }
        }
        static Sprite LoadSprite(string file)
        {
            string path = "Assets/Game/Assets/Images/" + file;
            var importer = AssetImporter.GetAtPath(path) as TextureImporter;
            if (importer != null && (importer.textureType != TextureImporterType.Sprite || importer.spriteImportMode != SpriteImportMode.Single))
            { importer.textureType = TextureImporterType.Sprite; importer.spriteImportMode = SpriteImportMode.Single; importer.SaveAndReimport(); }
            Sprite sprite = AssetDatabase.LoadAssetAtPath<Sprite>(path);
            if (sprite == null) throw new InvalidOperationException("Missing placeholder sprite: " + path);
            return sprite;
        }
        static RectTransform Node(string name, Transform parent, float x, float y, float w, float h)
        {
            var rect = new GameObject(name,typeof(RectTransform)).GetComponent<RectTransform>();
            if (parent != null) rect.SetParent(parent,false);
            rect.anchorMin = rect.anchorMax = new Vector2(.5f,.5f); rect.pivot = new Vector2(.5f,.5f);
            rect.anchoredPosition = new Vector2(x,y); rect.sizeDelta = new Vector2(w,h); return rect;
        }
        static RectTransform At(string name, Transform parent, float x, float y, float w, float h)
        { return Node(name,parent,x + w / 2 - 215,450 - y - h / 2,w,h); }
        static void Stretch(RectTransform r)
        { r.anchorMin = Vector2.zero; r.anchorMax = Vector2.one; r.offsetMin = r.offsetMax = Vector2.zero; }
        static Image Image(RectTransform rect, Color color, Sprite sprite = null, bool raycast = false)
        { var image = rect.gameObject.AddComponent<Image>(); image.sprite = sprite != null ? sprite : assets.pixel; image.color = color; image.raycastTarget = raycast; return image; }
        static Image Picture(string name, Transform parent, float x, float y, float w, float h, Color color, Sprite sprite = null)
        { return Image(Node(name,parent,x,y,w,h),color,sprite); }
        static TextMeshProUGUI Text(string name, Transform parent, float x, float y, float w, float h, string value, int size = 17, Color? color = null)
        {
            var label = Node(name,parent,x,y,w,h).gameObject.AddComponent<TextMeshProUGUI>(); label.font = font; label.text = value;
            label.fontSize = size; label.color = color ?? Ink; label.alignment = TextAlignmentOptions.Center; label.raycastTarget = false;
            label.textWrappingMode = TextWrappingModes.Normal; label.overflowMode = TextOverflowModes.Truncate; label.margin = Vector4.zero; return label;
        }
        static TextMeshProUGUI ScreenText(string name, Transform parent, float x, float y, float w, float h, string value, int size = 17, Color? color = null)
        { return Text(name,parent,x + w / 2 - 215,450 - y - h / 2,w,h,value,size,color); }
        static void Ref(UnityEngine.Object component, string name, UnityEngine.Object value)
        { var serialized = new SerializedObject(component); serialized.FindProperty(name).objectReferenceValue = value; serialized.ApplyModifiedPropertiesWithoutUndo(); }
        static void Refs(UnityEngine.Object component, string name, params UnityEngine.Object[] values)
        {
            var serialized = new SerializedObject(component); var property = serialized.FindProperty(name); property.arraySize = values.Length;
            for (int i = 0; i < values.Length; i++) property.GetArrayElementAtIndex(i).objectReferenceValue = values[i];
            serialized.ApplyModifiedPropertiesWithoutUndo();
        }
        static T Save<T>(RectTransform root, string folder, string name) where T : Component
        {
            var prefab = PrefabUtility.SaveAsPrefabAsset(root.gameObject,Base + "/" + folder + "/" + name + ".prefab");
            UnityEngine.Object.DestroyImmediate(root.gameObject); return prefab.GetComponent<T>();
        }
        static ShopButton Button(Transform parent, string name, float x, float y, float w, float h, string caption, Color color)
        {
            var root = Node(name,parent,x,y,w,h);
            Image(root,new Color(1,1,1,0),null,true);
            Picture("Shadow",root,0,-4,w,h,Dark);
            Image graphic = Picture("Face",root,0,0,w,h,color);
            var button = root.gameObject.AddComponent<Button>(); button.targetGraphic = graphic;
            var colors = button.colors; colors.highlightedColor = new Color(1.07f,1.07f,1.07f); colors.pressedColor = new Color(.9f,.9f,.9f); button.colors = colors;
            var label = Text("Caption",root,0,0,w - 8,h - 4,caption,17,color == Wood ? Ink : Cream); label.fontStyle = FontStyles.Bold;
            var view = root.gameObject.AddComponent<ShopButton>(); Ref(view,"button",button); Ref(view,"caption",label); return view;
        }
        static ShopButton ScreenButton(Transform parent, string name, float x, float y, float w, float h, string caption, Color color)
        { return Button(parent,name,x + w / 2 - 215,450 - y - h / 2,w,h,caption,color); }
        static ShopButton BuildButton()
        { var view = Button(null,"ShopButton",0,0,120,48,"Кнопка",Wood); return Save<ShopButton>((RectTransform)view.transform,"Common","ShopButton"); }
        static ItemView BuildItem()
        {
            var root = Node("Item",null,0,0,48,64); var hit = Image(root,new Color(1,1,1,0),null,true);
            var button = root.gameObject.AddComponent<Button>(); button.targetGraphic = hit; button.transition = Selectable.Transition.None;
            var rear = Picture("RearItem",root,5,8,31,43,Sage); rear.gameObject.SetActive(false);
            Picture("ContactShadow",root,0,-28,43,9,new Color(.2f,.1f,0,.18f),assets.circle);
            var body = Picture("Body",root,0,-2,37,45,Berry);
            var cap = Picture("Lid",root,0,23,31,10,Dark);
            Picture("Shine",root,-12,0,3,29,new Color(1,1,1,.27f));
            var emblem = Picture("Label",root,0,-4,25,21,Cream,assets.circle);
            var label = Text("ProductId",root,0,-4,30,23,"1",12,Berry); label.fontStyle = FontStyles.Bold;
            var bag = Node("PaperBag",root,0,0,42,58); Image(bag,ShopColors.Hex("C8A878")); Text("Unknown",bag,0,0,38,50,"?",26,Cream); bag.gameObject.SetActive(false);
            var view = root.gameObject.AddComponent<ItemView>(); Ref(view,"body",body); Ref(view,"cap",cap); Ref(view,"emblem",emblem); Ref(view,"rear",rear); Ref(view,"label",label); Ref(view,"bag",bag.gameObject); Ref(view,"button",button);
            return Save<ItemView>(root,"Gameplay","Item");
        }
        static ShelfView BuildShelf()
        {
            var root = Node("Shelf",null,0,0,118,56); Image(root,Wood);
            var inside = Node("BackPanel",root,0,0,108,48); Image(inside,ShopColors.Hex("E6D7B9"));
            Picture("Ledge",root,0,-25,118,5,Dark);
            var slots = Node("Slots",root,0,0,118,56); Stretch(slots);
            var template = Node("SlotTemplate",root,0,0,34,48); template.gameObject.SetActive(false);
            var barrier = Picture("Barrier",root,0,0,112,27,Wood); barrier.gameObject.SetActive(false);
            var status = Text("Mechanic",root,0,9,114,21,"",11,Dark); status.fontStyle = FontStyles.Bold;
            var view = root.gameObject.AddComponent<ShelfView>(); Ref(view,"slotsRoot",slots); Ref(view,"slotPrefab",template); Ref(view,"status",status); Ref(view,"barrier",barrier);
            return Save<ShelfView>(root,"Gameplay","Shelf");
        }
        static BasketView BuildBasket()
        {
            var root = Node("Basket",null,0,0,120,140);
            Picture("Handle",root,0,21,104,86,Dark,assets.circle); Picture("HandleHole",root,0,21,90,71,ShopColors.Hex("F4E8D0"),assets.circle);
            var slots = new RectTransform[3]; for (int i = 0; i < 3; i++) slots[i] = Node("Slot" + i,root,-33 + i * 33,19 + (i == 1 ? 7 : 0),30,53);
            Picture("BasketFront",root,0,-10,114,45,Wood);
            for (int i = 0; i < 4; i++) Picture("Wicker",root,0,-26 + i * 11,110,2,new Color(.4f,.2f,.1f,.22f));
            for (int i = 0; i < 5; i++) Picture("WickerVertical",root,-45 + i * 22,-10,2,41,new Color(.4f,.2f,.1f,.16f));
            var napkin = Picture("PackedNapkin",root,0,-4,103,31,Sage); napkin.gameObject.SetActive(false);
            var count = Text("Count",root,0,-12,100,27,"0/3",18,Dark); count.fontStyle = FontStyles.Bold;
            var order = Text("OrderName",root,3,-53,100,33,"Milk",12,Dark);
            var icon = Picture("OrderIcon",root,-52,-52,15,21,Berry,assets.circle);
            var view = root.gameObject.AddComponent<BasketView>(); Refs(view,"slots",slots); Ref(view,"order",order); Ref(view,"count",count); Ref(view,"napkin",napkin); Ref(view,"orderIcon",icon);
            return Save<BasketView>(root,"Gameplay","Basket");
        }
        static RectTransform Screen(string name)
        { return Node(name,null,0,0,430,900); }
        static CounterView BuildCounter()
        {
            var root = Node("Counter",null,0,0,398,101); var surface = Image(root,Wood);
            var slots = new RectTransform[5];
            for (int i = 0; i < 5; i++)
            {
                Picture("Plate" + i,root,-151 + i * 73,-28,64,14,Cream,assets.circle);
                slots[i] = Node("Slot" + i,root,-153 + i * 73,4,54,76);
            }
            var caption = Text("Caption",root,0,-65,370,24,"ПРИЛАВОК · 0/5",12,Dark);
            var view = root.gameObject.AddComponent<CounterView>(); Refs(view,"slots",slots); Ref(view,"surface",surface); Ref(view,"caption",caption);
            return Save<CounterView>(root,"Gameplay","Counter");
        }
        static RectTransform BuildCabinet()
        {
            var root = Node("Cabinet",null,0,0,398,348); Image(root,Wood);
            Picture("Back",root,0,.5f,384,331,ShopColors.Hex("F8ECD2"));
            for (int i = 0; i < 12; i++)
            {
                Color color = i % 2 == 0 ? Berry : Cream;
                Picture("AwningStripe",root,-187 + i * 34,185,34,26,color);
                Picture("AwningScallop",root,-187 + i * 34,171,34,26,color,assets.circle);
            }
            for (int i = 0; i < 3; i++)
            {
                Text("Stock" + i,root,-127 + i * 126,146,118,22,"Запас",12,Dark);
                Node("Column" + i,root,-127 + i * 126,-14.5f,118,289).gameObject.AddComponent<RectMask2D>();
            }
            return Save<RectTransform>(root,"Gameplay","Cabinet");
        }
        static ToastView BuildToast()
        {
            var root = Node("Toast",null,0,0,380,46); Image(root,Dark);
            var group = root.gameObject.AddComponent<CanvasGroup>(); group.alpha = 0; group.blocksRaycasts = false; group.interactable = false;
            var caption = Text("Caption",root,0,0,364,42,"",14,Cream);
            var view = root.gameObject.AddComponent<ToastView>(); Ref(view,"caption",caption); Ref(view,"group",group);
            return Save<ToastView>(root,"Common","Toast");
        }
        static void Awning(Transform parent, float y)
        {
            for (int i = 0; i < 12; i++)
            {
                var color = i % 2 == 0 ? Berry : Cream;
                Image(At("AwningStripe",parent,11 + i * 34,y,34,26),color);
                Image(At("AwningScallop",parent,11 + i * 34,y + 12,34,26),color,assets.circle);
            }
        }
        static HomeView BuildHome()
        {
            var root = Screen("HomeScreen"); Awning(root,28);
            ScreenText("Welcome",root,20,92,390,34,"ДОБРО ПОЖАЛОВАТЬ В",15,Dark);
            Image(At("ShopSign",root,30,145,370,135),Dark);
            var title = ScreenText("Title",root,40,150,350,122,"Rosie's\nCorner Shop",37,Cream); title.fontStyle = FontStyles.Bold;
            ScreenText("Tagline",root,30,300,370,45,"Маленькая лавка — большие заказы",17,Dark);
            Image(At("DisplayShelf",root,28,382,374,155),Wood);
            for (int i = 0; i < 5; i++)
            {
                var item = (ItemView)PrefabUtility.InstantiatePrefab(assets.item,root);
                var holder = At("DisplaySlot",root,42 + i * 70,405,60,105);
                item.Bind(new ItemState { type = i * 7 },assets,null,false); item.Place(holder,holder,false);
            }
            var play = ScreenButton(root,"Play",65,585,300,59,"Играть",Berry);
            var levels = ScreenButton(root,"Levels",65,665,143,48,"Уровни",Wood);
            var collection = ScreenButton(root,"Collection",222,665,143,48,"Коллекция",Wood);
            var settings = ScreenButton(root,"Settings",65,733,300,43,"Настройки",Green);
            var stats = ScreenText("Stats",root,35,792,360,40,"Монеты: 150",17,Dark);
            var view = root.gameObject.AddComponent<HomeView>(); Ref(view,"play",play); Ref(view,"levels",levels); Ref(view,"collection",collection); Ref(view,"settings",settings); Ref(view,"stats",stats);
            return Save<HomeView>(root,"Screens","HomeScreen");
        }
        static GameView BuildGame()
        {
            var root = Screen("GameScreen"); var view = root.gameObject.AddComponent<GameView>();
            Ref(view,"level",ScreenText("Level",root,9,12,70,48,"УРОВЕНЬ\n1",12,Dark));
            var progressBack = Image(At("ProgressTrack",root,82,23,191,27),Dark);
            var fill = Picture("Fill",progressBack.transform,0,0,185,21,Berry); fill.type = UnityEngine.UI.Image.Type.Filled; fill.fillMethod = UnityEngine.UI.Image.FillMethod.Horizontal; fill.fillAmount = 0;
            Ref(view,"progressFill",fill); Ref(view,"progress",ScreenText("Progress",root,82,23,191,27,"0 / 18",15,Cream));
            Ref(view,"coins",ScreenText("Coins",root,280,17,84,37,"150 м.",18,Dark));
            Ref(view,"settings",ScreenButton(root,"Pause",371,17,43,35,"II",Wood));
            Image(At("OrderLedge",root,13,181,404,12),Wood);
            var baskets = new RectTransform[3]; for (int i = 0; i < 3; i++) baskets[i] = At("BasketAnchor" + i,root,20 + i * 134,77,122,140);
            Refs(view,"baskets",baskets);
            Ref(view,"extraBasket",ScreenButton(root,"ExtraBasket",296,113,103,60,"+\n50 монет",Green));
            var counter = (CounterView)PrefabUtility.InstantiatePrefab(assets.counter,root);
            ((RectTransform)counter.transform).anchoredPosition = new Vector2(0,155.5f); Ref(view,"counter",counter);
            var cabinet = (RectTransform)PrefabUtility.InstantiatePrefab(assets.cabinet,root); cabinet.anchoredPosition = new Vector2(0,-127);
            var columns = new RectTransform[3]; var stock = new TextMeshProUGUI[3];
            for (int i = 0; i < 3; i++)
            {
                stock[i] = cabinet.Find("Stock" + i).GetComponent<TextMeshProUGUI>();
                columns[i] = (RectTransform)cabinet.Find("Column" + i);
            }
            Refs(view,"columns",columns); Refs(view,"stock",stock);
            var boosts = new ShopButton[3]; string[] names = { "Собрать", "Убрать 3", "Смешать" };
            for (int i = 0; i < 3; i++) boosts[i] = ScreenButton(root,"Booster" + i,18 + i * 101,772,92,48,names[i],Green);
            Refs(view,"boosters",boosts); Ref(view,"undo",ScreenButton(root,"Undo",321,772,92,48,"Отмена",Wood));
            Ref(view,"instruction",ScreenText("Instruction",root,20,842,390,40,"Берегите место на прилавке",13,Dark));
            var flight = Node("FlyingItems",root,0,0,430,900); Stretch(flight); Ref(view,"itemLayer",flight);
            var effects = Node("Effects",root,0,0,430,900); Stretch(effects); Ref(view,"effects",effects);
            return Save<GameView>(root,"Screens","GameScreen");
        }
        static CollectionView BuildCollection()
        {
            var root = Screen("CollectionScreen"); var view = root.gameObject.AddComponent<CollectionView>();
            ScreenText("Title",root,20,25,390,48,"Коллекция товаров",27,Dark);
            Ref(view,"summary",ScreenText("Summary",root,20,80,390,30,"Найдено 0",17,Dark));
            var viewport = At("Viewport",root,22,125,386,620); Image(viewport,Cream,null,true); viewport.gameObject.AddComponent<RectMask2D>();
            var content = Node("Content",viewport,-183,310,369,1000); content.anchorMin = content.anchorMax = new Vector2(0,1); content.pivot = new Vector2(0,1); content.anchoredPosition = Vector2.zero;
            var scroll = viewport.gameObject.AddComponent<ScrollRect>(); scroll.viewport = viewport; scroll.content = content; scroll.horizontal = false; scroll.vertical = true; scroll.movementType = ScrollRect.MovementType.Clamped;
            Ref(view,"content",content); Ref(view,"back",ScreenButton(root,"Back",80,780,270,48,"В лавку",Berry));
            return Save<CollectionView>(root,"Screens","CollectionScreen");
        }
        static WindowView BuildWindow(string name, bool levels = false)
        {
            var root = Screen(name); Image(root,new Color(.16f,.09f,.04f,.72f),null,true);
            var group = root.gameObject.AddComponent<CanvasGroup>();
            var panel = Node("Panel",root,0,0,370,levels ? 650 : 600); Image(panel,Cream);
            Picture("Ribbon",panel,0,242,390,47,name == "WinWindow" ? Green : Berry);
            var title = Text("Title",panel,0,242,350,44,"Лавка Рози",24,Cream); title.fontStyle = FontStyles.Bold;
            var description = Text("Body",panel,0,levels ? 181 : 126,325,levels ? 50 : 140,"Описание",17,Ink);
            var content = Node("Content",panel,0,levels ? -4 : 30,320,levels ? 290 : 1);
            var buttons = new ShopButton[5];
            for (int i = 0; i < 5; i++) buttons[i] = Button(panel,"Action" + i,0,levels ? -245 - i * 50 : 4 - i * 54,300,43,"Действие",i == 0 ? Green : i == 4 ? Berry : Wood);
            var view = root.gameObject.AddComponent<WindowView>(); Ref(view,"panel",panel); Ref(view,"group",group); Ref(view,"title",title); Ref(view,"description",description); Ref(view,"content",content); Refs(view,"actions",buttons);
            return Save<WindowView>(root,"Windows",name);
        }
        static RectTransform BuildConfetti()
        { var root = Node("Confetti",null,0,0,9,12); Image(root,ShopColors.Gold,assets.triangle); return Save<RectTransform>(root,"Effects","Confetti"); }
        static ShopApplication BuildRoot()
        {
            var root = Node("ShopRoot",null,0,0,430,900);
            var canvas = root.gameObject.AddComponent<Canvas>(); canvas.renderMode = RenderMode.ScreenSpaceOverlay; canvas.sortingOrder = 10;
            var scaler = root.gameObject.AddComponent<CanvasScaler>(); scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(430,900); scaler.screenMatchMode = CanvasScaler.ScreenMatchMode.Expand;
            root.gameObject.AddComponent<GraphicRaycaster>();
            Image(root,ShopColors.Hex("2B1B0E"));
            var safe = Node("SafeArea",root,0,0,430,900); Image(safe,ShopColors.Hex("F4E8D0"));
            for (int i = 0; i < 16; i++) Picture("Wallpaper",safe,-214 + i * 28,0,2,900,ShopColors.Hex("EBDBBC"));
            Picture("Wainscot",safe,0,-380,430,140,Sage);
            for (int i = 0; i < 11; i++) Picture("WainscotGroove",safe,-214 + i * 40,-380,2,140,ShopColors.Hex("7D9469"));
            Picture("ChairRail",safe,0,-310,430,8,Wood);
            var screens = Node("Screens",safe,0,0,430,900);
            var effects = Node("Effects",safe,0,0,430,900);
            var windows = Node("Windows",safe,0,0,430,900);
            var toast = (ToastView)PrefabUtility.InstantiatePrefab(assets.toast,safe); ((RectTransform)toast.transform).anchoredPosition = new Vector2(0,-408);
            var audio = root.gameObject.AddComponent<ShopAudio>();
            var app = root.gameObject.AddComponent<ShopApplication>(); Ref(app,"assets",assets); Ref(app,"safeArea",safe); Ref(app,"screenRoot",screens); Ref(app,"windowRoot",windows); Ref(app,"effects",effects); Ref(app,"toast",toast); Ref(app,"audio",audio);
            return Save<ShopApplication>(root,"Resources/Rosie","ShopRoot");
        }
        [MenuItem("Tools/Rosie's Corner Shop/Validate prefab references")]
        public static void ValidateReferences()
        {
            foreach (string guid in AssetDatabase.FindAssets("t:Prefab",new[] { Base }))
            {
                string path = AssetDatabase.GUIDToAssetPath(guid); var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(path);
                foreach (Transform transform in prefab.GetComponentsInChildren<Transform>(true))
                {
                    if (GameObjectUtility.GetMonoBehavioursWithMissingScriptCount(transform.gameObject) > 0) throw new Exception("Missing script in " + path);
                    foreach (MonoBehaviour behaviour in transform.GetComponents<MonoBehaviour>())
                    {
                        if (behaviour == null || behaviour.GetType().Namespace != "RosiesCornerShop.Core") continue;
                        var property = new SerializedObject(behaviour).GetIterator();
                        while (property.NextVisible(true)) if (property.propertyType == SerializedPropertyType.ObjectReference && property.name != "m_Script" && property.objectReferenceValue == null)
                            throw new Exception("Unassigned reference " + behaviour.GetType().Name + "." + property.propertyPath + " in " + path);
                    }
                }
            }
            Debug.Log("Rosie prefab references: PASS");
        }
    }
}
