### 階段目標
- [x] 按下書籤後新增一個按鈕，原本是未啟用狀態，直到成功取得資料後變為啟用狀態
- [x] 點擊按鈕將`{followers, followings}`複製到剪貼簿
- [x] 關閉UI的按鈕

<br>

#### 發散
- [x] 檢查網域，非instagram.com則throw error
- [x] 如果加入按鈕後操作到一半失敗，按鈕不就卡在畫面上
- [ ] 新增按鈕改成彈出對話框，可以按叉叉關閉並終止操作?
- [x] 第一階段完成先push到github，再接著改。

<br>

#### 工具
- https://chateverywhere.app/zh
- https://skalman.github.io/UglifyJS-online/

#### 踩雷
* 雖然在console`copy(await(async () => {})())`可以複製到非同步函數的返回值，但書籤的執行環境未定義copy()。
* Edge瀏覽instagram.com時剪貼簿的訪問權限是有的，不過`navigator.clipboard.writeText("")` 須掛在Button.onclick方能執行
<br>

* `const btn = document.createElement('button')` &&  `document.body.appendChild(btn)` 然後才修改的btn屬性能直接反映到網頁，包含 `innerText`, `onclick`, `style`, `disabled` 等等。
    * `btn.style.position = 'fixed'` 無視原有網頁元素直接蓋上去挺好用。
    * 若 `innerText=""`&&`style.padding=0` 則按鈕直接看不到。
    * `btn.style.padding='16px'` 16px以字串賦值。