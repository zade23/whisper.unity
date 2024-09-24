using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Networking;

public class KeywordSearch : MonoBehaviour
{
    InputField outputArea;
    InputField inputKeyword;
    Button searchButton;

    void Start()
    {
        // 获取组件
        outputArea = GameObject.Find("OutputArea").GetComponent<InputField>();
        inputKeyword = GameObject.Find("InputKeyword").GetComponent<InputField>();
        searchButton = GameObject.Find("SearchButton").GetComponent<Button>();

        // 添加按钮点击事件
        searchButton.onClick.AddListener(SearchData);
    }

    void SearchData() => StartCoroutine(SearchData_Coroutine());

    IEnumerator SearchData_Coroutine()
    {
        // 检查是否输入了关键词
        if (string.IsNullOrEmpty(inputKeyword.text))
        {
            outputArea.text = "请输入关键词！";
            yield break;
        }

        outputArea.text = "Loading...";
        string keyword = inputKeyword.text;

        // 对关键词进行URL编码
        string encodedKeyword = UnityWebRequest.EscapeURL(keyword);

        // 设置请求URL
        string url = "http://127.0.0.1:8000/search/?input_keyword=" + encodedKeyword;

        // 创建GET请求
        using (UnityWebRequest request = UnityWebRequest.Get(url))
        {
            // 发送请求并等待响应
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
