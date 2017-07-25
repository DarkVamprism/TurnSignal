using System.Collections;
using UnityEngine;
using Valve.VR;

public class OVR_Overlay 
{
    protected OVR_Handler OVR { get { return OVR_Handler.instance; } }
    public bool OverlayExists 
    { 
        get 
        {
            return OVR.Overlay != null;
        } 
    }
    protected CVROverlay Overlay { get { return OVR.Overlay; } }

    protected bool _overlayIsDashboard = false;
    public bool overlayIsDashboard 
    {
        get { return _overlayIsDashboard; }
        set { _overlayIsDashboard = true; }
    }

    protected bool validHandle
    {
        get 
        {
            return _overlayHandle != OpenVR.k_ulOverlayHandleInvalid;
        } 
}
    protected ulong _overlayHandle = OpenVR.k_ulOverlayHandleInvalid;
    public ulong overlayHandle { get { return _overlayHandle; } }

    private ulong _overlayThumbnailHandle = 0;
    private ulong overlayThumbnailHandle { get { return _overlayThumbnailHandle; } }

    protected string _overlayName = "OpenVR Overlay";
    public string overlayName 
    {
        get { return _overlayName; }
        set { _overlayName = value; }
    }

    protected string _overlayKey = "open_vr_overlay";
    public string overlayKey 
    {
        get {return _overlayKey; }
        set { _overlayKey = value; }
    }
    
    protected bool _created = false;
    public bool created { get { return _created; } }
    

    protected bool _focus = false;
    public bool focus { get { return _focus; } }


    protected bool _overlayHighQuality = false;
    public bool overlayHighQuality 
    {
        get 
        {
            if(OverlayExists && validHandle)
                _overlayHighQuality = ( Overlay.GetHighQualityOverlay() == _overlayHandle );
            
            return _overlayHighQuality;
        }
        set 
        {
            _overlayHighQuality = value;

            if(value && OverlayExists && validHandle)
                Overlay.SetHighQualityOverlay(_overlayHandle);
        }
    }

    protected Color _overlayColor = Color.white;
    public Color overlayColor 
    {
        get 
        {
            if(OverlayExists && validHandle)
            {
                float r = 0f, g = 0f, b = 0f;

                Overlay.GetOverlayColor(_overlayHandle, ref r, ref g, ref b);

                _overlayColor.r = r;
                _overlayColor.g = g;
                _overlayColor.b = b;
            }

            return _overlayColor;
        }
        set 
        {
            _overlayColor = value;

            if(OverlayExists && validHandle)
                Overlay.SetOverlayColor(_overlayHandle, _overlayColor.r, _overlayColor.g, _overlayColor.b);
        }
    }

    protected float _overlayAlpha = 1f;
    public float overlayAlpha 
    {
        get 
        { 
            if(OverlayExists && validHandle)
                Overlay.GetOverlayAlpha(_overlayHandle, ref _overlayAlpha);

            return _overlayAlpha;
        }

        set 
        {
            _overlayAlpha = value;

            if(OverlayExists && validHandle)
                Overlay.SetOverlayAlpha(_overlayHandle, _overlayAlpha);
        }
    }

    protected float _overlayWidthInMeters = 0f;
    public float overlayWidthInMeters 
    {
        get 
        {
            if(OverlayExists && validHandle)
                Overlay.GetOverlayWidthInMeters(_overlayHandle, ref _overlayWidthInMeters);

            return _overlayWidthInMeters;
        }
        set 
        {
            _overlayWidthInMeters = value;

            if(OverlayExists && validHandle)
                Overlay.SetOverlayWidthInMeters(_overlayHandle, value);
        }
    }

    // Skipping overlayAutoCurveDistanceRangeInMeters

    protected VRTextureBounds_t _overlayTextureBounds = new VRTextureBounds_t();
    public VRTextureBounds_t overlayTextureBounds 
    {
        get 
        {
            if(OverlayExists && validHandle)
                Overlay.GetOverlayTextureBounds(_overlayHandle, ref _overlayTextureBounds);
                
            return _overlayTextureBounds;
        }
        set 
        {
            _overlayTextureBounds = value;

            if(OverlayExists && validHandle)
                Overlay.SetOverlayTextureBounds(_overlayHandle, ref _overlayTextureBounds);
        }
    }

    protected ETrackingUniverseOrigin _overlayTransformAbsoluteTrackingOrigin = ETrackingUniverseOrigin.TrackingUniverseStanding;
    public ETrackingUniverseOrigin overlayTransformAbsoluteTrackingOrigin 
    {
        get { return _overlayTransformAbsoluteTrackingOrigin; }
        set { _overlayTransformAbsoluteTrackingOrigin = value; }
    }

    protected VROverlayTransformType _overlayTransformType = VROverlayTransformType.VROverlayTransform_Absolute;
    public VROverlayTransformType overlayTransformType 
    {
        get 
        {
            if(OverlayExists && validHandle)
                Overlay.GetOverlayTransformType(_overlayHandle, ref _overlayTransformType);

            return _overlayTransformType;
        }
        set 
        {
            _overlayTransformType = value;
        }
    }

    protected uint _overlayTransformTrackedDeviceRelativeIndex = OpenVR.k_unTrackedDeviceIndexInvalid;
    public uint overlayTransformTrackedDeviceRelativeIndex 
    {
        get { return _overlayTransformTrackedDeviceRelativeIndex; }
        set { 
                _overlayTransformTrackedDeviceRelativeIndex = value;
                overlayTransform = _overlayTransform;
            }
    }
    protected HmdMatrix34_t _overlayTransform;
    public HmdMatrix34_t overlayTransform 
    {
        get 
        {
            if(OverlayExists && validHandle)
            {
                VROverlayTransformType type = _overlayTransformType;
                switch(type)
                {
                    default:
                    case VROverlayTransformType.VROverlayTransform_Absolute:

                        Overlay.GetOverlayTransformAbsolute(
                            _overlayHandle, 
                            ref _overlayTransformAbsoluteTrackingOrigin, 
                            ref _overlayTransform);

                    break;

                    case VROverlayTransformType.VROverlayTransform_TrackedDeviceRelative:

                        Overlay.GetOverlayTransformTrackedDeviceRelative(
                            _overlayHandle,
                            ref _overlayTransformTrackedDeviceRelativeIndex,
                            ref _overlayTransform);

                    break;
                }
            }
            
            return _overlayTransform;
        }

        set 
        {
            _overlayTransform = value;

            if(OverlayExists && validHandle)
            {
                VROverlayTransformType type = _overlayTransformType;
                switch(type)
                {
                    default:
                    case VROverlayTransformType.VROverlayTransform_Absolute:

                        Overlay.SetOverlayTransformAbsolute(
                            _overlayHandle, 
                            _overlayTransformAbsoluteTrackingOrigin, 
                            ref _overlayTransform);

                    break;

                    case VROverlayTransformType.VROverlayTransform_TrackedDeviceRelative:

                        Overlay.SetOverlayTransformTrackedDeviceRelative(
                            _overlayHandle,
                            _overlayTransformTrackedDeviceRelativeIndex,
                            ref _overlayTransform);

                    break;
                }
            }
        }
    }

    protected bool _overlayVisible = false;
    public bool overlayVisible 
    {
        get 
        {
            if(OverlayExists && validHandle)
                _overlayVisible = Overlay.IsOverlayVisible(_overlayHandle);
            
            return _overlayVisible;
        }

        set 
        {
            _overlayVisible = value;

            if(OverlayExists && validHandle)
                if(value)
                    Overlay.ShowOverlay(_overlayHandle);
                else
                    Overlay.HideOverlay(_overlayHandle);
        }
    }

    protected VROverlayInputMethod _overlayInputMethod = VROverlayInputMethod.None;
    public VROverlayInputMethod overlayInputMethod 
    {
        get 
        {
            if(OverlayExists && validHandle)
                Overlay.GetOverlayInputMethod(_overlayHandle, ref _overlayInputMethod);
            
            return _overlayInputMethod;
        }

        set 
        {
            _overlayInputMethod = value;

            if(OverlayExists && validHandle)
                Overlay.SetOverlayInputMethod(_overlayHandle, _overlayInputMethod);
        }
    }

    protected HmdVector2_t _overlayMouseScale = new HmdVector2_t();
    public HmdVector2_t overlayMouseScale 
    {
        get 
        {
            if(OverlayExists && validHandle)
                Overlay.GetOverlayMouseScale(_overlayHandle, ref _overlayMouseScale);

            return _overlayMouseScale;
        }

        set 
        {
            _overlayMouseScale.v0 = value.v0;
            _overlayMouseScale.v1 = value.v1;

            if(OverlayExists && validHandle)
                Overlay.SetOverlayMouseScale(_overlayHandle, ref _overlayMouseScale);
        }
    }

    protected ETextureType _overlayTextureType;
    public ETextureType overlayTextureType 
    { 
        get { return _overlayTextureType; } 
        set { _overlayTextureType = value; }
    }

    protected Texture _overlayTexture;
    protected Texture_t _overlayTexture_t = new Texture_t();
    public Texture overlayTexture 
    {
        set 
        {   
            if(!value)
                return;
                
            _overlayTexture = value;

            _overlayTexture_t.handle = _overlayTexture.GetNativeTexturePtr();
            _overlayTexture_t.eType = overlayTextureType;
            _overlayTexture_t.eColorSpace = EColorSpace.Auto;

            if(OverlayExists && validHandle)
                Overlay.SetOverlayTexture(_overlayHandle, ref _overlayTexture_t);
        }
    }

    private Texture _overlayThumbnailTexture;
    protected Texture_t _overlayThumbnailTexture_t = new Texture_t();
    public Texture overlayThumbnailTexture 
    {
        set 
        {   
            if(!_overlayIsDashboard || !value)
                return;
                
            _overlayThumbnailTexture = value;

            _overlayThumbnailTexture_t.handle = _overlayThumbnailTexture.GetNativeTexturePtr();
            _overlayThumbnailTexture_t.eType = overlayTextureType;
            _overlayThumbnailTexture_t.eColorSpace = EColorSpace.Auto;

            if(OverlayExists && validHandle)
                Overlay.SetOverlayTexture(_overlayThumbnailHandle, ref _overlayThumbnailTexture_t);
        }
    }

    protected Vector2 _mousePos = new Vector2();
    public Vector2 overlayMousePosition { get { return _mousePos; } }

    protected bool _mouseLeftDown = false;
    public bool overlayMouseLeftDown { get { return _mouseLeftDown; } }

    protected bool _mouseRightDown = false;
    public bool overlayMouseRightDown { get { return _mouseRightDown; } }
    


    protected EVROverlayError error;
    protected VREvent_t pEvent;

    public OVR_Overlay()
    {
        OVR_Overlay_Handler.instance.RegisterOverlay(this);
    }

    ~OVR_Overlay()
    {
        OVR_Overlay_Handler.instance.DeregisterOverlay(this);
        DestroyOverlay();
    }

    public virtual bool CreateOverlay()
    {
        if(!OverlayExists)
            return ( _created = false );

        if(_overlayIsDashboard)
            error = Overlay.CreateDashboardOverlay(_overlayKey, _overlayName, ref _overlayHandle, ref _overlayThumbnailHandle);
        else
            error = Overlay.CreateOverlay(_overlayKey, _overlayName, ref _overlayHandle);

        bool allGood = !ErrorCheck(error);

        return ( _created = allGood );
    }

    public void UpdateOverlay() 
    {
        while(PollNextOverlayEvent(ref pEvent))
            DigestEvent(pEvent);
    }

    public bool DestroyOverlay()
    {
        if(!_created || !OverlayExists || !validHandle)
            return true;   

        error = Overlay.DestroyOverlay(_overlayHandle);

        if(!ErrorCheck(error))
        {
            _created = false;
            return true;
        }
        else
            return false;
    }

    public bool UpdateCurrentOverlay()
    {
        overlayHighQuality = _overlayHighQuality;
        overlayColor = _overlayColor;
        overlayAlpha = _overlayAlpha;
        overlayWidthInMeters = _overlayWidthInMeters;
        overlayTextureBounds = _overlayTextureBounds;

        overlayTransformType = _overlayTransformType;
        overlayTransform = _overlayTransform;
        overlayTransformAbsoluteTrackingOrigin = _overlayTransformAbsoluteTrackingOrigin;
        overlayTransformTrackedDeviceRelativeIndex = _overlayTransformTrackedDeviceRelativeIndex;

        overlayInputMethod = _overlayInputMethod;
        overlayVisible = _overlayVisible;

        overlayTexture = _overlayTexture;

        return !ErrorCheck(error);
    }

    public bool HideOverlay() 
    {
        overlayVisible = false;
        return !ErrorCheck(error);
    }
    public bool ShowOverlay() 
    {
        overlayVisible = true;
        return !ErrorCheck(error);
    }

    public bool ClearOverlayTexture()
    {
        if(OverlayExists && validHandle)
            error = Overlay.ClearOverlayTexture(_overlayHandle);
        
        return !ErrorCheck(error);
    }

    protected bool PollNextOverlayEvent(ref VREvent_t pEvent)
    {
		if (!OverlayExists)
			return false;

		var size = (uint)System.Runtime.InteropServices.Marshal.SizeOf(typeof(Valve.VR.VREvent_t));

		return Overlay.PollNextOverlayEvent(_overlayHandle, ref pEvent, size);
    }

    protected virtual void DigestEvent(VREvent_t pEvent)
    {
        EVREventType eventType = (EVREventType) pEvent.eventType;
        switch(eventType)
        {
            case EVREventType.VREvent_MouseMove:
                UpdateMouseData(pEvent.data.mouse);
            break;

            case EVREventType.VREvent_MouseButtonDown:
                UpdateMouseData(pEvent.data.mouse, true);
            break;

            case EVREventType.VREvent_MouseButtonUp:
                UpdateMouseData(pEvent.data.mouse, false);
            break;

            case EVREventType.VREvent_FocusEnter:
                _focus = true;
            break;

            case EVREventType.VREvent_FocusLeave:
                _focus = false;
            break;

            default:
                Debug.Log("Overlay - " + overlayName + " - : " + eventType);
            break;
        }
    }

    protected void UpdateMouseData(VREvent_Mouse_t mD)
    {
        _mousePos.x = mD.x;
        _mousePos.y = mD.y;
    }
    protected void UpdateMouseData(VREvent_Mouse_t mD, bool state)
    {
        UpdateMouseData(mD);

        switch((EVRMouseButton) mD.button)
        {
            case EVRMouseButton.Left:
                _mouseLeftDown = state;
            break;

            case EVRMouseButton.Right:
                _mouseRightDown = state;
            break;
        }
    }

    protected bool ErrorCheck(EVROverlayError error)
    {
        bool err = (error != EVROverlayError.None);

        if(OverlayExists)
            Debug.Log("Error: " + Overlay.GetOverlayErrorNameFromEnum(error));

        return err;
    }
}
