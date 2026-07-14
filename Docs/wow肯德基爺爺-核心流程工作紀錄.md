# 核心流程工作紀錄

**署名：wow肯德基爺爺**  
**角色：遊戲流程與系統整合工程師**  
**更新日期：2026-07-14**

## 角色與責任

我負責將 渲染、異變、關卡、計時、通關與玩家紀錄整合成可控的遊戲流程，(並非實作 V-Code 語法本身或製作 UI／美術資產)。

負責範圍如下：

- `Assets/Scripts/Core`
- `Assets/Scripts/Level`
- `Assets/Scripts/Anomalies`
- `Assets/Scripts/Data`
- `Assets/Scripts/Progression`

協作原則是：UI 與 V-Code 模組透過公開接口和事件接入，不能直接修改關卡狀態、計時器、異變管理器或通關結果。

## 已完成工作

### 1. 建立關卡流程協調層

新增 `LevelSessionController`，作為關卡生命週期的唯一入口，負責：

- 載入、開始與重開關卡。
- 更新 Safe、Flow、Crisis 階段。
- 接收編譯與渲染的結果。
- 計算目標圖重合度並判定通關。
- 暫停／繼續遊戲。
- 結束時清理異變效果。
- 成功通關時寫入玩家最佳成績與解鎖進度。

### 2. 定義 V-Code／渲染的單一接線接口

新增 `CompilationSubmission`，規定每次 Compile 後，V-Code／渲染模組只能呼叫：

```csharp
session.SubmitCompilation(
    CompilationSubmission.Successful(renderer.CanvasTexture, executed.SystemCommands));
```

若解析或執行失敗，則提交：

```csharp
session.SubmitCompilation(CompilationSubmission.Failed());
```

核心流程會自行處理異變系統指令、重合度、通關與存檔。這能避免 UI、解析器與關卡系統彼此直接相依。

### 3. 強化關卡載入與解鎖安全性

- `LevelCatalog` 新增安全的 `TryGet`。
- `LevelLoader` 會保留目前關卡，避免無效索引覆寫正在遊玩的資料。
- `LevelSessionController.StartLevel(index)` 會檢查場景接線、索引合法性與玩家解鎖狀態。
- 失敗時以 `LastStartFailure` 與 `LevelStartRejected` 事件提供 UI 顯示原因。

### 4. 完整化關卡狀態

合法流程如下：

```text
Preparing → Playing → Paused → Playing
                    ↘ Won / Lost
```

- 只有 `Playing` 狀態會倒數、進入新階段、觸發異變與接受通關提交。
- `Paused`、`Won`、`Lost` 狀態不會再推進遊戲。
- 重新開始會重設分數、計時器與異變執行狀態。

### 5. 將異變難度資料化

`AnomalyRule` 已可在每個 `LevelDefinition` 的 Inspector 中設定：

- `Enabled`
- `Type`
- `EarliestPhase`
- `TriggerChance`
- `CooldownSeconds`
- `TriggerOnce`
- `ResolveBonusSeconds`
- `TimerMultiplier`（目前供 `ControlInversion` 使用）

異變類別目前只處理干擾與解決判定；獎勵秒數與倒數倍率從關卡資料讀取，不再硬編碼於各異變類別。

### 6. 文件與協作規範

- 更新 `AGENT.md`：記錄關卡流程唯一入口、事件責任、存檔時機與異變設定責任。
- 新增 `Docs/LevelSession-Integration.md`：規定並記錄編譯提交、選關、暫停、重開與異變設定接口。

## 尚未完成／需要其他組員接線的工作

1. V-Code／渲染組需在 `IDEEditorController` 實際呼叫 `SubmitCompilation(...)`。
2. UI 組需建立真實的 `AnomalyContext`，將程式碼讀寫、異變覆蓋面板與計時器回呼傳入 `ConfigureAnomalyContext(...)`。
3. 專案尚未建立 `Bootstrap`、`MainMenu`、`Level` 場景、Prefab、關卡 ScriptableObject、目標圖與音效資產。
4. 新增程式需由 Unity Editor 匯入並生成 `.meta` 檔案。
5. 需要在 Unity Editor 進行完整編譯與 Play Mode 測試。

## 已知設計決策

目前像素重合度只評估目標圖的非透明像素是否被畫中，額外繪製的像素不會扣分。正式設計關卡前，團隊需要決定是否要讓額外線條扣分，以避免玩家以大量繪圖方式取得高分。

## 下一步動作

優先建立一個最小 `Level` 場景，以第一關、單一目標圖、臨時 UI 與一種異變驗證完整迴圈：

```text
輸入程式 → Compile → 畫布更新 → 重合度 → 通關／失敗 → 存檔
```

在此流程能於 Play Mode 穩定運作前，不建議再新增大型玩法。
