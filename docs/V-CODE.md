# V-Code 語言規格

## 基本規則

- 指令必須以分號 `;` 結束。
- 關鍵字不分大小寫；函數與變數名稱不分大小寫。
- 畫布固定為 64×64；V-Code 使用 Cartesian 座標，初始筆位置為畫布幾何中心 `(0, 0)`、初始方向為向右（0 度）、顏色為白色、筆寬為 2。64 為偶數，因此 `(0, 0)` 位於中央四個像素之間。
- 正角度 `TURN(angle)` 為逆時針旋轉。

## 繪圖指令

| 指令 | 參數 | 行為與限制 |
| --- | --- | --- |
| `MOVE(distance)` | 有限數值 | 向目前方向畫線並移動筆位置；負值代表反向移動。 |
| `TURN(angle)` | 有限數值 | 改變筆方向。 |
| `COLOR(value)` | `#RRGGBB`、`#RRGGBBAA` 或顏色名稱 | 設定筆色。支援 `black`、`white`、`red`、`green`、`blue`、`yellow`、`cyan`、`magenta`、`gray`、`grey`、`clear`。 |
| `WIDTH(pixels)` | 大於 0 的數值 | 設定筆寬。 |
| `CIRCLE(radius)` | 大於 0 的數值 | 以目前筆位置為圓心畫圓，不移動筆位置。 |
| `RECT(width, height)` | 兩個非 0 數值 | 以目前筆位置為第一個頂點畫矩形，不移動筆位置。 |

畫布外的圖形會被安全裁切；筆位置仍依原始 `MOVE` 距離前進，之後可再移回畫布內。

## 程式結構

```text
LET side = 80;

FUNC square(size) {
  LOOP(4) {
    MOVE(size);
    TURN(90);
  }
}

IF(side > 0) {
  square(side);
} ELSE {
  CIRCLE(20);
}
```

支援數字、字串、布林值、變數、四則運算、比較、`IF / ELSE`、`LOOP` 與函數呼叫。`LOOP` 次數必須是非負整數。

## 系統指令

| 指令 | 行為 |
| --- | --- |
| `SHIELD(true)` | 送出防護罩系統命令。 |
| `SHIELD(false)` | 關閉防護罩。 |
| `SYSTEM.RESET()` | 送出重設系統命令。 |

系統指令由 `LevelSessionController` 交給 `AnomalyManager` 處理，不直接修改關卡狀態。
