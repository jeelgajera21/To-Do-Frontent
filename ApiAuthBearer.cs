﻿using System.Net.Http.Headers;

namespace To_Do_UI
{
    public class ApiAuthBearer
    {
        private readonly HttpClient _client;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public ApiAuthBearer(IHttpClientFactory httpClientFactory, IHttpContextAccessor httpContextAccessor)
        {
            _httpContextAccessor = httpContextAccessor;
            _client = httpClientFactory.CreateClient("ApiClient");
        }

        public HttpClient GetHttpClient()
        {
            var token = _httpContextAccessor.HttpContext?.Session.GetString("Token");

            if (!string.IsNullOrEmpty(token))
            {
                _client.DefaultRequestHeaders.Authorization =
                    new AuthenticationHeaderValue("Bearer", token);
            }

            return _client;
        }
    }
}
