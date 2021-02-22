using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System;

public class PlayerPrefsEditorWindow : EditorWindow {
    string companyName;
    string productName;
    List<PlayerPrefItem> playerPrefs = new List<PlayerPrefItem>(); 

    enum PlayerPrefTypes { Float, Integer, String };

    [Serializable]
    public class PlayerPrefItem {
        string key;
        object value;
        PlayerPrefTypes type;
    }


    [MenuItem("Window/PlayerPrefs Editor")]
    public static void ShowWindow() {
        var editorWindow = GetWindow<PlayerPrefsEditorWindow>("PlayerPrefs");
        editorWindow.ShowUtility();
    }

    private void Awake() {
        companyName = PlayerSettings.companyName;
        productName = PlayerSettings.productName;
        var pathToPrefs = "";
        pathToPrefs = $@"SOFTWARE\Unity\UnityEditor\{companyName}\{productName}";

        LoadPlayerPrefs();
    }

    private void OnGUI() {
        GUILayout.BeginVertical();
        if (GUILayout.Button("Reload"))
            Awake();
        GUILayout.BeginHorizontal();
        if (GUILayout.Button("Int")) 
            PlayerPrefs.SetInt("int", UnityEngine.Random.Range(1, 100));
        if (GUILayout.Button("Float"))
            PlayerPrefs.SetFloat("float", UnityEngine.Random.Range(1, 100));
        if (GUILayout.Button("String"))
            PlayerPrefs.SetString("string", "some text");
        GUILayout.EndHorizontal();
        GUILayout.EndVertical();
    }

    void LoadPlayerPrefs() {
        if (Application.platform == RuntimePlatform.WindowsEditor) {
            //Windows PlayerPrefs located on registry: HKEY_CURRENT_USER\Software\<CompanyName>]\<ProductName>\keys.
#if UNITY_5_5_OR_NEWER
            Microsoft.Win32.RegistryKey regKey = Microsoft.Win32.Registry.CurrentUser.OpenSubKey($@"Software\Unity\UnityEditor\{companyName}\{productName}");
#else
            Microsoft.Win32.RegistryKey registryKey = Microsoft.Win32.Registry.CurrentUser.OpenSubKey($@"Software\{companyName}\productName}");
#endif
            if (regKey != null) {
                string[] keys = regKey.GetValueNames();
                foreach (var _key in keys) {
                    string key = _key;
                    var value = regKey.GetValue(key);
                    int index = key.LastIndexOf("_");
                    if (index == -1) {
                        Debug.LogError("Not correct reg value");
                        continue;
                    }
                    key = key.Remove(index, key.Length - index);
                }
            }
        }
    }


}
