# Stock Analytics

Pl stock prediction system is a  simple crawler stock data and simple stock, company analytics system to prediction next day stock price.

## Project structures

 1.Representation
  Pl.Sas.Cms => a web app to management content and show analytics results
  Pl.Sas.Crawler.Beater => beater for crawler system and schedule task
  Pl.Sas.Crawler.Downloader => a downloader for download stock data
  Pl.Sas.Crawler.Processor => parser downloader data
  Pl.Sas.Realtime.Worker => update stock view data and listen stock price realtime
  Pl.Sas.Worker => run schedule task (company analytics, stock prices analytic, user invest)

 2.Core
  Pl.Sas.Core => contains all business, interface, constants, and settings class of system

 3.Infrastructure
  Pl.Sas.Infrastructure => all 3rd part provider that support in system, and some helper, logging vvv
  Pl.Sas.MachineLearning => contains ssa and ftt ML algorithm

 4.Tests
  Pl.Sas.InvestmentPrinciplesTests => Test investment principle and find stock index
  Pl.Sas.UnitTests => a unit test for all project

 5.SolutionItems

- CHANGELOG => app version info
- Command => all common command template
- CONTRIBUTING => code rule, things rule
- README => about introduce for system
- LICENSE => license info and developer signature, third party

## System architectures

 See => <https://docs.microsoft.com/en-us/dotnet/architecture/modern-web-apps-azure/common-web-application-architectures>

## Code conventions

 See => <https://github.com/ktaranov/naming-convention/blob/master/C%23%20Coding%20Standards%20and%20Naming%20Conventions.md>

## Branch description

- main => Toàn bộ các change mới nhất của dự án được merge về branch này
- application => kích hoạt deploy ứng dụng
- infrastructure => kích hoạt up hạ tầng
- haproxy => kích hoạt deploy haproxy

## Địa chỉ dịch vụ

- SqlServer => 42.112.27.31,3400  sa/pl123456@1234
- Rabbit => Webmanager -><http://42.112.27.31:3401>  plsas/pl13245: Client connection -> 42.112.27.31:3402  plsas/pl13245
- Redis => 42.112.27.31:3403
- Haproxy => 103.121.89.235/haproxyStats  admin/loipv89
- Log => <http://42.112.27.31:3404> Hoặc <http://log.vuigreens.com>
- Web status => <http://42.112.27.31:3409> Hoặc <http://status.vuigreens.com>
- Web dashboard => <http://42.112.27.31:3406> Hoặc <http://sas.vuigreens.com>

