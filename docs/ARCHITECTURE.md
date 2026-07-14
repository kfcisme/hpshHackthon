# 架構與整合契約

## 關卡資料流

```text
IDEEditorController
  → VCodeParser / VCodeInterpreter
  → DrawCommand + SystemCommand
  → CanvasRenderer / TurtleRasterizer
  → Texture2D
  → LevelSessionController
  → LevelCompletionEvaluator + AnomalyManager + GameLoopController
  → EventBus → UI
```

`LevelSessionController` 是關卡規則的唯一協調者。UI、V-Code 與 Rendering 不得直接改寫計時器、勝負或玩家存檔。

## 公開整合介面

| 方法 | 呼叫端 | 行為 |
| --- | --- | --- |
| `BeginLevel(int)` | 關卡選擇 UI | 載入關卡並啟動計時與流程狀態 |
| `ConfigureAnomalyContext(AnomalyContext)` | Level UI 組裝 | 注入異變讀寫程式碼、提示與計時器回呼 |
| `SubmitSystemCommands(IEnumerable<SystemCommand>)` | IDE／V-Code | 將 `SHIELD`、`SYSTEM.RESET` 送交異變系統 |
| `SubmitRenderedCanvas(Texture2D)` | IDE／Rendering | 計算重合度、發布事件並判定通關 |

## 模組責任

| 模組 | 可以做 | 不可以做 |
| --- | --- | --- |
| V-Code | 解析、驗證、輸出 `DrawCommand`／`SystemCommand` | 控制關卡勝負、直接存檔 |
| Rendering | 將 `DrawCommand` 光柵化為 Texture | 比對目標圖、修改計時器 |
| Level | 管理階段、重合度、勝負與關卡資料 | 解析玩家程式碼 |
| Anomaly | 注入與解除異變 | 直接寫玩家存檔 |
| UI | 收集輸入、訂閱 EventBus、顯示資料 | 持有遊戲規則 |

## EventBus 事件

`CompilationFinished`、`MatchChanged`、`AnomalyTriggered`、`AnomalyResolved`、`LevelFinished` 皆由核心流程發布，UI 僅訂閱與呈現。
