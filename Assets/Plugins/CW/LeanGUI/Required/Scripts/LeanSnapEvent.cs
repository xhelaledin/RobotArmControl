using UnityEngine;
using UnityEngine.Events;
using System.Collections.Generic;

namespace Lean.Gui
{
    /// <summary>
    /// This component allows you to fire different events when the <b>LeanSnap</b> component's <b>Position</b> reaches specific values.
    /// </summary>
    [ExecuteInEditMode]
    [RequireComponent(typeof(LeanSnap))]
    [AddComponentMenu(LeanGui.ComponentMenuPrefix + "Snap Event")]
    public class LeanSnapEvent : MonoBehaviour
    {
        [System.Serializable]
        public class PositionEvent
        {
            public Vector2Int Position;
            public UnityEvent OnAction;
        }

        /// <summary>
        /// List of positions and the actions that will be invoked when <b>LeanSnap.Position</b> matches them.
        /// </summary>
        public List<PositionEvent> Events = new List<PositionEvent>();

        private LeanSnap snap;

        protected virtual void OnEnable()
        {
            snap = GetComponent<LeanSnap>();

            if (snap != null)
            {
                // Subscribe to runtime snapping changes
                snap.OnPositionChanged.AddListener(HandlePositionChanged);

                // ✅ Also check immediately in case it's already at one of the positions
                HandlePositionChanged(snap.Position);
            }
        }

        protected virtual void OnDisable()
        {
            if (snap != null)
            {
                snap.OnPositionChanged.RemoveListener(HandlePositionChanged);
            }
        }

        private void HandlePositionChanged(Vector2Int newPosition)
        {
            for (int i = 0; i < Events.Count; i++)
            {
                if (Events[i].Position == newPosition)
                {
                    if (Events[i].OnAction != null)
                    {
                        Events[i].OnAction.Invoke();
                    }
                }
            }
        }
    }
}

#if UNITY_EDITOR
namespace Lean.Gui.Editor
{
    using UnityEditor;
    using TARGET = LeanSnapEvent;

    [CanEditMultipleObjects]
    [CustomEditor(typeof(TARGET))]
    public class LeanSnapEvent_Editor : Editor
    {
        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            // Draw the Events list normally
            EditorGUILayout.PropertyField(serializedObject.FindProperty("Events"), true);

            // Live preview of current snap position
            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Live Snap Position", EditorStyles.boldLabel);

            foreach (var t in targets)
            {
                var snapEvent = (TARGET)t;
                var snap = snapEvent.GetComponent<LeanSnap>();
                if (snap != null)
                {
                    EditorGUILayout.LabelField("Current: " + snap.Position.ToString());
                }
            }

            serializedObject.ApplyModifiedProperties();
        }
    }
}
#endif
