using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;

[RequireComponent(typeof(Text))]
public class CustomText : MonoBehaviour
{
    private Text _text;
    private string _lastText;

    public UnityEvent<string> onTextChanged;

    private void Awake()
    {
        _text = GetComponent<Text>();
        _lastText = _text.text;
    }

    private void Update()
    {
        // 检查文本是否发生变化
        if (_text.text != _lastText)
        {
            // 更新最后的文本
            _lastText = _text.text;
            // 触发事件
            onTextChanged?.Invoke(_lastText);
        }
    }
}
