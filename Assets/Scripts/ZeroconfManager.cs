using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mono.Zeroconf;

public class ZeroconfManager : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        RegisterServices();   
    }

    void RegisterServices() {

        //_osc._udp.
        RegisterService service = new RegisterService();
        service.Name = "Augmenta Simulator";
        service.RegType = "_osc._udp";
        service.ReplyDomain = "local.";
        service.UPort = 36278;

        service.Register();
    }
}
