# GPM LAT
## GPM_AGV_LAT_CORE

```
這是LAT核心專案,所有的商業邏輯、模型都在此專案。
```
---
## 參考專案
- ganghaoagv  http://gitlab.gpmcorp.com.tw/lat/ganghaoagv.git

相對路徑:
``` 
    ┌─ ganghaoagv
    ├─ GPM_AGV_LAT_CORE(此專案)
    ├─ LAT-Solution.sln(你的方案檔)
```

---

## 應用層專案(進行核心開發的時候不會用到)
-  web後端 http://gitlab.gpmcorp.com.tw/lat/gpm_agv_lat_app.git
-  web前端 http://gitlab.gpmcorp.com.tw/lat/gpm_agv_lat_web.git

---

## Migrate [.Net]
```
using GPM_AGV_LAT_CORE;
...
...

//In your project ,anywhere
Starup.StartService();

```

