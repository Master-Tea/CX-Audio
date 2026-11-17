# CX Audio 的 Microsoft Store 上架评估

评估日期：2026-11-17。依据微软当前公开政策 7.20（2026-10-22 生效）及本仓库代码；未登录 Partner Center、未提交审核，以下是技术准备度判断，不代表微软预先批准。

## 结论与建议

普通模式的 WinUI 3 / .NET 桌面音频串流功能有采用 MSIX 上架的可行路径。当前带“下载、提权安装第三方音频驱动”的完整版本不宜直接作为已准备就绪的商店包提交。

建议先制作独立的 Store 构建：保留普通音频串流、无线接收和无需提权即可使用的功能，移除应用内 VB-CABLE 驱动下载/提权安装流程。若要在商店版保留依赖第三方驱动的高级模式，应先向微软明确披露并确认审核路径，提供该驱动的 WHCP 认证材料和具体依赖说明。数字签名有效不等于已经确认 WHCP 认证。

GitHub 完整版本可以继续提供高级模式和官方驱动安装入口。不要在商店宣传文案承诺硬件级“彻底切断发声”：端点静音无法阻断绕过 Windows 音频引擎的所有路径，也存在新设备或取消静音后保护重新生效的时间窗口。

## 当前差距

| 项目 | 代码/产物现状 | 提交前需要完成 |
| --- | --- | --- |
| 应用类型 | WinUI 3 全信任桌面应用，现有 MSIX 可安装启动 | 用正式 Store 构建在干净的 Windows 10/11 和普通用户账户验证 |
| 包身份 | 本地侧载身份 `MasterTea.CXAudio`，发布者 `CN=MasterTea` | 在 Partner Center 注册开发者账户、预留名称 CX Audio；按账户给定的 Identity Name、Publisher 和 PublisherDisplayName 构建商店包，名称可用性待后台确认 |
| 签名 | 自签名用于 GitHub 侧载 | MSIX 商店提交不要求购买 CA 代码签名证书，审核通过后由商店重新签名；不要把导入自签名证书的脚本作为商店安装流程 |
| 包版本 | 侧载修复包 1.4.2.2 | Store 构建必须使用第四段为 0 的新版本；当前版本用于升级本机诊断安装，不应直接提交商店 |
| 第三方驱动 | VB-CABLE，由应用从官网下载、验证签名，调用 UAC 安装器 | 商店政策 10.2.4 通常不允许依赖非微软驱动，但 WHCP 驱动可逐案考虑；认证备注必须披露依赖。MSIX 自身不能安装内核驱动 |
| 管理员权限 | 主程序正常运行无需管理员；驱动安装和部分防火墙修复流程可能提权 | 商店构建移除或重做这些流程并披露。`runFullTrust` 不等于管理员权限，也不保证提权安装流程能通过审核 |
| 受限能力 | MSIX 声明 `runFullTrust` | 在 Partner Center 按要求说明用途：WASAPI 系统音频采集、本地 HTTP/WebSocket 服务、原生桌面窗口；只保留必要能力 |
| 隐私 | 会采集系统正在播放的音频，并经局域网/USB 发送；日志含设备名称、接收端 IP、连接状态 | 发布可公开访问的隐私政策 URL，在应用内及商店后台提供链接；说明音频实时传输、是否持久存储、日志位置/删除方法、访问码、网络安全边界、第三方下载与用户控制。Win32/桌面桥产品按政策必须有隐私政策 |
| 网络安全 | HTTP/WebSocket 局域网音频；有访问码验证与错误尝试限流 | 审核音频和控制接口授权、局域网明文传输的披露、访问码轮换、默认网络绑定及断连行为；避免误称端到端加密 |
| 第三方许可 | 保留 VB-CABLE 来源和 Donationware 说明，发布包不修改第三方驱动文件 | 按 VB-Audio 当前许可保留用户可识别来源及支持/购买机会；专业批量部署需相应许可。设备显示名 CX Audio 仅为 Windows 端点别名，不能宣称是自研驱动 |
| 认证测试 | 已有音频事务测试和本机安装启动检查 | 运行 Windows App Certification Kit，保存报告；测试安装、升级、卸载、崩溃恢复、音频设备热插拔、无驱动、无网络、标准用户、DPI/无障碍等情景 |
| 商店材料 | README、图标及发布说明已存在 | 准备真实界面截图、简中描述、系统要求、第三方依赖说明、支持 URL、隐私 URL、年龄评级、市场/价格及审核测试步骤；不能只提交 GitHub README |
| 架构/最低系统 | 当前 x64，自包含 Windows App SDK；声明最低 17763，项目目标 19041 | 只勾选实际验证的架构和系统；核对所用 SDK 的真实最低系统要求与包清单是否一致。ARM64 需单独构建验证，不必为首次上架强行支持 |

## 若选择 EXE/MSI 上架

微软也支持传统 EXE/MSI：必须提供固定、带版本的 HTTPS 下载地址；安装器需离线完整包，支持静默安装（允许 UAC）；安装器以及其中 PE 文件应使用链到微软受信任根的代码签名证书。当前自签名证书、ZIP/MSIX 和运行时下载驱动的流程不能直接满足这一整套要求。选择 EXE/MSI 仍须遵守第三方驱动依赖政策，不会自动豁免 10.2.4。

因此不建议为了保留驱动安装按钮就立即切换到 EXE/MSI 商店提交。

## 可执行的准备顺序

1. 在 Partner Center 建立产品并确认名称和包身份。
2. 添加独立的 Store 构建开关，移除下载/提权安装驱动和防火墙提权入口；决定高级模式是否完全从第一版移除。
3. 发布正式隐私政策与支持页面，补齐应用内链接及商店材料。
4. 清理清单与能力，检查真实最低 Windows 版本；生成第四段为 0 的 Store 包。
5. 在干净虚拟机及真实音频设备上测试并运行 WACK，整理普通用户可复现的审核说明。
6. 若坚持商店提供第三方虚拟声卡功能，先向微软说明驱动名称/版本/WHCP 材料并获取具体审核要求，再决定最终发行方案。

## 官方来源

- [Microsoft Store Policies 7.20](https://learn.microsoft.com/en-us/windows/apps/publish/store-policies)：10.2.4 第三方依赖与驱动，10.5 隐私，10.6 能力，10.1 准确说明产品行为。
- [Prepare to package a desktop application](https://learn.microsoft.com/en-us/windows/msix/desktop/desktop-to-uwp-prepare)：MSIX 不支持 Windows 驱动安装；桌面应用的提权限制。该文档更新早于当前 Store 政策，逐案驱动例外应以最新政策及微软审核结论为准。
- [App package requirements for MSIX app](https://learn.microsoft.com/en-us/windows/apps/publish/publish-your-app/msix/app-package-requirements)：Store 包身份、签名、版本及 WACK。
- [App package requirements for MSI/EXE app](https://learn.microsoft.com/en-us/windows/apps/publish/publish-your-app/msi/app-package-requirements)：可信代码签名、固定 HTTPS 地址、静默与离线安装。
- [VB-Audio licensing](https://vb-audio.com/Services/licensing.htm)：VB-CABLE 原样分发、来源/Donationware 披露及专业部署许可。
