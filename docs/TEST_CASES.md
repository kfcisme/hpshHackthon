# 測試案例清單

本文件使用一致的測試 ID，讓程式、Unity Test Runner 結果、Issue 與 PR 可以互相引用。

## 測試 ID 格式

`<模組>-<層級>-<三位序號>`

| 欄位 | 說明 | 範例 |
| --- | --- | --- |
| 模組 | 功能所屬模組的英文大寫縮寫 | `VCODE`、`RENDER` |
| 層級 | 測試目的或所在層級 | `LEX`、`PARSE`、`EXEC`、`VALID`、`INT` |
| 三位序號 | 同一模組與層級下的流水號 | `001` |

範例：`VCODE-EXEC-001` 表示第一個 V-Code 執行層測試。

## 已自動化測試

| 測試 ID | 對應測試 | 模組 | 測試輸入／前置條件 | 預期結果 | 狀態 |
| --- | --- | --- | --- | --- | --- |
| `VCODE-EXEC-001` | `ParsesAndExecutesRecursiveFunction` | V-Code | 宣告並遞迴呼叫 `line(n)` | 成功產生一個 `MOVE` 繪圖命令 | 已自動化 |
| `VCODE-EXEC-002` | `TurnsPlayerInstructionsIntoDrawCommands` | V-Code | 依序輸入 `COLOR`、`WIDTH`、`MOVE`、`TURN`、`CIRCLE`、`RECT` | 成功依輸入順序產生六個正確的 `DrawCommand` | 已自動化 |
| `VCODE-EXEC-003` | `AllowsVariablesAndLoopsInDrawingInstructions` | V-Code | 使用 `LET` 與 `LOOP(4)` 繪圖 | 成功產生顏色與八個移動／轉向命令 | 已自動化 |
| `VCODE-VALID-001` | `RejectsInvalidCommandArguments` | V-Code | 指令缺參數、錯誤型別、零尺寸、負半徑或非布林防護罩 | 回傳診斷訊息，不產生未處理例外 | 已自動化 |
| `VCODE-LEX-001` | `RejectsUnterminatedString` | V-Code | 輸入未結束的 `COLOR` 字串 | Lexer 回傳診斷訊息 | 已自動化 |

## 待補測試

| 測試 ID | 對應測試 | 模組 | 測試情境 | 預期結果 | 狀態／優先度 |
| --- | --- | --- | --- | --- | --- |
| `VCODE-PARSE-001` | `RejectsMalformedSyntax` | V-Code | 缺少分號、區塊結尾與不支援字元 | 回傳語法診斷且不會卡住解析器 | 已自動化 |
| `VCODE-EXEC-004` | `RejectsRuntimeFailures` | V-Code | 函數參數數量不符、未定義變數、除以零 | 執行失敗並回傳可讀診斷 | 已自動化 |
| `VCODE-EXEC-005` | `RejectsAnEmptyLoopThatExceedsTheInstructionLimit` | V-Code | 超過指令數上限的空迴圈 | 安全中止，且不影響 Unity 主執行緒 | 已自動化 |
| `VCODE-SYS-001` | `EmitsSystemCommands` | V-Code | `SHIELD(true)` 與 `SYSTEM.RESET()` | 正確輸出對應的 `SystemCommand` | 已自動化 |
| `RENDER-UNIT-001` | `CentersTheTurtleForAnyCanvasResolution` | Rendering | 以非 64 解析度建立畫布 | 初始筆位置為邏輯座標 `(0, 0)`，即畫布幾何中心 | 已自動化 |
| `RENDER-UNIT-002` | `RasterizesShapesAndClipsLinesOutsideTheCanvas` | Rendering | `MOVE`、`TURN`、`WIDTH`、圓形與邊界外繪圖 | 畫布產生正確像素且不越界 | 已自動化 |
| `RENDER-UNIT-003` | `PenalizesPixelsDrawnOutsideTheTarget` | Rendering | 在目標像素外另畫一個像素 | 額外像素會降低重合度 | 已自動化 |
| `LEVEL-UNIT-001` | `DoesNotCompleteAConfigurationWithoutATarget` | Level | 缺少目標圖的關卡設定 | 即使收到 100% 也不可通關 | 已自動化 |
| `RENDER-INT-001` | — | V-Code / Rendering | V-Code 範例繪製正方形 | `IDEEditorController` 成功將命令送至 `CanvasRenderer` | 待補／高 |
| `LEVEL-INT-001` | — | Level | 成功渲染後提交畫布 | 正確更新重合度，並依門檻判定通關 | 待補／高 |
| `ANOMALY-INT-001` | — | Anomaly | 提交 `SHIELD`、`RESET` 系統命令 | 正確解除對應異變 | 待補／中 |

## 執行方式

在 Unity 6（`6000.5.3f1`）開啟專案後，執行 **Window → General → Test Runner → EditMode → Run All**。測試失敗時，請在 Issue 或 PR 標題、說明與提交訊息中附上測試 ID，例如：`Fix VCODE-VALID-001 command validation`。
