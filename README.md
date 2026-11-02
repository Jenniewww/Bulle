
# La forme de ma bulle | 我的个人空间泡泡

**Projet de réalité mixte sur la proxémie**  
**关于人际距离与个人空间的混合现实项目**

**Version 1.0 — Novembre 2026**

---

## 1. Présentation du projet | 项目简介

### Français

**La forme de ma bulle** est un projet de réalité mixte consacré à la **proxémie**, c'est-à-dire à la manière dont les individus perçoivent et organisent leur espace personnel.

L'objectif est de rendre visible un espace habituellement invisible : les distances que chaque personne souhaite maintenir autour d'elle.

Grâce au casque **Meta Quest 3**, l'utilisateur peut visualiser quatre zones autour de son corps et modifier leurs dimensions à l'aide des contrôleurs VR.

Le projet cherche à explorer plusieurs questions :

- Comment représenter visuellement l'espace personnel ?
- Les dimensions de cet espace varient-elles selon les individus ?
- Comment ces dimensions évoluent-elles lorsqu'une autre personne s'approche ?
- La familiarité avec une personne influence-t-elle la perception des distances interpersonnelles ?
- La réalité mixte peut-elle aider à mieux comprendre la notion d'espace personnel ?

Le projet associe développement informatique, interaction immersive et expérimentation auprès d'utilisateurs.

### 中文

**La forme de ma bulle（我的个人空间泡泡）** 是一个研究**人际距离学（Proxémie）**的混合现实项目。

项目希望将通常不可见的个人空间转化为可以观察、调整和测量的虚拟区域。

用户佩戴 **Meta Quest 3** 后，可以在真实环境中看到围绕自身的四个距离区域，并通过 VR 手柄改变它们的大小和形状。

项目主要探索：

- 如何直观地可视化个人空间？
- 不同人的个人空间尺寸是否存在差异？
- 当其他人靠近时，个人空间是否会发生变化？
- 熟悉程度是否影响人们对人际距离的感知？
- 混合现实是否能够帮助人们理解个人空间？

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

Ces catégories constituent un cadre théorique. Elles ne correspondent pas nécessairement à des dimensions identiques pour tous les individus.

Notre application permet donc aux participants de construire leur propre représentation de ces quatre espaces.

### 中文

项目主要参考 **Edward T. Hall** 的人际距离理论。

该理论将人际距离划分为四个主要区域：

1. **亲密距离（Intime）**
2. **个人距离（Personnelle）**
3. **社交距离（Sociale）**
4. **公共距离（Publique）**

这些分类提供了理论框架，但并不意味着所有人的个人空间都具有完全相同的尺寸。

因此，本项目允许参与者自行调整四个区域，而不是采用固定半径。

### Références | 参考文献

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
| XR Interaction Toolkit / Input System | Interactions avec les contrôleurs / 手柄交互 |
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
    |     +-- Main Camera
    |     +-- Left Controller
    |     +-- Right Controller
    |
    +-- FourZones
    |     +-- Intime
    |     +-- Personnelle
    |     +-- Sociale
    |     +-- Publique
    |
    +-- Acquisition
    |     +-- CoordinateController
    |     +-- AcquisitionFeedbackUI
    |
    +-- Photon Fusion
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

**中文** — 以上展示项目的主要模块。双人网络同步仍属于后续开发内容。

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

应用围绕用户显示四个个人空间区域：

- 亲密区
- 个人区
- 社交区
- 公共区

每个区域由五个控制点定义。

用户可以通过移动控制点改变区域的大小和形状。

### 4.2 Interaction avec les contrôleurs | 手柄交互

**FR**

Le prototype permet trois interactions principales :

1. **Boutons A/B/X/Y** : contrôler l'affichage des zones.
2. **Grip / Grab** : sélectionner et déplacer les points de contrôle.
3. **Sauvegarde CSV** : enregistrer les coordonnées des 20 points.

Les contrôleurs permettent ainsi de personnaliser les quatre zones directement dans l'environnement immersif.

**中文**

当前原型支持三种主要操作：

1. **A/B/X/Y 按键**：控制区域显示。
2. **Grip / Grab**：选中并拖动控制点，改变区域形状。
3. **保存 CSV**：记录全部 20 个控制点的坐标。

参与者可以直接在混合现实环境中完成区域调整。

### 4.3 Suivi de l'utilisateur | 用户位置跟随

**FR**

Les zones sont positionnées relativement à l'utilisateur et suivent ses déplacements.

Le système vise à conserver une représentation cohérente de l'espace personnel lorsque l'utilisateur se déplace.

**中文**

FourZones 可以根据用户的位置移动，使个人空间可视化围绕佩戴者。

系统需要保证用户移动时，四个区域保持正确的位置和方向。

### 4.4 Acquisition et sauvegarde des coordonnées | 坐标采集与保存

**FR**

Le système d'acquisition des coordonnées est désormais **fonctionnel et stable dans la version 1.0**.

L'utilisateur peut enregistrer les positions des 20 points de contrôle définissant les quatre zones proxémiques.

Chaque zone comporte cinq points de contrôle. Les coordonnées sont enregistrées dans un fichier CSV, ce qui permet une analyse quantitative ultérieure.

Le système permet :

- L'enregistrement des coordonnées 3D des 20 points.
- La génération automatique de fichiers CSV.
- La conservation des données de chaque acquisition.
- L'analyse des dimensions des quatre zones à l'aide de Python.

**Les problèmes techniques liés à l'acquisition et à la sauvegarde des coordonnées ont été résolus.**

**中文**

v1.0 版本中的坐标采集和保存功能现已**稳定运行**。

用户可以保存定义四个人际距离区域的全部 20 个控制点的三维坐标，每个区域包含五个控制点。

系统已经实现：

- 记录 20 个控制点的三维坐标。
- 自动生成 CSV 文件。
- 保留每次实验的采集结果。
- 使用 Python 计算四个区域的几何尺寸。

**此前与坐标采集和保存有关的技术问题已经解决。**

### 4.5 Retour visuel de sauvegarde | CSV 保存反馈界面

**FR**

La version 1.0 dispose d'un retour visuel intégré dans le casque Meta Quest 3.

Lorsqu'un participant déclenche l'enregistrement des coordonnées, une interface confirme la sauvegarde du fichier CSV.

Cette fonctionnalité permet de réaliser les acquisitions directement dans l'environnement immersif, sans devoir retirer le casque pour vérifier chaque enregistrement sur l'ordinateur.

Le système d'acquisition et son interface de confirmation sont opérationnels dans la version stable.

**中文**

v1.0 已经实现 Quest 3 头显内的 CSV 保存反馈界面。

参与者触发坐标保存后，系统会在头显中显示保存结果，确认 CSV 已成功生成。

因此，参与者不需要每次摘下头显检查电脑，就能确认数据是否保存。

坐标采集与 UI 反馈功能目前均已在稳定版本中实现。

### 4.6 Version exécutable Windows | Windows 可执行版本

**FR**

Une version Windows stable destinée à la démonstration est disponible localement :

`D:\Bulle\Builds\Client\Bulle.exe`

Elle permet de présenter les fonctionnalités principales du prototype.

Le fonctionnement nécessite un environnement PCVR correctement configuré.

**中文**

项目已生成可用于演示的 Windows 稳定版本：

`D:\Bulle\Builds\Client\Bulle.exe`

该版本可以用于向老师展示项目的主要功能。

运行时需要正确配置 OpenXR、SteamVR、ALVR 和 Quest 3。

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
|   |-- summary_all.csv
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

**FR** — Les dossiers `Data`, `Reports` et `Builds` peuvent contenir des fichiers locaux ou générés qui ne sont pas nécessairement publiés sur GitHub.

**中文** — `Data`、`Reports` 和 `Builds` 可能包含本地数据或自动生成文件，不一定全部上传到 GitHub。

---

## 6. Scripts principaux | 主要脚本

| Script | Fonction / 功能 |
|---|---|
| `FiveSpheresController.cs` | Gestion des points de contrôle / 控制点管理 |
| `BulleFollower.cs` | Suivi de la caméra / 头显跟随 |
| `ControllerRayVisual.cs` | Visualisation des rayons / 手柄射线 |
| `CoordinateManager.cs` | Acquisition et export CSV / 坐标采集与导出 |
| `ControllerAcquisition.cs` | Déclenchement de l'acquisition / 触发保存 |
| `AcquisitionFeedbackUI.cs` | Retour visuel de sauvegarde / 保存反馈 |
| `NetworkBullePlayer.cs` | Prototype d'état réseau / 网络玩家原型 |
| `BullePlayerSpawner.cs` | Prototype de création des joueurs / 玩家生成原型 |
| `FourZonesNetworkSync.cs` | Expérimentation de synchronisation / 同步实验 |

**FR** — Certains scripts réseau correspondent à des expérimentations et ne constituent pas encore un système multijoueur complet.

**中文** — 部分网络脚本属于实验性实现，双人同步尚未完成。

---

## 7. Protocole expérimental et questionnaire | 实验流程与问卷

### 7.1 Objectif de l'expérience | 实验目标

**FR**

L'expérience vise à étudier la perception de l'espace personnel à travers une représentation immersive de quatre zones proxémiques.

Les participants peuvent définir eux-mêmes les dimensions de leurs zones, puis observer si celles-ci évoluent lorsqu'une autre personne s'approche.

Les données géométriques sont enregistrées afin de comparer les configurations avant et après l'interaction.

**中文**

实验旨在通过四个人际距离区域的混合现实可视化，研究参与者对个人空间的感知。

参与者可以自行定义四个区域的大小，并观察当其他人靠近时，是否希望改变这些区域。

实验通过保存前后两次坐标，对比互动前后的个人空间配置。

### 7.2 Déroulement de l'expérience | 实验具体流程

#### Étape 1 — Présentation du projet | 第一步：项目介绍

**FR**

L'expérimentateur présente le projet *La forme de ma bulle*, explique la notion de proxémie et introduit les quatre zones :

- Intime
- Personnelle
- Sociale
- Publique

Le participant découvre ensuite leur représentation dans le casque Meta Quest 3.

**中文**

实验人员首先介绍 *La forme de ma bulle* 项目，解释人际距离学的基本概念，并介绍四个区域：

- 亲密区
- 个人区
- 社交区
- 公共区

随后，参与者通过 Meta Quest 3 观察四个区域的混合现实可视化。

#### Étape 2 — Apprentissage des interactions | 第二步：学习三种操作

**FR**

Avant de commencer, le participant apprend à utiliser les trois principales interactions :

1. **Boutons A/B/X/Y** : contrôler l'affichage des zones.
2. **Grip / Grab** : sélectionner et déplacer les points de contrôle afin de modifier les formes.
3. **Sauvegarde CSV** : enregistrer les coordonnées des 20 points, avec une confirmation visuelle dans le casque.

Une courte phase de familiarisation permet au participant de tester les commandes.

**中文**

正式实验前，参与者需要掌握三种主要操作：

1. **A/B/X/Y 按键**：控制区域的显示。
2. **Grip / Grab**：抓取并拖动控制点，修改泡泡的形状。
3. **保存 CSV**：记录全部 20 个控制点，并通过头显中的 UI 确认保存成功。

参与者可以先进行短暂练习，熟悉手柄操作。

#### Étape 3 — Calibration individuelle | 第三步：独自调整四个区域

**FR**

Le participant se trouve seul dans l'espace expérimental.

Il ajuste librement les dimensions des quatre zones afin qu'elles correspondent à sa perception personnelle des distances intime, personnelle, sociale et publique.

Une fois satisfait, il valide sa configuration.

**Première acquisition :** les coordonnées des 20 points sont enregistrées dans un fichier CSV.

**中文**

参与者首先独自在实验空间中调整四个区域。

根据自己对亲密距离、个人距离、社交距离和公共距离的理解，自由修改各区域的大小和形状。

调整满意后，确认当前配置。

**第一次保存：** 将 20 个控制点的坐标记录到 CSV 文件中。

#### Étape 4 — Approche d'autres personnes | 第四步：他人正面靠近

**FR**

Une autre personne s'approche progressivement du participant par l'avant.

Deux situations sont proposées :

- **Une personne familière** : quelqu'un que le participant connaît.
- **Une personne inconnue** : quelqu'un que le participant ne connaît pas.

Dans chaque situation, le participant observe les quatre zones et indique s'il souhaite modifier leurs dimensions.

Il peut ajuster les zones à l'aide des contrôleurs.

L'objectif est d'observer si la présence et la familiarité d'une autre personne influencent la représentation de l'espace personnel.

**中文**

另一名人员从参与者正面逐渐靠近。

实验设置两种情境：

- **熟人靠近**：参与者认识的人。
- **陌生人靠近**：参与者不认识的人。

在每种情境中，参与者观察自己的四个区域，并判断是否希望调整区域大小。

如果有需要，可以使用手柄重新调整。

这一阶段旨在观察他人的出现以及熟悉程度，是否与个人空间的调整有关。

#### Étape 5 — Deuxième acquisition | 第五步：第二次保存

**FR**

Après les interactions, le participant confirme sa configuration finale.

**Deuxième acquisition :** les coordonnées des 20 points sont enregistrées dans un nouveau fichier CSV.

Les deux acquisitions peuvent ensuite être comparées afin de mesurer :

- Les variations de surface.
- Les variations de rayon équivalent.
- Les modifications de largeur et de profondeur.
- Les zones qui restent inchangées.

**中文**

完成互动后，参与者确认最终的四区域配置。

**第二次保存：** 将调整后的 20 个控制点记录到新的 CSV 文件中。

随后使用 Python 对比两次测量，分析：

- 各区域的面积变化。
- 等效半径变化。
- 宽度和深度变化。
- 哪些区域保持不变。

### 7.3 Questionnaire de retour d'expérience | 实验反馈问卷

**FR**

À la fin de l'expérience, le participant remplit un questionnaire portant notamment sur :

- La compréhension des quatre zones.
- La facilité d'utilisation des contrôleurs.
- La lisibilité de la représentation visuelle.
- La perception de l'approche d'une personne familière ou inconnue.
- L'utilité de la visualisation pour comprendre son espace personnel.
- Le confort général de l'expérience en réalité mixte.

**中文**

实验结束后，参与者填写体验问卷，主要涉及：

- 对四个人际距离区域的理解。
- 手柄操作是否容易。
- 泡泡可视化是否清晰。
- 熟人和陌生人靠近时的感受。
- 可视化是否有助于理解个人空间。
- 混合现实体验的整体舒适度。

**Questionnaire / 实验问卷：**

[Accéder au questionnaire / 填写问卷](https://docs.google.com/forms/d/e/1FAIpQLScVudi-cjfZsHRLrLHXSIc4pNQ077iVJjswrPWrQFQzQKS5XA/viewform)

### 7.4 Données collectées | 数据采集内容

**FR**

Chaque participant produit deux fichiers CSV contenant chacun les coordonnées des 20 points de contrôle.

Les données sont ensuite analysées avec Python pour comparer les dimensions initiales et finales des quatre zones.

**中文**

每名参与者生成两份 CSV 文件，每份记录 20 个控制点的坐标。

之后使用 Python 比较四个区域在实验前后的尺寸变化。

**Remarque méthodologique / 方法说明：**

Le protocole actuel prévoit deux acquisitions : une avant et une après les situations d'approche. Il permet donc d'analyser une variation globale, mais pas d'isoler quantitativement l'effet de la personne familière de celui de la personne inconnue.

当前流程只保存两次：他人靠近前和两种靠近情境结束后。因此，可以分析总体变化，但无法仅凭这两份 CSV 分别量化熟人与陌生人的影响。

---

## 8. Résultats expérimentaux | 实验结果

### 8.1 Données disponibles | 现有数据

**FR**

La première série de données analysée date du **30 octobre 2026**.

Elle comprend :

- 8 participants.
- 16 fichiers CSV.
- 2 acquisitions par participant.
- 20 points de contrôle par acquisition.
- 4 zones composées chacune de 5 points.

**中文**

首批分析数据采集于 **2026 年 10 月 30 日**。

包括：

- 8 名参与者。
- 16 份 CSV。
- 每名参与者两次测量。
- 每次测量 20 个控制点。
- 四个区域，每个区域 5 个控制点。

### 8.2 Méthode de calcul | 计算方法

**FR**

Pour chaque zone, les cinq points sont projetés sur le plan horizontal XZ.

Une approximation polygonale de la surface est calculée avec la formule du lacet :

**A = 1/2 × |Σ(xᵢzᵢ₊₁ − xᵢ₊₁zᵢ)|**

Le rayon équivalent est défini comme le rayon d'un cercle de même surface :

**R_eq = √(A/π)**

Cette grandeur permet de comparer les dimensions des zones, même lorsque leurs formes ne sont pas circulaires.

La largeur et la profondeur sont estimées à partir des coordonnées extrêmes sur les axes X et Z.

**中文**

将每个区域的五个控制点投影到水平 XZ 平面。

使用鞋带公式计算五边形近似面积：

**A = 1/2 × |Σ(xᵢzᵢ₊₁ − xᵢ₊₁zᵢ)|**

然后计算等效半径：

**R_eq = √(A/π)**

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

**Variation (%) = (A₂ − A₁) / A₁ × 100**

下图展示第二次相对于第一次测量的面积变化百分比：

**变化率（%）=（第二次面积 − 第一次面积）/ 第一次面积 × 100**

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

**第二**，不同参与者的空间尺寸存在明显的个体差异。这支持项目采用可个性化调整的空间表示方式。

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
|-- summary_all.csv
|-- bulle_geometry_summary.csv
|
|-- figures/
    |-- radius_by_participant.png
    |-- area_change.png
```

**FR** — Les images présentées dans ce README apparaissent après leur génération et leur ajout au dépôt Git.

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

---

## 13. Problèmes techniques | 技术问题

### 13.1 Problèmes résolus | 已解决的问题

**FR**

Les difficultés techniques liées à l'enregistrement des coordonnées et à l'exportation des fichiers CSV ont été résolues.

La version stable permet désormais de réaliser les acquisitions expérimentales avec un retour visuel directement dans le casque.

L'analyse automatique des données CSV a également été validée sur 16 fichiers expérimentaux.

**中文**

此前坐标采集、CSV 导出及保存反馈相关的技术问题已经解决。

目前稳定版本可以在头显内完成数据保存，并显示反馈信息。

Python 已成功分析首批 16 份实验 CSV 数据。

### 13.2 Points à améliorer | 待完善问题

**FR**

Les améliorations futures concernent principalement :

- La synchronisation des zones entre deux casques.
- La calibration d'un espace physique partagé.
- L'amélioration des visualisations statistiques.
- L'analyse conjointe des questionnaires et des mesures géométriques.

**中文**

后续重点包括：

- 双头显之间的 FourZones 同步。
- 共享真实空间坐标校准。
- 完善实验结果可视化。
- 结合问卷和几何数据进行分析。

---

## 14. Feuille de route | 后续开发计划

### Version 1.0 — Prototype stable | 稳定原型

**FR**

La version 1.0 constitue la première version stable destinée à la démonstration et aux acquisitions expérimentales.

Elle comprend :

- La visualisation des quatre zones.
- Les interactions avec les contrôleurs.
- La déformation des zones.
- La sauvegarde des coordonnées.
- Le retour visuel de confirmation.
- L'exportation CSV.
- L'analyse géométrique des données.
- Un exécutable Windows.

**中文**

v1.0 是项目首个可用于演示和实验采集的稳定版本。

包含：

- 四个区域可视化。
- 手柄交互。
- 区域变形。
- 坐标保存。
- 保存反馈 UI。
- CSV 导出。
- 几何数据分析。
- Windows EXE。

### Version suivante — Améliorations | 后续版本优化

**FR**

Les prochaines étapes comprennent :

- L'amélioration des visualisations des résultats.
- La comparaison des données géométriques et des questionnaires.
- La synchronisation de deux utilisateurs.
- La calibration spatiale commune.
- L'enregistrement de mesures complémentaires.
- L'amélioration du protocole expérimental.

**中文**

下一阶段计划：

- 完善实验结果可视化。
- 将几何数据与问卷进行关联。
- 实现双人同步。
- 完成共享空间校准。
- 增加其他测量指标。
- 完善实验流程。

---

## 15. Gestion des versions | 版本管理

### Version 1.0

**Novembre 2026**

**FR**

La version 1.0 correspond à la première version stable du projet disposant d'un prototype XR local, d'une acquisition de coordonnées, d'une interface de confirmation de sauvegarde, d'une chaîne d'analyse des données et d'un exécutable Windows.

La synchronisation multijoueur constitue une extension en développement.

**中文**

v1.0 代表项目已经形成首个稳定的本地 XR 原型，具备个人空间可视化、交互、坐标采集、保存反馈 UI、数据分析流程，并已生成 Windows 可执行版本。

双人同步属于后续扩展功能，不作为当前 v1.0 已完成的功能。

### Dépôt GitHub | GitHub 仓库

https://github.com/Jenniewww/Bulle

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

**Version 1.0 — Prototype stable pour la démonstration et l'acquisition expérimentale**

**版本 1.0 — 可用于展示与实验数据采集的稳定原型**

| Fonction / 功能 | État / 状态 |
|---|---|
| Visualisation des quatre zones / 四区域可视化 | Réalisée / 已实现 |
| Interaction A/B/X/Y / 按键控制 | Réalisée / 已实现 |
| Déformation avec Grip / Grip 拖动变形 | Réalisée / 已实现 |
| Acquisition des 20 coordonnées / 20 点坐标采集 | Fonctionnelle et stable / 稳定运行 |
| Export CSV / CSV 导出 | Fonctionnel / 正常运行 |
| Confirmation visuelle dans le casque / 头显保存反馈 | Fonctionnelle / 已实现 |
| Analyse Python / Python 数据分析 | Fonctionnelle / 已实现 |
| Analyse de 16 fichiers CSV / 16 份 CSV 分析 | Terminée / 已完成 |
| Visualisation statistique / 统计可视化 | En cours de finalisation / 正在完善 |
| Build Windows / Windows EXE | Disponible / 已提供 |
| Synchronisation entre deux casques / 双头显同步 | Développement futur / 后续开发 |

---

**La forme de ma bulle — Version 1.0**

*Rendre visible l'espace personnel pour mieux comprendre les distances humaines.*

*让个人空间变得可见，从而更好地理解人与人之间的距离。*
