
--
why microservices? in my points of view

1. 2009, low cost
- azure 採購 server 的成本低 (以量制價)
- azure 維運機房的成本低 (有全球頂尖的 developer + IT operators + architect), 除了基本水電，全自動化維運，只要基本的警衛等人員
- purchase VM, buy CPU hours (charge in minutes), 不用就關掉 (resource 可放出給其他 user), 提高資源使用率


1. 2010, cloud native
- paas / saas 出現, 按照服務量計價, 更進一步提升 resource usage rate
- paas (如 media services), 轉 1hr video NTD 50, MS 可統一 scale out
- 服務重心由 IaaS (buy VM hours) -> PaaS (buy workload)
- WebAPP / Azure Functions ... 越來越往 serverless 架構演進，更進一步提升 resource usage

1. scale out v.s. scale up
- 兩台比一台好
- 兩台比一台便宜
- 兩台比一台可靠
- 自動化 scale out
- 降低服務的細粒度，越低越適合雲端的模式 (cloud native)

think: 為何早年我們的 application 難以放到雲端執行?
- monolitch architecture
- blah blah ...

solution: microservices (就是 cloud native 的方法與架構，歸納而成的架構建議)
container 降低了 microservices 的門檻，將 microservices 推向主流的架構
2012 ~ 2016, 系統架構逐漸朝向 microservices 發展，不但享有 microservices 的各種優點，同時也更適合 cloud service 的運作方式，更貼近 cloud native 的
設計架構

> 大型 application 微服務化的經驗分享