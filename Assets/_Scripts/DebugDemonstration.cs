using NUnit.Framework;
using UnityEngine;
using System.Collections;
using System;
using System.Collections.Generic;

public class DebugDemonstration : MonoBehaviour
{
    [SerializeField] int numeroInteroPriv = 0;
    [HideInInspector] public float numeroDecimale = 0.0f;
    public int NumeroIntero => numeroInteroPriv;
    public int NumeroIntero1() { return numeroInteroPriv; }
    public int numeroInteroPub { get; private set; }

    string stringa = "";
    bool veroFalso = false;

    int[] intArray = new int[3];
    List<int> intList = new List<int>();
    Dictionary<int, string> intStringDictionary = new();
    HashSet<int> ints = new HashSet<int>();

    public ValueClass valueClass = new ValueClass();


    private void Awake()
    {
        valueClass.ReadInt();
        //Debug.Log("AWAKE");
    }

    //private void Start()
    //{
    //    Debug.Log("START");
    //}

    //private void Update()
    //{
    //    Debug.Log("UPDATE");
    //}

    //private void FixedUpdate()
    //{
    //    Debug.Log("FIXED UPDATE");
    //}

    //private void LateUpdate()
    //{
    //    Debug.Log("LATE UPDATE");
    //}

    //private void OnEnable()
    //{
    //    Debug.Log("ENABLED");
    //}

    //private void OnDisable()
    //{
    //    Debug.Log("DISABLED");
    //}

    //private void OnDestroy()
    //{
    //    Debug.Log("DESTROYED");
    //}
}

[Serializable]

public class ValueClass
{
    public int valInt;
    public string valString;

    public void ReadInt()
    {
        Debug.Log($"{valInt}");
    }
}
