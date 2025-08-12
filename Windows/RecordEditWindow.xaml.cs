using System;
using System.Windows;
using System.Windows.Controls;
using AliDnsManager.Services;

namespace AliDnsManager.Windows;

/// <summary>
/// RecordEditWindow.xaml 的交互逻辑
/// </summary>
public partial class RecordEditWindow : Window
{
    public DnsRecord Record { get; private set; }
    private readonly bool _isEditMode;

    public RecordEditWindow(string domainName, DnsRecord? existingRecord = null)
    {
        InitializeComponent();

        txtDomainName.Text = domainName;
        _isEditMode = existingRecord != null;

        if (_isEditMode && existingRecord != null)
        {
            // 编辑模式，填充现有数据
            Title = "编辑DNS记录";
            txtWindowTitle.Text = "编辑DNS记录";
            Record = existingRecord;
            LoadRecord(existingRecord);
        }
        else
        {
            // 新增模式
            Title = "添加DNS记录";
            txtWindowTitle.Text = "添加DNS记录";
            Record = new DnsRecord();
        }
    }

    private void LoadRecord(DnsRecord record)
    {
        txtRR.Text = record.RR;

        // 设置记录类型
        foreach (ComboBoxItem item in cmbType.Items)
        {
            if (item.Content.ToString() == record.Type)
            {
                cmbType.SelectedItem = item;
                break;
            }
        }

        txtValue.Text = record.Value;

        // 设置TTL
        cmbTTL.Text = record.TTL.ToString();

        txtPriority.Text = record.Priority.ToString();

        // 设置线路（按 Tag 英文 code 匹配，兼容中文显示值）
        {
            var lineCode = NormalizeLineCode(record.Line);
            foreach (ComboBoxItem item in cmbLine.Items)
            {
                var tag = item.Tag?.ToString() ?? string.Empty;
                if (string.Equals(tag, lineCode, StringComparison.OrdinalIgnoreCase))
                {
                    cmbLine.SelectedItem = item;
                    break;
                }
            }
        }
    }

    private void BtnOK_Click(object sender, RoutedEventArgs e)
    {
        if (!ValidateInput())
            return;

        try
        {
            // 更新Record对象
            Record.RR = txtRR.Text.Trim();
            Record.Type = ((ComboBoxItem)cmbType.SelectedItem).Content.ToString() ?? "A";
            Record.Value = txtValue.Text.Trim();
            Record.TTL = long.Parse(cmbTTL.Text);
            Record.Priority = long.Parse(txtPriority.Text);
            Record.Line = ((ComboBoxItem)cmbLine.SelectedItem).Tag?.ToString() ?? "default";

            DialogResult = true;
            Close();
        }
        catch (Exception ex)
        {
            MessageBox.Show($"保存失败: {ex.Message}", "错误", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    private void BtnCancel_Click(object sender, RoutedEventArgs e)
    {
        DialogResult = false;
        Close();
    }

    private bool ValidateInput()
    {
        if (string.IsNullOrWhiteSpace(txtRR.Text))
        {
            MessageBox.Show("请输入主机记录", "验证错误", MessageBoxButton.OK, MessageBoxImage.Warning);
            txtRR.Focus();
            return false;
        }

        if (cmbType.SelectedItem == null)
        {
            MessageBox.Show("请选择记录类型", "验证错误", MessageBoxButton.OK, MessageBoxImage.Warning);
            cmbType.Focus();
            return false;
        }

        if (string.IsNullOrWhiteSpace(txtValue.Text))
        {
            MessageBox.Show("请输入记录值", "验证错误", MessageBoxButton.OK, MessageBoxImage.Warning);
            txtValue.Focus();
            return false;
        }

        if (!long.TryParse(cmbTTL.Text, out long ttl) || ttl <= 0)
        {
            MessageBox.Show("TTL必须是大于0的数字", "验证错误", MessageBoxButton.OK, MessageBoxImage.Warning);
            cmbTTL.Focus();
            return false;
        }

        if (!long.TryParse(txtPriority.Text, out long priority) || priority < 0)
        {
            MessageBox.Show("优先级必须是大于等于0的数字", "验证错误", MessageBoxButton.OK, MessageBoxImage.Warning);
            txtPriority.Focus();
            return false;
        }

        // 验证记录值格式
        string recordType = ((ComboBoxItem)cmbType.SelectedItem).Content.ToString() ?? "";
        string recordValue = txtValue.Text.Trim();

        switch (recordType)
        {
            case "A":
                if (!IsValidIPv4(recordValue))
                {
                    MessageBox.Show("A记录的值必须是有效的IPv4地址", "验证错误", MessageBoxButton.OK, MessageBoxImage.Warning);
                    txtValue.Focus();
                    return false;
                }
                break;
            case "AAAA":
                if (!IsValidIPv6(recordValue))
                {
                    MessageBox.Show("AAAA记录的值必须是有效的IPv6地址", "验证错误", MessageBoxButton.OK, MessageBoxImage.Warning);
                    txtValue.Focus();
                    return false;
                }
                break;
            case "MX":
                if (priority == 0)
                {
                    MessageBox.Show("MX记录必须设置优先级", "验证错误", MessageBoxButton.OK, MessageBoxImage.Warning);
                    txtPriority.Focus();
                    return false;
                }
                break;
        }

        return true;
    }

    private string NormalizeLineCode(string? input)
    {
        var s = (input ?? string.Empty).Trim();
        if (string.IsNullOrEmpty(s)) return "default";
        switch (s.ToLowerInvariant())
        {
            case "默认":
            case "default": return "default";
            case "电信":
            case "telecom": return "telecom";
            case "联通":
            case "unicom": return "unicom";
            case "移动":
            case "mobile": return "mobile";
            case "海外":
            case "oversea": return "oversea";
            case "教育网":
            case "edu": return "edu";
            default: return s; // 保留原值，便于兼容其它线路
        }
    }

    private bool IsValidIPv4(string ip)
    {
        return System.Net.IPAddress.TryParse(ip, out var address) && address.AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork;
    }

    private bool IsValidIPv6(string ip)
    {
        return System.Net.IPAddress.TryParse(ip, out var address) && address.AddressFamily == System.Net.Sockets.AddressFamily.InterNetworkV6;
    }
}
