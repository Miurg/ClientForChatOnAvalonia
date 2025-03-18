using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using ClientForChatOnAvalonia.Models;
using System.Windows;
using System.Net.Security;
using Newtonsoft.Json.Linq;
using System.Diagnostics;
using System.Text.Json;
using System.Net.Http.Headers;

namespace ClientForChatOnAvalonia.Services
{
    public class ApiService
    {
        private readonly TokenService _tokenService = new TokenService();
        private readonly HttpClientHandler _handler;
        private readonly HttpClient _httpClient;
        public ApiService()
        {
            _handler = new HttpClientHandler();
            _handler.ServerCertificateCustomValidationCallback = (sender, cert, chain, SslPolicyErrors) => true;
            TimeSpan.FromSeconds(10);
            _httpClient = new HttpClient(_handler);
        }

        public async Task<bool> LoginAsync(string username, string password)
        {
            var loginRequest = new LoginRequestModel
            {
                Username = username,
                Password = password
            };

            var json = JsonConvert.SerializeObject(loginRequest);
            var content = new StringContent(json, Encoding.UTF8, "application/json");
            try
            {
                HttpResponseMessage response = await _httpClient.PostAsync("https://26.74.71.132:7168/api/auth/login", content);
                if (response.IsSuccessStatusCode)
                {
                    var responseJson = await response.Content.ReadAsStringAsync();
                    var loginResponse = JsonConvert.DeserializeObject<LoginResponseModel>(responseJson);
                    _tokenService.SaveToken(loginResponse?.Token);
                    return true;
                }
                else
                {
                    return false;
                }
            }
            catch (HttpRequestException ex)
            {
                Debug.WriteLine($"Network error: {ex.Message}\nStackTrace:{ex.StackTrace}");
                return false;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error: {ex.Message}\nStackTrace:{ex.StackTrace}");
                return false;
            }

        }
        public async Task<UserModel> FetchUserAsync(int userId)
        {
            try
            {
                _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _tokenService.GetToken());
                HttpResponseMessage response = await _httpClient.GetAsync($"https://26.74.71.132:7168/api/users/{userId}");
                if (response.IsSuccessStatusCode)
                {
                    var json = await response.Content.ReadAsStringAsync();
                    return JsonConvert.DeserializeObject<UserModel>(json);
                }
                return null;
            }
            catch (HttpRequestException ex) when ((int?)ex.StatusCode == 401)
            {
                Debug.WriteLine("Ошибка: неверный токен или истёк срок действия (401 Unauthorized)");
                return null;
            }
            catch (HttpRequestException ex)
            {
                Debug.WriteLine($"Network error: {ex.Message}\nStackTrace:{ex.StackTrace}");
                return null;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error: {ex.Message}\nStackTrace:{ex.StackTrace}");
                return null;
            }
        }

        public async Task<List<MessageDto>> FetchMessagesAsync(int offset, int limit)
        {
            try
            {
                _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _tokenService.GetToken());
                var pageNumber =(offset / limit)+1;
                var response = await _httpClient.GetAsync($"https://26.74.71.132:7168/api/Messages/paged?page={pageNumber}&pageSize={limit}");
                if (response.IsSuccessStatusCode)
                {
                    var jsonResponse = await response.Content.ReadAsStringAsync();
                    var messages = JsonConvert.DeserializeObject<List<MessageDto>>(jsonResponse);

                    // Сохраняем полученные сообщения в локальной БД
                    return messages;
                }
                else
                {
                    Console.WriteLine($"Ошибка при получении сообщений: {response.StatusCode}");
                    return null;
                }
            }
            catch (HttpRequestException ex)
            {
                Debug.WriteLine($"Network error: {ex.Message}\nStackTrace:{ex.StackTrace}");
                return null;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error: {ex.Message}\nStackTrace:{ex.StackTrace}");
                return null;
            }
        }
    }
}
