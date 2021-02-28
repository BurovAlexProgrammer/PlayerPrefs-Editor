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
    PlayerPrefItem newPlayerPref = new PlayerPrefItem();

    readonly string[] PlayerPrefTypes = new string[3] { "Float", "Integer", "String" };
    string selectedNewPrefType = "String";
    bool unsavedChanges = false;

    public class PlayerPrefItem {
        static int _newIndex = 0;
        static int newIndex { get { return _newIndex++; } }
        public PlayerPrefItem() {
            this.index = newIndex;
            this.type = "String";
            this.key = "";
            this.value = "";
        }
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
        //}
        int? indexForRemove = null;

        //Styles
        var commonStateStyle = new GUIStyle(GUI.skin.label) { fontStyle = FontStyle.Bold, fontSize = 14 };
        var newStateStyle = new GUIStyle(commonStateStyle);
        newStateStyle.normal.textColor = Color.green;
        var editStateStyle = new GUIStyle(commonStateStyle);
        editStateStyle.normal.textColor = Color.yellow;


        GUILayout.BeginVertical();
        GUILayout.Space(4);
        //DrawTestToolbar();

        DrawTopButtons();

        DrawToolbar();
        GUILayout.Space(4);
        DrawHeaders();

        GUILayout.Space(4);
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

            GUILayout.BeginHorizontal(); {
                //Status
                EditorGUILayout.LabelField(item.state, item.state == "+" ? newStateStyle : item.state == "*" ? editStateStyle : commonStateStyle, GUILayout.Width(13));
                //Key
                item.key = EditorGUILayout.TextField(item.key, GUILayout.Width(200));
                //Type
                var typeIndex = EditorGUILayout.Popup("", GetTypesIndex(item.type), PlayerPrefTypes, GUILayout.Width(70));
                item.type = typeIndex == -1 ? "" : PlayerPrefTypes[typeIndex];
                //Value
                if (item.type == "String")
                    item.value = EditorGUILayout.TextField(ConvertToString(item.value), GUILayout.Width(200));
                if (item.type == "Integer")
                    item.value = EditorGUILayout.IntField(ConvertToInt(item.value), GUILayout.Width(200));
                if (item.type == "Float")
                    item.value = EditorGUILayout.FloatField(ConvertToFloat(item.value), GUILayout.Width(200));
                //Button remove
                if (GUILayout.Button("X", GUILayout.Width(30))) {
                    indexForRemove = item.index;
                }
            } GUILayout.EndHorizontal();
            GUILayout.Space(4);
        }
        
        GUILayout.EndVertical();
        //Removing an item
        if (indexForRemove != null) {
            unsavedChanges = true;
            playerPrefs.Remove(playerPrefs.Where(x => x.index == indexForRemove).First());
            indexForRemove = null;
        }
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

    void DrawTopButtons() {
        var redColor = new Color(0.7f, 0.2f, 0.2f);
        var buttonSaveStyle = new GUIStyle(GUI.skin.button);
        buttonSaveStyle.fontStyle = FontStyle.Bold;
        buttonSaveStyle.normal.textColor = redColor;
        buttonSaveStyle.focused.textColor = redColor;

        GUILayout.BeginHorizontal();
        if (unsavedChanges) {
            if (GUILayout.Button("Save", buttonSaveStyle, GUILayout.Width(100)))
                SaveItems();
            if (GUILayout.Button("Reset", GUILayout.Width(100)))
                Reload();
        }
        GUILayout.FlexibleSpace();
        if (GUILayout.Button("Reload", GUILayout.Width(100)))
            Reload();
        GUILayout.Space(6);
        GUILayout.EndHorizontal();
    }

    void SaveItems() {
        unsavedChanges = false;
    }

    void Reload() {
        unsavedChanges = false;
        Awake();
    }

    void ValidateAndCreate(PlayerPrefItem item) {
        var hasIssues = false;
        if (String.IsNullOrEmpty(item.key.Trim())) {
            hasIssues = true;
            ShowMessage("Enter key for the new item.");
        }

        if (!hasIssues) {
            //TODO Adding item
        }

    }

    void ShowMessage(string message) {
        //TODO show dialog window
    } 

    void DrawHeaders() {
        var headerStyle = new GUIStyle(GUI.skin.label);
        headerStyle.fontStyle = FontStyle.Bold;
        headerStyle.padding = new RectOffset(4, 0, 0, 0);
        headerStyle.border.right = 2;

        GUILayout.BeginHorizontal();
        {
            GUILayout.Space(20);
            EditorGUILayout.LabelField("Key", headerStyle, GUILayout.Width(200));
            EditorGUILayout.LabelField("Type", headerStyle, GUILayout.Width(70));
            EditorGUILayout.LabelField("Value", headerStyle, GUILayout.Width(200));
        }
        GUILayout.EndHorizontal();
    }

    void DrawToolbar() {
        //styles
        var commonButtonStyle = new GUIStyle(GUI.skin.button);
        commonButtonStyle.fixedWidth = 60;
        var selectedButtonStyle = new GUIStyle(commonButtonStyle);
        selectedButtonStyle.normal.textColor = Color.yellow;
        var unselectedButtonStyle = new GUIStyle(commonButtonStyle);
        unselectedButtonStyle.normal.textColor = Color.white;
        var createButtonStyle = new GUIStyle(GUI.skin.button);
        createButtonStyle.normal.textColor = Color.green;
        createButtonStyle.focused.textColor = Color.green;
        createButtonStyle.fixedWidth = 100;
        //createButtonStyle.stretchWidth = true;

        var newItemStyle = new GUIStyle(GUI.skin.button);
        newItemStyle.normal.background = Texture2D.grayTexture;

        GUILayout.BeginVertical(newItemStyle);
        {
            GUILayout.Space(4);
            GUILayout.Label("New item");
            GUILayout.Space(4);
            GUILayout.BeginHorizontal();
            {
                GUILayout.Space(10);
                newPlayerPref.key = EditorGUILayout.TextField(newPlayerPref.key, GUILayout.Width(200));
                var typeIndex = 0;
                typeIndex = EditorGUILayout.Popup(typeIndex, PlayerPrefTypes, GUILayout.Width(70));
                //TODO issue: does not save selected type
                selectedNewPrefType = typeIndex == -1 ? "" : PlayerPrefTypes[typeIndex];
                newPlayerPref.type = selectedNewPrefType;
                if (selectedNewPrefType == "String")
                    newPlayerPref.value = EditorGUILayout.TextField(ConvertToString(newPlayerPref.value), GUILayout.Width(200));
                if (selectedNewPrefType == "Float")
                    newPlayerPref.value = EditorGUILayout.FloatField(ConvertToFloat(newPlayerPref.value), GUILayout.Width(200));
                if (selectedNewPrefType == "Integer")
                    newPlayerPref.value = EditorGUILayout.IntField(ConvertToInt(newPlayerPref.value), GUILayout.Width(200));
                GUILayout.FlexibleSpace();
                if (GUILayout.Button("Create", createButtonStyle)) 
                    ValidateAndCreate(newPlayerPref);
            }
            GUILayout.EndHorizontal();
        }
        GUILayout.Space(8);
        GUILayout.EndVertical();
    }

    void DrawTestToolbar() {
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
        //playerPrefs.Add(new PlayerPrefItem("Integer", "testt", 123)); //for test

    }
}

