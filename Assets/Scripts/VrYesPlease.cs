using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR;

public class VrYesPlease : MonoBehaviour {
    bool isVrEnabled = true;
	// Use this for initialization
	void Start () {
        if (isVrEnabled)
        {
            StartCoroutine(vrSettingActivor("cardboard"));
            isVrEnabled = false;
        }
        else
        {
            StartCoroutine(vrSettingActivor("none"));
            isVrEnabled = false;
        }
	}

    private IEnumerator vrSettingActivor(string device)
    {
        XRSettings.LoadDeviceByName(device);
        yield return null;
        XRSettings.enabled = isVrEnabled;
    }
}
