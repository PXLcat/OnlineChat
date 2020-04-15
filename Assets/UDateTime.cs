using System;
#if UNITY_EDITOR
using UnityEditor;
#endif
using UnityEngine;


//https://gist.github.com/EntranceJew/f329f1c6a0c35ac51763455f76b5eb95


// we have to use UDateTime instead of DateTime on our classes
// we still typically need to either cast this to a DateTime or read the DateTime field directly
[System.Serializable]
public class UDateTime : ISerializationCallbackReceiver
{
    [HideInInspector] public DateTime dateTime;

    // if you don't want to use the PropertyDrawer then remove HideInInspector here
    [HideInInspector] [SerializeField] private string _dateTime;

    public static implicit operator DateTime(UDateTime udt)
    {
        return (udt.dateTime);
    }

    public static implicit operator UDateTime(DateTime dt)
    {
        return new UDateTime() { dateTime = dt };
    }

    public void OnAfterDeserialize()
    {
        DateTime.TryParse(_dateTime, out dateTime);
    }

    public void OnBeforeSerialize()
    {
        _dateTime = dateTime.ToString();
    }
}

// if we implement this PropertyDrawer then we keep the label next to the text field
#if UNITY_EDITOR
[CustomPropertyDrawer(typeof(UDateTime))]
public class UDateTimeDrawer : PropertyDrawer
{

    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        using (var propertyScope = new EditorGUI.PropertyScope(position, label, property))
        {
            label = propertyScope.content;

            EditorGUI.PropertyField(position, property.FindPropertyRelative("_dateTime"), label);
        }
    }

}
#endif