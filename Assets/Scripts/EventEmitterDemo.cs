using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EventEmitterDemo : MonoBehaviour {


    //Just set this to true while the game is running in the editor to trigger the event
    public bool emitEvent = false;

    public string myEventMessage = "Hadouken!!!";


	// Update is called once per frame
	void Update () {
		
        if (emitEvent)
        {
            emitEvent = false;

            EventManager.TriggerEvent<SimpleEvent, string>(myEventMessage);
                    

        }

	}
}
