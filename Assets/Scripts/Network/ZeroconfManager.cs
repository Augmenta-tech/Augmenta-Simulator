using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mono.Zeroconf;

public class ZeroconfManager : MonoBehaviour
{
    private RegisterService service;

    private string _name = "name";

    void OnEnable() {

        CreateService();
    }

    void OnDisable() {

        DestroyService();
    }

    void CreateService() {

        if (!FindObjectOfType<NodeManager>())
            _name = "Augmenta Simulator";

        //_osc._udp.
        service = new RegisterService();
        service.Name = _name;
        service.RegType = "_osc._udp";
        service.ReplyDomain = "local.";
        service.UPort = 36278;

        service.Register();
    }

    void DestroyService() {

        service.Dispose();
    }

    public void UpdateName(string newName) {

        _name = newName;

        if(service != null)
            DestroyService();

        CreateService();
    }
}
