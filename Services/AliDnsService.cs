using AlibabaCloud.SDK.Alidns20150109;
using AlibabaCloud.SDK.Alidns20150109.Models;
using Tea;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace AliDnsManager.Services
{
    public class AliDnsService
    {
        private Client? _client;
        private string _accessKeyId = "";
        private string _accessKeySecret = "";
        private string _endpoint = "alidns.cn-hangzhou.aliyuncs.com";

        public bool IsConfigured => !string.IsNullOrEmpty(_accessKeyId) && !string.IsNullOrEmpty(_accessKeySecret);

        public void Configure(string accessKeyId, string accessKeySecret, string? endpoint = null)
        {
            _accessKeyId = accessKeyId;
            _accessKeySecret = accessKeySecret;
            if (!string.IsNullOrEmpty(endpoint))
            {
                _endpoint = endpoint;
            }

            var config = new AlibabaCloud.OpenApiClient.Models.Config
            {
                AccessKeyId = _accessKeyId,
                AccessKeySecret = _accessKeySecret,
                Endpoint = _endpoint
            };

            _client = new Client(config);
        }

        public async Task<List<DomainInfo>> GetDomainsAsync()
        {
            if (_client == null) throw new InvalidOperationException("DNS服务未配置");

            var request = new DescribeDomainsRequest();
            var response = await _client.DescribeDomainsAsync(request);
            
            var domains = new List<DomainInfo>();
            if (response.Body.Domains?.Domain != null)
            {
                foreach (var domain in response.Body.Domains.Domain)
                {
                    domains.Add(new DomainInfo
                    {
                        DomainName = domain.DomainName ?? "",
                        DomainId = domain.DomainId ?? "",
                        RecordCount = domain.RecordCount ?? 0,
                        CreateTime = domain.CreateTime ?? ""
                    });
                }
            }

            return domains;
        }

        public async Task<List<DnsRecord>> GetDomainRecordsAsync(string domainName)
        {
            if (_client == null) throw new InvalidOperationException("DNS服务未配置");

            var request = new DescribeDomainRecordsRequest
            {
                DomainName = domainName
            };

            var response = await _client.DescribeDomainRecordsAsync(request);
            var records = new List<DnsRecord>();

            if (response.Body.DomainRecords?.Record != null)
            {
                foreach (var record in response.Body.DomainRecords.Record)
                {
                    records.Add(new DnsRecord
                    {
                        RecordId = record.RecordId ?? "",
                        RR = record.RR ?? "",
                        Type = record.Type ?? "",
                        Value = record.Value ?? "",
                        TTL = record.TTL ?? 600,
                        Priority = record.Priority ?? 0,
                        Line = record.Line ?? "default",
                        Status = record.Status ?? "",
                        Locked = record.Locked ?? false
                    });
                }
            }

            return records;
        }

        public async Task<string> AddDomainRecordAsync(string domainName, string rr, string type, string value, long ttl = 600, long priority = 0, string line = "default")
        {
            if (_client == null) throw new InvalidOperationException("DNS服务未配置");

            var request = new AddDomainRecordRequest
            {
                DomainName = domainName,
                RR = rr,
                Type = type,
                Value = value,
                TTL = ttl,
                Priority = priority,
                Line = line
            };

            var response = await _client.AddDomainRecordAsync(request);
            return response.Body.RecordId ?? "";
        }

        public async Task<bool> UpdateDomainRecordAsync(string recordId, string rr, string type, string value, long ttl = 600, long priority = 0, string line = "default")
        {
            if (_client == null) throw new InvalidOperationException("DNS服务未配置");

            var request = new UpdateDomainRecordRequest
            {
                RecordId = recordId,
                RR = rr,
                Type = type,
                Value = value,
                TTL = ttl,
                Priority = priority,
                Line = line
            };

            var response = await _client.UpdateDomainRecordAsync(request);
            return !string.IsNullOrEmpty(response.Body.RecordId);
        }

        public async Task<bool> DeleteDomainRecordAsync(string recordId)
        {
            if (_client == null) throw new InvalidOperationException("DNS服务未配置");

            var request = new DeleteDomainRecordRequest
            {
                RecordId = recordId
            };

            var response = await _client.DeleteDomainRecordAsync(request);
            return !string.IsNullOrEmpty(response.Body.RecordId);
        }

        public async Task<bool> SetDomainRecordStatusAsync(string recordId, string status)
        {
            if (_client == null) throw new InvalidOperationException("DNS服务未配置");

            var request = new SetDomainRecordStatusRequest
            {
                RecordId = recordId,
                Status = status
            };

            var response = await _client.SetDomainRecordStatusAsync(request);
            return !string.IsNullOrEmpty(response.Body.RecordId);
        }
    }

    public class DomainInfo
    {
        public string DomainName { get; set; } = "";
        public string DomainId { get; set; } = "";
        public long RecordCount { get; set; }
        public string CreateTime { get; set; } = "";
    }

    public class DnsRecord
    {
        public string RecordId { get; set; } = "";
        public string RR { get; set; } = "";
        public string Type { get; set; } = "";
        public string Value { get; set; } = "";
        public long TTL { get; set; }
        public long Priority { get; set; }
        public string Line { get; set; } = "";
        public string Status { get; set; } = "";
        public bool Locked { get; set; }
    }
}
