using System.Collections;
using System.Collections.Generic;

using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class OnTabSelectNext : MonoBehaviour
{
    void Update()
    {
        EventSystem evnt = EventSystem.current;
        if (Input.GetKeyDown(KeyCode.Tab))
        {
            Selectable next = evnt.currentSelectedGameObject.GetComponent<Selectable>().FindSelectableOnDown();

            if (next != null)
            {

                InputField inputfield = next.GetComponent<InputField>();
                if (inputfield != null) inputfield.OnPointerClick(new PointerEventData(evnt));

                evnt.SetSelectedGameObject(next.gameObject, new BaseEventData(evnt));
            }
        }
    }
}
