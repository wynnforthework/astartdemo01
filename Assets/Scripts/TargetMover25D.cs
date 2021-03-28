using UnityEngine;
using System.Linq;
using Pathfinding;
using UnityEngine.EventSystems;
using System.Collections.Generic;

/// <summary>
/// Moves the target in example scenes.
/// This is a simple script which has the sole purpose
/// of moving the target point of agents in the example
/// scenes for the A* Pathfinding Project.
///
/// It is not meant to be pretty, but it does the job.
/// </summary>
[HelpURL("http://arongranberg.com/astar/docs/class_pathfinding_1_1_target_mover.php")]
public class TargetMover25D : MonoBehaviour {
	/// <summary>Mask for the raycast placement</summary>
	public LayerMask mask;

	public Transform target;
	public Transform road;
	public Transform Obstacles;
	public MapMaker maker;
	List<RaycastResult> list = new List<RaycastResult>();
	IAstarAI[] ais;

	/// <summary>Determines if the target position should be updated every frame or only on double-click</summary>
	public bool onlyOnDoubleClick;
	public bool use2D;

	Camera cam;

	public void Start() {
		//Cache the Main Camera
		cam = Camera.main;
		// Slightly inefficient way of finding all AIs, but this is just an example script, so it doesn't matter much.
		// FindObjectsOfType does not support interfaces unfortunately.
		ais = FindObjectsOfType<MonoBehaviour>().OfType<IAstarAI>().ToArray();
		useGUILayout = false;
	}

	public void OnGUI() {
		if (onlyOnDoubleClick && cam != null && Event.current.type == EventType.MouseDown && Event.current.clickCount == 2) {
			UpdateTargetPosition();
		}
	}

	/// <summary>Update is called once per frame</summary>
	void Update() {
		if (!onlyOnDoubleClick && cam != null) {
			UpdateTargetPosition();
		}
	}

	public void UpdateTargetPosition() {
		Vector3 newPosition = Vector3.zero;
		bool positionFound = false;

		if (use2D) {
			///相应的GameObject对象
			GameObject go = null;

			///判断是否点再ui上
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
					int r = go.GetComponentInParent<Box>().row;
					int c = go.GetComponentInParent<Box>().col;
					newPosition = new Vector3(c - maker.col * 0.5f + 0.5f, maker.row * 0.5f - 0.5f - r, 0);
					positionFound = true;
				}
			}
			
			
		} else {
			// Fire a ray through the scene at the mouse position and place the target where it hits
			RaycastHit hit;
			if (Physics.Raycast(cam.ScreenPointToRay(Input.mousePosition), out hit, Mathf.Infinity, mask)) {
				newPosition = hit.point;
				positionFound = true;
			}
		}

		if (positionFound && newPosition != target.position) {
			target.localPosition = newPosition;

			if (onlyOnDoubleClick) {
				for (int i = 0; i < ais.Length; i++) {
					if (ais[i] != null) ais[i].SearchPath();
				}
			}
		}
	}

	Vector3 worldPosToUILocalPos()
    {
		///相应的GameObject对象
		GameObject go = null;

		///判断是否点再ui上
		if (EventSystem.current.IsPointerOverGameObject())
		{
			go = ClickUI();
		}
		go = ClickUI();
		if (go != null)
		{
			string name = go.name;
			if (name.Equals("red") || name.Equals("green"))
			{
				return go.transform.parent.localPosition;
			}
		}
		return Vector3.zero;
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

}

