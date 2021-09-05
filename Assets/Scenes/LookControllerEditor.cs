//using System.Collections;
//using System.Collections.Generic;
//using UnityEngine;
//using UnityEditor;

//[CustomEditor(typeof(LookController))]
//public class LookControllerEditor : Editor
//{
//    private string[] CameraShakeType;

//    private int pickedIdx = 0;

//    private LookController controller;

//    private SerializedProperty controllerProperty;

//    MonoRecoilController[] controllers;

//    public void OnEnable()
//    {
//        controller = target as LookController;
//        controllers = controller.GetComponentsInChildren<MonoRecoilController>(true);

//        CameraShakeType = new string[controllers.Length];
//        for (int i = 0; i < controllers.Length; i++)
//        {
//            CameraShakeType[i] = controllers[i].GetType().Name;
//        }

//        controllerProperty = serializedObject.FindProperty("_camShakeCtrl");
//        if (controllerProperty.objectReferenceValue != null)
//        {
//            pickedIdx = System.Array.IndexOf(CameraShakeType, controllerProperty.objectReferenceValue.GetType().Name);
//        }
//    }

//    public override void OnInspectorGUI()
//    {

//        DrawDefaultInspector();

//        serializedObject.Update();

//        pickedIdx = EditorGUILayout.Popup("Pick controller:", pickedIdx, CameraShakeType);
//        if (pickedIdx >= 0)
//        {
//            controllerProperty.objectReferenceValue = controllers[pickedIdx];
//        }

//        serializedObject.ApplyModifiedProperties();
//    }
//}
