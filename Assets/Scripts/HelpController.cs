using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HelpController : MonoBehaviour
{
    public GameObject helpText;

    private void Start() {
        helpText.SetActive(false);
    }

    private void Update() {

        if (Input.GetKeyUp(KeyCode.F1))
            ToggleHelp();
    }

    public void ToggleHelp() {
        helpText.SetActive(!helpText.activeInHierarchy);
    }
}
