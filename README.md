# La forme de ma bulle | 我的个人空间泡泡

> **Version 1.0 · Projet de réalité mixte sur la proxémie**  
> **v1.0 · 关于人际距离与个人空间的混合现实项目**

## 1. Présentation | 项目简介

**FR** — *La forme de ma bulle* propose de visualiser et d'ajuster en réalité mixte quatre zones de proxémie : intime, personnelle, sociale et publique. L'utilisateur peut modifier leur forme par des points de contrôle. L'expérience s'accompagne d'un protocole d'observation et d'un questionnaire sur la perception des distances sociales.

**中文** — 本项目利用 Meta Quest 3 的混合现实透视画面，将亲密、个人、社交、公共四种人际距离区域可视化。参与者可以使用手柄调节控制点，形成自己的空间轮廓，并通过实验观察、CSV 数据记录和问卷研究这种可视化对人际互动的影响。

**Version / 版本说明** — **v1.0 表示首个可交付的单人实验版本**；双机互看、双人形状同步和共同空间坐标校准属于后续增强功能，不是 v1.0 的交付前提。`Builds/Client/Bulle.exe` 是项目当前已有的 Windows 构建入口；独立运行、Quest 3 连接及 MR 画面仍需在目标电脑上执行验收。

## 2. Environnement | 技术环境

| 技术 | 用途 |
| --- | --- |
| Unity 6.3 LTS / C# | 场景、交互及构建 |
| Meta Quest 3 | XR 头显 |
| OpenXR + SteamVR + ALVR (PCVR) | 头显追踪、Windows PCVR 连接、透视实验 |
| Unity Input System | Grip、Trigger、A/B/X/Y、手柄射线 |
| Catmull–Rom spline | 根据控制点绘制区域轮廓与可视化带状面 |
| Photon Fusion 2 | 双人网络增强：当前采用 **Host/Client** 方向，仍在开发 |
| Python 3 | 分析 CSV 中的几何尺寸 |
| Git / GitHub | 协作与版本管理 |

## 3. Fonctionnalités v1.0 | 当前交付内容

**现有原型 / Prototype local**

- FourZones 的四个区域和交互式控制点（具体启用状态以 Unity 场景为准）。
- XR 手柄射线选择，Grip 拖动控制点，以及区域显隐控制。
- 通过 `Acquisition` 保存坐标；原始导出格式为 `Index;Nom;X;Y;Z`。
- Windows 桌面构建：`Builds/Client/Bulle.exe`（已存在于开发电脑，**不等于已经完成运行验收**）。
- 配套实验文档与 Google Forms 问卷。

**后续增强 / Extensions en cours**

- Photon Fusion 双人 Avatar：每个用户在对方的头显中出现。
- 双人位置、轮廓控制点与显隐状态同步。
- 两台 Quest 3 的物理空间坐标校准。
- 实验数据的自动配对统计、稳定的头显内保存反馈。

## 4. Structure du dépôt | 建议目录结构

```text
Bulle/
├── Assets/                   # Unity 项目资源、场景和脚本（务必保留 .meta）
│   ├── Scenes/AnchorTest.unity
│   ├── Scripting/
│   ├── Prefab/
│   ├── Photon/               # Photon Fusion 包，不要手工删除
│   ├── XR/                   # XR/插件配置，不要手工删除
│   └── ...
├── Packages/
├── ProjectSettings/
├── Builds/
│   └── Client/
│       ├── Bulle.exe
│       ├── Bulle_Data/       # Unity 构建资源（示意；以实际文件夹名为准）
│       └── UnityPlayer.dll  # Windows Unity Player 常见依赖
├── Data/                     # Editor 下导出的实验 CSV
├── Reports/                  # Python 分析报告与图像（生成）
├── Tools/
│   └── analyze_bulle_csv.py
├── docs/
│   ├── Protocole de test.docx
│   ├── Définition des bulles.docx
│   ├── DATA_METHOD_CN.md
│   └── RELEASE_CHECKLIST_CN.md
└── README.md
```

**注意：** 不要在 Windows 资源管理器里随意移动 `Assets/` 内的 Unity 资源；如需整理资源目录，请在 Unity Project 面板中移动，保持 `.meta` 与 GUID 引用。

## 5. Scripts et responsabilités | 脚本职责

| 脚本 | 作用 / 状态 |
| --- | --- |
| `FiveSpheresController.cs` | 五个控制点、头部位置跟随、Grip 拖动；当前功能核心，保留 |
| `SphereSplineLink.cs`, `SplineRibbon.cs` | Catmull–Rom 曲线和可视化面；保留 |
| `FourZonesVisibility.cs` | 四个区域的显隐操作；保留 |
| `ControllerRayVisual.cs` | 手柄射线渲染；保留，后续核对抓取方向的一致性 |
| `CoordinateManager.cs`, `ControllerAcquisition.cs` | Trigger 按键坐标采集与 CSV 导出；保留 |
| `AcquisitionFeedbackUI.cs` | **可选增强**：在 Quest 画面里显示保存成功/失败；需添加并配置 |
| `NetworkBullePlayer.cs` | 双人 Avatar 原型；为未来增强保留 |
| `FourZonesNetworkAvatar.cs`, `FourZonesNetworkSync.cs` | 旧网络尝试；先归档、避免与新方案并行启动 |
| `FusionConnection.cs` | 旧 Shared Mode 启动脚本；正式场景不使用（由 Fusion Bootstrap 接管） |
| `HeadPositionSyncTest.cs` | 旧 NGO 测试；正式场景不使用 |
| `AnchorSupportCheck.cs` | 空间锚点支持检测；PCVR 下可禁用，保留为诊断参考 |
| `BulleFollower`, `BulleControls`, `BulleVisibility`, `BulleDeformer`, `BulleWireframe`, `ProxemicRingsController` | 旧/可选表现方式；是否删除需以场景与 Prefab 引用为准 |

## 6. Données expérimentales | CSV 数据与几何分析

**V1.0 既有 CSV：** `Index;Nom;X;Y;Z`，其中记录 20 个控制点时，按四层各五点进行解释。旧格式没有显式的身体中心，因而可以计算轮廓面积和 X/Z 跨度，但不能可靠推断控制点距身体中心的真实距离。

**可选增强版导出：** 在保持旧格式前五列不变的基础上，添加 `Zone`、`Point`、`CenterX/Y/Z`、`Phase`、`SessionId` 等字段，记录区域归属、空间参照、测量阶段与匿名实验编号；同一次启动第一次保存自动为 `initial`，第二次及之后为 `final`。升级代码见 `Unity/Scripting/CoordinateManager.cs`。

运行（Windows，Python 3）：

```powershell
cd D:\Bulle
python Tools\analyze_bulle_csv.py --data D:\Bulle\Data --out D:\Bulle\Reports
```

输出：`Reports/summary_all.csv`、每份输入对应的 `metrics_*.csv` 与 `outline_*.svg`；存在同一匿名会话的 `initial` 与 `final` 时，还会生成 `paired_changes.csv`。

**度量方法**：每层 5 点投影到 Unity 的 X/Z 平面 → 根据平面角度排列 → 连接最后一点和第一点形成分析用五边形 → 用鞋带公式计算面积 `A` → 以 `R_eq = sqrt(A / pi)` 得到**等效面积半径**。另外输出 X 向宽度、Z 向深度及轮廓周长。这个值是**五点多边形近似**，不是 Unity Catmull–Rom 渲染曲面的精确面积；细节见 [`docs/DATA_METHOD_CN.md`](docs/DATA_METHOD_CN.md)。

## 7. Protocole et questionnaire | 实验方案与问卷

- [Protocole de test](docs/Protocole%20de%20test.docx) — 个人校准、与不同熟悉程度的人接近、双人互动与前后对比。
- [Définition des bulles](docs/D%C3%A9finition%20des%20bulles.docx) — Hall 的四种距离定义及相关研究综述。
- **[Questionnaire Google Forms — Répondre](https://docs.google.com/forms/d/e/1FAIpQLScVudi-cjfZsHRLrLHXSIc4pNQ077iVJjswrPWrQFQzQKS5XA/viewform)** — 参与者主观反馈问卷（已创建链接）。

**实验建议：** 每次启动自动生成匿名 `SessionId`；第一次保存为 `initial`，第二次及之后保存为 `final`。保存两份独立 CSV，才能在统计上比较面积、等效半径和形状变化。收集实验数据时应告知参与者用途并取得同意，避免将姓名写入公开仓库。

## 8. Exécuter le build | Windows 构建与运行

现有构建入口：`D:\Bulle\Builds\Client\Bulle.exe`。

1. 在 Windows 电脑上打开 **整个 `Builds/Client/` 目录中的 `Bulle.exe`**。
2. 不要只复制 `Bulle.exe`：应连同 `Bulle_Data`、`UnityPlayer.dll` 及构建产生的其他文件一起分发。
3. 启动 SteamVR / ALVR 并连接 Meta Quest 3，确认 XR 设备跟踪与 Passthrough 正常。
4. 检查 FourZones 是否显示、Grip 是否可以拖动、按钮显隐是否有效、Trigger 是否成功保存 CSV。
5. 如果修改 Unity C# 或场景，**旧的 EXE 不会自动更新**；需要在 `File > Build Profiles` 重新构建 Windows x64，并替换对应输出。

**数据路径**：旧版脚本默认写入 Windows 的 `Documents/RelevesVR`；本次提供的升级版在 **Unity Editor** 下写入项目 `Data/`，而在 EXE 下写入 Unity 的 `Application.persistentDataPath/Data`（常见于 `%USERPROFILE%\AppData\LocalLow\<Company>\<Product>\Data`）。实际位置在 Console 的 `[BULLE CSV] Saved ...` 中显示。

## 9. Roadmap | 后续开发优先级

1. **v1.0 验收与冻结**：检查 EXE、单人功能、CSV 20 点和场景引用，提交 Git tag `v1.0`。
2. **v1.0.x 实验工具优化**：保存成功的 XR UI 反馈、中心参照、初始/最终两次导出、Python 分析。
3. **v1.1 双机显示**：Fusion Host/Client 为两人生成 Avatar，先同步位置和对方的 FourZones。
4. **v1.2 同步行为与空间**：控制点与显隐同步、统一真实空间参考点、双人实验验证。
5. **后续研究分析**：配对比较初始/最终的四层面积、等效半径及主观问卷结果。

---

**Statut / 当前状态**：Bulle v1.0 est la première version de référence du prototype expérimental local. La visualisation mutuelle sur deux casques reste une extension à développer et à valider. / Bulle v1.0 为单人实验基准版；双机互看与空间同步保留为后续优化。
