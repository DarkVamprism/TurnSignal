﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using Valve.VR;

public class Unity_Overlay : MonoBehaviour 
{
	public enum OverlayTrackedDevice 
	{
		None,
		HMD,
		RightHand,
		LeftHand,
		CustomIndex
	}

	[Space(10)]
	public string overlayName = "Unity Overlay";
	public string overlayKey = "unity_overlay";

	public bool isDashboardOverlay = false;

	[Space(10)]

	public bool simulateInEditor = false;

	[Space(10)]

	public Texture overlayTexture;
	public Camera cameraForTexture;

	public Texture thumbNailTexture;

	[Space(10)]

	public bool autoUpdateOverlay = true;

	[Space(10)]

	public bool isVisible = true;
	public bool highQuality = false;
	public Color colorTint = Color.white;
	[Range(0f, 1f)]
	public float opacity = 1.0f;
	public float widthInMeters = 1.0f;

	[Space(10)]
	public OverlayTrackedDevice deviceToTrack = OverlayTrackedDevice.None;
	public uint customDeviceIndex = 0;
	
	[Space(10)]
	public bool enableSimulatedMouse = false;
	public Vector2 mouseScale = new Vector2(1f, 1f);

	[Space(10)]
	public bool simulateUnityMouseInput = false;
	public GraphicRaycaster canvasGraphicsCaster;
	public float mouseDragStartDelay = 0.1f;


	protected OVR_Handler ovrHandler = OVR_Handler.instance;
	protected OVR_Overlay overlay = new OVR_Overlay();
	
	protected Unity_Overlay_Opts opts = new Unity_Overlay_Opts();

	protected RenderTexture cameraTexture;

	protected Texture _mainTex;

	protected VRTextureBounds_t textureBounds = new VRTextureBounds_t();
	protected HmdVector2_t mouseScale_t = new HmdVector2_t();
	protected SteamVR_Utils.RigidTransform matrixConverter;

	private HashSet<Selectable> enterTargets = new HashSet<Selectable>();
	private HashSet<Selectable> downTargets = new HashSet<Selectable>();

	protected bool mouseDown = false;
	protected bool mouseDragging = false;
	protected float mouseDownTime = 0f;

	protected Unity_Overlay_UI_Handler uiHandler = new Unity_Overlay_UI_Handler();


	void Start () 
	{
		matrixConverter = new SteamVR_Utils.RigidTransform(transform);

		if(cameraForTexture != null)
		{
			int width = (int) (cameraForTexture.pixelWidth);
			int height = (int) (cameraForTexture.pixelHeight * 2);

			Debug.Log("Width: " + width);
			Debug.Log("Height: " + height);

			cameraTexture = new RenderTexture(width, height, 24);
			cameraForTexture.targetTexture = cameraTexture;
			cameraForTexture.enabled = false;
		}
		


		overlay.overlayTextureType = SystemInfo.graphicsDeviceVersion.StartsWith("OpenGL") ? ETextureType.OpenGL : ETextureType.DirectX;
		
		overlay.overlayKey = overlayKey;
		overlay.overlayName = overlayName;

		if(isDashboardOverlay)
			overlay.overlayIsDashboard = true;
	}

	void OnDestroy()
	{
		overlay.DestroyOverlay();
		Debug.Log("Overlay Destroyed: " + !overlay.created);
	}

	void OnEnable()
	{
		overlay.ShowOverlay();
		Debug.Log("Overlay Shown!");
	}

	void OnDisable()
	{
		overlay.HideOverlay();
		Debug.Log("Overlay Hidden!");
	}
	
	void Update() 
	{
		if(autoUpdateOverlay)
			UpdateOverlay();
	}

	public void UpdateOverlay()
	{
		if(!ovrHandler.OpenVRConnected)
			return;

		if(ovrHandler.OpenVRConnected && !overlay.created)
		{
			if(!overlay.CreateOverlay())
			{
				Debug.Log("Failed to create overlay!");
				return;
			}
			else
				Debug.Log("Created Overlay!");
		}
		
		UpdateOpts();
		UpdateTexture();
		UpdateMouse();

		if(simulateUnityMouseInput && _mainTex)
		{
			var mPos = overlay.overlayMousePosition;
			if(mPos.x < 0f || mPos.x > 1f || mPos.y < 0f || mPos.y > 1f)
				return;

			var pd = uiHandler.pD;

			if(mouseDown && !mouseDragging)
			{
				pd.Reset();
				pd.clickCount = 1;
			}
			else if(mouseDown && mouseDragging)
			{
				pd.clickCount = 0;
				pd.clickTime += mouseDownTime;
				pd.dragging = true;
			}

			pd.button = PointerEventData.InputButton.Left;
			pd.position = new Vector2(mPos.x * _mainTex.width,(1f - mPos.y) * _mainTex.height);

			var nTargs = uiHandler.GetUITargets(cameraForTexture, canvasGraphicsCaster, pd);

			uiHandler.EnterTargets(nTargs);

			foreach(Selectable ub in nTargs)
				if(enterTargets.Contains(ub))
					enterTargets.Remove(ub);

			uiHandler.ExitTargets(enterTargets);
			enterTargets = nTargs;

			if(mouseDown)
			{
				if(!mouseDragging)
				{
					foreach(Selectable sel in nTargs)
						downTargets.Add(sel);

					uiHandler.SubmitTargets(downTargets);
					uiHandler.StartDragTargets(downTargets);
					uiHandler.DownTargets(downTargets);
				}
				else
				{
					uiHandler.MoveTargets(downTargets);
					uiHandler.DragTargets(downTargets);
					uiHandler.DownTargets(downTargets);
				}
			}
			else
			{
				uiHandler.UpTargets(downTargets);
				uiHandler.EndDragTargets(downTargets);
				uiHandler.DropTargets(downTargets);

				downTargets.Clear();
			}
		}
	}

	void UpdateMouse()
	{
		if(mouseDown)
			mouseDownTime += Time.deltaTime;

		if(mouseDown && mouseDownTime > 0f)
			mouseDragging = true;

		if(overlay.overlayMouseLeftDown && !mouseDown)
			mouseDown = true;

		if(mouseDown && !overlay.overlayMouseLeftDown)
		{
			mouseDown = false;
			mouseDragging = false;
			mouseDownTime = 0f;
		}
	}

	void UpdateTexture()
	{
		overlay.overlayTextureBounds = textureBounds;
		
		if(cameraForTexture)
		{
			RenderTexture.active = cameraTexture;
			cameraForTexture.Render();
			
			_mainTex = cameraTexture;
		}
		else if(overlayTexture)
			_mainTex = overlayTexture;
		else
			_mainTex = null;

		if(_mainTex != null)
		{
			float texWidth = (float)_mainTex.width / (float)_mainTex.height;

			textureBounds.uMin = 0;
			textureBounds.vMin = 1;
			textureBounds.uMax = texWidth;
			textureBounds.vMax = 0;

			overlay.overlayTexture = _mainTex;
		}
			

		if(isDashboardOverlay)
			overlay.overlayThumbnailTexture = thumbNailTexture;
	}

	void UpdateOpts()
	{
		if(opts.pos != transform.position || opts.rot != transform.rotation)
		{
			matrixConverter.pos = transform.position;
			matrixConverter.rot = transform.rotation;

			overlay.overlayTransform = matrixConverter.ToHmdMatrix34();

			opts.pos = transform.position;
			opts.rot = transform.rotation;
		}

		if( opts.isVisible != isVisible ) 
		{
			overlay.overlayVisible = isVisible;

			opts.isVisible = isVisible;
		}

		if( opts.highQuality != highQuality ) 
		{
			overlay.overlayHighQuality = highQuality;

			opts.highQuality = highQuality;
		}

		if( opts.colorTint != colorTint ) 
		{
			overlay.overlayColor = colorTint;

			opts.colorTint = colorTint;
		}

		if( opts.opacity != opacity ) 
		{
			overlay.overlayAlpha = opacity;

			opts.opacity = opacity;
		}

		if( opts.widthInMeters != widthInMeters ) 
		{
			overlay.overlayWidthInMeters = widthInMeters;

			opts.widthInMeters = widthInMeters;
		}

		if( opts.deviceToTrack != deviceToTrack ) 
		{
			if(deviceToTrack == OverlayTrackedDevice.None)
				overlay.overlayTransformType = VROverlayTransformType.VROverlayTransform_Absolute;
			else
			{
				uint index = 0;
				switch(deviceToTrack)
				{
					case OverlayTrackedDevice.HMD:
						index = ovrHandler.poseHandler.hmdIndex;
					break;

					case OverlayTrackedDevice.RightHand:
						index = ovrHandler.poseHandler.rightIndex;
					break;

					case OverlayTrackedDevice.LeftHand:
						index = ovrHandler.poseHandler.leftIndex;
					break;

					case OverlayTrackedDevice.CustomIndex:
						index = customDeviceIndex;
					break;
				}

				overlay.overlayTransformType = VROverlayTransformType.VROverlayTransform_TrackedDeviceRelative;
				overlay.overlayTransformTrackedDeviceRelativeIndex = index;
			}

			opts.deviceToTrack = deviceToTrack;
		}

		if( opts.customDeviceIndex != customDeviceIndex ) 
		{	
			overlay.overlayTransformTrackedDeviceRelativeIndex = customDeviceIndex;
			opts.customDeviceIndex = customDeviceIndex;
		}

		if( opts.enableSimulatedMouse != enableSimulatedMouse ) 
		{
			overlay.overlayInputMethod = 
				enableSimulatedMouse ? VROverlayInputMethod.Mouse : VROverlayInputMethod.None;

			opts.enableSimulatedMouse = enableSimulatedMouse;
		}

		if( opts.mouseScale != mouseScale ) 
		{
			mouseScale_t.v0 = mouseScale.x;
			mouseScale_t.v1 = mouseScale.y;

			overlay.overlayMouseScale = mouseScale_t;
			opts.mouseScale = mouseScale;
		}
	}
}

public struct Unity_Overlay_Opts 
{
	public Vector3 pos;
	public Quaternion rot;
	public bool isVisible;
	public bool highQuality;
	public Color colorTint;
	public float opacity;
	public float widthInMeters;

	public Unity_Overlay.OverlayTrackedDevice deviceToTrack;
	public uint customDeviceIndex;

	public bool enableSimulatedMouse;
	public Vector2 mouseScale;
	
}

public class Unity_Overlay_UI_Handler 
{
	public Camera cam;
	public PointerEventData pD = new PointerEventData(EventSystem.current);
	public AxisEventData aD = new AxisEventData(EventSystem.current);

	public HashSet<Selectable> GetUITargets(Camera camA, GraphicRaycaster gRay, PointerEventData pD)
	{
		if(cam != camA)
			cam = camA;

		aD.Reset();
		aD.moveVector = (this.pD.position - pD.position);

		float x1 = this.pD.position.x,
			  x2 = pD.position.x,
			  y1 = this.pD.position.y,
			  y2 = pD.position.y;

		float xDiff = x1 - x2;
		float yDiff = y1 - y2;

		MoveDirection dir = MoveDirection.None;

		if(xDiff > yDiff)
			if(xDiff > 0f)
				dir = MoveDirection.Right;
			else if(xDiff < 0f)
				dir = MoveDirection.Left;
		else if (yDiff > xDiff)
			if(yDiff > 0f)
				dir = MoveDirection.Up;
			else if(yDiff < 0f)
				dir = MoveDirection.Down;

		aD.moveDir = dir;

		var ray = cam.ScreenPointToRay(pD.position);
		Debug.DrawRay(ray.origin, ray.direction * 10, Color.yellow);

		List<RaycastResult> hits = new List<RaycastResult>();
		HashSet<Selectable> uibT = new HashSet<Selectable>();

		gRay.Raycast(pD, hits);

		if(hits.Count > 0)
			pD.pointerCurrentRaycast = pD.pointerPressRaycast = hits[0];

		Debug.Log(pD.enterEventCamera);

		for(int i = 0; i < hits.Count; i++)
		{
			var go = hits[i].gameObject;
			Selectable u = GOGetter(go);
			
			if(u)
				uibT.Add(u);
		}

		this.pD = pD;

		return uibT;
	}
	
	public Selectable GOGetter(GameObject go, bool tryPar = false)
	{
		Selectable sel = go.GetComponentInParent<Selectable>();

		if(sel)
			Debug.Log(sel.gameObject.name);

		return sel;
	}

	public void EnterTargets(HashSet<Selectable> t)
	{
		foreach(Selectable b in t)
			ExecuteEvents.Execute(b.gameObject, pD, ExecuteEvents.pointerEnterHandler);
	}

	public void ExitTargets(HashSet<Selectable> t)
	{
		foreach(Selectable b in t)
			ExecuteEvents.Execute(b.gameObject, pD, ExecuteEvents.pointerExitHandler);
	}

	public void DownTargets(HashSet<Selectable> t)
	{
		foreach(Selectable b in t) 
			ExecuteEvents.Execute(b.gameObject, pD, ExecuteEvents.pointerDownHandler);
	}

	public void UpTargets(HashSet<Selectable> t)
	{
		foreach(Selectable b in t)
			ExecuteEvents.Execute(b.gameObject, pD, ExecuteEvents.pointerUpHandler);
	}

	public void SubmitTargets(HashSet<Selectable> t)
	{
		foreach(Selectable b in t)
			ExecuteEvents.Execute(b.gameObject, pD, ExecuteEvents.submitHandler);
	}

	public void StartDragTargets(HashSet<Selectable> t)
	{
		foreach(Selectable b in t)
			ExecuteEvents.Execute(b.gameObject, pD, ExecuteEvents.beginDragHandler);
	}

	public void DragTargets(HashSet<Selectable> t)
	{
		foreach(Selectable b in t)
			ExecuteEvents.Execute(b.gameObject, pD, ExecuteEvents.dragHandler);
	}

	public void MoveTargets(HashSet<Selectable> t)
	{
		foreach(Selectable b in t)
			ExecuteEvents.Execute(b.gameObject, aD, ExecuteEvents.moveHandler);
	}

	public void EndDragTargets(HashSet<Selectable> t)
	{
		foreach(Selectable b in t)
			ExecuteEvents.Execute(b.gameObject, pD, ExecuteEvents.endDragHandler);
	}

	public void DropTargets(HashSet<Selectable> t)
	{
		foreach(Selectable b in t)
			ExecuteEvents.Execute(b.gameObject, pD, ExecuteEvents.dropHandler);
	}
}
