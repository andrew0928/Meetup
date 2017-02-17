

# Agenda:

* HDD (Hype Driven Development)
* Our Case Study: HRD system
* Our Goal: Massive Cloud Hosting, as LPaaS
* Prerequirement and infrastructure of Microservices

# Microservices 定義

> 參考書: microservices

一群協同運作的小型自主服務 (autonomous service)

特點:
* 小巧，並且專注做好每一件事
* 自主性
  * 開放 API，透過網路存取服務，是必要的手段
  * 目的: 解耦合。最高指導原則是: 你能夠獨立對服務進行變更，獨立部屬他，而不需要改變其他事情嗎?
  > 你必須正確的塑模擬的服務，並將 API 弄對
* 技術異質性
* 彈性
* 擴展
* 容易部屬
* 組織調教
* 組合性
* 最佳化可替換性

其他分解的技術?

> 微服務的優勢源自細粒度。其他分解技術能帶來一樣的好處嗎?

* share library
  * 沒有技術異質性
  * 同一個 process
* module
  * seperate process
  * 

沒有銀子彈  
* 必須精於處理部屬問題
* 測試
* 監控
* 面臨分散式交易 (CAP 問題)


# 採用 Microservices 動機

* 小型的服務，何必動用三層式架構?
* 三層式架構的自主性差
* Scalability of your team / organization
  - 若是三層式架構，每一層都有一個團隊負責，改版時是個大災難...
  - 若要團隊能各自負責獨立 "模組"，自主性是關鍵
  - 切割成獨立的小型服務，透過 API 存取，自主性最佳
  - 每個服務責任夠明確，規模不複雜，就不需要三層架構


# 導入 Microservices 需要的基礎建設

* 微服務 -> 必然是分散式系統
  - 第一步: 先搞定 RPC
  - 第二步: 切割系統邊界
  - 第四步: 必要的開發流程 (devops / ci / cd)
  - 第三步: 必要的基礎建設 (infrastructure)
      - API gateway, Service Discovery
      - Event System, Message Queue, Service Bus, Middleware
      - Orchestration, Containers Cluster, Service Fabric
      - Logging, Tracking System, ...
  - scalability (scale out)
  - API management
  - dev process (devops / ci / cd is must)
  - version is important (source code, interface, document)
  - large scale deployment is must

# 經驗分享: migration from monolitch to Microservices

關鍵: 取得兩大架構的平衡
你想解決的主要問題是甚麼?

  - refactory
  - split
  - find boundary


* 實際解構案例: Orca HCM


-----

# Reference

* [架构师之路16年精选50篇](http://mp.weixin.qq.com/s/OlFKpcnBOgcPZmjvdzCCiA)
【服务化与为服务】
《互联网架构为什么要做服务化》
《微服务架构究竟多“微”合适》
《要想微服务，先搞定RPC框架》
《RPC-client序列化原理与细节》
《RPC-client异步收发细节》


-----

# 大型 web application 轉移到微服務的經驗分享

Andrew Wu, 任職一宇數位技術長 & 技術發展顧問。有 18 年大型商用軟體與雲端服務
的開發經驗，負責系統架構的規劃與設計。

由大型商用軟體 (人才發展系統) 轉型為雲端服務，會面臨許多技術與架構轉換的挑戰。
單體式 (monolitch) app 移轉到微服務 (microservices) 就是其中最關鍵的環節。
這個 session 將分享我們如何轉移到微服務的經驗，以及如何面對技術與架構上的挑戰。


# 講師介紹

熱衷於 OOP, .NET, 軟體工程, 與 Cloud 相關技術。近年鑽研如何將 .NET 解決方案微服務
化，熱衷於 (windows) container, devops, 以及分散式系統等主題，同時在部落格上也持續
分享相關主題的一系列文章。期許能將這些實作經驗分享到社群。

# 講師經歷

一宇數位 技術長, 技術顧問
Microsoft MVP, 微軟最有價值專家
經營: 安德魯的部落格
曾任: 資策會 雲端系列課程 Azure PaaS 講師, 專欄作家

 