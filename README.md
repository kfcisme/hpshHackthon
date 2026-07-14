# Glitch Compiler／異變編譯器

一款 Unity 2D 程式解謎遊戲。玩家在 IDE 輸入 V-Code，將指令繪製到畫布；畫布與關卡目標圖的重合度達門檻後通關。異變系統會在限時內干擾程式碼、畫布或控制規則。

## 開發環境

- Unity 6（`6000.5.3f1`）
- Unity Test Framework
- TextMeshPro 與 UGUI

## 快速開始

1. 以 Unity Hub 開啟此資料夾。
2. 建立 `Bootstrap`、`MainMenu`、`Level` 三個場景，並依序加入 Build Settings。
3. 在 `Level` 場景建立並綁定 `LevelSessionController`、`IDEEditorController`、`CanvasRenderer`、`LevelLoader`、`GameLoopController`、`LevelTimer`、`LevelCompletionEvaluator` 與 `AnomalyManager`。
4. 將 `CanvasRenderer` 的 `RawImage`、IDE 的 `TMP_InputField` 與 `LevelSessionController` 參考指向對應元件。
5. 建立 `LevelDefinition` 與 512×512 目標圖，然後呼叫 `LevelSessionController.BeginLevel(index)`。

## V-Code 範例

```text
COLOR("blue");
WIDTH(4);
LOOP(4) {
  MOVE(100);
  TURN(90);
}
CIRCLE(30);
```

## 文件索引

- [協作規範](AGENT.md)
- [架構與整合契約](docs/ARCHITECTURE.md)
- [V-Code 語言規格](docs/V-CODE.md)
- [測試案例與測試 ID](docs/TEST_CASES.md)
- [協作與提交規範](docs/CONTRIBUTING.md)
- [關卡製作指南](docs/LEVEL_AUTHORING.md)
