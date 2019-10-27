using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text.Json;
using System.Threading.Tasks;
using MQTT.Demo.Services.Models;

namespace MQTT.Demo.Services.Https
{
    public class BoxStateHttpClient
    {
        private readonly HttpClient _boxStateClient;
        private readonly BoxStateHttpSetting _boxStateHttpSetting;

        public BoxStateHttpClient(HttpClient httpClient,
            BoxStateHttpSetting boxStateHttpSetting)
        {
            _boxStateClient = httpClient;
            _boxStateHttpSetting = boxStateHttpSetting;
            _boxStateClient.BaseAddress = new Uri(boxStateHttpSetting.Uri);
            _boxStateClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("*/*"));
            var pList = new List<KeyValuePair<string, string>>
            {
                new KeyValuePair<string, string>("username", "123"),
                new KeyValuePair<string, string>("password", boxStateHttpSetting.Password)
            };
            var httpContext = new StringContent(JsonSerializer.Serialize(pList));
            httpContext.Headers.ContentType = new MediaTypeHeaderValue("application/json");
        }

        public async Task<bool> IsConnectedAsync(string boxNo)
        {
            _boxStateClient.DefaultRequestHeaders.Add("Authorization", _boxStateHttpSetting.Authorization);
            var responseMessage = await _boxStateClient.GetAsync(_boxStateHttpSetting.BoxStateUri + "/" + boxNo);

            //    如果验证失败和服务器出问题都代表Authorization数据有问题
            if (responseMessage.StatusCode == HttpStatusCode.Unauthorized ||
                responseMessage.StatusCode == HttpStatusCode.InternalServerError)
                throw new UnauthorizedAccessException("Authorization is Error");
            var resultContentStr = await responseMessage.Content.ReadAsStringAsync();
            var boxStateResult = JsonSerializer.Deserialize<BoxStateResult>(resultContentStr);
            Console.WriteLine(boxStateResult.Data.Count);
            return boxStateResult.Data.Count > 0;
        }

        private class BoxStateResult
        {
            public int Code { get; set; }
            public IList<object> Data { get; set; }
        }
    }
}