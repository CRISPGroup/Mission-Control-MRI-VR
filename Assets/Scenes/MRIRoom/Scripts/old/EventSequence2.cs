/*
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
#if UNITY_EDITOR
using UnityEditor;
using UnityEditorInternal;
#endif

public class EventSequence2 : MonoBehaviour
{

    //A class for setting up a sequence of events.
    //Key to this is calling SetWaitVar(string, bool) from other classes so the sequence can continue.
    //Example:
    //  You enable another object using an event entry, and only want to continue if that object finishes doing something.
    //  So you add a WaitVar entry with say 'PlayerOpenedDoor'.
    //  And have the other object call SetWaitVar("PlayerOpenedDoor", true) once it's done.
    //  Now the sequence continues.

    [SerializeField] List<SequenceEntry> SequenceEntries = new List<SequenceEntry>();
    [SerializeField] UnityEvent SequenceComplete;
    [SerializeField] string Notes;
    [SerializeField] bool StartOnAwake = true;

    Dictionary<string, bool> WaitVars;
    Coroutine SequenceCoroutine;
    bool Paused;
    float WaitTimer;
    int SequenceIndex;
    bool SequenceStarted;

    void Awake()
    {
        if (!StartOnAwake) return;
        StartSequence();
    }
    public void StartSequence()
    {
        if (SequenceEntries == null || SequenceEntries.Count == 0 || SequenceStarted) return;
        SequenceStarted = true;
        WaitVars = new Dictionary<string, bool>();

        foreach (SequenceEntry entry in SequenceEntries)
        {
            if (entry.Type == SequenceEntry.EntryType.WaitVar)
            {
                if (!WaitVars.ContainsKey(entry.WaitVar))
                    WaitVars.Add(entry.WaitVar, false);
            }
        }

        SequenceCoroutine = StartCoroutine(SequenceEnumerator());
    }

    public void WaitVarToggle(string waitVar)
    {
        if (WaitVars.ContainsKey(waitVar))
        {
            WaitVars[waitVar] = !WaitVars[waitVar];
        }
    }
    public void WaitVarEnable(string waitVar)
    {
        Debug.Log("WaitVarEnable " + waitVar);
        if (WaitVars.ContainsKey(waitVar))
        {
            WaitVars[waitVar] = true;
        }
    }
    public void WaitVarDisable(string waitVar)
    {
        if (WaitVars.ContainsKey(waitVar))
        {
            WaitVars[waitVar] = false;
        }
    }

    IEnumerator SequenceEnumerator()
    {

        //process sequence
        foreach (SequenceEntry entry in SequenceEntries)
        {
            switch (entry.Type)
            {
                case SequenceEntry.EntryType.Event:
                    entry.Event.Invoke();
                    break;
                case SequenceEntry.EntryType.WaitTime:
                    yield return new WaitForSecondsRealtime(entry.WaitTime);
                    break;
                case SequenceEntry.EntryType.WaitVar:
                    while (WaitVars[entry.WaitVar] != entry.WaitVal)
                        yield return null;
                    break;
            }
        }

        SequenceComplete.Invoke();
        yield return null;
    }

    public void SetPause(bool pause)
    {
        Paused = pause;

    }
    void Update()
    {
        if (Paused) return;
        if (WaitTimer > 0)
        {
            WaitTimer -= Time.deltaTime;
        }
    }

    [System.Serializable]
    public class SequenceEntry
    {
        public enum EntryType { WaitTime, WaitVar, Event }
        public EntryType Type;
        public float WaitTime;
        public string WaitVar;
        public bool WaitVal;
        public UnityEvent Event;
        public SequenceEntry()
        {
            Type = EntryType.Event;
            WaitVar = "VariableName";
            WaitTime = 0;
            Event = new UnityEvent();
        }
    }
}

//CUSTOM EDITOR & PROPERTYDRAWER
#if UNITY_EDITOR
[CustomEditor(typeof(EventSequence))]
public class EventSequenceEditor : Editor  {

    ReorderableList reorderableList;

    public override void OnInspectorGUI(){
        
        //initializing
        serializedObject.Update();
        if(reorderableList==null){
            reorderableList = new ReorderableList(serializedObject, serializedObject.FindProperty("SequenceEntries"), true, false, true, true); 
            reorderableList.drawElementCallback = (Rect rect, int index, bool isActive, bool isFocused) =>{
                rect.x+=5;
                EditorGUI.PropertyField(rect, serializedObject.FindProperty("SequenceEntries").GetArrayElementAtIndex(index), true);
            };
            reorderableList.elementHeightCallback += (int index)=>{ 
                float propertyHeight = EditorGUI.GetPropertyHeight(reorderableList.serializedProperty.GetArrayElementAtIndex(index), true);
                float spacing = EditorGUIUtility.singleLineHeight / 2;
                return propertyHeight + spacing;    
            };
        }


        //inspector drawing
        GUILayout.Space(10);
        
        EditorGUILayout.HelpBox("A component for setting up a sequence of events.\r\nKey to this is calling SetWaitVar(string, bool) from other classes so the sequence can continue.", MessageType.Info);
        
        GUILayout.Label("Sequence", EditorStyles.boldLabel);
        EditorGUILayout.PropertyField(serializedObject.FindProperty("StartOnAwake"));
        reorderableList.DoLayoutList();
        GUILayout.Label("On complete event", EditorStyles.boldLabel);
        EditorGUILayout.PropertyField(serializedObject.FindProperty("SequenceComplete"));
        
        GUILayout.Label("Notes (multiline)", EditorStyles.boldLabel);
        serializedObject.FindProperty ("Notes").stringValue = EditorGUILayout.TextArea(serializedObject.FindProperty ("Notes").stringValue);
        GUILayout.Space(10);

        //cleanup
        serializedObject.ApplyModifiedProperties();
    }
	
}

[CustomPropertyDrawer(typeof(EventSequence.SequenceEntry))]
public class SequenceEntry : PropertyDrawer{ 
        
    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label){
        EditorGUI.BeginProperty(position, label, property);
        int indent = EditorGUI.indentLevel;
        EditorGUI.indentLevel = 0;
        
        Rect typeRect = new Rect(position.x, position.y, 75, position.height);
        Rect varRect  = new Rect(position.x + 80, position.y, position.width-85, position.height-5);

        EventSequence.SequenceEntry.EntryType type = (EventSequence.SequenceEntry.EntryType)property.FindPropertyRelative("Type").enumValueIndex;
        EditorGUI.PropertyField(typeRect, property.FindPropertyRelative("Type"), GUIContent.none);
        switch(type){ 
            case EventSequence.SequenceEntry.EntryType.Event:
                EditorGUI.PropertyField(varRect, property.FindPropertyRelative("Event"), GUIContent.none);
            break;
            case EventSequence.SequenceEntry.EntryType.WaitTime:
                EditorGUI.PropertyField(varRect, property.FindPropertyRelative("WaitTime"), GUIContent.none);
            break;
            case EventSequence.SequenceEntry.EntryType.WaitVar:
                Rect varValRect  = new Rect(position.x + 80, position.y, 10, position.height-5);
                varRect  = new Rect(position.x + 100, position.y, position.width-85, position.height-5);
                EditorGUI.PropertyField(varValRect, property.FindPropertyRelative("WaitVal"), GUIContent.none);
                EditorGUI.PropertyField(varRect, property.FindPropertyRelative("WaitVar"), GUIContent.none);
            break;
        }
        
        EditorGUI.indentLevel = indent;
        EditorGUI.EndProperty();
    }
    
    public override float GetPropertyHeight (SerializedProperty property, GUIContent label) {
 
        float propertyHeight=0;
        propertyHeight += EditorGUI.GetPropertyHeight(property.FindPropertyRelative("Type"));
        EventSequence.SequenceEntry.EntryType type = (EventSequence.SequenceEntry.EntryType)property.FindPropertyRelative("Type").enumValueIndex;
        if(type ==EventSequence.SequenceEntry.EntryType.Event)
             propertyHeight = EditorGUI.GetPropertyHeight(property.FindPropertyRelative("Event"));
        
        return propertyHeight;
    }
}
#endif
*/