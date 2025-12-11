using NUnit.Framework;
using UnityEngine;
using System.Collections;
using System;
using System.Collections.Generic;

public class DebugDemonstration : MonoBehaviour
{
    [SerializeField] int numeroInteroPriv = 0;
    public float numeroDecimale = 0.0f;

    string stringa = "ciao sono una stringa 012034";
    bool veroFalso = false;

    int[] intArray;
    List<int> intList = new List<int>();
    Dictionary<int, string> intStringDictionary = new();
    HashSet<int> ints = new HashSet<int>();
    [System.Flags]
    public enum State { VIVO=0 >> 1 , MORTO=2, ADDORMENTATO }

    public State state = State.VIVO;

    public ValueClass valueClass = new ValueClass();

    [SerializeField] float health = 0f; //VARIABILE PRIVATA

    //Incapsulation - sola lettura
    public float Health => health;

    //Incapsulation - sola lettura tramite metodo
    public float GetHealth () { return health; }

    //Incapsulation - sola lettura da var pubblica
    public float myHealth {  get; private set; }

    //Incapsulation - scrittura tramite metodo
    public void SetHealth(float health) { myHealth = health; }
    //o
    public void Heal(float healthToAdd) { myHealth += health; }
    //o
    public void TakeDamage(float damage) { myHealth -= damage; }


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


    private void Awake()
    {
        //valueClass.ReadInt();
        Debug.Log("AWAKE");

        intArray = new int[100];
    }

    private void Start()
    {
        Debug.Log("START");
    }

    private void Update()
    {
        Debug.Log("UPDATE");
    }

    private void FixedUpdate()
    {
        Debug.Log("FIXED UPDATE");
    }

    private void LateUpdate()
    {
        Debug.Log("LATE UPDATE");
    }

    private void OnEnable()
    {
        Debug.Log("ENABLED");
    }

    private void OnDisable()
    {
        Debug.Log("DISABLED");
    }

    private void OnDestroy()
    {
        Debug.Log("DESTROYED");
    }
}

