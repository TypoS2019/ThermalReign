using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using Ship;
using Thermal;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(ShipMainframe))]
public class HUDManager : MonoBehaviour
{
    [SerializeField] private Camera activeCamera;
    [SerializeField] private List<Camera> cameras = new List<Camera>();
    [SerializeField] private Texture genericMarker;
    [SerializeField] private Texture projectileMarker;
    [SerializeField] private Texture shipMarker;
    [SerializeField] private Texture targetShipMarker;

    [SerializeField] private GUISkin skin;
    [SerializeField] private Canvas Canvas;
    private ShipMainframe _mainframe;
    
    
    private void OnValidate()
    {
        if (activeCamera != null && !cameras.Contains(activeCamera))
        {
            cameras.Add(activeCamera);
        }
    }

    private void Awake()
    {
        _mainframe = GetComponent<ShipMainframe>();
    }

    private void OnGUI()
    {
        AddSignatureMarkersGUI();
        AddSystemsGUI();
        AddThermalGUI();
        AddRadarGUI();
    }

    private void AddSystemsGUI()
    {
        
    }

    private void AddThermalGUI()
    {
        
    }

    private void AddRadarGUI()
    {
        
    }
    
    private void AddSignatureMarkersGUI()
    {
        foreach (var signature in _mainframe.SignatureInfos)
        {
            float angle = Vector3.Angle(activeCamera.transform.forward, signature.Value.LastKnownLocation - activeCamera.transform.position);
            if (Mathf.Abs(angle) < 90 && signature.Value.LastTimeSeen + 5 > Time.time)
            {
                GUI.color = GetMarkerColor(signature.Key, signature.Value);
                Vector3 screenPoint = activeCamera.WorldToScreenPoint(signature.Value.LastKnownLocation);
                GUI.DrawTexture(GetMarkerRect(screenPoint, 32, 32), GetMarkerTexture(signature.Key, signature.Value));
                if (signature.Value.Type != SignatureType.GuidedProjectile)
                {
                    GUI.Label(new Rect(screenPoint.x + 17, Screen.height - screenPoint.y - 16, 200, 100), GetSignatureInfoText(signature.Value), skin.label);
                }
            }
        }
    }

    private Texture GetMarkerTexture(ThermalSignature signature, SignatureInfo info)
    {
        if (signature == _mainframe.target) return targetShipMarker;
        if (info!.Type == SignatureType.GuidedProjectile) return projectileMarker;
        if (info!.Type == SignatureType.Ship) return shipMarker;
        return genericMarker;
    }
    
    private String GetSignatureInfoText(SignatureInfo info)
    {
        StringBuilder builder = new StringBuilder();
        builder.Append("SIGNAL:").Append(info.Strength.ToString().ToUpper()).Append("\n");
        builder.Append(info.SignatureName != null ? "NAME:" + info.SignatureName + "\n" : "");
        builder.Append("TYPE:").Append(info.Type).Append("\n");
        builder.Append("DISTANCE:").Append(Vector3.Distance(info.LastKnownLocation, transform.position)).Append(" m\n");
        return builder.ToString();
    }
    
    private Rect GetMarkerRect(Vector2 position, float width, float height)
    {
        return new Rect(position.x - width/2, Screen.height - position.y - height/2, width, height);
    }

    private Color GetMarkerColor(ThermalSignature signature, SignatureInfo info)
    {
        if (!signature.Equals(_mainframe.target))
        {
            if (info.Strength > SignatureStrength.Lost)
            {
                switch (info.Allegiance)
                {
                    case Allegiance.Enemy:
                        return Color.red;
                    case Allegiance.Player:
                        return Color.green;
                    default:
                        return Color.white;
                }
            }
            return Color.gray;
        }
        return Color.cyan;
    }
}
