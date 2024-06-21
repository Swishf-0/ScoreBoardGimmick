# ScoreBoardGimmick


<p>

スコアをランキング形式で表示するギミックです。

マルチでの同期も行います。

<img height=380 src=res/images/10.png> <img height=380 src=res/images/9.png>

</p>


# 使い方

## サンプルシーン

<p>

<p>

`Scenes`にサンプルシーンが入っています。

<img width=350 src=res/images/3.png>

</p>

<p>

シーンを開いたときに「TMP Importer」が表示された場合は「Import TMP Essentials」を押します。

<img width=450 src=res/images/12.png>

</p>


</p>

<hr>

<p>

シーン内にはスコアボード (`ScoreBoard`) とスコア操作パネル (`ScoreBoardSample`) が設置してあります。

<img width=350 src=res/images/7.png>

<img width=350 src=res/images/8.png>

<p>

- 「+1」、「-1」ボタン: 1刻みでスコアを変動可能です
- 「+0.1」、「-0.1」ボタン: 0.1刻みでスコアを変動可能です、確認にはスコアボードのオプションで小数を有効にします (後述)
- 「CLEAR」ボタン: スコアのクリアを行えます
- 「Add Samples」ボタン: 適当にいくつかスコア登録を行います

</p>

<hr>

</p>


## スコアボードの設定

<p>

シーンに設置した`ScoreBoard`プレハブを選択するとインスペクタでスコアボードの設定が行えます。

<img width=350 src=res/images/5.png>

- `Smaller Is Better`: 値が小さい方が上位としたい場合にチェックを入れます
- `Is Float`: スコアが小数である場合にチェックを入れます
- `Number Of Float Digits`: スコアが小数である場合に表示する小数点以下の桁数を設定できます

</p>


## ベストスコアのみ登録する

<p>

サンプルシーンでは`ScoreBoardSample`スクリプトの

 - `Best Only`

にチェックを入れます。

実際に使用する場合はスクリプト内で関数を呼び出す際にフラグを設定します。(後述)

<img width=350 src=res/images/4.png>

</p>


## スクリプトでの呼び出し

<p>

`ScoreBoardSample.cs`を参考にしてください。

`AddScore()`がスコアの登録箇所です。

- `_bestOnly`は`true`もしくは`false`を渡してください。  
混ぜて使うと正しく動作しないため必ずどちらか1つを使用してください。  
もし切り替えたい場合は一度スコアボードをクリアしてください。(`ClearScoreBoard()`)

  - `true`: ベストスコアのみ登録されます  
既に登録されているユーザーに対してそれより低い得点で呼び出した場合は無視されます
  - `false`: 送信したスコアで上書き更新されます
- 5番目の引数 (`sync`) は基本`true`で送ってください。同期が行われます。

<img width=650 src=res/images/6.png>

</p>


## 見た目の変更

<p>

背景や枠などの見た目の変更は`ScoreBoard`プレハブを編集してください。

<img width=350 src=res/images/1.png>

各ユーザーのランキングテキストの見た目の変更は`item`プレハブを編集してください。

<img width=350 src=res/images/2.png>

見た目変更例の Prefab Variant は `Prefabs > Custom`に入っています。

<img width=350 src=res/images/11.png>

<p>

・ 変更例

<img height=280 src=res/images/10.png>

</p>


</p>
