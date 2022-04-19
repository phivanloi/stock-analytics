# Stock Analytics

Pl stock prediction system is a  simple crawler stock data and simple stock, company analytics system to prediction next day stock price.

## Project structures

 1.Representation
  Pl.Sps.Cms => a web app to management content and show analytics results
  Pl.Sps.Crawler.Beater => beater for crawler system and schedule task
  Pl.Sps.Crawler.Downloader => a downloader for download stock data
  Pl.Sps.Crawler.Processor => parser downloader data
  Pl.Sps.Realtime.Worker => update stock view data and listen stock price realtime
  Pl.Sps.Worker => run schedule task (company analytics, stock prices analytic, user invest)

 2.Core
  Pl.Sps.Core => contains all business, interface, constants, and settings class of system

 3.Infrastructure
  Pl.Sps.Infrastructure => all 3rd part provider that support in system, and some helper, logging vvv
  Pl.Sps.MachineLearning => contains ssa and ftt ML algorithm

 4.Tests
  Pl.Sps.InvestmentPrinciplesTests => Test investment principle and find stock index
  Pl.Sps.UnitTests => a unit test for all project

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

- main contain all member change that pm accept
- product to deploy docker production environment
- stop to stop and clean all docker container

## Địa chỉ các thành phần hạ tầng production

- Web sps system => <http://sps.vuigreens.com>
- Log => <http://log.vuigreens.com>

## Địa chỉ các thành phần hạ tầng test

- Sql => localhost:9001 sa/Loipv123456@1234
- Rabbit => Webmanager -><http://localhost:9002>  loipv/spsloipv132: Client connection -> localhost:9003
- Redis => localhost:9004
- Log => <http://localhost:9005>
- Monitoring => <http://localhost:9006> monitoring/234fdswe65
- Web sps system => <http://localhost:9007>
