using DefaultNamespace;
using Sirenix.OdinInspector;
using Sirenix.OdinInspector.Editor;
using Sirenix.Utilities;
using Sirenix.Utilities.Editor;
using UnityEditor;
using UnityEngine;

public class HexMapCreaterWindow : OdinEditorWindow
{
    [MenuItem("Tools/蜂窝地图生成器", priority = 2)]
    public static void OpenIntermediateDataViewWindow()
    {
        var window = GetWindow<HexMapCreaterWindow>("蜂窝地图生成器");
        window.position = GUIHelper.GetEditorWindowRect().AlignCenter(500, 700);
        window.Show();
    }
    
    private static string MAPDATA_CONFIG_PATH = "Assets/_Resources/MapData/";
    private static string PREFAB_CONFIG_PATH = "Assets/_Resources/Map/floor_0_lod0 ";

    #region 创建地图文件
    
    private GridHexXZ<GridObject> maxGridHexXZ;
    
    [LabelText("地图最大宽"), LabelWidth(80)]
    public int maxWidth = 30;
    [LabelText("地图最大高"), LabelWidth(80)]
    public int maxHeight = 30;
    
    [HorizontalGroup("AssetSetting")][LabelText("配置文件:"), LabelWidth(100)] [AssetSelector(Filter = "t:HexMapData"), OnValueChanged("OnDataChanged")]
    public HexMapData hexMapData;

    [HorizontalGroup("FileName", MaxWidth = 300), PropertySpace(5), LabelText("文件名:"), LabelWidth(115), ShowIf("@hexMapData == null")]
    public string fileName;

    [HorizontalGroup("FileName", MaxWidth = 60), PropertySpace(5), Button("生成", ButtonSizes.Small), ShowIf("@hexMapData == null")]
    private void CreateVirtualObjectAsset()
    {
        if (string.IsNullOrEmpty(fileName))
        {
            EditorUtility.DisplayDialog("提示", "未设置文件名", "确定");
            return;
        }

        hexMapData = ScriptableObject.CreateInstance<HexMapData>();
        var filePath = MAPDATA_CONFIG_PATH + $"{fileName.ToLower()}.asset";
            
        AssetDatabase.CreateAsset(hexMapData, filePath);
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

        CreateHexMap();
    }
    
    private void CreateHexMap()
    {
        if (baseParent != null)
        {
            GameObject.DestroyImmediate(baseParent.gameObject);
        }
        
        baseParent = new GameObject("Base").transform;
        baseParent.position = hexMapData.basePos;
        baseParent.rotation = hexMapData.baseRot;
        baseParent.hideFlags = HideFlags.HideInHierarchy;
        GameObject basePrefab = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/_Resources/map_base.prefab");
        if(hexMapData.maxHeight == 0)
            hexMapData.maxHeight = maxHeight;
        if(hexMapData.maxHeight == 0)
            hexMapData.maxWidth = maxWidth;
        
        maxGridHexXZ = new GridHexXZ<GridObject>(hexMapData.maxHeight, hexMapData.maxHeight, 16.6f, Vector3.zero, (gridObj, x, z) =>
        {
            var mapObj = new GridObject();
            return mapObj;
        });
        
        
        for (int x = 0; x < hexMapData.maxHeight; x++)
        {
            for (int z = 0; z < hexMapData.maxHeight; z++)
            {
                Transform trans = Instantiate(basePrefab, baseParent).transform;
                trans.name = $"x:{x} z:{z}";
                trans.position = maxGridHexXZ.GetWorldPosition(x, z);
                maxGridHexXZ.GetGridObject(x, z).VisualTransform = trans;
            }
        }

        if (parent == null)
        {
            parent = new GameObject("HexParent");
        }
        
        for (int i = 0; i < hexMapData.hexChildList.Count; i++)
        {
            var data = hexMapData.hexChildList[i];
            GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>($"{PREFAB_CONFIG_PATH}{(int)data.MapType + 1}.prefab");
            var tmpTrans = Instantiate(prefab, parent.transform).transform;
            var childPos = maxGridHexXZ.GetWorldPosition(data.X, data.Z);
            tmpTrans.position = childPos;
            tmpTrans.rotation = data.rot;
            data.Transform = tmpTrans;
        }
    }

    private void OnDataChanged()
    {
        CleanScene();

        if (hexMapData == null)
        {
            return;
        }
        
        CreateHexMap();
    }
    
    #endregion

    #region 编辑地图

    [AssetSelector(SearchInFolders = new string[]{"Assets/_Resources/Map/"}), LabelText("预制体"), LabelWidth(50), PreviewField(Height = 100), ShowIf("@hexMapData != null"), OnValueChanged("HexPrefabChanged")]
    public GameObject prefab;

    private HexMapChild.MapTypeFlag curMapType;
    
    [LabelText("当前选中的物体"), LabelWidth(80), ShowIf("@parent != null")]
    public GameObject curSelHex;
    
    private GameObject parent;
    private Transform baseParent;
    
    [Button("旋转"), ShowIf("@curSelHex != null")]
    public void RotateSelected()
    {
        curSelHex.transform.Rotate(new Vector3(0, 1, 0), 60);
    }

    #endregion

    [Button("保存"), ShowIf("@baseParent != null && hexMapData != null"), PropertySpace(20)]
    private void SaveData()
    {
        hexMapData.basePos = baseParent.position;
        hexMapData.baseRot = baseParent.rotation;

        for (int i = hexMapData.hexChildList.Count - 1; i >= 0; i--)
        {
            if (hexMapData.hexChildList[i].Transform == null)
            {
                hexMapData.hexChildList.RemoveAt(i);
            }
        }
        
        EditorUtility.SetDirty(hexMapData);
        AssetDatabase.SaveAssetIfDirty(hexMapData);
    }
    
    protected override void OnEnable()
    {
        SceneView.duringSceneGui -= OnSceneGUI;
        SceneView.duringSceneGui += OnSceneGUI;
        
        if (prefab == null)
        {
            prefab = AssetDatabase.LoadAssetAtPath<GameObject>($"{PREFAB_CONFIG_PATH}{1.ToString()}.prefab");
        }
    }

    protected override void OnGUI()
    {
        base.OnGUI();
        if (Selection.gameObjects.Length > 0)
        {
            if (Selection.gameObjects[0].name.Contains("map_type"))
            {
                curSelHex = Selection.gameObjects[0];
            }
        }
    }
    
    private void OnSceneGUI(SceneView sceneView)
    {
        if (maxGridHexXZ == null)
        {
            maxGridHexXZ = new GridHexXZ<GridObject>(maxWidth, maxHeight, 0.83f, Vector3.zero, (gridObj, x, z) =>
            {
                var mapObj = new GridObject();
                return mapObj;
            });
        }

        if (Event.current.type == EventType.MouseDown && Event.current.button == 0)
        {
            if (parent == null)
            {
                parent = new GameObject("HexParent");
            }
            
            Vector2 mousePos = Event.current.mousePosition;
            //retina 屏幕需要拉伸值
            float mult = 1;
#if UNITY_5_4_OR_NEWER
            mult = EditorGUIUtility.pixelsPerPoint;
#endif
            //转换成摄像机可接受的屏幕坐标,左下角是(0,0,0);右上角是(camera.pixelWidth,camera.pixelHeight,0)
            mousePos.y = sceneView.camera.pixelHeight - mousePos.y * mult;
            mousePos.x *= mult;
            //近平面往里一些,才能看到摄像机里的位置
            Vector3 fakePoint = mousePos;
            fakePoint.z = 20;
            var ray = sceneView.camera.ScreenPointToRay(fakePoint);
            Vector3 dir = ray.direction;
            float num = (-ray.origin.y) / dir.y;
            var pos = ray.origin + ray.direction * num;

            maxGridHexXZ.GetXZ(pos, out int x, out int z);
            var childPos = maxGridHexXZ.GetWorldPosition(x, z);

            bool alreadySet = false;
            HexMapChild child = null;
            for (int i = 0; i < hexMapData.hexChildList.Count; i++)
            {
                var tmp = hexMapData.hexChildList[i];
                if (tmp.X == x && tmp.Z == z)
                {
                    child = tmp;
                    alreadySet = true;
                    break;
                }
            }
            
            if (prefab == null)
            {
                prefab = AssetDatabase.LoadAssetAtPath<GameObject>($"{PREFAB_CONFIG_PATH}{1.ToString()}.prefab");
            }

            curMapType = HexMapChild.MapTypeFlag.type1;
            switch (prefab.name)
            {
                case "map_type1":
                    curMapType = HexMapChild.MapTypeFlag.type1;
                    break;
                case "map_type2":
                    curMapType = HexMapChild.MapTypeFlag.type2;
                    break;
                case "map_type3":
                    curMapType = HexMapChild.MapTypeFlag.type3;
                    break;
            }

            if (!alreadySet)
            {
                var tmpTrans = Instantiate(prefab, parent.transform).transform;
                tmpTrans.rotation = Quaternion.Euler(0, 30, 0);
                child = new HexMapChild(x, z, tmpTrans.rotation);
                child.Transform = tmpTrans;
                hexMapData.hexChildList.Add(child);
            }
            else
            {
                if (child.Transform != null)
                {
                    GameObject.DestroyImmediate(child.Transform.gameObject);
                }
                child.Transform = Instantiate(prefab, parent.transform).transform;
            }

            child.MapType = curMapType;
            child.Transform.position = childPos;
        }
    }

    private void HexPrefabChanged()
    {
        
    }

    private void CleanScene()
    {
        if(baseParent != null)
            GameObject.DestroyImmediate(baseParent.gameObject);
        if(parent != null)
            GameObject.DestroyImmediate(parent.gameObject);
    }

    private void OnDisable()
    {
        SceneView.duringSceneGui -= OnSceneGUI;
        CleanScene();
    }
}

public class GridObject
{
    public Transform VisualTransform;
}

