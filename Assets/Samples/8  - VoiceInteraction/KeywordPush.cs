using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Networking;

[System.Serializable]
public class ResponseItem
{
    public string _id;
    public float _score;
    public Source _source;
}

[System.Serializable]
public class Source
{
    public string Question;
    public string Answer;
    public string AnswerNPC;
}

[System.Serializable]
public class Response
{
    public ResponseItem[] items;
}

public class KeywordPush : MonoBehaviour
{
    InputField outputArea;
    CustomText inputKeywords; 

    private void Start()
    {
        outputArea = GameObject.Find("OutputArea").GetComponent<InputField>();
        inputKeywords = GameObject.Find("InputKeyword/InputKeywords").GetComponent<CustomText>(); // 获取 CustomText 组件

        // 添加文本变化监听器
        inputKeywords.onTextChanged.AddListener(OnInputChanged);
    }

    private void OnInputChanged(string newText)
    {
        // 如果输入框有内容，则进行查询
        if (!string.IsNullOrEmpty(newText))
        {
            StartCoroutine(SearchData_Coroutine(newText));
        }
        else
        {
            outputArea.text = ""; // 如果没有内容，清空输出区域
        }
    }

    private IEnumerator SearchData_Coroutine(string keyword)
    {
        outputArea.text = "Loading...";

        // 对关键词进行URL编码
        string encodedKeyword = UnityWebRequest.EscapeURL(keyword);
        string url = "http://127.0.0.1:8000/search/?input_keyword=" + encodedKeyword;

        using (UnityWebRequest request = UnityWebRequest.Get(url))
        {
            yield return request.SendWebRequest();

            // 检查请求结果
            if (request.result == UnityWebRequest.Result.ConnectionError || request.result == UnityWebRequest.Result.ProtocolError)
            {
                outputArea.text = "错误：" + request.error;
            }
            else
            {
                // 解析返回的 JSON 数据
                var jsonResponse = request.downloadHandler.text;
                Response response = JsonUtility.FromJson<Response>("{\"items\":" + jsonResponse + "}");

                if (response.items.Length > 0)
                {
                    var firstItem = response.items[0];
                    string answerNPC = firstItem._source.AnswerNPC;
                    string score = firstItem._score.ToString();

                    // 输出格式化后的内容
                    outputArea.text = $"{answerNPC}\n\nScore: {score}";
                }
                else
                {
                    outputArea.text = "没有找到相关内容。";
                }
            }
        }
    }
}
