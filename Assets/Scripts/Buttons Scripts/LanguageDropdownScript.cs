using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class LanguageDropdownScript : MonoBehaviour {
    public void DropdownValueChanged() {
        int value = gameObject.GetComponent<TMP_Dropdown>().value;
        ChangeSceneClass.DropdownLanguageChange(value);
    }
}
