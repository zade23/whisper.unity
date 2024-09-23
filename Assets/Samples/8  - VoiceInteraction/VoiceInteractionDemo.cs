using System.Diagnostics;
using UnityEngine;
using UnityEngine.UI;
using Whisper.Utils;
using Button = UnityEngine.UI.Button;
using Toggle = UnityEngine.UI.Toggle;

namespace Whisper.Samples
{
    /// <summary>
    /// Record audio clip from microphone and make a transcription.
    /// </summary>
    public class VoiceInteractionDemo : MonoBehaviour
    {
        public WhisperManager whisper;
        public MicrophoneRecord microphoneRecord;
        public bool streamSegments = true;
        public bool printLanguage = true;

        [Header("UI")] 
        public Button button;
        public Text buttonText;
        public Text outputText;
        public Text timeText;
        public Dropdown languageDropdown;
        public Toggle vadToggle;
        
        private string _buffer;

        private void Awake()
        {
            // 订阅 WhisperManager 的 OnNewSegment 事件，当有新片段时调用 OnNewSegment 方法
            whisper.OnNewSegment += OnNewSegment;
            // 订阅 WhisperManager 的 OnProgress 事件，当进度更新时调用 OnProgressHandler 方法
            whisper.OnProgress += OnProgressHandler;
            
            // 订阅 MicrophoneRecord 的 OnRecordStop 事件，当录音停止时调用 OnRecordStop 方法
            microphoneRecord.OnRecordStop += OnRecordStop;
            
            // 为按钮添加点击事件监听器，当按钮被按下时调用 OnButtonPressed 方法
            button.onClick.AddListener(OnButtonPressed);
            // 设置语言下拉菜单的初始值为当前语言
            languageDropdown.value = languageDropdown.options
            .FindIndex(op => op.text == whisper.language);
            // 为语言下拉菜单添加值改变事件监听器，当选项改变时调用 OnLanguageChanged 方法
            languageDropdown.onValueChanged.AddListener(OnLanguageChanged);

            // 设置 VAD（语音活动检测）切换开关的初始状态为当前 VAD 状态
            vadToggle.isOn = microphoneRecord.vadStop;
            // 为 VAD 切换开关添加值改变事件监听器，当状态改变时调用 OnVadChanged 方法
            vadToggle.onValueChanged.AddListener(OnVadChanged);
        }

        /// <summary>
        /// 当 VAD（语音活动检测）切换开关的值改变时调用此方法。
        /// </summary>
        /// <param name="vadStop">VAD 切换开关的新状态。</param>
        private void OnVadChanged(bool vadStop)
        {
            // 更新 MicrophoneRecord 的 VAD 状态
            microphoneRecord.vadStop = vadStop;
        }

        /// <summary>
        /// 当按钮按下时调用此方法
        /// </summary>
        private void OnButtonPressed()
        {
            if (!microphoneRecord.IsRecording)
            {
                microphoneRecord.StartRecord();
                buttonText.text = "Stop";
            }
            else
            {
                microphoneRecord.StopRecord();
                buttonText.text = "Record";
            }
        }
        
        private async void OnRecordStop(AudioChunk recordedAudio)
        {
            buttonText.text = "Record";
            _buffer = "";

            var sw = new Stopwatch();
            sw.Start();
            
            var res = await whisper.GetTextAsync(recordedAudio.Data, recordedAudio.Frequency, recordedAudio.Channels);
            if (res == null || !outputText) 
                return;

            var time = sw.ElapsedMilliseconds;
            var rate = recordedAudio.Length / (time * 0.001f);
            timeText.text = $"Time: {time} ms\nRate: {rate:F1}x";

            var text = res.Result;
            
            // 将语言附加到文本中
            if (printLanguage)
                text += $"\n\nLanguage: {res.Language}";
            
            outputText.text = text;
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
            whisper.language = opt.text;
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
            // 更新输出文本为缓冲区的内容并添加省略号
            outputText.text = _buffer + "...";
        }
    }
}
