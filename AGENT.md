# AGENT.md — 《異變編譯器 / Glitch Compiler》協作指南

## 1. 專案核心：先畫圖，再處理變異

《異變編譯器》是一款 Unity 2D 的**視覺化程式繪圖解謎遊戲**。玩家的主要目標不是撰寫一般用途程式，也不是單純找錯；而是以 V-Code 控制海龜畫筆，在畫布上繪製出每一關指定的目標圖形。

每一關的基本流程如下：

```text
讀取關卡目標與 Starter Code
→ 玩家修改／撰寫 V-Code
→ Compile：解析、執行、畫到畫布
→ 比較畫布與 TargetImage 的重合度
→ 達到 PassPercentage 即通關
```

關卡有倒數時間。玩家繪圖期間，系統會依關卡規則與 Safe / Flow / Crisis 階段**隨機觸發變異**。變異是第二層壓力：它暫時干擾程式碼、畫面或操作，玩家需要先排除它，才能穩定地繼續完成圖形；它不能取代「用 V-Code 畫出目標圖」這個主要勝利條件。

## 2. 玩法與通關規則

- 關卡資料由 `LevelDefinition` 提供目標圖 `TargetImage`、初始程式 `StarterCode`、時限、通關門檻與可觸發的變異規則。
- 畫布固定為 64×64，V-Code 的邏輯座標以畫布幾何中心 `(0, 0)` 為原點；初始方向向右、顏色白色、筆寬 2。
- V-Code 輸出的 `DrawCommand` 交由 `TurtleRasterizer` 光柵化為畫布，再以 `PixelMatchEvaluator` 計算與目標圖的重合百分比。
- 重合度以目標圖的非透明像素為基準；目標外額外畫上的像素會扣分。目標圖必須是可讀取的 64×64、且 `PassPercentage` 大於 0，達到門檻才通關。
- 時間歸零即失敗；暫停、勝利與失敗狀態不可再推進關卡或觸發變異。
- 成功通關後才寫入玩家最佳成績與解鎖進度。

## 3. V-Code：服務於繪圖

V-Code 的設計目的，是讓玩家用可學習、可推理的程式結構產生圖形。產生關卡、教學、測試或範例時，只能使用已支援的語法，並確保可以畫出目標圖。

| 類別 | 指令／結構 | 用途 |
| --- | --- | --- |
| 移動 | `MOVE(distance)`、`TURN(angle)` | 移動畫筆並畫線、改變方向。 |
| 筆刷 | `COLOR(value)`、`WIDTH(pixels)` | 改變線條顏色與寬度。 |
| 形狀 | `CIRCLE(radius)`、`RECT(width, height)` | 在目前位置畫圓或矩形。 |
| 程式結構 | `LET`、`FUNC`、`IF / ELSE`、`LOOP` | 將繪圖邏輯抽象、重複與條件化。 |
| 變異應對 | `SHIELD(boolean)`、`SYSTEM.RESET()` | 傳送系統命令給變異系統，不直接改變勝負。 |

完整語言行為以 `Docs/V-CODE.md` 與 `VCodeParser` / `VCodeInterpreter` 的實作為準。

## 4. 變異設計原則

變異只能在關卡進行中造成**可理解、可解除、可逆**的阻礙；解除後玩家應能回到原本的繪圖任務。它們的目標是讓玩家在倒數中調整與除錯，不應永久毀損關卡資料、目標圖或玩家存檔。

目前的變異類型：

- `GhostComment`：在程式碼加入干擾註解；玩家刪除後解除。
- `SyntaxShift`：把 `TURN` 改為 `BURN`；玩家以搜尋取代或手動修正後解除。
- `CanvasMask`：以畫面覆蓋干擾視野；玩家送出 `SHIELD(true)` 後解除。
- `ControlInversion`：以控制干擾與計時倍率施壓；玩家送出 `SYSTEM.RESET()` 後解除。

每個新變異必須實作 `IAnomaly` 的 `OnTrigger`、`CheckResolved`、`OnResolve`、`OnCleanup`。變異的觸發階段、機率、冷卻、是否只觸發一次、解除獎勵秒數與倒數倍率，都必須由 `AnomalyRule` 配置，而非硬編碼在關卡流程中。

## 5. 架構責任與整合邊界

```text
IDEEditorController
  → VCodeParser / VCodeInterpreter
  → DrawCommand + SystemCommand
  → CanvasRenderer / TurtleRasterizer
  → Texture2D
  → LevelSessionController
  → 重合度判定、關卡狀態、異變與存檔
  → EventBus
  → UI 顯示
```

- `VCode`：解析與執行玩家程式，輸出繪圖與系統命令；不得決定通關、失敗或存檔。
- `Rendering`：只把 `DrawCommand` 畫成 Texture；不得修改計時器或遊戲狀態。
- `LevelSessionController`：關卡生命週期的唯一協調者，負責接收一次 Compile 的結果、重合度、勝負與通關紀錄。
- `AnomalyManager`：依關卡規則觸發、追蹤與清理變異；不直接寫入玩家存檔。
- `UI`：收集輸入、顯示程式碼／畫布／計時／結果與變異提示；僅訂閱 `EventBus`，不持有遊戲規則。

V-Code 與渲染在每次 Compile 後，應只透過 `LevelSessionController.SubmitCompilation(CompilationSubmission)` 將結果提交給關卡流程。詳細契約見 `Docs/LevelSession-Integration.md`。

## 6. 協作規範

- 改動公開整合介面、V-Code 指令或測試案例時，同步更新 `Docs/ARCHITECTURE.md`、`Docs/V-CODE.md` 與 `Docs/TEST_CASES.md`。
- 新增關卡時，以透明背景的目標圖為基準，確保圖形能由現有 V-Code 指令合理產生，並先驗證無變異的繪圖閉環，再加入變異。
- 遊戲邏輯與視覺呈現需分離；變異效果優先使用可清理的 Overlay、輸入或渲染效果。
- Scene、Prefab、ScriptableObject 與 `.meta` 僅能透過 Unity Editor 修改；變更時需註記資產負責範圍。
- 新功能或修正必須補上相應測試，至少涵蓋：V-Code 執行、繪圖結果、通關判定，或變異觸發／解除的其中一項。

## 7. 當前優先目標

先完成並驗證一個最小可玩關卡：玩家輸入 V-Code 畫出單一目標圖形、畫布更新並判定重合度；在倒數期間至少出現一種可解除變異；通關或時間到後正確呈現結果並處理進度。後續所有功能都應強化這條核心體驗。
