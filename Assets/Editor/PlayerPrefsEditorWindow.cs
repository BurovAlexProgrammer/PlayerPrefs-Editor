using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System;
using System.Reflection;
using System.Linq;

public class PlayerPrefsEditorWindow : EditorWindow {
    string companyName;
    string productName;
    List<PlayerPrefItem> originPlayerPrefs = new List<PlayerPrefItem>();
    List<PlayerPrefItem> playerPrefs = new List<PlayerPrefItem>();

    readonly string[] PlayerPrefTypes = new string[3] { "Float", "Integer", "String" };

    public class PlayerPrefItem {
        static int _newIndex = 0;
        static int newIndex { get { return _newIndex++; } }
        public PlayerPrefItem(string type, string key, object value) {
            this.index = newIndex;
            this.type = type;
            this.key = key;
            this.value = value;
        }
        public int index;
        public string key;
        public object value;
        public string type;
        public string state;

        public PlayerPrefItem Clone() {
            var clonedItem = new PlayerPrefItem(this.type, this.key, this.value);
            clonedItem.index = this.index;
            return clonedItem;
        }
    }

    [MenuItem("Window/PlayerPrefs Editor")]
    public static void ShowWindow() {
        var editorWindow = GetWindow<PlayerPrefsEditorWindow>("PlayerPrefs");
        editorWindow.Close(); //temp for reload window  //TODO delete
        editorWindow = GetWindow<PlayerPrefsEditorWindow>("PlayerPrefs");
        editorWindow.Show();
    }

    private void Awake() {
        companyName = PlayerSettings.companyName;
        productName = PlayerSettings.productName;
        var pathToPrefs = "";
        pathToPrefs = $@"SOFTWARE\Unity\UnityEditor\{companyName}\{productName}";
        LoadPlayerPrefs();
    }

    //Rect rect1 = new Rect(0, 0, 300, 50);
    private void OnGUI() {

        //On data changed
        //EditorGUI.BeginChangeCheck();
        //item.key = EditorGUILayout.TextField(item.key, GUILayout.Width(200));
        //if (EditorGUI.EndChangeCheck()) {
        //    Debug.Log("Changed");
        //}
        var unsavedChanges = false;

        GUIStyle playerPrefCardStyle = new GUIStyle(GUI.skin.box);

        GUIStyle commonStateStyle = new GUIStyle(GUI.skin.label) { fontStyle = FontStyle.Bold, fontSize = 14 };
        GUIStyle newStateStyle = new GUIStyle(commonStateStyle);
        newStateStyle.normal.textColor = Color.green;
        GUIStyle editStateStyle = new GUIStyle(commonStateStyle);
        editStateStyle.normal.textColor = Color.yellow;

        GUILayout.BeginVertical();
        DrawTestToolbar();

        foreach (PlayerPrefItem item in playerPrefs) {
            var originItem = originPlayerPrefs.DefaultIfEmpty(null).FirstOrDefault((x) => x.index == item.index);
            //if new key
            if (originItem == null) {
                item.state = "+";
                unsavedChanges = true;
            }
            //if changed else not changed
            else {
                if (!originItem.key.Equals(item.key) || !originItem.value.Equals(item.value) || !originItem.type.Equals(item.type)) {
                    item.state = "*";
                    unsavedChanges = true;
                }
                else
                    item.state = "";
            }

            GUILayout.BeginHorizontal();

            EditorGUILayout.LabelField(item.state, item.state == "+" ? newStateStyle : item.state == "*" ? editStateStyle : commonStateStyle, GUILayout.Width(13));
            item.key = EditorGUILayout.TextField(item.key, GUILayout.Width(200));
            var typeIndex = EditorGUILayout.Popup("", GetTypesIndex(item.type), PlayerPrefTypes, GUILayout.Width(70));
            item.type = typeIndex == -1 ? "" : PlayerPrefTypes[typeIndex];
            if (item.type == "String")
                item.value = EditorGUILayout.TextField(ConvertToString(item.value), GUILayout.Width(200));
            if (item.type == "Integer")
                item.value = EditorGUILayout.IntField(ConvertToInt(item.value), GUILayout.Width(200));
            if (item.type == "Float")
                item.value = EditorGUILayout.FloatField(ConvertToFloat(item.value), GUILayout.Width(200));
            GUILayout.EndHorizontal();
            GUILayout.Space(4);
        }
        
        GUILayout.EndVertical();
    }

    string ConvertToString(object obj) {
        return obj?.ToString() ?? "";
    }

    int ConvertToInt(object obj) {
        var line = ConvertToString(obj);
        int result;
        if (int.TryParse(line, out result)) 
            return result;
        return 0;
    }
    float ConvertToFloat(object obj) {
        var line = ConvertToString(obj);
        float result;
        if (float.TryParse(line, out result))
            return result;
        return 0;
    }

    int GetTypesIndex(string type) {
        return PlayerPrefTypes.ToList().FindIndex(x => x == type);
    }

    void DrawTestToolbar() {
        if (GUILayout.Button("Reload"))
            Awake();
        GUILayout.BeginHorizontal();
        if (GUILayout.Button("Int"))
            PlayerPrefs.SetInt("int", 123);
        if (GUILayout.Button("Float"))
            PlayerPrefs.SetFloat("float", 76.765f);
        if (GUILayout.Button("String"))
            PlayerPrefs.SetString("string", "some text");
        GUILayout.EndHorizontal();
    }

    void LoadPlayerPrefs() {
        originPlayerPrefs.Clear();
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
                    var t = regKey.GetType();
                    int index = key.LastIndexOf("_");
                    if (index == -1) {
                        Debug.LogError("Not correct reg value");
                        continue;
                    }
                    key = key.Remove(index, key.Length - index);
                    //string
                    if (value.GetType() == typeof(byte[])) {
                        var stringValue = PlayerPrefs.GetString(key);
                        originPlayerPrefs.Add(new PlayerPrefItem("String", key, stringValue));
                    }
                    //int, float
                    if (value.GetType() == typeof(int)) {
                        //if GetInt returns default twice then it is float
                        if (PlayerPrefs.GetInt(key, 0) == 0 && PlayerPrefs.GetInt(key, -1) == -1) {
                            var floatValue = PlayerPrefs.GetFloat(key);
                            originPlayerPrefs.Add(new PlayerPrefItem("Float", key, floatValue));
                        }
                        else {
                            var intValue = PlayerPrefs.GetInt(key);
                            originPlayerPrefs.Add(new PlayerPrefItem("Integer", key, intValue));
                        }
                    }
                }
            }
        }
        playerPrefs.Clear();
        playerPrefs = originPlayerPrefs.Select(x => x.Clone()).ToList();
        playerPrefs.Add(new PlayerPrefItem("Integer", "testt", 123));

    }

    /// <summary>
    /// Return last control ID setted in GUI
    /// </summary>
    /// <returns>Last control ID setted</returns>
    public static int GetLastControlId() {
        FieldInfo getLastControlId = typeof(EditorGUIUtility).GetField("s_LastControlID", BindingFlags.Static | BindingFlags.NonPublic);
        if (getLastControlId != null)
            return (int)getLastControlId.GetValue(null);
        return 0;
    }

}

