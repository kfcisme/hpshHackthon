# 關卡流程單一接口規範

## 適用範圍

V-Code 解析、執行與畫布渲染完成後，僅能透過 `LevelSessionController.SubmitCompilation(...)` 將結果交給遊戲流程。

不得直接呼叫 `GameLoopController`、`LevelTimer`、`AnomalyManager`、`LevelCompletionEvaluator`，也不得自行發布 `MatchChanged` 或 `LevelFinished`。

## 呼叫時機

每次玩家按下 Compile，V-Code 組完成「解析 -> 執行 -> 渲染」後呼叫一次：

```csharp
var session = /* Level 場景中的 LevelSessionController */;

if (!parsed.Success || !executed.Success)
{
    session.SubmitCompilation(CompilationSubmission.Failed());
    return;
}

renderer.Render(executed.DrawCommands);
session.SubmitCompilation(
    CompilationSubmission.Successful(renderer.CanvasTexture, executed.SystemCommands));
```

## 輸入契約

| 狀況 | 傳入內容 | 核心流程行為 |
|---|---|---|
| 解析或執行失敗 | `CompilationSubmission.Failed()` | 不更新分數、不判定通關、不改變異變的系統指令狀態。錯誤顯示由既有 `CompilationFinished` 事件處理。 |
| 成功但沒有繪圖指令 | `Successful(null, commands)` | 接收 `SHIELD`／`SYSTEM.RESET` 等系統指令；不更新分數。 |
| 成功且畫布已渲染 | `Successful(renderer.CanvasTexture, commands)` | 接收系統指令、計算重合度、發布 `MatchChanged`，達標時進入勝利。 |

## 回傳值

`SubmitCompilation` 回傳 `false` 代表目前不在 `Playing` 狀態或提交資料為空，呼叫端不必重試。回傳 `true` 代表核心流程已接受這次編譯結果；這不等於程式碼本身成功。

## 選關與解鎖規範

選關或重新開始時，只能呼叫 `LevelSessionController.StartLevel(index)`；不能直接呼叫 `LevelLoader.Load(index)`。

```csharp
if (!session.StartLevel(selectedIndex))
{
    // 顯示訊息時讀取 session.LastStartFailure。
    // InvalidIndex：關卡資料不存在；Locked：尚未解鎖；MissingDependencies：場景接線不完整。
}
```

`StartLevel` 會先檢查索引、再檢查玩家解鎖進度，只有兩者都通過才替換目前關卡。載入失敗時會保留既有關卡，並發布 `LevelStartRejected` 事件，供 UI 顯示原因。

## 暫停與重新開始規範

只能透過 `LevelSessionController` 呼叫 `PauseLevel()`、`ResumeLevel()`、`RestartCurrentLevel()`。暫停時不會倒數、更新關卡階段、觸發異變或接受通關判定；勝利或失敗後不能繼續或再次暫停。

## 異變設定規範

每個 `LevelDefinition` 的 `Anomalies` 清單可在 Inspector 設定：

| 欄位 | 用途 |
|---|---|
| `Enabled` | 關閉後該條規則完全不觸發。 |
| `Type` | 選擇 GhostComment、SyntaxShift、CanvasMask 或 ControlInversion。 |
| `EarliestPhase` | 異變最早可出現的階段。 |
| `TriggerChance`、`CooldownSeconds`、`TriggerOnce` | 控制觸發機率、兩次觸發間隔與是否只出現一次。 |
| `ResolveBonusSeconds` | 異變被正確解決後，加回計時器的秒數。 |
| `TimerMultiplier` | 僅 ControlInversion 使用；異變期間的倒數倍率。 |

異變類別只處理干擾與解決判定。`AnomalyManager` 讀取目前規則，統一處理獎勵秒數；因此同一種異變能在不同關卡採用不同難度設定。

## 責任邊界

- **V-Code／Rendering 組：** 產生 `CompilationSubmission`，並只呼叫這一個方法。
- **Level 組：** 驗證狀態、更新異變指令、計算重合度與決定勝敗。
- **UI 組：** 訂閱事件並呈現，不能從 UI 直接改分數、計時器或關卡狀態。
