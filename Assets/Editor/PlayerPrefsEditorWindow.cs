using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

public class PlayerPrefsEditorWindow : EditorWindow {
    string companyName;
    string productName;
    public static PlayerPrefsEditorWindow instance;

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
            PlayerPrefs.SetInt("int", Random.Range(1, 100));
        if (GUILayout.Button("Float"))
            PlayerPrefs.SetFloat("float", Random.Range(1, 100));
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
            }
        }
    }
}
