using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HelpController : MonoBehaviour
{
    public GameObject HelpText;

    private void Start() {
        HelpText.SetActive(false);
    }

    private void Update() {

        if (Input.GetKeyUp(KeyCode.F1))
            ToggleHelp();
    }

    public void ToggleHelp() {
        HelpText.SetActive(!HelpText.activeInHierarchy);
    }
}
