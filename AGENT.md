# AGENT.md - 《異變編譯器：Glitch_Compiler》開發協作指南

## 協作接口規範（2026-07-14）

* **關卡流程唯一入口：** `LevelSessionController` 是關卡生命週期的唯一協調者。UI、V-Code 與渲染模組不得直接控制 `GameLoopController`、`LevelTimer` 或 `AnomalyManager`。
* **模組接線方式：** V-Code／渲染在成功執行後，分別呼叫 `SubmitSystemCommands(...)` 與 `SubmitRenderedCanvas(...)`；UI 完成後以 `ConfigureAnomalyContext(...)` 注入讀寫程式碼、異變提示與計時器等回呼。
* **事件責任：** 關卡開始、計時更新、階段改變、重合度改變、異變與勝敗結果一律由 `EventBus` 發布，UI 僅訂閱與呈現，不持有或修改遊戲規則。
* **資料寫入時機：** 僅 `LevelSessionController` 可在成功通關時呼叫玩家紀錄寫入；失敗、重新編譯與異變解決不得直接寫入存檔。

## 1. 專案概述 (Project Overview)
* **遊戲名稱：** 《異變編譯器》(Glitch Compiler)
* **遊戲類型：** 2D 編程解謎 (Coding Puzzle) + 異變尋錯 (Anomaly Detection)
* **核心玩法：** 玩家必須在左側的虛擬 IDE（整合開發環境）中編寫「極簡視覺化程式碼 (V-Code)」，在右側 2D 畫布中繪製出與目標圖形重合度達 **95%** 以上的圖案以通關。
* **主要挑戰：** 遊戲進行中會隨機觸發「異變 (Anomalies)」，暫時干擾程式碼編輯器、畫布或操作規則。玩家必須在有限時間內切換到 Debug/除錯模式解決異變，兼顧「邏輯思考」與「即時應變」。

---

## 2. AI 協作角色設定 (AI Persona & Role)
當 AI（ChatGPT）讀取此專案時，必須扮演 **資深遊戲架構師與系統開發工程師 (Senior Game Architect & System Developer)**。
* **回答原則：** 提供的程式碼或設計必須模組化、高內聚低耦合，並且隨時考慮「異變機制」如何注入現有系統。
* **風格語調：** 專業、清晰，聚焦於易理解的程式解謎體驗與變異機制的可讀性。

---

## 3. 核心系統架構規格 (Core System Architecture)

專案主要由以下四大核心模組組成，AI 在設計與擴充功能時必須嚴格遵守模組邊界：

| 模組名稱 | 負責職責 | 輸入/輸出規格 |
| :--- | :--- | :--- |
| **1. V-Code 解析器**<br>`VCodeParser` | 讀取左側編輯器的文字，實時編譯並輸出為幾何繪圖指令字節碼 (Bytecode)。 | **輸入：** String (Raw Code)<br>**輸出：** `List<DrawCommand>` |
| **2. 畫布渲染引擎**<br>`CanvasRenderer` | 接收繪圖指令並在右側 2D 畫布上繪製圖形；計算與目標底圖的重合百分比。 | **輸入：** `List<DrawCommand>`, Target Bitmap<br>**輸出：** Rendered Canvas, `float matchPercentage` |
| **3. 異變管理系統**<br>`AnomalyManager` | 監控計時器與玩家狀態，負責異變事件的生成、注入（暫時調整 UI、畫布或輸入規則）、驗證解除條件與倒數懲罰。 | **輸入：** Game State, Timer<br>**輸出：** `AnomalyEvent`, UI Change Effects |
| **4. 遊戲流程控制器**<br>`GameLoopController` | 控制關卡三階段（安全期 -> 流暢期 -> 危機期），管理遊戲總計時器 (Timer) 與勝負判定。 | **輸入：** Player Actions, System Events<br>**輸出：** Level State (Win/Lose/Pause) |

---

## 4. 遊戲機制實作規範 (Mechanics Guidelines)

### A. 極簡指令集 (V-Code Spec)
AI 在生成關卡目標或測試用例時，僅能使用以下支援的語法結構：
* **基礎移動：** `MOVE(distance)`, `TURN(angle)`
* **外觀控制：** `COLOR(hex_or_name)`, `WIDTH(pixels)`
* **形狀函數：** `CIRCLE(radius)`, `RECT(width, height)`
* **邏輯結構：** `LOOP(count) { ... }`, `IF(condition) { ... } ELSE { ... }`
* **系統指令：** `SHIELD(boolean)`, `SYSTEM.RESET()`

### B. 異變事件表 (Anomaly Registry)
新增異變功能時，必須實作 `IAnomaly` 介面，並包含以下三個核心生命週期：
1. `OnTrigger()`: 定義異變如何表現（如：插入干擾文字、調整畫布顯示、反轉控制）。
2. `CheckResolved()`: 監聽玩家行為，回傳 `boolean` 判斷異變是否已被解決。
3. `OnResolve()`: 消除視覺干擾，停止計時器倒數懲罰，並給予玩家時間獎勵（預設增加 **10 秒**）。
<!-- 
#### 已註冊之異變標準規範：
* **干擾註解 (Ghost Comments)：** 自動在編輯器插入額外文字，需透過 `Ctrl+Y` 或 `Delete` 移除。
* **語法重載 (Syntax Shift)：** 關鍵字變異（例：`TURN` 變 `BURN`），需用全域替代 `Ctrl+F` 修正。
* **畫布遮罩 (Visual Deadlock)：** 畫布出現暫時遮罩，需在代碼最首行新增 `SHIELD(true);` 解除。
* **鎖定反轉 (Control Inversion)：** 鍵盤輸入反轉且計時加速，需在代碼輸入 `SYSTEM.RESET();` 或點擊畫面防護罩。 -->

---

## 5. 代碼開發規範 (Coding Standards)

* **狀態分離：** 遊戲邏輯（Game Logic）與視覺渲染（Visual Rendering）必須徹底分離。異變效果應優先以 UI Overlay 或可逆的渲染效果呈現，避免永久破壞核心數據（除非是編輯器文字干擾類異變）。
* **事件驅動：** 異變的觸發與解除應採用「事件發布/訂閱模式 (Event Bus / Observer Pattern)」，避免 `GameLoopController` 與具體 UI 產生深度依賴。
* **可配置性：** 所有關卡的設定（目標圖形參數、限制時間、會觸發的異變類型與機率）必須以 JSON 或 ScriptableObject / 資料表的形式儲存，避免硬編碼 (Hardcoding)。
### 資料存儲與玩家狀態規範 (Data Persistence & Player State)
* **動態與靜態資料分離 (Runtime vs. Persistent Data)：** 遊戲運行時的短暫狀態（例如：當前關卡的剩餘時間、當前畫布重合度、一次性防禦罩是否觸發）嚴禁寫入持久化資料夾。只有在「關卡結束 (Level Completed)」或「玩家在商店完成交易」時，才觸發 `PlayerProfileManager.Save()`。
* **序列化格式 (Serialization)：** 玩家存檔結構應設計為可序列化的資料類別（如 `PlayerData` DTO），本地開發調試時優先使用 **JSON** 格式保存，方便開發者檢查與修改進度；正式發布版 (Release Build) 再透過介面切換為二進制加密儲存 (AES/Binary Formatter) 以防作弊。
* **解耦外掛與戰鬥系統：** `PlayerProfileManager` 只負責提供「玩家目前裝備了哪些防毒外掛」的資料結構（Data List），具體外掛如何防禦異變，由 `AnomalyManager` 在關卡初始化時向 `PlayerProfileManager` 讀取並註冊對應效果，避免存儲系統過度干涉關卡運作。
---

## 6. 當前開發里程碑與代辦事項 (Roadmap & Tasks)

- [ ] **Phase 1: 核心原型 (MVP)**
    - [ ] 實作左側文字編輯器 UI 與即時文本監聽。
    - [ ] 完成 `VCodeParser` 基礎指令解析（`MOVE`, `TURN`, `COLOR`, `LOOP`）。
    - [ ] 實作 `CanvasRenderer` 繪圖邏輯與像素對比算法（重合度判定）。
- [ ] **Phase 2: 異變機制接入**
    - [ ] 建立 `AnomalyManager` 與倒數計時系統。
    - [ ] 實作第一個異變：【惡意註解】與其消除判定。
    - [ ] 實作第二個異變：【語法重載】與全域搜尋替代機制。
- [ ] **Phase 3: 關卡與回饋效果**
    - [ ] 設計清楚的畫布變異提示、解除回饋與音效提示系統。
    - [ ] 製作前 3 個教學與進階關卡 JSON 資料。
- [ ] **Phase 4: 玩家系統與局外養成 (Player & Progression)**
    - [ ] 設計 `PlayerData` 資料結構（關卡評分紀錄、貨幣數量、擁有物品 ID 列表）。
    - [ ] 實作 `PlayerProfileManager` 的本地 Save/Load JSON 讀寫功能。
    - [ ] 實作「防毒商店 / IDE 升級介面 (Shop UI)」，並串接扣款與物品解鎖邏輯。
    - [ ] 關卡初始化時，支援讀取玩家已裝備的外掛（如：自動注入 `Firewall.vbx` 抵擋第一次編輯器侵蝕）。
