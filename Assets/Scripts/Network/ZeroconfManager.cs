using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mono.Zeroconf;

public class ZeroconfManager : MonoBehaviour
{
    private RegisterService service;

    void OnEnable() {

        //_osc._udp.
        service = new RegisterService();
        service.Name = "Augmenta Simulator";
        service.RegType = "_osc._udp";
        service.ReplyDomain = "local.";
        service.UPort = 36278;

        service.Register();
    }

    void OnDisable() {

        service.Dispose();

    }
}
