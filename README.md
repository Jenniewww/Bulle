
# La forme de ma bulle | 我的个人空间泡泡

**Projet de réalité mixte sur la proxémie**  
**关于人际距离与个人空间的混合现实项目**

**Version : 1.0 — 2 novembre 2026**

---

## 1. Présentation du projet | 项目简介

### Français

**La forme de ma bulle** est un projet de réalité mixte consacré à la **proxémie**, c'est-à-dire à la manière dont les individus perçoivent et organisent les distances interpersonnelles.

L'objectif est de rendre visible une notion habituellement invisible : **l'espace personnel qui entoure chaque individu**.

À l'aide d'un casque Meta Quest 3, l'utilisateur peut visualiser quatre zones autour de son corps et modifier leurs dimensions à l'aide des contrôleurs VR.

Le projet cherche notamment à répondre aux questions suivantes :

- Comment représenter visuellement l'espace personnel ?
- Les dimensions de cet espace varient-elles selon les individus ?
- Comment ces dimensions évoluent-elles entre deux situations expérimentales ?
- La visualisation des zones peut-elle modifier la perception des distances interpersonnelles ?
- Comment comparer les espaces personnels de deux individus dans un même environnement ?

Le projet combine développement informatique, interaction immersive et expérimentation auprès d'utilisateurs.

### 中文

**La forme de ma bulle（我的个人空间泡泡）** 是一个研究**人际距离学（Proxémie）**的混合现实项目。

项目希望把通常不可见的个人空间转化为可以观察、调整和测量的虚拟区域。

用户佩戴 Meta Quest 3 后，可以在真实环境中看到围绕自身的四个距离区域，并使用 VR 手柄改变这些区域的形状和尺寸。

项目主要研究：

- 如何直观地可视化个人空间？
- 不同人的个人空间尺寸是否存在差异？
- 同一人在不同实验情境下是否会调整个人空间？
- 空间可视化是否会影响人们对人际距离的感知？
- 如何在同一个真实空间中比较两个人的个人空间？

项目结合了 Unity 开发、沉浸式交互和用户实验。

---

## 2. Fondements théoriques | 理论基础

### Français

Le projet s'appuie principalement sur les travaux d'**Edward T. Hall** concernant la proxémie.

Hall distingue quatre grandes catégories de distances interpersonnelles :

1. **Distance intime**
2. **Distance personnelle**
3. **Distance sociale**
4. **Distance publique**

Ces catégories constituent un cadre théorique et non des dimensions universelles applicables à tous les individus.

Notre application permet aux participants de construire leur propre représentation de ces quatre espaces.

### 中文

项目主要参考 **Edward T. Hall** 的人际距离理论。

该理论将人际距离划分为四个主要区域：

1. **亲密距离（Intime）**
2. **个人距离（Personnelle）**
3. **社交距离（Sociale）**
4. **公共距离（Publique）**

这些分类提供了理论框架，但并不意味着所有人的空间边界都具有完全相同的尺寸。

因此，本项目允许参与者自行调整四个区域，而不是直接采用固定半径。

### Références principales | 主要参考文献

- Hall, E. T. (1966). *The Hidden Dimension*.
- Moles, A. A., & Rohmer, E. (1982). *Labyrinthes du vécu*.
- Moles, A. A. (1998). *Psychosociologie de l'espace*.

---

## 3. Environnement technique | 技术环境

| Technologie / 技术 | Utilisation / 用途 |
|---|---|
| Unity 6.3 LTS | Développement de l'application / 应用开发 |
| C# | Programmation des interactions / 交互逻辑 |
| Meta Quest 3 | Casque de réalité mixte / 混合现实头显 |
| OpenXR | Suivi XR / XR 追踪 |
| SteamVR + ALVR | Connexion PCVR / 头显与电脑连接 |
| Universal Render Pipeline (URP) | Rendu graphique / 图形渲染 |
| XR Interaction Toolkit | Interactions avec les contrôleurs / 手柄交互 |
| Photon Fusion 2 | Prototype multijoueur / 多人网络原型 |
| Python | Analyse des données / 数据分析 |
| Matplotlib | Visualisation des résultats / 数据可视化 |
| Git / GitHub | Gestion des versions / 版本管理 |

### Architecture générale | 总体架构

```text
Meta Quest 3
    |
    | SteamVR / ALVR / OpenXR
    v
Unity 6.3 LTS
    |
    +-- XR Origin
    |     |
    |     +-- Main Camera
    |     +-- Left Controller
    |     +-- Right Controller
    |
    +-- FourZones
    |     |
    |     +-- Zone_Intime
    |     +-- Zone_Personnelle
    |     +-- Zone_Sociale
    |     +-- Zone_Publique
    |
    +-- Acquisition
    |     |
    |     +-- CoordinateController
    |     +-- AcquisitionFeedbackUI
    |
    +-- Photon Fusion
    |     |
    |     +-- NetworkRunner
    |     +-- NetworkPlayer
    |
    v
CSV Data
    |
    v
Python Analysis
    |
    v
Reports / Figures
```

**FR** — Cette architecture présente les principaux modules du projet. Les fonctions multijoueurs restent expérimentales.

**中文** — 以上展示项目的主要模块。多人网络功能仍处于实验开发阶段。

---

## 4. Fonctionnalités de la version 1.0 | 1.0 版本功能

### 4.1 Visualisation des quatre zones | 四个区域可视化

**FR**

L'application représente quatre espaces autour de l'utilisateur :

- Zone intime
- Zone personnelle
- Zone sociale
- Zone publique

Chaque zone possède cinq points de contrôle permettant de modifier sa géométrie.

Les frontières sont représentées à l'aide d'éléments graphiques construits dans Unity.

**中文**

应用围绕用户显示四个个人空间区域。

每个区域由五个控制点定义，用户可以通过移动控制点改变区域形状。

Unity 使用样条曲线和相关渲染组件构建区域边界。

### 4.2 Interaction avec les contrôleurs | 手柄交互

**FR**

Le prototype permet de sélectionner et déplacer les points de contrôle avec les rayons des manettes VR et la touche Grip.

Des fonctionnalités de visibilité et de changement d'échelle ont également été développées ou testées.

**中文**

用户可以使用 VR 手柄射线选中控制点，并通过 Grip 按键进行拖动。

项目还开发或测试了区域显示、隐藏和缩放功能。

### 4.3 Suivi de l'utilisateur | 用户位置跟随

**FR**

Les zones sont positionnées relativement à l'utilisateur et suivent ses déplacements.

Le positionnement vertical et la gestion de l'orientation du casque ont fait l'objet de plusieurs ajustements.

**中文**

FourZones 可以根据用户的位置移动。

开发过程中曾遇到头显倾斜导致区域平面倾斜，以及区域高度不稳定的问题，因此位置跟随逻辑仍需持续验证。

### 4.4 Acquisition des coordonnées | 坐标采集

**FR**

Le système d'acquisition enregistre les coordonnées des vingt points de contrôle dans un fichier CSV.

Chaque acquisition contient cinq points pour chacune des quatre zones.

Les fichiers sont ensuite analysés avec Python.

**中文**

坐标采集模块可以将四个区域共 20 个控制点的坐标保存为 CSV 文件。

每个区域包含 5 个点。

这些数据用于计算区域面积、等效半径以及不同实验条件下的变化。

### 4.5 Retour visuel de sauvegarde | 保存反馈界面

**FR**

Un composant `AcquisitionFeedbackUI` a été ajouté afin de fournir une confirmation visuelle lors de l'enregistrement.

L'affichage doit être relié au gestionnaire de coordonnées et à la caméra XR, puis validé dans le casque.

**中文**

项目新增 `AcquisitionFeedbackUI`，用于在保存坐标时向用户显示视觉反馈。

该组件需要绑定 `CoordinateManager` 和 XR `Main Camera`，并在头显中完成实际测试。

### 4.6 Version exécutable Windows | Windows 可执行版本

**FR**

Une version Windows de l'application a été générée.

Chemin local :

`D:\Bulle\Builds\Client\Bulle.exe`

La présence de cet exécutable ne garantit pas que toutes les configurations XR et réseau fonctionnent sur un autre ordinateur.

**中文**

项目已经生成 Windows 可执行程序：

`D:\Bulle\Builds\Client\Bulle.exe`

但生成 EXE 并不等于所有设备都已通过测试。运行时仍需要正确的 OpenXR、SteamVR、ALVR 和头显环境。

---

## 5. Organisation du projet | 项目文件结构

```text
Bulle/
|
|-- Assets/
|   |-- Scenes/
|   |-- Scripting/
|   |-- Photon/
|   |-- XR/
|   |-- Settings/
|   |-- ...
|
|-- Data/
|   |-- Arthur1_Coordonnees_....csv
|   |-- Arthur2_Coordonnees_....csv
|   |-- ...
|
|-- Tools/
|   |-- analyze_bulle_csv.py
|   |-- visualize_bulle.py
|
|-- Reports/
|   |-- bulle_geometry_summary.csv
|   |-- figures/
|       |-- radius_by_participant.png
|       |-- area_change.png
|
|-- Builds/
|   |-- Client/
|       |-- Bulle.exe
|       |-- Bulle_Data/
|       |-- ...
|
|-- docs/
|   |-- Protocole de test.docx
|   |-- Définition des bulles.docx
|
|-- README.md
|-- .gitignore
```

**FR** — Les fichiers générés doivent être distingués des sources Unity. Les dossiers `Library`, `Temp`, `Obj` et les fichiers de projet IDE générés automatiquement ne doivent généralement pas être versionnés.

**中文** — 应区分 Unity 源文件和自动生成文件。通常不需要将 `Library`、`Temp`、`Obj` 以及自动生成的 IDE 项目文件提交到 Git。

Les dossiers `Tools`, `Reports` et `docs` correspondent à l'organisation recommandée. Leur présence dépend des fichiers effectivement créés.

`Tools`、`Reports` 和 `docs` 是推荐的项目组织方式，实际内容以本地已创建的文件为准。

---

## 6. Scripts principaux | 主要脚本

| Script | Fonction / 功能 |
|---|---|
| `FiveSpheresController.cs` | Gestion des points de contrôle / 控制点管理 |
| `BulleFollower.cs` | Suivi de la caméra / 跟随头显 |
| `ControllerRayVisual.cs` | Visualisation des rayons / 手柄射线 |
| `CoordinateManager.cs` | Acquisition et export CSV / 坐标采集 |
| `AcquisitionFeedbackUI.cs` | Retour visuel de sauvegarde / 保存反馈 |
| `NetworkBullePlayer.cs` | Prototype d'état réseau / 网络玩家原型 |
| `BullePlayerSpawner.cs` | Prototype de création des joueurs / 玩家生成原型 |
| `FourZonesNetworkSync.cs` | Expérimentation de synchronisation / 同步实验 |

### Remarque | 说明

**FR** — Certains scripts réseau correspondent à des essais antérieurs et ne constituent pas encore un système multijoueur complet. Avant toute suppression, il faut vérifier les références dans les scènes et les prefabs.

**中文** — 部分网络脚本属于此前的实验性实现，并不代表双人同步已经完成。删除脚本前必须检查场景和 Prefab 是否引用它们。

---

## 7. Protocole expérimental | 实验方案

### 7.1 Objectif | 实验目标

**FR**

Étudier comment les participants définissent leur espace personnel à l'aide d'une représentation immersive et observer les variations possibles entre plusieurs acquisitions.

**中文**

研究参与者如何通过沉浸式可视化定义个人空间，并观察不同测量之间的变化。

### 7.2 Déroulement | 实验流程

**FR**

Le protocole prévoit :

1. Présentation du concept de proxémie.
2. Découverte des quatre zones.
3. Ajustement individuel des frontières.
4. Enregistrement des coordonnées.
5. Interaction dans une situation expérimentale définie.
6. Nouvelle acquisition.
7. Questionnaire de retour d'expérience.

Une extension à deux participants est également envisagée.

**中文**

实验方案包括：

1. 介绍人际距离理论。
2. 认识四个个人空间区域。
3. 自行调整区域边界。
4. 保存第一次坐标。
5. 在指定实验情境中进行互动。
6. 再次保存坐标。
7. 填写体验问卷。

后续计划进一步扩展为双人互动实验。

### Documents | 实验文档

- [Protocole de test / 实验方案](docs/Protocole%20de%20test.docx)
- [Définition des bulles / 泡泡定义](docs/D%C3%A9finition%20des%20bulles.docx)

### Questionnaire | 问卷

**Formulaire destiné aux participants / 参与者问卷：**

[Accéder au questionnaire Google Forms / 打开 Google 问卷](https://docs.google.com/forms/d/e/1FAIpQLScVudi-cjfZsHRLrLHXSIc4pNQ077iVJjswrPWrQFQzQKS5XA/viewform)

**FR** — Les réponses au questionnaire pourront être rapprochées des mesures géométriques, sous réserve d'un protocole de collecte approprié.

**中文** — 后续可以在符合数据收集与隐私要求的前提下，将问卷回答与几何测量结果关联分析。

---

## 8. Résultats expérimentaux | 实验结果

### 8.1 Données disponibles | 现有数据

**FR**

La première série de données analysée date du **30 octobre 2026**.

Elle comprend :

- 8 participants
- 16 fichiers CSV
- 2 acquisitions par participant
- 20 points de contrôle par acquisition
- 4 zones composées chacune de 5 points

Les fichiers sont enregistrés dans `Data/`.

**中文**

首批分析数据采集于 **2026 年 10 月 30 日**。

包括：

- 8 名参与者
- 16 份 CSV
- 每名参与者两次测量
- 每次测量 20 个控制点
- 四个区域，每个区域 5 个控制点

原始数据保存在 `Data/`。

### 8.2 Méthode de calcul | 计算方法

**FR**

Pour chaque zone, les cinq points sont projetés sur le plan horizontal XZ.

Une approximation polygonale de la surface est calculée avec la formule du lacet :

A = 1/2 × |Σ(xᵢzᵢ₊₁ − xᵢ₊₁zᵢ)|

Le rayon équivalent est défini comme le rayon d'un cercle de même surface :

R_eq = √(A/π)

Cette grandeur permet de comparer les dimensions des zones, même lorsque leurs formes ne sont pas circulaires.

La largeur et la profondeur sont estimées à partir des coordonnées extrêmes sur les axes X et Z.

**中文**

将每个区域的五个控制点投影到水平 XZ 平面。

使用鞋带公式计算五边形近似面积：

A = 1/2 × |Σ(xᵢzᵢ₊₁ − xᵢ₊₁zᵢ)|

然后计算等效半径：

R_eq = √(A/π)

等效半径表示一个与原区域面积相等的圆的半径。

即使区域形状不规则，也可以使用该指标比较不同区域的总体尺寸。

宽度和深度分别由 X、Z 坐标的最大值与最小值之差计算。

**Limite / 局限：** Il s'agit d'une approximation à cinq sommets et non de la surface exacte des splines Unity. / 这是五边形近似，不是 Unity 样条曲线的精确面积。

### 8.3 Résultats globaux | 总体结果

Rayon équivalent moyen lors de la première acquisition (n = 8).

第一次测量的平均等效半径（n = 8）。

| Zone / 区域 | Rayon moyen / 平均半径 |
|---|---:|
| Intime / 亲密 | 0.388 m |
| Personnelle / 个人 | 0.752 m |
| Sociale / 社交 | 1.265 m |
| Publique / 公共 | 2.001 m |

**FR**

Les dimensions moyennes augmentent progressivement de la zone intime à la zone publique, conformément à l'organisation attendue des quatre catégories.

Cependant, les dimensions varient fortement d'un participant à l'autre.

**中文**

从总体平均值看，四个区域由内向外逐渐扩大，符合四种人际距离区域的预期层级。

但不同参与者的区域尺寸存在明显差异。

### 8.4 Visualisation des dimensions | 区域尺寸可视化

Le graphique suivant compare les rayons équivalents des quatre zones pour chaque participant lors de la première acquisition.

下图展示第一次测量中，每名参与者四个区域的等效半径。

![Rayons équivalents par participant / 各参与者等效半径](Reports/figures/radius_by_participant.png)

**FR**

Cette visualisation permet d'observer les différences individuelles, notamment pour les zones sociale et publique.

**中文**

该图可以直观比较不同参与者的空间尺寸，尤其是社交区和公共区的差异。

### 8.5 Évolution entre les deux acquisitions | 两次测量的变化

Le graphique suivant présente la variation relative de surface entre les deux acquisitions :

Variation (%) = (A₂ − A₁) / A₁ × 100

下图展示第二次相对于第一次测量的面积变化百分比：

变化率（%）=（第二次面积 − 第一次面积）/ 第一次面积 × 100

![Variation des surfaces / 两次测量的面积变化](Reports/figures/area_change.png)

### 8.6 Observations individuelles | 个体观察

**Arthur**

**FR** — Les quatre zones sont plus petites lors de la deuxième acquisition.

**中文** — 第二次测量中，四个区域的面积均有所减小。

**Charlotte**

**FR** — Les dimensions calculées sont identiques entre les deux acquisitions.

**中文** — 两次测量的四个区域尺寸完全相同。

**Gabriel**

**FR** — Certaines zones diminuent, tandis que la zone publique reste inchangée.

**中文** — 部分区域缩小，但公共区域保持不变。

**Helena**

**FR** — Les zones intime et personnelle restent inchangées, tandis que les zones sociale et publique diminuent.

**中文** — 亲密区和个人区保持不变，社交区和公共区缩小。

**Mona**

**FR** — Les variations sont très limitées ; la zone intime diminue légèrement.

**中文** — 两次测量总体变化很小，亲密区略微缩小。

**Ryhem**

**FR** — Les dimensions calculées sont identiques entre les deux acquisitions.

**中文** — 两次测量的区域尺寸完全相同。

**Xintian**

**FR** — Les zones ne varient pas toutes dans le même sens. Lors de la deuxième acquisition, les surfaces personnelle et sociale sont presque identiques, ce qui nécessite une vérification de la géométrie et de l'ordre des points.

**中文** — 不同区域的变化方向并不一致。第二次测量中个人区与社交区面积几乎相同，需要进一步检查区域几何形状和控制点顺序。

**Zijie**

**FR** — Les zones intime et personnelle augmentent, tandis que les zones sociale et publique restent inchangées.

**中文** — 亲密区和个人区扩大，社交区和公共区保持不变。

---

## 9. Discussion | 讨论

### 9.1 Variabilité interindividuelle | 个体差异

**FR**

Les résultats montrent une variabilité importante des dimensions choisies par les participants.

La zone publique présente notamment des dimensions très différentes selon les individus.

Cela souligne l'intérêt d'une interface permettant à chacun de personnaliser son espace.

**中文**

实验结果显示，参与者选择的个人空间尺寸具有较大差异。

其中公共区域的个体差异尤其明显。

这说明允许用户自行定义空间边界具有研究价值，而不是直接采用统一的固定尺寸。

### 9.2 Modifications entre acquisitions | 两次测量的调整

**FR**

Les modifications observées ne sont pas uniformes.

Certains participants conservent exactement les mêmes dimensions, tandis que d'autres modifient une ou plusieurs zones.

Ces différences doivent être interprétées en fonction des conditions expérimentales, qui ne sont pas directement représentées dans les fichiers CSV.

**中文**

两次测量之间的变化并不一致。

部分参与者完全没有调整，另一些参与者则修改了一个或多个区域。

由于 CSV 本身没有记录完整的实验情境，因此不能仅根据面积变化推断参与者调整空间的原因。

### 9.3 Limites méthodologiques | 方法局限

**FR**

Plusieurs limites doivent être prises en compte :

1. Le nombre de participants reste limité.
2. La correspondance entre les indices CSV et les quatre zones doit être vérifiée dans Unity.
3. Les surfaces sont calculées à partir de polygones à cinq sommets, et non des splines exactes.
4. Les anciens fichiers CSV ne contiennent pas systématiquement la position du centre du corps.
5. Les mesures géométriques ne suffisent pas à établir un effet causal sur les comportements sociaux.
6. L'absence de modification ne signifie pas nécessairement une absence de changement de perception.

**中文**

目前分析存在以下局限：

1. 参与者数量较少。
2. 需要确认 CSV 的点序号与四个区域的对应关系。
3. 面积使用五边形近似，而非 Unity 样条曲线的精确面积。
4. 旧版 CSV 缺少明确的身体中心坐标。
5. 几何数据本身不足以证明可视化改变了社交行为。
6. 没有修改泡泡形状，并不一定意味着参与者的主观感受没有变化。

---

## 10. Conclusions préliminaires | 初步结论

### Français

Cette première phase expérimentale permet de tirer trois observations principales.

**Premièrement**, les participants parviennent à définir quatre espaces de proximité distincts à l'aide de l'interface immersive.

**Deuxièmement**, les dimensions des zones présentent des différences importantes entre les participants. Cette variabilité justifie l'intérêt d'une représentation personnalisable.

**Troisièmement**, les modifications entre deux acquisitions sont hétérogènes : certaines zones restent stables, tandis que d'autres évoluent.

Ces observations ne permettent pas encore de conclure que la visualisation modifie directement les comportements interpersonnels.

Elles constituent néanmoins une base quantitative pour poursuivre les expérimentations, améliorer le protocole et relier les mesures spatiales aux réponses des participants.

### 中文

第一阶段实验可以得到三个初步观察结果。

**第一**，参与者能够使用沉浸式界面定义四个不同层级的个人空间区域。

**第二**，不同参与者的空间尺寸存在显著的描述性差异。这支持项目采用可个性化调整的空间表示方式。

**第三**，两次测量之间的变化具有个体差异：部分区域保持稳定，部分区域发生变化。

目前还不能据此证明空间可视化会直接改变人际互动行为。

但这些结果已经为后续实验提供了可量化的基础，可以进一步结合问卷、实验情境和双人互动数据开展研究。

---

## 11. Analyse des données avec Python | Python 数据分析

### Français

Les scripts Python permettent de traiter automatiquement les fichiers CSV et de produire des statistiques et des graphiques.

### 中文

Python 脚本用于批量处理 CSV 文件、计算区域尺寸并生成可视化图表。

### Installation | 安装依赖

```powershell
python -m pip install pandas matplotlib
```

### Analyse géométrique | 几何分析

```powershell
cd D:\Bulle

python Tools\analyze_bulle_csv.py --data D:\Bulle\Data --out D:\Bulle\Reports
```

### Visualisation | 生成图表

```powershell
python Tools\visualize_bulle.py
```

### Fichiers générés | 输出文件

```text
Reports/
|
|-- bulle_geometry_summary.csv
|
|-- figures/
    |-- radius_by_participant.png
    |-- area_change.png
```

**FR** — Les images présentées dans ce README apparaîtront après leur génération et leur ajout au dépôt Git.

**中文** — 生成图片并提交到 Git 仓库后，README 中的图表链接才能正常显示。

---

## 12. Prototype multijoueur | 双人同步原型

### Français

Une extension multijoueur est en cours de développement avec Photon Fusion 2.

L'objectif est de permettre à deux personnes équipées de casques Meta Quest 3 de visualiser simultanément leurs espaces personnels respectifs.

Les principales étapes sont :

1. Connecter deux ordinateurs à une même session.
2. Créer une représentation réseau pour chaque participant.
3. Synchroniser la position de chaque casque.
4. Afficher une copie distante des quatre zones.
5. Synchroniser les vingt points de contrôle et les paramètres de visibilité.
6. Calibrer les coordonnées physiques des deux dispositifs.

Les essais de connexion ne constituent pas encore une validation complète de la synchronisation des zones.

### 中文

项目正在使用 Photon Fusion 2 探索双人同步。

最终目标是让两名佩戴 Meta Quest 3 的用户，在同一个真实空间中同时看到自己的 FourZones 和对方的 FourZones。

主要开发步骤：

1. 让两台电脑进入同一个网络房间。
2. 为每名用户创建网络身份。
3. 同步各自的头显位置。
4. 显示对方的 FourZones 副本。
5. 同步 20 个控制点及显隐状态。
6. 校准两套设备的真实空间坐标。

目前尚未完成双人 FourZones 同步的端到端验证。

### Difficultés rencontrées | 已遇到的问题

- Chargement du plugin natif `nanosockets` sur un ordinateur / 一台电脑曾出现 `nanosockets` 加载错误。
- Différence entre client virtuel et véritable deuxième ordinateur / 需要区分 Unity 虚拟客户端和真实 PC2。
- Gestion de l'autorité réseau / 网络状态控制权。
- Synchronisation des formes déformables / 可变形区域同步。
- Alignement des repères spatiaux / 两套空间坐标系对齐。

---

## 13. Problèmes techniques connus | 已知技术问题

### Positionnement des zones | 区域定位

**FR** — La hauteur des zones et leur orientation doivent rester cohérentes lorsque l'utilisateur incline ou déplace la tête.

**中文** — 需要保证用户低头、歪头或移动时，区域保持正确高度和水平姿态。

### Interaction XR | XR 交互

**FR** — La direction des rayons des contrôleurs doit correspondre à leur orientation réelle.

**中文** — 手柄射线方向需要与真实手柄指向一致。

### Acquisition | 数据采集

**FR** — La sauvegarde doit être accompagnée d'un retour visuel clair et d'un contrôle de la validité des fichiers.

**中文** — 保存坐标时需要明确的视觉反馈，并验证生成的 CSV 是否完整。

### Analyse | 数据分析

**FR** — Les méthodes d'estimation géométrique doivent être vérifiées par rapport à l'ordre des points et aux splines Unity.

**中文** — 需要验证五点顺序、区域分组和 Unity 样条曲线之间的一致性。

---

## 14. Feuille de route | 后续开发计划

### Priorité 1 — Stabilisation de la version 1.0
### 优先级 1 — 稳定 1.0 版本

- Vérifier la hauteur et l'horizontalité des zones. / 检查区域高度和水平状态。
- Vérifier les interactions Grip et les rayons des contrôleurs. / 检查 Grip 与手柄射线。
- Tester le retour visuel lors de l'acquisition. / 测试保存反馈。
- Vérifier les 20 points exportés dans les CSV. / 验证 CSV 的 20 个控制点。
- Valider le lancement de l'exécutable Windows. / 验证 Windows EXE 运行。

### Priorité 2 — Exploitation expérimentale
### 优先级 2 — 实验数据分析

- Vérifier les correspondances entre zones et indices. / 验证区域和点序号对应关系。
- Comparer les acquisitions avant et après interaction. / 比较前后两次测量。
- Produire des visualisations reproductibles. / 生成可复现图表。
- Relier les mesures aux questionnaires. / 将测量结果与问卷关联。
- Améliorer les statistiques avec davantage de participants. / 扩大样本后完善统计分析。

### Priorité 3 — Extension multijoueur
### 优先级 3 — 双人功能扩展

- Connecter deux véritables ordinateurs. / 连接两台真实电脑。
- Synchroniser les positions des utilisateurs. / 同步用户位置。
- Afficher les zones de l'autre participant. / 显示对方区域。
- Synchroniser les déformations. / 同步形状变化。
- Calibrer le même espace physique. / 统一真实空间坐标系。
- Tester l'interaction à deux. / 开展双人互动测试。

### Priorité 4 — Améliorations futures
### 优先级 4 — 后续优化

- Améliorer l'ergonomie et la lisibilité. / 优化界面与可读性。
- Mesurer les distances réelles entre participants. / 测量真实人际距离。
- Enregistrer les données relatives au centre du corps. / 保存相对于身体中心的坐标。
- Étudier la détection de l'environnement et les ancres spatiales. / 探索环境识别和空间锚点。
- Améliorer la reproductibilité du protocole. / 提高实验可重复性。

---

## 15. Gestion des versions | 版本管理

### Version 1.0

**Date : 2 novembre 2026**

**FR**

La version 1.0 correspond à la première version du projet disposant d'un prototype XR local, d'une acquisition de coordonnées, d'une chaîne d'analyse des données et d'un exécutable Windows.

La synchronisation multijoueur constitue une extension en développement.

**中文**

v1.0 代表项目已经形成首个较完整的本地 XR 原型，具备个人空间可视化、交互、坐标采集、数据分析流程，并已生成 Windows 可执行版本。

双人同步属于后续扩展功能，不作为当前 v1.0 已完成的功能。

### Dépôt GitHub | GitHub 仓库

https://github.com/Jenniewww/Bulle

### Mise à jour du dépôt | 更新仓库

```powershell
cd D:\Bulle

git add README.md Tools/ Reports/

git commit -m "docs: update bilingual README and experimental results"

git push origin main
```

---

## 16. Éthique et données | 伦理与数据

### Français

Les fichiers expérimentaux peuvent contenir des noms de participants et des données liées à leurs choix spatiaux.

Avant toute publication des données brutes, il convient de vérifier le consentement des participants et de privilégier l'anonymisation.

Les résultats présentés dans ce dépôt sont exploratoires et ne doivent pas être interprétés comme des conclusions générales sur les comportements humains.

### 中文

实验文件可能包含参与者姓名，以及反映个人空间偏好的测量数据。

公开原始 CSV 前，应确认参与者的知情同意，并尽可能使用匿名编号。

本项目的实验结果属于探索性研究，不应直接推广为适用于所有人的普遍结论。

---

## 17. État actuel du projet | 项目当前状态

| Fonction / 功能 | État / 状态 |
|---|---|
| Prototype Unity XR / Unity XR 原型 | Réalisé / 已实现 |
| Visualisation FourZones / 四区域显示 | Réalisé / 已实现 |
| Interaction par contrôleurs / 手柄交互 | Prototype fonctionnel, à stabiliser / 原型可用，待稳定 |
| Acquisition CSV / CSV 采集 | Réalisée / 已实现 |
| Analyse géométrique Python / Python 几何分析 | Réalisée / 已实现 |
| Visualisations statistiques / 统计图表 | Script proposé, à exécuter / 已提供脚本，待运行 |
| Retour visuel de sauvegarde / 保存反馈 | À valider dans le casque / 待头显验证 |
| Build Windows / Windows 构建 | Généré / 已生成 |
| Synchronisation de deux casques / 双头显同步 | En développement / 开发中 |
| Calibration spatiale commune / 共享空间校准 | À développer / 待开发 |

---

**La forme de ma bulle — Version 1.0**

*Rendre visible l'espace personnel pour mieux comprendre les distances humaines.*

*让个人空间变得可见，从而更好地理解人与人之间的距离。*
