using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mono.Zeroconf;

public class ZeroconfManager : MonoBehaviour
{
    private RegisterService service;

    private string _name = "name";
    Dictionary<string, string> _keys;

    void OnEnable() {

        _name = "Simulator";
        UpdateService();
    }

    void OnDisable() {

        DestroyService();
    }

    void UpdateService()
    {
        if (service != null) DestroyService();
        service = new RegisterService();
        service.Name = "Augmenta - " + _name;
        service.RegType = "_osc._udp";
        service.ReplyDomain = "local.";
        service.UPort = 36278;

        if (_keys != null && _keys.Count > 0)
        {
            TxtRecord txt = new TxtRecord();
            foreach (KeyValuePair<string, string> kv in _keys) txt.Add(kv.Key, kv.Value);
            service.TxtRecord = txt;
        }

        service.Register();
    }

    void DestroyService() {

        service.Dispose();
    }

    public void Setup(string newName, Dictionary<string, string> newKeys = null)
    {
        _name = newName;
        _keys = newKeys;
        UpdateService();
    }
}
