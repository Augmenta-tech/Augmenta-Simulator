using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mono.Zeroconf;

public class ZeroconfManager : MonoBehaviour
{
    private RegisterService service;

    private string _name = "Simulator";
    private int _port = 36278;
    private Dictionary<string, string> _keys;

    void OnDisable() {

        if (service != null) DestroyService();
    }

    void UpdateService()
    {
        if (service != null) DestroyService();

        service = new RegisterService();
        service.Name = "Augmenta - " + _name;
        service.RegType = "_osc._udp";
        service.ReplyDomain = "local.";
        service.UPort = (ushort)_port;

        if (_keys != null && _keys.Count > 0)
        {
            TxtRecord txt = new TxtRecord();
            foreach (KeyValuePair<string, string> kv in _keys) {
                txt.Add(kv.Key, kv.Value);
            }
            service.TxtRecord = txt;
        }

        service.Register();
    }

    void DestroyService() {

        service.Dispose();
    }

    public void Setup(int newPort = 36278, string newName = "Simulator", Dictionary<string, string> newKeys = null)
    {
        if(newKeys == null) {
            newKeys = new Dictionary<string, string>();
            newKeys.Add("mac", NetworkManager.GetMacAddress());
        }

        _name = newName;
        _port = newPort;
        _keys = newKeys;
        UpdateService();
    }
}
