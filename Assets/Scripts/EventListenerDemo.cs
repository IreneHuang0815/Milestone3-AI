using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class EventListenerDemo : MonoBehaviour {


    public string lastMesgRecvd = "";

    private UnityAction<string> simpleEventListener; 


    void Awake() {
        
        simpleEventListener = new UnityAction<string> (simpleEventHandler);

    }

    void OnEnable() {

        EventManager.StartListening<SimpleEvent, string> ( simpleEventListener);
    }


    void OnDisable() {
        //listeners can be a resource leak. So we need to make sure to stop listening when the component is not in use
        EventManager.StopListening<SimpleEvent, string> ( simpleEventListener);
    }



    void simpleEventHandler(string s) {

        Debug.Log("Received SimpleEvent with message: " + s);

        lastMesgRecvd = s;
    }


}
