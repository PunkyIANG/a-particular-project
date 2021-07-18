using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;


namespace SomeProject.IngameConsole
{
    public class UIController : MonoBehaviour
    {
        private TextField _userInput;

        // Start is called before the first frame update
        void Start()
        {
            var root = GetComponent<UIDocument>().rootVisualElement;

            _userInput = root.Q<TextField>("user_input");
            _userInput.focusable = true;
        }

        // Update is called once per frame
        void Update()
        {
            
        }
    }
}