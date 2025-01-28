using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UIElements;

public class ButtonHook : MonoBehaviour
{
    [SerializeField] private UIDocument _uiDocument;
    [SerializeField] private string _elementId;
    [Tooltip("True if it is a visual element and not a button")]
    [SerializeField] private bool _isVisualElementOnly = false;
    [SerializeField] private UnityEvent _onClick;
    
    private VisualElement _element;
    private Button _buttonElement;
    
    private void Reset()
    {
        if (_uiDocument == null)
        {
            _uiDocument = GetComponent<UIDocument>();
        }
    }

    private void OnEnable()
    {
        if (_uiDocument != null)
        {
            var root = _uiDocument.rootVisualElement;
            if (_isVisualElementOnly)
            {
                _element = root.Q(_elementId);
                if (_element != null)
                {
                    _element.RegisterCallback<PointerUpEvent>(OnPointerUp);
                }
            }
            else
            {
                _buttonElement = root.Q<Button>(_elementId);
                if (_buttonElement != null)
                {
                    _buttonElement.clickable.clicked += OnPointerUp;
                }
            }
        }
    }

    private void OnDisable()
    {
        if (_element != null)
        {
            _element.UnregisterCallback<PointerUpEvent>(OnPointerUp);
        }
        if (_buttonElement != null)
        {
            _buttonElement.clickable.clicked -= OnPointerUp;
        }
    }

    private void OnPointerUp(PointerUpEvent eventData)
    {
        _onClick?.Invoke();
    }
    
    private void OnPointerUp()
    {
        _onClick?.Invoke();
    }
}
