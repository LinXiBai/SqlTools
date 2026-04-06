using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using CoreToolkit.Common.Helpers;
using CoreToolkit.MES.Core;
using CoreToolkit.MES.Models;

namespace CoreToolkit.MES.Helpers
{
    /// <summary>
    /// 基于 HttpClient 的 MES 客户端实现
    /// </summary>
    public class MesHttpClient : IMesClient, IDisposable
    {
        private readonly HttpClient _httpClient;
        private string _authHeaderValue;
        private bool _disposed;

        public string BaseUrl { get; }

        public MesHttpClient(string baseUrl, int timeoutSeconds = 30)
        {
            BaseUrl = baseUrl?.TrimEnd('/', '\\');
            _httpClient = new HttpClient
            {
                Timeout = TimeSpan.FromSeconds(timeoutSeconds)
            };
            _httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
        }

        public void SetAuthorization(string authHeaderValue)
        {
            _authHeaderValue = authHeaderValue;
            if (!string.IsNullOrEmpty(_authHeaderValue))
            {
                _httpClient.DefaultRequestHeaders.Authorization =
                    AuthenticationHeaderValue.Parse(_authHeaderValue);
            }
            else
            {
                _httpClient.DefaultRequestHeaders.Authorization = null;
            }
        }

        #region Track In / Track Out

        public MesResponse TrackIn(TrackInRequest request)
        {
            return TrackInAsync(request).GetAwaiter().GetResult();
        }

        public Task<MesResponse> TrackInAsync(TrackInRequest request)
        {
            return PostAsync("/api/mes/trackin", request);
        }

        public MesResponse TrackOut(TrackOutRequest request)
        {
            return TrackOutAsync(request).GetAwaiter().GetResult();
        }

        public Task<MesResponse> TrackOutAsync(TrackOutRequest request)
        {
            return PostAsync("/api/mes/trackout", request);
        }

        #endregion

        #region Data Upload

        public MesResponse UploadProcessData(ProcessDataReport report)
        {
            return UploadProcessDataAsync(report).GetAwaiter().GetResult();
        }

        public Task<MesResponse> UploadProcessDataAsync(ProcessDataReport report)
        {
            return PostAsync("/api/mes/processdata", report);
        }

        public MesResponse ReportEquipmentStatus(EquipmentStatusReport report)
        {
            return ReportEquipmentStatusAsync(report).GetAwaiter().GetResult();
        }

        public Task<MesResponse> ReportEquipmentStatusAsync(EquipmentStatusReport report)
        {
            return PostAsync("/api/mes/equipmentstatus", report);
        }

        public MesResponse ReportAlarm(AlarmReport report)
        {
            return ReportAlarmAsync(report).GetAwaiter().GetResult();
        }

        public Task<MesResponse> ReportAlarmAsync(AlarmReport report)
        {
            return PostAsync("/api/mes/alarm", report);
        }

        #endregion

        #region Material Verify

        public MesResponse VerifyMaterial(MaterialVerifyRequest request)
        {
            return VerifyMaterialAsync(request).GetAwaiter().GetResult();
        }

        public Task<MesResponse> VerifyMaterialAsync(MaterialVerifyRequest request)
        {
            return PostAsync("/api/mes/verifymaterial", request);
        }

        #endregion

        #region Query

        public MesResponse<WorkOrderInfo> QueryWorkOrder(string workOrderNumber)
        {
            return QueryWorkOrderAsync(workOrderNumber).GetAwaiter().GetResult();
        }

        public async Task<MesResponse<WorkOrderInfo>> QueryWorkOrderAsync(string workOrderNumber)
        {
            var url = $"/api/mes/workorder?workOrderNumber={Uri.EscapeDataString(workOrderNumber ?? "")}";
            return await GetAsync<WorkOrderInfo>(url).ConfigureAwait(false);
        }

        #endregion

        #region Generic Methods

        public MesResponse Post(string actionUrl, object data)
        {
            return PostAsync(actionUrl, data).GetAwaiter().GetResult();
        }

        public async Task<MesResponse> PostAsync(string actionUrl, object data)
        {
            return await SendAsync<MesResponse>(HttpMethod.Post, actionUrl, data).ConfigureAwait(false);
        }

        private async Task<MesResponse<T>> GetAsync<T>(string actionUrl)
        {
            return await SendAsync<MesResponse<T>>(HttpMethod.Get, actionUrl, null).ConfigureAwait(false);
        }

        private async Task<TResponse> SendAsync<TResponse>(HttpMethod method, string actionUrl, object data)
            where TResponse : MesResponse, new()
        {
            try
            {
                var uri = new Uri(new Uri(BaseUrl), actionUrl);
                
                using (var request = new HttpRequestMessage(method, uri))
                {
                    if (data != null && method != HttpMethod.Get)
                    {
                        var json = JsonHelper.Serialize(data);
                        using (var content = new StringContent(json, Encoding.UTF8, "application/json"))
                        {
                            request.Content = content;
                            using (var response = await _httpClient.SendAsync(request).ConfigureAwait(false))
                            {
                                var responseJson = await response.Content.ReadAsStringAsync().ConfigureAwait(false);

                                if (!response.IsSuccessStatusCode)
                                {
                                    return new TResponse
                                    {
                                        IsSuccess = false,
                                        Code = ((int)response.StatusCode).ToString(),
                                        Message = $"HTTP {(int)response.StatusCode}: {responseJson}"
                                    };
                                }

                                var result = JsonHelper.Deserialize<TResponse>(responseJson);
                                if (result == null)
                                {
                                    result = new TResponse { IsSuccess = true };
                                }
                                return result;
                            }
                        }
                    }
                    else
                    {
                        using (var response = await _httpClient.SendAsync(request).ConfigureAwait(false))
                        {
                            var responseJson = await response.Content.ReadAsStringAsync().ConfigureAwait(false);

                            if (!response.IsSuccessStatusCode)
                            {
                                return new TResponse
                                {
                                    IsSuccess = false,
                                    Code = ((int)response.StatusCode).ToString(),
                                    Message = $"HTTP {(int)response.StatusCode}: {responseJson}"
                                };
                            }

                            var result = JsonHelper.Deserialize<TResponse>(responseJson);
                            if (result == null)
                            {
                                result = new TResponse { IsSuccess = true };
                            }
                            return result;
                        }
                    }
                }
            }
            catch (TaskCanceledException)
            {
                return new TResponse { IsSuccess = false, Code = "TIMEOUT", Message = "请求超时。" };
            }
            catch (Exception ex)
            {
                return new TResponse { IsSuccess = false, Code = "EXCEPTION", Message = ex.Message };
            }
        }

        #endregion

        #region IDisposable

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!_disposed)
            {
                if (disposing)
                {
                    _httpClient?.Dispose();
                }
                _disposed = true;
            }
        }

        #endregion
    }
}
