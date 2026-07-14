# 關卡製作指南

## LevelDefinition

每個關卡建立一個 `LevelDefinition` ScriptableObject，至少設定：

| 欄位 | 用途 |
| --- | --- |
| `Id` | 穩定且唯一的關卡 ID；存檔與分析使用，建立後不要任意更名。 |
| `Title` | UI 顯示名稱。 |
| `Tutorial` | 關卡說明與 V-Code 提示。 |
| `TargetImage` | 與畫布解析度一致的 64×64 透明背景目標 PNG（必須開啟 Read/Write Enabled）。 |
| `StarterCode` | 玩家初始看到的 V-Code。 |
| `TimeLimitSeconds` | 倒數秒數。 |
| `PassPercentage` | 通關重合度門檻。 |
| `Anomalies` | 異變類型、最早階段、機率與冷卻時間。 |

## 目標圖規則

- 使用透明背景，並在 Import Settings 開啟 **Read/Write Enabled**；目標圖必須恰為 64×64。
- 目標圖的非透明像素必須被畫到正確顏色；目標外額外畫上的像素會降低重合度，不能以塗滿畫布取得通關。
- 目標線條應使用 V-Code 可產生的顏色、筆寬與幾何圖形。
- 先以無異變關卡驗證 V-Code 範例，再加入一種異變並測試解除流程。

### 由 Starter Code 產生目標

選取 `LevelDefinition` 後，可在 Inspector 按 **從 Starter Code 產生 64×64 目標圖**。它會先解析並執行 Starter Code；只有成功時才會在 `Assets/Levels/Generated` 建立一張可讀取的 PNG，並自動指派到 `TargetImage`。這適合用來確保參考程式與目標圖完全一致。

## 關卡驗收

1. `StartLevel(index)` 能正確載入資料與啟動計時。
2. Starter Code 能編譯，或 Tutorial 明確說明要修正的錯誤。
3. 畫布輸出與目標圖的重合度符合預期。
4. 異變可觸發、可解除，且通關後才寫入玩家紀錄。
5. `TargetImage` 已指定、關卡門檻大於 0，否則不可作為可發佈關卡。
