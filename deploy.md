# 部署与发布指南（AliDnsManager）

本指南面向在 Windows 10/11 上部署与发布 AliDnsManager（WPF，.NET 9）的人员。

## 1. 开发环境准备
- Windows 10/11
- [.NET 9 SDK/Runtime](https://dotnet.microsoft.com/)
- 克隆仓库后进入 `AliDnsManager` 目录

## 2. 还原依赖与本地运行
- 还原依赖：`dotnet restore`
- 构建调试：`dotnet build`
- 本地运行：`dotnet run`

注意：运行时需要有效的阿里云 RAM 用户 AccessKey。建议使用最小权限策略，仅授权 AliDNS 所需权限。

## 3. 发布（Publish）

### 3.1 目标与风格
- 目标 Runtime：`win-x64`
- UI：WPF
- 目标框架：`net9.0-windows`

### 3.2 依赖框架（Framework-dependent）
生成较小体积，要求目标机器已安装 .NET 9 Runtime。

```bash
dotnet publish -c Release -r win-x64 --self-contained false
```

产物路径：`AliDnsManager/bin/Release/net9.0-windows/win-x64/publish/`

### 3.3 自包含（Self-contained）+ 单文件
无需在目标机器安装 .NET 运行时，体积更大。

```bash
dotnet publish -c Release -r win-x64 --self-contained true -p:PublishSingleFile=true
```

可按需增加 ReadyToRun/Trim 等参数（谨慎裁剪 WPF 应用）。

## 4. 版本与发布流程（建议）
1. 更新版本号与变更日志（如在 README 或发行说明中）
2. 打 Tag：`git tag vX.Y.Z && git push origin vX.Y.Z`
3. GitHub Releases：创建 Release，上传 `publish` 目录产物（zip）
4. 在 Release 页面附带运行要求（Windows 版本、是否需要 .NET Runtime）

## 5. 目录结构与产物说明
- `publish/`：最终分发目录（exe、dll、依赖等）
- `AliDnsManager.dll` / `AliDnsManager.exe`：应用主体
- `App.ico`：应用图标（已在 csproj 中配置）

## 6. 环境与权限
- 网络需可访问 `alidns.cn-hangzhou.aliyuncs.com`
- 应用首次运行需在【账户配置】中输入 AccessKeyId/AccessKeySecret
- 凭证加密：使用 Windows DPAPI 加密存储，配置文件位于用户 AppData 目录

## 7. 常见问题
- 无法启动：请先安装 .NET 9 Runtime（若用依赖框架模式）
- 连接失败：检查 AccessKey 权限、网络连通性
- 发布后 UI 异常：确认未启用影响 WPF 的过度裁剪（Trim）

