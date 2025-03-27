## About
台達 SCARA 機械手臂 - 相框組裝控制程式。臺科大機器人研究社 - NTUST 50th 校慶 

## API注意事項
1. 使用全速運轉需上傳100號程式至手臂，詳見 DeltaScara手冊/DROE CPP API
2. robot.StartCmd()可開啟全速運轉
3. 開啟全速運轉後才能使用 MovL、MovP、MovJ等運動函式
4. 未開啟全速運轉，只能使用 GotoMovL、GotoMovP等運動函式
5. 手臂出現Alarm會自動斷開全速運轉功能，解除Alarm後需再重新呼叫 robot.StartCmd()

## IO說明
1. DO2: 氣缸夾爪電磁閥
2. DO3: 前吸盤電磁閥
3. DO5: 後吸盤電磁閥
4. DI4: 開始按鈕
   
## Demo
https://github.com/user-attachments/assets/7375bae0-0bef-4282-b1fd-7192291887fc

