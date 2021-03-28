using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.EventSystems;
using Newtonsoft.Json;
using System.IO;
using Microsoft.Win32.SafeHandles;
using UnityEngine.UI;

public class MapMaker : MonoBehaviour
{
    public GameObject prefab;
    // 行
    public int row;
    // 列
    public int col;

    private bool selected;
    private Box holdBox;
    List<RaycastResult> list = new List<RaycastResult>();

    public GameObject Obstacles;
    public GameObject Obstacle;

    public bool isTargetMove;
    public Text testMapBtn;

    // Start is called before the first frame update
    void Start()
    {
        LoadMap();
    }

    // Update is called once per frame
    void Update()
    {
        if (isTargetMove) return;

        /// 鼠标左键没有点击，就不执行判断逻辑
		if (Input.GetMouseButtonDown(0))
        {
            ///相应的GameObject对象
            GameObject go = null;

            ///判断是否点再ui上
            if (EventSystem.current.IsPointerOverGameObject())
            {
                go = ClickUI();
            }
            else
            {
                go = ClickScene();
            }

            if (go == null)
            {
                Debug.Log("Click Nothing");
            }
            else
            {
                // 高亮点中GameObject
                //EditorGUIUtility.PingObject(go);
                //Selection.activeObject = go;
                //Debug.Log(go, go);
                string name = go.name;
                if (name.Equals("red") || name.Equals("green"))
                {
                    selected = !go.GetComponentInParent<Box>().selected;
                    go.GetComponentInParent<Box>().selected = selected;
                }
            }
        }

        if(Input.GetMouseButton(0))
        {
            GameObject go = null;
            if (EventSystem.current.IsPointerOverGameObject())
            {
                go = ClickUI();
            }
            if (go == null)
            {
                Debug.Log("Click Nothing");
            }
            else
            {
                // 高亮点中GameObject
                //EditorGUIUtility.PingObject(go);
                //Selection.activeObject = go;
                //Debug.Log(go, go);
                string name = go.name;
                if (name.Equals("red") || name.Equals("green"))
                {
                    go.GetComponentInParent<Box>().selected = selected;
                }
            }
        }
        
    }

    /// <summary>
    /// 点中ui
    /// </summary>
    private GameObject ClickUI()
    {
        //场景中的EventSystem

        PointerEventData eventData = new PointerEventData(EventSystem.current);

        //鼠标位置
        eventData.position = Input.mousePosition;

        //调用所有GraphicsRacaster里面的Raycast，然后内部会进行排序，
        //直接拿出来，取第一个就可以用了
        EventSystem.current.RaycastAll(eventData, list);

        //这个函数抄的unity源码的，就是取第一个值
        var raycast = FindFirstRaycast(list);

        //获取父类中事件注册接口
        //如Button，Toggle之类的，毕竟我们想知道哪个Button被点击了，而不是哪张Image被点击了
        //当然可以细分为IPointerClickHandler, IBeginDragHandler之类细节一点的，各位可以自己取尝试
        var go = ExecuteEvents.GetEventHandler<IEventSystemHandler>(raycast.gameObject);

        //既然没拿到button之类的，说明只有Image挡住了，取点中结果即可
        if (go == null)
        {
            go = raycast.gameObject;
        }
        return go;
    }

    /// <summary>
    /// Return the first valid RaycastResult.
    /// </summary>
    private RaycastResult FindFirstRaycast(List<RaycastResult> candidates)
    {
        for (var i = 0; i < candidates.Count; ++i)
        {
            if (candidates[i].gameObject == null)
                continue;

            return candidates[i];
        }
        return new RaycastResult();
    }

    /// <summary>
    /// 点中场景中对象
    /// 然后无聊嘛，顺便把点场景的也顺手做了，不过这部分网上介绍挺多的，就不展开说了。
    /// </summary>
    private GameObject ClickScene()
    {
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        RaycastHit hit;
        if (Physics.Raycast(ray, out hit))
        {
            GameObject go = hit.collider.gameObject;
            return go;
        }

        return null;
    }

    public void CreateMap()
    {
        if (row + col < 2) return;

        if (prefab == null) return;

        transform.GetComponent<RectTransform>().sizeDelta = new Vector2(col * 100, row * 100);

        int maxCount = Mathf.Max(row * col, transform.childCount);
        for(int i=0;i<maxCount;i++)
        {
            int r = i / col;
            int c = i % col;
            if(i>=transform.childCount)
            {
                Transform item = Instantiate<Transform>(prefab.transform);
                item.SetParent(transform);
                item.name = "box_" + (i+1);
                item.localPosition = new Vector3(c * 100 - col * 50,r * -100 + row * 50);
                item.localRotation = new Quaternion(0, 0, 0, 0);
                item.localScale = Vector3.one;
                item.gameObject.layer = 4;
                item.GetComponent<Box>().col = c;
                item.GetComponent<Box>().row = r;
                item.GetComponent<Box>().selected = false;

                Transform item2 = Instantiate<Transform>(Obstacle.transform);
                item2.SetParent(Obstacles.transform);
                item2.name = "box_" + (i + 1);
                item2.gameObject.layer = 4;
                Vector3 v3 = new Vector3(c - col * 0.5f + 0.5f, row * 0.5f - 0.5f - r, 0);
                Debug.Log(v3);
                item2.position = v3;
            } else
            {
                if(i>row*col)
                {
                    Debug.Log(i);
                    transform.GetChild(i).gameObject.SetActive(false);
                } else
                {
                    transform.GetChild(i).gameObject.SetActive(true);
                    transform.GetChild(i).GetComponent<Box>().selected = false;
                }
            }
        }
    }

    public void SaveMap()
    {
        List<int> data = new List<int>();
        for(int i=0;i< row * col; i++)
        {
            if(transform.GetChild(i).GetComponent<Box>().selected)
            {
                data.Add(i);
            }
        }
        Map map = new Map();
        map.row = row;
        map.col = col;
        map.data = data;
        string mapJson = JsonConvert.SerializeObject(map, new JsonSerializerSettings() { ReferenceLoopHandling = ReferenceLoopHandling.Ignore});
        Debug.Log(mapJson);
        string path = Application.dataPath+"/Resources/map.json";
        Debug.Log(path);
        SafeFileHandle handle = File.Open(path, FileMode.OpenOrCreate).SafeFileHandle;
        FileStream file = new FileStream(handle,FileAccess.ReadWrite);
        byte[] bts = System.Text.Encoding.UTF8.GetBytes(mapJson);
        file.Write(bts, 0, bts.Length);
        if (file != null) {
            file.Close();  
        }
    }

    public void LoadMap()
    {
        TextAsset json = Resources.Load<TextAsset>("map");
        Map map = JsonConvert.DeserializeObject<Map>(json.text);
        row = map.row;
        col = map.col;
        CreateMap();
        for (int i = 0; i<map.data.Count; i++)
        {
            transform.GetChild(map.data[i]).gameObject.layer = 0;
            transform.GetChild(map.data[i]).GetComponent<Box>().selected = true;
            Obstacles.transform.GetChild(map.data[i]).gameObject.SetActive(false);

        }
    }

    public void TestMap()
    {
        isTargetMove = !isTargetMove;
        testMapBtn.text = isTargetMove? "测试中" : "测试地图";
    }
}
