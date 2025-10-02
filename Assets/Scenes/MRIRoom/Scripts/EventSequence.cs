using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using Malee.List;

#if UNITY_EDITOR
using UnityEditor;
#endif

public class EventSequence : MonoBehaviour {
    [SerializeField] List<SequenceEntry> SequenceEntries    = new List<SequenceEntry>();
    [SerializeField] UnityEvent SequenceComplete;
    [SerializeField] bool       StartOnAwake                = true;
    [SerializeField] bool       ClearTriggersEveryStep      = true;
    [SerializeField] bool       ClearTriggersAfterTrigger   = true;
    //RUNTIME
    [SerializeField] List<string>   Triggers;
    [SerializeField] int            SequenceStep;

    Coroutine   SequenceCoroutine;
    bool        Paused;
    float       WaitTimer;
    bool        SequenceStarted;
    UnityAction onComplete;
    [SerializeField] UnityEvent onDisable;

    void Awake(){ 
        if(!StartOnAwake) 
            return;
        StartSequence(null);
    }

    private void OnEnable()
    {
        if (SequenceStarted)
        {
            RestartSequence();
        }
    }

    private void OnDisable()
    {
        onDisable.Invoke();
    }

    [ContextMenu("Trigger sequence")]
    public void StartSequence() => StartSequence(null);
    public void StartSequence(UnityAction _onComplete){
        if(SequenceEntries==null || SequenceEntries.Count==0 || SequenceStarted) 
            return;

        onComplete        = _onComplete;
        SequenceStarted   = true;
        Triggers          = new List<string>();
        SequenceCoroutine = StartCoroutine(SequenceEnumerator());
    }

    /*
     *   public void RestartSequence()
    {
        if (SequenceCoroutine != null)
            StopCoroutine(SequenceCoroutine);

        SequenceStarted = false;
        SequenceStep = 0;
        Triggers.Clear();
        Paused = false;
        WaitTimer = 0;

        StartSequence();
    }
     * */



    public void RestartSequence()
    {
        if (SequenceCoroutine != null)
            StopCoroutine(SequenceCoroutine);
        // Reset the sequence-related fields
        SequenceStep = 0;
        Triggers.Clear();
        SequenceStarted = false;
        Paused = false;
        WaitTimer = 0;

        // Restart the sequence
        StartSequence(null);
    }

    public void SetTrigger(string trigger){

        //Debug.Log("Trigger received: "+trigger);
        if (trigger.Contains("|")){ 

            string part = trigger.Split('|')[0];

            for(int i =0;i<Triggers.Count;i++){ 
                if(Triggers[i].Split('|')[0]==part){
                    Triggers[i] = trigger;
                    return;
                }                    
            }
        }

        if(!Triggers.Contains(trigger)) 
            Triggers.Add(trigger);
    }

    IEnumerator SequenceEnumerator(){
        //process sequence
        SequenceEntry entry;
        for(int i=0;i<SequenceEntries.Count;i++){

            while(Paused)
                yield return null;


            SequenceStep = i;
            entry        = SequenceEntries[i];

            //Debug.Log("Starting step "+i+", type "+entry.Type);
            switch(entry.Type){ 
            //EVENT
                case SequenceEntry.EntryType.Event:
                    entry.Event.Invoke();
                break;
            //WAIT
                case SequenceEntry.EntryType.Wait:
                    //yield return new WaitForSecondsRealtime(entry.WaitTime);
                    yield return new WaitForSeconds(entry.WaitTime);
                break;
            //TRIGGER
                case SequenceEntry.EntryType.Trigger:{
                    
                    string tag="";
                    if(entry.Triggers.Count!=0){
                        if(entry.TriggerAND){
                        //AND
                            bool triggered = false;
                            while(triggered==false){ 
                                bool failed = false;
                                for(int j=0;j<entry.Triggers.Count;j++){ 
                                    if(!Triggers.Contains(entry.Triggers[j].Trigger)){
                                        failed = true;
                                        break;
                                    }
                                }
                                if(failed==false){
                                    triggered = true;
                                    tag = entry.Triggers[0].Tag;
                                }
                                if(!triggered)
                                    yield return null;
                            }
                        }else{
                        //OR
                            bool triggered = false;
                            while(triggered==false){ 
                                for(int j=0;j<entry.Triggers.Count;j++){ 
                                    if(Triggers.Contains(entry.Triggers[j].Trigger)){ 
                                        tag       = entry.Triggers[j].Tag;
                                        triggered = true;
                                        Triggers.Remove(entry.Triggers[j].Trigger);
                                        break;
                                    }
                                }
                                if(!triggered)
                                    yield return null;
                            }
                        }
                    }

                    if(ClearTriggersAfterTrigger)
                        Triggers.Clear();

                    if(tag!="" && tag!="next"){ 
                        if(tag=="start"){ 
                            i =- 1;
                        } else if(tag=="end"){ 
                            i = SequenceEntries.Count;
                        } else{
                            SequenceEntry found = SequenceEntries.Find(e=>e.Tag == tag);
                            if(found!=null)
                                i = SequenceEntries.IndexOf(found) -1;
                            else
                                Debug.LogWarning("Trigger tag not found, going to next.");
                        }
                    }
                break; }
            //GOTO
                case SequenceEntry.EntryType.Goto:{
                    if(entry.GotoTag!="" && entry.GotoTag!="next"){ 
                        if(entry.GotoTag=="start"){ 
                            i = -1;
                        } else if(entry.GotoTag=="end"){ 
                            i = SequenceEntries.Count;
                        } else{
                            SequenceEntry found = SequenceEntries.Find(e=>e.Tag == entry.GotoTag);
                            if(found!=null)
                                i = SequenceEntries.IndexOf(found) -1;
                            else
                                Debug.LogWarning("Trigger tag not found, going to next.");
                        }
                    }

                break;}
            }  
            
            if(ClearTriggersEveryStep)
                Triggers.Clear();
        }

        SequenceComplete.Invoke();
        if(onComplete!=null)
            onComplete.Invoke();
    }

    public void SetPause(bool pause){ 
        Paused = pause;   
    }
    void Update(){ 
        if(Paused) 
            return;
        if(WaitTimer>0)
            WaitTimer-=Time.deltaTime;
    }

    public void SetClearTriggersAfterTriggers(bool value){
        ClearTriggersAfterTrigger = value;
    }
    
    [System.Serializable]
    public class SequenceEntry{ 
        public enum EntryType{Wait, Trigger, Event, Goto}

        public EntryType    Type;
        public float        WaitTime;
        public List<TriggerTagPair> Triggers;
        public bool         TriggerAND;
        public UnityEvent   Event;
        public string       Tag;
        public string       GotoTag;
    }
    [System.Serializable]
    public class TriggerTagPair{ 
        public string Trigger;
        public string Tag;
    }
}

//CUSTOM EDITOR & PROPERTYDRAWER
#if UNITY_EDITOR
[CustomEditor(typeof(EventSequence))]
public class EventSequenceEditor : Editor  {

    ReorderableList reorderableList;
    SerializedProperty SequenceEntriesProperty;

    void OnEnable(){ 
        serializedObject.Update();
        SequenceEntriesProperty = serializedObject.FindProperty("SequenceEntries");
    }
    public override void OnInspectorGUI()
    {
        //initializing
        if (reorderableList == null)
        {
            reorderableList = new ReorderableList(SequenceEntriesProperty, true, true, true);
            reorderableList.drawElementCallback += (Rect rect, SerializedProperty element, GUIContent label, bool isActive, bool isFocused) =>
            {
                rect.x += 5;
                EditorGUI.PropertyField(rect, element, true);
            };
            reorderableList.getElementHeightCallback += (SerializedProperty element) =>
            {
                float propertyHeight = EditorGUI.GetPropertyHeight(element, true);
                float spacing = EditorGUIUtility.singleLineHeight / 2;
                return propertyHeight + spacing;
            };
            reorderableList.paginate = true;
            reorderableList.pageSize = 10;
        }

        //inspector drawing
        GUILayout.Space(10);
        EditorGUILayout.HelpBox("A component for setting up a sequence of events with 3 types of entries...\r\n", MessageType.Info);

        GUILayout.Space(10);
        EditorGUILayout.PropertyField(serializedObject.FindProperty("StartOnAwake"));
        GUILayout.BeginHorizontal();
        EditorGUILayout.PropertyField(serializedObject.FindProperty("ClearTriggersEveryStep"));
        EditorGUILayout.PropertyField(serializedObject.FindProperty("ClearTriggersAfterTrigger"));
        GUILayout.EndHorizontal();

        // Ajout de l'event onDisable
        GUILayout.Space(10);
        GUILayout.Label("On Disable Event", EditorStyles.boldLabel);
        EditorGUILayout.PropertyField(serializedObject.FindProperty("onDisable")); // Ceci affiche l'événement dans l'inspecteur

        //CURRENT TRIGGERS
        if (Application.isPlaying)
        {
            GUILayout.Space(10);
            if (GUILayout.Button("Start sequence"))
                (target as EventSequence).StartSequence(null);
            GUI.enabled = false;
            EditorGUILayout.PropertyField(serializedObject.FindProperty("SequenceStep"), true);
            EditorGUILayout.PropertyField(serializedObject.FindProperty("Triggers"), true);
            GUI.enabled = true;
        }

        GUILayout.Space(10);
        GUILayout.Label("Sequence", EditorStyles.boldLabel);
        reorderableList.DoLayoutList();
        GUILayout.Label("On complete event", EditorStyles.boldLabel);
        EditorGUILayout.PropertyField(serializedObject.FindProperty("SequenceComplete"));
        GUILayout.Space(10);

        //cleanup
        serializedObject.ApplyModifiedProperties();
    }

}

[CustomPropertyDrawer(typeof(EventSequence.SequenceEntry))]
public class SequenceEntryDrawer : PropertyDrawer{ 

    Rect typeRect, varRect, tagRect, modeRect;
        
    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label){

        EditorGUI.BeginProperty(position, label, property);
        int indent = EditorGUI.indentLevel;
        EditorGUI.indentLevel = 0;
        
        typeRect = new Rect(position.x, position.y, 75, position.height);
        varRect  = new Rect(position.x + 80, position.y, position.width-130, position.height-5);
        tagRect  = new Rect(position.width+5, position.y, 40, Mathf.Clamp(position.height-5, 10,20));

        EventSequence.SequenceEntry.EntryType type = (EventSequence.SequenceEntry.EntryType)property.FindPropertyRelative("Type").enumValueIndex;
        EditorGUI.PropertyField(typeRect, property.FindPropertyRelative("Type"), GUIContent.none);
        switch(type){ 
            case EventSequence.SequenceEntry.EntryType.Event:
                EditorGUI.PropertyField(varRect, property.FindPropertyRelative("Event"), GUIContent.none);
            break;
            case EventSequence.SequenceEntry.EntryType.Wait:
                EditorGUI.PropertyField(varRect, property.FindPropertyRelative("WaitTime"), GUIContent.none);
            break;
            case EventSequence.SequenceEntry.EntryType.Goto:
                EditorGUI.PropertyField(varRect, property.FindPropertyRelative("GotoTag"), GUIContent.none);
            break;
            case EventSequence.SequenceEntry.EntryType.Trigger:
                modeRect  = new Rect(position.x, position.y+20, 75, 15);
                EditorGUI.LabelField(modeRect, "AND mode");
                    
                modeRect  = new Rect(position.x+90, position.y+20, 10, 15);
                EditorGUI.PropertyField(modeRect, property.FindPropertyRelative("TriggerAND"), new GUIContent());
                varRect  = new Rect(position.x + 90, position.y, position.width-140, position.height-5);
                EditorGUI.PropertyField(varRect, property.FindPropertyRelative("Triggers"), new GUIContent("Trigger(|value) / GOTO tag"), true);
            break;
        }
        
        EditorGUI.PropertyField(tagRect, property.FindPropertyRelative("Tag"), GUIContent.none);

        EditorGUI.indentLevel = indent;
        EditorGUI.EndProperty();
    }
    
    public override float GetPropertyHeight (SerializedProperty property, GUIContent label) {
 
        float propertyHeight=0;
        propertyHeight += EditorGUI.GetPropertyHeight(property.FindPropertyRelative("Type"));
        EventSequence.SequenceEntry.EntryType type = (EventSequence.SequenceEntry.EntryType)property.FindPropertyRelative("Type").enumValueIndex;
        if(type ==EventSequence.SequenceEntry.EntryType.Event)
             propertyHeight = EditorGUI.GetPropertyHeight(property.FindPropertyRelative("Event"));
        if(type ==EventSequence.SequenceEntry.EntryType.Trigger)
             propertyHeight = EditorGUI.GetPropertyHeight(property.FindPropertyRelative("Triggers"))+15;
        
        return propertyHeight;
    }
}

[CustomPropertyDrawer(typeof(EventSequence.TriggerTagPair))]
public class TriggerTagPairDrawer : PropertyDrawer{ 

    Rect triggerRect, tagRect;

    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label){

        EditorGUI.BeginProperty(position, label, property);
        int indent = EditorGUI.indentLevel;
        EditorGUI.indentLevel = 0;
        
        triggerRect = new Rect(position.x, position.y, position.width/2, position.height);
        tagRect     = new Rect(position.x+position.width/2+5, position.y, position.width/2-4, position.height);
        
        EditorGUI.PropertyField(triggerRect, property.FindPropertyRelative("Trigger"), GUIContent.none);
        EditorGUI.PropertyField(tagRect, property.FindPropertyRelative("Tag"), GUIContent.none);

        EditorGUI.indentLevel = indent;
        EditorGUI.EndProperty();
    }
}
#endif