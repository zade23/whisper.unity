using System.Diagnostics;
using UnityEngine;
using UnityEngine.UI;
using Whisper.Utils;

namespace Whisper.Samples
{
    /// <summary>
    /// Takes audio clip and make a transcription.
    /// </summary>
    public class AudioClipDemo : MonoBehaviour
    {
        public WhisperManager manager;
        public AudioClip clip;
        public bool streamSegments = true;
        public bool echoSound = true;
        public bool printLanguage = true;

        [Header("UI")]
        public Button button;
        public Text outputText;
        public Text timeText;
        public ScrollRect scroll;
        public Dropdown languageDropdown;
        public Toggle translateToggle;
        
        private string _buffer;
        
        private void Awake()
        {
            // 订阅新片段事件，当有新片段时调用 OnNewSegment 方法
            manager.OnNewSegment += OnNewSegment;
            
            // 订阅进度事件，当进度更新时调用 OnProgressHandler 方法
            manager.OnProgress += OnProgressHandler;
            
            // 为按钮点击事件添加监听器，当按钮被点击时调用 ButtonPressed 方法
            button.onClick.AddListener(ButtonPressed);
            
            // 设置语言下拉菜单的默认值为当前管理器的语言
            languageDropdown.value = languageDropdown.options
            .FindIndex(op => op.text == manager.language);
            
            // 为语言下拉菜单的值改变事件添加监听器，当值改变时调用 OnLanguageChanged 方法
            languageDropdown.onValueChanged.AddListener(OnLanguageChanged);

            // 设置翻译切换开关的默认状态为当前管理器的翻译状态
            translateToggle.isOn = manager.translateToEnglish;
            
            // 为翻译切换开关的值改变事件添加监听器，当值改变时调用 OnTranslateChanged 方法
            translateToggle.onValueChanged.AddListener(OnTranslateChanged);
        }

        /// <summary>
        /// 当按钮被按下时调用此方法。
        /// </summary>
        public async void ButtonPressed()
        {
            // 清空缓冲区
            _buffer = "";
            
            // 如果需要回声播放音频片段
            if (echoSound)
            AudioSource.PlayClipAtPoint(clip, Vector3.zero);

            // 创建并启动一个计时器
            var sw = new Stopwatch();
            sw.Start();
            
            // 异步获取音频片段的文本
            var res = await manager.GetTextAsync(clip);
            
            // 如果结果为空或输出文本为空，则返回
            if (res == null || !outputText) 
            return;

            // 获取经过的时间（毫秒）
            var time = sw.ElapsedMilliseconds;
            
            // 计算处理速率
            var rate = clip.length / (time * 0.001f);
            
            // 更新时间文本
            timeText.text = $"Time: {time} ms\nRate: {rate:F1}x";

            // 获取结果文本
            var text = res.Result;
            
            // 如果需要打印语言信息，则附加语言信息
            if (printLanguage)
            text += $"\n\nLanguage: {res.Language}";
            
            // 更新输出文本
            outputText.text = text;
            
            // 滚动到最底部
            UiUtils.ScrollDown(scroll);
        }
        
        /// <summary>
        /// 当语言下拉菜单的值改变时调用此方法。
        /// </summary>
        /// <param name="ind">下拉菜单选项的索引。</param>
        private void OnLanguageChanged(int ind)
        {
            // 获取选中的选项
            var opt = languageDropdown.options[ind];
            // 更新管理器的语言
            manager.language = opt.text;
        }
        
        /// <summary>
        /// 当翻译切换开关的值改变时调用此方法。
        /// </summary>
        /// <param name="translate">翻译切换开关的新状态。</param>
        private void OnTranslateChanged(bool translate)
        {
            // 更新管理器的翻译状态
            manager.translateToEnglish = translate;
        }

        /// <summary>
        /// 当进度更新时调用此方法。
        /// </summary>
        /// <param name="progress">当前进度百分比。</param>
        private void OnProgressHandler(int progress)
        {
            // 如果时间文本为空，则返回
            if (!timeText)
            return;
            // 更新时间文本为当前进度
            timeText.text = $"Progress: {progress}%";
        }
        
        /// <summary>
        /// 当有新片段时调用此方法。
        /// </summary>
        /// <param name="segment">新的音频片段。</param>
        private void OnNewSegment(WhisperSegment segment)
        {
            // 如果不需要流式传输片段或输出文本为空，则返回
            if (!streamSegments || !outputText)
            return;

            // 将新片段的文本附加到缓冲区
            _buffer += segment.Text;
            // 更新输出文本为缓冲区内容并添加省略号
            outputText.text = _buffer + "...";
            // 滚动到最底部
            UiUtils.ScrollDown(scroll);
        }
    }
}


