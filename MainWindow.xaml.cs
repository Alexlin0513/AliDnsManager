using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using AliDnsManager.Services;
using AliDnsManager.Windows;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Windows.Threading;

namespace AliDnsManager;

/// <summary>
/// Interaction logic for MainWindow.xaml
/// </summary>
public partial class MainWindow : Window
{
    private readonly AliDnsService _dnsService;
    private readonly ConfigService _configService;
    private readonly DispatcherTimer _timer;
    private List<DomainInfo> _domains = new();
    private List<DnsRecord> _records = new();

    public MainWindow()
    {
        InitializeComponent();
        _dnsService = new AliDnsService();
        _configService = new ConfigService();

        // 初始化定时器用于更新时间
        _timer = new DispatcherTimer();
        _timer.Interval = TimeSpan.FromSeconds(1);
        _timer.Tick += Timer_Tick;
        _timer.Start();

        LoadSavedConfig();
        UpdateUI();

        // 添加选择变化事件
        dgRecords.SelectionChanged += DgRecords_SelectionChanged;
    }

    private void LoadSavedConfig()
    {
        try
        {
            var (accessKeyId, accessKeySecret) = _configService.LoadConfig();
            txtAccessKeyId.Text = accessKeyId;
            txtAccessKeySecret.Password = accessKeySecret;
        }
        catch
        {
            // 忽略加载配置错误
        }
    }

    private void Timer_Tick(object? sender, EventArgs e)
    {
        txtCurrentTime.Text = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
    }

    private void DgRecords_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        UpdateRecordInfo();
        UpdateUI();
    }

    private void UpdateUI()
    {
        bool isConnected = _dnsService.IsConfigured;

        cmbDomains.IsEnabled = isConnected;
        btnRefreshDomains.IsEnabled = isConnected;
        btnAddRecord.IsEnabled = isConnected && cmbDomains.SelectedItem != null;
        btnEditRecord.IsEnabled = isConnected && dgRecords.SelectedItem != null;
        btnDeleteRecord.IsEnabled = isConnected && dgRecords.SelectedItem != null;
        btnRefreshRecords.IsEnabled = isConnected && cmbDomains.SelectedItem != null;
        btnEnableRecord.IsEnabled = isConnected && dgRecords.SelectedItem != null;
        btnDisableRecord.IsEnabled = isConnected && dgRecords.SelectedItem != null;

        // 更新连接状态
        txtStatus.Text = isConnected ? "已连接" : "未连接";
        statusIndicator.Fill = isConnected ? new SolidColorBrush(Color.FromRgb(40, 167, 69)) : new SolidColorBrush(Color.FromRgb(220, 53, 69));

        // 更新域名计数
        txtDomainCount.Text = _domains.Count > 0 ? $"共 {_domains.Count} 个域名" : "";

        // 更新记录计数
        txtRecordCount.Text = $"{_records.Count} 条记录";

        // 更新状态栏指示器
        statusBarIndicator.Fill = isConnected ? new SolidColorBrush(Color.FromRgb(40, 167, 69)) : new SolidColorBrush(Color.FromRgb(108, 117, 125));
    }

    private void UpdateRecordInfo()
    {
        if (dgRecords.SelectedItem is DnsRecord record)
        {
            txtSelectedRecord.Text = $"{record.RR} ({record.Type})";
        }
        else
        {
            txtSelectedRecord.Text = "无";
        }
    }

    private async void BtnConnect_Click(object sender, RoutedEventArgs e)
    {
        if (string.IsNullOrWhiteSpace(txtAccessKeyId.Text) || string.IsNullOrWhiteSpace(txtAccessKeySecret.Password))
        {
            MessageBox.Show("请输入AccessKey ID和AccessKey Secret", "错误", MessageBoxButton.OK, MessageBoxImage.Warning);
            return;
        }

        try
        {
            SetStatusMessage("正在连接...", "#FFC107");
            btnConnect.IsEnabled = false;

            _dnsService.Configure(txtAccessKeyId.Text.Trim(), txtAccessKeySecret.Password.Trim());

            // 测试连接
            await _dnsService.GetDomainsAsync();

            // 保存配置
            _configService.SaveConfig(txtAccessKeyId.Text.Trim(), txtAccessKeySecret.Password.Trim());

            UpdateUI();
            SetStatusMessage("连接成功", "#28A745");

            // 自动加载域名列表
            await LoadDomainsAsync();
        }
        catch (Exception ex)
        {
            MessageBox.Show($"连接失败: {ex.Message}", "错误", MessageBoxButton.OK, MessageBoxImage.Error);
            SetStatusMessage("连接失败", "#DC3545");
        }
        finally
        {
            btnConnect.IsEnabled = true;
        }
    }

    private async void BtnRefreshDomains_Click(object sender, RoutedEventArgs e)
    {
        await LoadDomainsAsync();
    }

    private async Task LoadDomainsAsync()
    {
        try
        {
            SetStatusMessage("正在加载域名列表...", "#17A2B8");
            _domains = await _dnsService.GetDomainsAsync();

            cmbDomains.ItemsSource = _domains;
            cmbDomains.DisplayMemberPath = "DomainName";
            cmbDomains.SelectedValuePath = "DomainName";

            SetStatusMessage($"已加载 {_domains.Count} 个域名", "#28A745");
            UpdateUI();
        }
        catch (Exception ex)
        {
            MessageBox.Show($"加载域名失败: {ex.Message}", "错误", MessageBoxButton.OK, MessageBoxImage.Error);
            SetStatusMessage("加载域名失败", "#DC3545");
        }
    }

    private async void CmbDomains_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (cmbDomains.SelectedItem is DomainInfo domain)
        {
            await LoadRecordsAsync(domain.DomainName);
        }
        UpdateUI();
    }

    private async Task LoadRecordsAsync(string domainName)
    {
        try
        {
            SetStatusMessage($"正在加载 {domainName} 的DNS记录...", "#17A2B8");
            _records = await _dnsService.GetDomainRecordsAsync(domainName);

            dgRecords.ItemsSource = _records;

            SetStatusMessage($"已加载 {_records.Count} 条DNS记录", "#28A745");
        }
        catch (Exception ex)
        {
            MessageBox.Show($"加载DNS记录失败: {ex.Message}", "错误", MessageBoxButton.OK, MessageBoxImage.Error);
            SetStatusMessage("加载DNS记录失败", "#DC3545");
        }
    }

    private async void BtnRefreshRecords_Click(object sender, RoutedEventArgs e)
    {
        if (cmbDomains.SelectedItem is DomainInfo domain)
        {
            await LoadRecordsAsync(domain.DomainName);
        }
    }

    private void BtnAddRecord_Click(object sender, RoutedEventArgs e)
    {
        if (cmbDomains.SelectedItem is DomainInfo domain)
        {
            var dialog = new RecordEditWindow(domain.DomainName);
            if (dialog.ShowDialog() == true)
            {
                _ = AddRecordAsync(dialog.Record);
            }
        }
    }

    private void BtnEditRecord_Click(object sender, RoutedEventArgs e)
    {
        if (dgRecords.SelectedItem is DnsRecord record && cmbDomains.SelectedItem is DomainInfo domain)
        {
            var dialog = new RecordEditWindow(domain.DomainName, record);
            if (dialog.ShowDialog() == true)
            {
                _ = UpdateRecordAsync(dialog.Record);
            }
        }
    }

        private void DgRecords_PreviewMouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (e.OriginalSource is DependencyObject source)
            {
                // 若双击发生在“多选”列的复选框内，忽略
                if (FindVisualParent<CheckBox>(source) != null)
                {
                    return;
                }

                var row = ItemsControl.ContainerFromElement(dgRecords, source) as DataGridRow;
                if (row == null) return; // 非数据行，忽略
            }

            if (dgRecords.SelectedItem is DnsRecord record && cmbDomains.SelectedItem is DomainInfo domain)
            {
                e.Handled = true; // 阻止 DataGrid 自身的编辑启动
                var dialog = new RecordEditWindow(domain.DomainName, record);
                if (dialog.ShowDialog() == true)
                {
                    _ = UpdateRecordAsync(dialog.Record);
                }
            }
        }

        private void DgRecords_BeginningEdit(object sender, DataGridBeginningEditEventArgs e)
        {
            // 任何尝试进入单元格编辑时，阻止并改为弹出编辑窗口
            e.Cancel = true;
            if (dgRecords.SelectedItem is DnsRecord record && cmbDomains.SelectedItem is DomainInfo domain)
            {
                var dialog = new RecordEditWindow(domain.DomainName, record);
                if (dialog.ShowDialog() == true)
                {
                    _ = UpdateRecordAsync(dialog.Record);
                }
            }
        }


    private async void BtnDeleteRecord_Click(object sender, RoutedEventArgs e)
    {
        if (dgRecords.SelectedItem is DnsRecord record)
        {
            var result = MessageBox.Show($"确定要删除记录 {record.RR}.{((DomainInfo)cmbDomains.SelectedItem).DomainName} 吗？",
                "确认删除", MessageBoxButton.YesNo, MessageBoxImage.Question);

            if (result == MessageBoxResult.Yes)
            {
                await DeleteRecordAsync(record.RecordId);
            }
        }
    }

    private async void BtnEnableRecord_Click(object sender, RoutedEventArgs e)
    {
        if (dgRecords.SelectedItem is DnsRecord record)
        {
            await SetRecordStatusAsync(record.RecordId, "Enable");
        }
    }

    private async void BtnDisableRecord_Click(object sender, RoutedEventArgs e)
    {
        if (dgRecords.SelectedItem is DnsRecord record)
        {
            await SetRecordStatusAsync(record.RecordId, "Disable");
        }
    }

    private async Task AddRecordAsync(DnsRecord record)
    {
        try
        {
            txtStatusBar.Text = "正在添加DNS记录...";
            var domain = (DomainInfo)cmbDomains.SelectedItem;

            await _dnsService.AddDomainRecordAsync(
                domain.DomainName,
                record.RR,
                record.Type,
                record.Value,
                record.TTL,
                record.Priority,
                record.Line);

            txtStatusBar.Text = "DNS记录添加成功";
            await LoadRecordsAsync(domain.DomainName);
        }
        catch (Exception ex)
        {
            MessageBox.Show($"添加DNS记录失败: {ex.Message}", "错误", MessageBoxButton.OK, MessageBoxImage.Error);
            txtStatusBar.Text = "添加DNS记录失败";
        }
    }

    private async Task UpdateRecordAsync(DnsRecord record)
    {
        try
        {
            txtStatusBar.Text = "正在更新DNS记录...";
            var domain = (DomainInfo)cmbDomains.SelectedItem;

            await _dnsService.UpdateDomainRecordAsync(
                record.RecordId,
                record.RR,
                record.Type,
                record.Value,
                record.TTL,
                record.Priority,
                record.Line);

            txtStatusBar.Text = "DNS记录更新成功";
            await LoadRecordsAsync(domain.DomainName);
        }
        catch (Exception ex)
        {
            MessageBox.Show($"更新DNS记录失败: {ex.Message}", "错误", MessageBoxButton.OK, MessageBoxImage.Error);
            txtStatusBar.Text = "更新DNS记录失败";
        }
    }

    private async Task DeleteRecordAsync(string recordId)
    {
        try
        {
            txtStatusBar.Text = "正在删除DNS记录...";
            var domain = (DomainInfo)cmbDomains.SelectedItem;

            await _dnsService.DeleteDomainRecordAsync(recordId);

            txtStatusBar.Text = "DNS记录删除成功";
            await LoadRecordsAsync(domain.DomainName);
        }
        catch (Exception ex)
        {
            MessageBox.Show($"删除DNS记录失败: {ex.Message}", "错误", MessageBoxButton.OK, MessageBoxImage.Error);
            txtStatusBar.Text = "删除DNS记录失败";
        }
    }

    private async Task SetRecordStatusAsync(string recordId, string status)
    {
        try
        {
            txtStatusBar.Text = $"正在{(status == "Enable" ? "启用" : "禁用")}DNS记录...";
            var domain = (DomainInfo)cmbDomains.SelectedItem;

            await _dnsService.SetDomainRecordStatusAsync(recordId, status);

            txtStatusBar.Text = $"DNS记录{(status == "Enable" ? "启用" : "禁用")}成功";
            await LoadRecordsAsync(domain.DomainName);
        }
        catch (Exception ex)
        {
            MessageBox.Show($"{(status == "Enable" ? "启用" : "禁用")}DNS记录失败: {ex.Message}", "错误", MessageBoxButton.OK, MessageBoxImage.Error);
            txtStatusBar.Text = $"{(status == "Enable" ? "启用" : "禁用")}DNS记录失败";
        }
    }

    private void MenuClearConfig_Click(object sender, RoutedEventArgs e)
    {
        var result = MessageBox.Show("确定要清除保存的配置吗？", "确认", MessageBoxButton.YesNo, MessageBoxImage.Question);
        if (result == MessageBoxResult.Yes)
        {
            _configService.ClearConfig();
            txtAccessKeyId.Text = "";
            txtAccessKeySecret.Password = "";
            // endpoint 字段已移除，无需重置
            MessageBox.Show("配置已清除", "提示", MessageBoxButton.OK, MessageBoxImage.Information);
        }
    }

    private void MenuExit_Click(object sender, RoutedEventArgs e)
    {
        Close();
    }

    private void MenuAbout_Click(object sender, RoutedEventArgs e)
    {
        MessageBox.Show("阿里云DNS解析管理器 v1.0.0\n\n一个简单易用的阿里云DNS记录管理工具\n\n作者：Alexlin",
            "关于", MessageBoxButton.OK, MessageBoxImage.Information);
    }


        // 查找视觉树中的父级元素
        private static T? FindVisualParent<T>(DependencyObject child) where T : DependencyObject
        {
            DependencyObject? parentObject = VisualTreeHelper.GetParent(child);
            while (parentObject != null)
            {
                if (parentObject is T parent)
                    return parent;
                parentObject = VisualTreeHelper.GetParent(parentObject);
            }
            return null;
        }
    private void SetStatusMessage(string message, string colorHex)
    {
        txtStatusBar.Text = message;
        statusBarIndicator.Fill = new SolidColorBrush((Color)ColorConverter.ConvertFromString(colorHex));
    }
}