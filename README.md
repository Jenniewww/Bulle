# La forme de ma bulle | 我的个人空间泡泡

> **Projet de réalité mixte sur la proxémie** · **关于人际距离与个人空间的混合现实项目**

## 1. Présentation du projet | 项目简介

**FR** — *La forme de ma bulle* explore la **proxémie** : la manière dont chacun perçoit et organise son espace personnel autour de soi. L'objectif est de visualiser, dans un casque de réalité mixte, des zones de proximité et de permettre à l'utilisateur de les ajuster de façon interactive. À terme, le projet vise à comparer les perceptions de deux personnes présentes dans un même espace et à les compléter par un questionnaire.

**中文** — 本项目以**人际距离学（Proxémie）**为主题，探索人们如何感知和划分自身周围的个人空间。通过混合现实头显，在真实环境中显示不同距离区域，并允许用户交互式调整。后续计划支持同一空间中的两名用户共同体验，并结合问卷收集主观感受。

## 2. Environnement et approche technique | 技术环境与实现路径

| Élément / 技术 | Utilisation / 用途 |
| --- | --- |
| **Unity 6.3 LTS, C#** | Développement de la scène 3D et des interactions / 三维场景与交互开发 |
| **OpenXR** | Interface XR pour le suivi du casque et des contrôleurs / 头显及手柄追踪 |
| **Meta Quest 3** | Casque de test / 测试设备 |
| **SteamVR + ALVR (PCVR)** | Connexion du casque au PC et essais de passthrough / 连接电脑及透视模式测试 |
| **XR Interaction / Input System** | Entrées des contrôleurs, sélection et déplacement / 手柄输入、选择与拖动 |
| **Photon Fusion 2 (Shared Mode)** | Prototype de synchronisation multijoueur / 多人同步原型，尚未完成 |
| **Git / GitHub** | Gestion des versions et collaboration / 版本管理与协作 |

**FR — Parcours de réalisation :** mise en place de l'environnement XR → affichage des objets dans la vue réelle (*passthrough*) → création des zones et des points de contrôle → interactions avec les manettes → expérimentation d'une synchronisation entre deux casques.

**中文 — 实现流程：** 配置 XR 环境 → 在真实场景透视画面中叠加虚拟对象 → 构建个人空间区域及控制点 → 实现手柄交互 → 探索双头显同步。

## 3. Fonctionnalités et scripts | 已实现功能与主要脚本

**FR** — Le prototype local permet d'afficher des objets/zones autour de l'utilisateur, de les faire suivre sa position et de manipuler des points de contrôle à l'aide des rayons des manettes et de la touche **Grip**. Des essais de changement d'échelle et de visibilité ont également été réalisés. Le réglage de la hauteur des zones reste à stabiliser.

**中文** — 本地原型已能在用户周围显示虚拟区域，使其跟随用户移动，并通过左右手柄射线与 **Grip** 选择、拖动控制点。还实现或测试了区域缩放、显隐控制。目前区域高度定位仍需修正。

| Script / 脚本 | Rôle / 作用 |
| --- | --- |
| `FiveSpheresController.cs` | Positionnement de cinq points de contrôle, suivi de la tête, sélection par rayon et déplacement avec Grip / 五个控制点布局、头显跟随、射线选择及 Grip 拖动 |
| `BulleFollower.cs` | Suivi de la caméra XR par l'objet « bulle » / 泡泡对象跟随 XR 相机 |
| `FourZonesNetworkSync.cs` | Essai de création (*spawn*) d'un objet réseau avec Photon Fusion / 使用 Photon Fusion 生成网络对象的实验脚本 |

**FR** — D'autres composants de scène gèrent les zones, leur affichage et les entrées XR. La liste ci-dessus présente les scripts identifiés à ce stade, sans prétendre être un inventaire exhaustif du dépôt.

**中文** — 场景中还有用于区域显示、交互和 XR 输入的组件。以上是目前已确认的主要脚本，并非仓库全部代码清单。

## 4. Pistes testées et difficultés | 已尝试方案与遇到的问题

- **Passthrough / 透视显示：** essais avec OpenXR, couches de composition et ALVR ; des problèmes de superposition ont parfois rendu le décor noir ou masqué les objets virtuels. / 曾遇到黑屏、虚拟物体被遮挡或消失等图层问题。
- **Multijoueur / 双人同步：** essais avec Photon Fusion 2 en mode Shared ; une machine a pu rejoindre une session, mais l'autre a rencontré une erreur de chargement du plugin natif `nanosockets`. La synchronisation entre deux casques n'est donc **pas encore validée**. / 一台电脑成功连接会话，另一台遇到 `nanosockets` 加载错误，尚未完成双机验证。
- **Positionnement vertical / 高度定位：** plusieurs réglages ont été testés pour placer les zones au niveau du ventre ; le comportement reste instable et nécessite une vérification de la hiérarchie des objets et des références XR. / 腹部高度定位仍不稳定，需要检查对象层级及 XR 引用。
- **Compréhension de l'environnement / 环境识别：** détection des murs et du sol, ancres spatiales et suivi d'autres personnes ont été envisagés, mais ne sont pas des fonctionnalités finalisées. / 墙地识别、空间锚点和他人追踪仍属于探索方向。

## 5. Prochaines étapes | 后续计划

1. **Stabiliser le prototype local / 稳定本地原型：** hauteur des zones, interactions et affichage passthrough.
2. **Tester à deux / 双人测试：** résoudre les problèmes réseau, synchroniser les positions et calibrer les repères physiques des deux casques.
3. **Améliorer la représentation / 完善空间表达：** rendre les zones plus lisibles et ajustables selon les retours d'utilisation.
4. **Ajouter le questionnaire / 加入问卷：** définir les questions, le protocole de passation et l'exploitation des réponses.

## 6. Protocole expérimental et questionnaire | 实验方案与问卷

**FR** — Deux documents de travail accompagnent le prototype :

- [Protocole de test](docs/Protocole%20de%20test.docx) : déroulement de l'expérience, consignes de calibration et de comparaison à deux, questionnaire en deux parties.
- [Définition des bulles](docs/D%C3%A9finition%20des%20bulles.docx) : définitions des quatre distances proxémiques et revue exploratoire d'études liées à la proxémie et à la réalité virtuelle.

**中文** — 项目附有两份工作文档：

- [实验方案](docs/Protocole%20de%20test.docx)：包含个人空间校准、双人对比实验、操作说明和两阶段问卷。
- [泡泡定义与文献](docs/D%C3%A9finition%20des%20bulles.docx)：解释四种人际距离，并整理人际距离与虚拟现实相关研究。

### Déroulement envisagé | 计划实验流程

1. **Calibration individuelle / 个人校准：** présentation des quatre zones, ajustement initial dans le casque, puis ajustement en présence d'une personne proche, d'une connaissance et d'une personne inconnue. / 介绍四个区域，用户先自行调整，再与亲近的人、熟人和陌生人分别测试并调整。
2. **Interaction à deux / 双人交互：** comparaison des distances et comportements sans bulles visibles, puis avec visualisation des bulles. / 对比不显示泡泡和显示泡泡时的距离及互动行为。
3. **Retour utilisateur / 用户反馈：** questionnaire après la calibration, puis après l'interaction à deux. / 校准后及双人互动后分别填写问卷。

### Axes du questionnaire (brouillon) | 问卷方向（草案）

- **Confort et design / 舒适度与设计：** hauteur des halos, transparence, couleurs, lisibilité, facilité de modification. / 高度、透明度、颜色、可见性与操作难度。
- **Compréhension / 理解程度：** compréhension des zones intime, personnelle, sociale et publique. / 对亲密、个人、社交、公共四种空间的理解。
- **Effet sur l'interaction / 对互动的影响：** rapprochement ou éloignement, franchissement des zones, gêne éventuelle. / 距离变化、跨越边界及不适感。
- **Expérience XR / XR 体验：** sentiment de présence et inconfort éventuel lié au casque. / 沉浸感及可能的晕动不适。

**FR — État :** le protocole et les questions existent sous forme de documents de travail ; ils ne sont **pas encore intégrés à l'application**. Aucun résultat de participants n'est rapporté. Le protocole prévoit notamment d'enregistrer les positions des points de contrôle avant et après interaction, de figer la forme validée et de mesurer les distances entre utilisateurs ; ces fonctions restent à développer ou à valider. Les modalités de recueil, de consentement et d'analyse devront être précisées avant les tests.

**中文 — 当前状态：** 已有实验流程和问卷草案，但**尚未集成到应用程序中**，也没有参与者测试结果。方案提出记录互动前后的控制点坐标、锁定确认后的泡泡形状、测量双人距离等开发需求，目前仍需实现或验证。正式测试前还需要明确数据收集、参与者同意和分析方式。

---

**État du projet / 项目状态 :** prototype XR local partiellement fonctionnel ; positionnement spatial, synchronisation à deux et protocole utilisateur encore en cours de développement. / 本地 XR 原型部分功能可用，空间定位、双人同步和用户实验仍在开发中。
