using EngineCommon;
using UnityEngine;

namespace SomeProject.Hexagon
{
    public class InputManager : MonoBehaviour
    {
        [SerializeField] private float _zoomMultplier = 1f;
        
        private Camera _camera;
        private Vector2 _startingMouseWorldPosition;


        private void Start()
        {
            _camera = Camera.main;
        }

        private void Update()
        {
            var mousePosition = Input.mousePosition;
            Ray ray = _camera.ScreenPointToRay(mousePosition);
            RaycastHit hit;
            if (Physics.Raycast(ray, out hit))
            {
                // Do the hex things
            }


            var scrollWheelInput = Input.GetAxis("Mouse ScrollWheel");
            _camera.orthographicSize = Mathf.Max(1, _camera.orthographicSize - scrollWheelInput * _zoomMultplier);

            var mouseWorldPosition = _camera.ScreenToWorldPoint(mousePosition).DropZ();

            if (Input.GetMouseButtonDown(0))
            {
                _startingMouseWorldPosition = mouseWorldPosition;
            }
            if (Input.GetMouseButton(0))
            {
                var pan = mouseWorldPosition - _startingMouseWorldPosition;
                _camera.transform.position = _camera.transform.position - pan.ZeroZ();
            }
        }
    }
}