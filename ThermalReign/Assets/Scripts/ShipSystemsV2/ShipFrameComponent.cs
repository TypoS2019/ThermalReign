using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Collections;
using UnityEngine;

public class ShipFrameComponent : MonoBehaviour
{
    [SerializeField] protected float bootTime;
    [SerializeField] protected float shutdownTime;
    [SerializeField] protected float rebootTime;

    [SerializeField, ReadOnly] private ComponentStatus status;
    [SerializeField] private bool _boot;
    [SerializeField] private bool _shutdown;
    [SerializeField] private bool _reboot;
    [SerializeField] private float _stateEntryTime;
    
    protected virtual void BootUpdate(){}

    protected virtual void OnlineUpdate(){}

    protected virtual void OfflineUpdate(){}

    protected virtual void ShutdownUpdate(){}

    protected virtual void RebootUpdate(){}


    public void Boot()
    {
        _boot = true;
    }

    public void Shutdown()
    {
        _shutdown = true;
    }
    
    public void Reboot()
    {
        _reboot = true;
    }

    protected void OnEnable()
    {
        StartCoroutine(Offline());
    }

    private IEnumerator Offline()
    {
        ProcessSystemStateSwitch(ComponentStatus.Offline);
        while (enabled)
        {
            if (_boot)
            {
                StartCoroutine(Booting());
                yield break;
            }
            OfflineUpdate();
            yield return null;
        }
    }

    private IEnumerator Online()
    {
        ProcessSystemStateSwitch(ComponentStatus.Online);
        while (enabled)
        {
            if (_shutdown)
            {
                StartCoroutine(ShuttingDown());
                yield break;
            }
            if (_reboot)
            {
                StartCoroutine(Rebooting());
                yield break;
            }
            OnlineUpdate();
            yield return null;
        }
    }

    private IEnumerator Booting()
    {
        ProcessSystemStateSwitch(ComponentStatus.Booting);
        while (enabled)
        {
            if (_stateEntryTime + bootTime <+ Time.time)
            {
                StartCoroutine(Online());
                yield break;
            }
            BootUpdate();
            yield return null;
        }
    }
    
    private IEnumerator ShuttingDown()
    {
        ProcessSystemStateSwitch(ComponentStatus.ShuttingDown);
        while (enabled)
        {
            if (_stateEntryTime + shutdownTime <+ Time.time)
            {
                StartCoroutine(Offline());
                yield break;
            }
            ShutdownUpdate();
            yield return null;
        }
    }
    
    private IEnumerator Rebooting()
    {
        ProcessSystemStateSwitch(ComponentStatus.Rebooting);
        while (enabled)
        {
            if (_stateEntryTime + rebootTime <+ Time.time)
            {
                StartCoroutine(Online());
                yield break;
            }
            RebootUpdate();
            yield return null;
        }
    }

    private void ProcessSystemStateSwitch(ComponentStatus newStatus)
    {
        _reboot = false;
        _boot = false;
        _shutdown = false;
        _stateEntryTime = Time.time;
        status = newStatus;
    }
    
    public enum ComponentStatus
    {
        Offline,
        Online,
        Booting,
        ShuttingDown,
        Rebooting
    }
}
