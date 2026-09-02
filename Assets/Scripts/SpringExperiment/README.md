# Spring Experiment

Mở `Assets/Scenes/SpringExperiment.unity` và bấm Play. Scene khởi tạo toàn bộ giá đỡ, lò xo, vật nặng, vector lực, thước đo và UI lúc chạy.

Nếu muốn dựng lại scene từ đầu, dùng menu `Tools > Physics > Build Spring Experiment Scene`.

`SpringPhysics` là model độc lập với Unity scene: `Weight = m * 9.81`, `Extension = Weight / k`, `CurrentLength = NaturalLength + Extension`. Các lớp còn lại chỉ chịu trách nhiệm trình bày và tương tác.
