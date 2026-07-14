# 協作與提交規範

## 開始工作前

1. 閱讀 `AGENT.md`、[架構文件](ARCHITECTURE.md) 與所屬模組文件。
2. 將工作限制在一個清楚的模組或功能範圍。
3. 修改 Scene、Prefab、ScriptableObject 前，先在團隊中宣告負責人；這些 Unity 序列化資產不應同時由多人編輯。

## 程式與文件

- C# 與 Markdown 檔使用 UTF-8。
- 新功能或修正應加入／更新對應測試，並在 [測試案例](TEST_CASES.md) 登記測試 ID。
- 不可直接讓 UI、V-Code 或 Rendering 修改計時器、勝負與存檔；請使用 `LevelSessionController`。
- 新公開介面須同步更新 `ARCHITECTURE.md`。

## Commit 與 PR

- Commit 使用動詞開頭，並在適用時附測試 ID，例如：`Fix VCODE-VALID-001 command validation`。
- PR 說明需包含：變更目的、影響模組、測試 ID、Unity Test Runner 結果、場景／Prefab 變更。
- 合併前執行 EditMode 測試；涉及場景或 UI 時，另附 PlayMode 截圖或錄影。

## Unity 資產

- 請從 Unity Editor 建立或修改 `.unity`、`.prefab`、`.asset` 與 `.meta`。
- 不提交 `Library/`、`Temp/`、`Obj/`、Build 產物或個人 IDE 設定。
