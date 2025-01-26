using System;
using System.Collections;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;

public class CaptionUIController : MonoBehaviour
{
    private const string TEXT_INITIAL = "Come on Twistan! Get up!";
    private const int DURATION_INITIAL = 4;
    private const float DURATION_FADEOUT_TEXT = 2f;
    
    private static Color COLOR_TEXT = new Color(1f, 1f, 1f, 0.7f);
    
    [SerializeField] private TextMeshProUGUI _text;

    private bool _initialPopupClosed = false;
    /// <summary>
    /// @string: caption text
    /// @float: time duration to play the text. If negative number then it won't fade out
    /// </summary>
    private Action<(string, float)> _captionTextHandle;

    private void Reset()
    {
        if (_text == null)
        {
            _text = GetComponent<TextMeshProUGUI>();
        }
    }

    private void Start()
    {
        EventBus.Register(EventHooks.ShowCaptionText, _captionTextHandle = ShowCaptionText);
    }
    
    private void OnDestroy()
    {
        EventBus.Unregister(EventHooks.ShowCaptionText, _captionTextHandle);
    }

    public void PopupStateChange(bool isOpen)
    {
        if (!isOpen && !_initialPopupClosed)
        {
            _initialPopupClosed = true;
            _text.text = TEXT_INITIAL;
            StartCoroutine(InitialText());
        }
    }

    private IEnumerator InitialText()
    {
        _text.color = COLOR_TEXT;
        _text.enabled = true;
        yield return new WaitForSeconds(DURATION_INITIAL);
        float elapsed = 0f;

        while (elapsed < DURATION_FADEOUT_TEXT) {
            elapsed += Time.deltaTime;
            float alpha = Mathf.Lerp(1, 0f, elapsed / DURATION_FADEOUT_TEXT);
            _text.color = new Color(_text.color.r, _text.color.g, _text.color.b, alpha);
            yield return null;
        }

        _text.enabled = false;
    }

    private void ShowCaptionText((string captionText, float duration) eventData)
    {
        StartCoroutine(ShowText(eventData.captionText, eventData.duration));
    }
    
    private IEnumerator ShowText(string captionText, float duration)
    {
        _text.color = COLOR_TEXT;
        _text.text = captionText;
        _text.enabled = true;

        if (duration > 0)
        {
            yield return new WaitForSeconds(duration);
            float elapsed = 0f;

            while (elapsed < DURATION_FADEOUT_TEXT)
            {
                elapsed += Time.deltaTime;
                float alpha = Mathf.Lerp(1, 0f, elapsed / DURATION_FADEOUT_TEXT);
                _text.color = new Color(_text.color.r, _text.color.g, _text.color.b, alpha);
                yield return null;
            }

            _text.enabled = false;
        }
    }
}
