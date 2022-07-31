using Pl.Sas.Core.Entities;
using Skender.Stock.Indicators;

namespace Pl.Sas.Core.Trading
{
    public class EmaHigLowTrading : BaseTrading
    {
        private readonly List<EmaResult> _highEmas;
        private readonly List<EmaResult> _lowEmas;
        private readonly List<EmaResult> _limitEmas;
        private readonly List<RsiResult> _fastRsis;
        private readonly List<RsiResult> _slowRsis;
        private readonly TradingCase _tradingCase;

        public EmaHigLowTrading(List<ChartPrice> chartPrices, int lowEma = 20, int highEma = 20, int limitEma = 70)
        {
            var quotes = chartPrices.Select(q => q.ToQuote()).OrderBy(q => q.Date).ToList();
            _limitEmas = quotes.Use(CandlePart.Close).GetEma(limitEma).ToList();
            _highEmas = quotes.Use(CandlePart.High).GetEma(highEma).ToList();
            _lowEmas = quotes.Use(CandlePart.Low).GetEma(lowEma).ToList();
            _fastRsis = quotes.GetRsi(1).ToList();
            _slowRsis = quotes.GetRsi(14).ToList();
            _tradingCase = new();
        }

        public TradingCase Trading(List<ChartPrice> chartPrices, List<ChartPrice> tradingHistory, string exchangeName)
        {
            foreach (var day in chartPrices)
            {
                if (tradingHistory.Count <= 0)
                {
                    _tradingCase.AssetPosition = $"T-{_tradingCase.NumberChangeDay}";
                    _tradingCase.AddNote(0, $"{day.TradingDate:yy/MM/dd}, O:{day.OpenPrice:0,0.00}, H:{day.HighestPrice:0,0.00}, L:{day.LowestPrice:0,0.00}, C:{day.ClosePrice:0,0.00}, chứng khoán:{_tradingCase.NumberStock:0,0}, Tải sản: {_tradingCase.Profit(day.ClosePrice):0,0} |-> không giao dịch.");
                    tradingHistory.Add(day);
                    continue;
                }

                RebuildStatus(tradingHistory[^1]);
                _tradingCase.IsBuy = false;
                _tradingCase.IsSell = false;
                var timeTrading = GetTimeTrading(exchangeName, DateTime.Now);

                if (_tradingCase.NumberStock <= 0)
                {
                    _tradingCase.IsBuy = BuyCondition(tradingHistory[^1]) > 0 && _tradingCase.ContinueBuy;
                    if (_tradingCase.IsBuy)
                    {
                        _tradingCase.BuyPrice = CalculateOptimalBuyPrice(tradingHistory, day.OpenPrice);
                        _tradingCase.ActionPrice = _tradingCase.BuyPrice;
                        if (_tradingCase.ActionPrice <= day.LowestPrice)
                        {
                            _tradingCase.ActionPrice = day.ClosePrice;
                            _tradingCase.NumberPriceClose++;
                        }
                        else
                        {
                            _tradingCase.NumberPriceNeed++;
                        }
                        var (stockCount, excessCash, totalTax) = Buy(_tradingCase.TradingMoney, _tradingCase.ActionPrice * 1000, _tradingCase.NumberChangeDay);
                        _tradingCase.TradingMoney = excessCash;
                        _tradingCase.TotalTax += totalTax;
                        _tradingCase.NumberStock += stockCount;
                        _tradingCase.NumberChangeDay = 0;
                        _tradingCase.MaxPriceOnBuy = day.ClosePrice;
                        _tradingCase.StopLossPrice = _tradingCase.ActionPrice - (_tradingCase.ActionPrice * GetExchangeFluctuationsRate(exchangeName));
                        if (timeTrading == TimeTrading.NST || DateTime.Now.Date != day.TradingDate)
                        {
                            _tradingCase.AssetPosition = $"C-{_tradingCase.NumberChangeDay}, {day.ClosePrice.GetPercent(_tradingCase.ActionPrice):0.0}";
                        }
                        else
                        {
                            if (timeTrading == TimeTrading.DON)
                            {
                                _tradingCase.AssetPosition = $"C-{_tradingCase.NumberChangeDay}, {day.ClosePrice.GetPercent(_tradingCase.ActionPrice):0.0}";
                            }
                            else
                            {
                                _tradingCase.AssetPosition = $"Mua";
                            }
                        }
                        _tradingCase.AddNote(0, $"{day.TradingDate:yy/MM/dd}, O:{day.OpenPrice:0,0.00}, H:{day.HighestPrice:0,0.00}, L:{day.LowestPrice:0,0.00}, C:{day.ClosePrice:0,0.00}, chứng khoán:{_tradingCase.NumberStock:0,0}, Tải sản: {_tradingCase.Profit(day.ClosePrice):0,0} |-> Mua {_tradingCase.NumberStock:0,0} cổ giá {_tradingCase.ActionPrice:0,0.00} thuế {totalTax:0,0}");
                    }
                    else
                    {
                        _tradingCase.AssetPosition = $"T-{_tradingCase.NumberChangeDay}";
                        _tradingCase.AddNote(0, $"{day.TradingDate:yy/MM/dd}, O:{day.OpenPrice:0,0.00}, H:{day.HighestPrice:0,0.00}, L:{day.LowestPrice:0,0.00}, C:{day.ClosePrice:0,0.00}, chứng khoán:{_tradingCase.NumberStock:0,0}, Tải sản: {_tradingCase.Profit(day.ClosePrice):0,0} (T-{_tradingCase.NumberChangeDay}) |-> Không giao dịch.");
                    }
                }
                else
                {
                    if (_tradingCase.NumberChangeDay > _timeStockCome)
                    {
                        _tradingCase.IsSell = SellCondition(tradingHistory[^1]) > 0;
                        if (_tradingCase.IsSell)
                        {
                            _tradingCase.SellPrice = CalculateOptimalSellPrice(tradingHistory, day.OpenPrice);
                            var lastBuyPrice = _tradingCase.ActionPrice;
                            _tradingCase.ActionPrice = _tradingCase.SellPrice;
                            if (_tradingCase.ActionPrice >= day.HighestPrice)
                            {
                                _tradingCase.ActionPrice = day.ClosePrice;
                                _tradingCase.NumberPriceClose++;
                            }
                            else
                            {
                                _tradingCase.NumberPriceNeed++;
                            }
                            var (totalProfit, totalTax) = Sell(_tradingCase.NumberStock, _tradingCase.ActionPrice * 1000);
                            _tradingCase.TradingMoney = totalProfit;
                            _tradingCase.TotalTax += totalTax;
                            var selNumberStock = _tradingCase.NumberStock;
                            _tradingCase.NumberStock = 0;
                            _tradingCase.NumberChangeDay = 0;
                            if (timeTrading == TimeTrading.NST || DateTime.Now.Date != day.TradingDate)
                            {
                                _tradingCase.AssetPosition = $"T-{_tradingCase.NumberChangeDay}";
                            }
                            else
                            {
                                if (timeTrading == TimeTrading.DON)
                                {
                                    _tradingCase.AssetPosition = $"T-{_tradingCase.NumberChangeDay}";
                                }
                                else
                                {
                                    _tradingCase.AssetPosition = $"Bán";
                                }
                            }
                            _tradingCase.AddNote(_tradingCase.ActionPrice > lastBuyPrice ? 1 : -1, $"{day.TradingDate:yy/MM/dd}, O:{day.OpenPrice:0,0.00}, H:{day.HighestPrice:0,0.00}, L:{day.LowestPrice:0,0.00}, C:{day.ClosePrice:0,0.00}, chứng khoán:{_tradingCase.NumberStock:0,0}, Tải sản: {_tradingCase.Profit(day.ClosePrice):0,0} |-> Bán {selNumberStock:0,0} cổ giá {_tradingCase.ActionPrice:0,0.00} ({_tradingCase.ActionPrice.GetPercent(lastBuyPrice):0,0.00}%), Max: ({_tradingCase.MaxPriceOnBuy.GetPercent(lastBuyPrice):0,0.00}%) thuế {totalTax:0,0}");
                        }
                        else
                        {
                            _tradingCase.AssetPosition = $"C-{_tradingCase.NumberChangeDay}, {day.ClosePrice.GetPercent(_tradingCase.ActionPrice):0.0}";
                            _tradingCase.AddNote(0, $"{day.TradingDate:yy/MM/dd}, O:{day.OpenPrice:0,0.00}, H:{day.HighestPrice:0,0.00}, L:{day.LowestPrice:0,0.00}, C:{day.ClosePrice:0,0.00}, chứng khoán:{_tradingCase.NumberStock:0,0}, Tải sản: {_tradingCase.Profit(day.ClosePrice):0,0} (C-{_tradingCase.NumberChangeDay}, {day.ClosePrice.GetPercent(_tradingCase.ActionPrice):0,0.00}%) |-> Không giao dịch");
                        }
                    }
                    else
                    {
                        _tradingCase.AssetPosition = $"C-{_tradingCase.NumberChangeDay}, {day.ClosePrice.GetPercent(_tradingCase.ActionPrice):0.0}";
                        _tradingCase.AddNote(0, $"{day.TradingDate:yy/MM/dd}, O:{day.OpenPrice:0,0.00}, H:{day.HighestPrice:0,0.00}, L:{day.LowestPrice:0,0.00}, C:{day.ClosePrice:0,0.00}, chứng khoán:{_tradingCase.NumberStock:0,0}, Tải sản: {_tradingCase.Profit(day.ClosePrice):0,0} (C-{_tradingCase.NumberChangeDay}, {day.ClosePrice.GetPercent(_tradingCase.ActionPrice):0,0.00}%) |-> Không giao dịch do mới mua {_tradingCase.NumberChangeDay} ngày");
                    }
                }

                _tradingCase.NumberChangeDay++;
                if (_tradingCase.NumberStock <= 0)
                {
                    _tradingCase.NumberDayInMoney++;
                }
                else
                {
                    _tradingCase.NumberDayInStock++;
                }
                tradingHistory.Add(day);
            }

            return _tradingCase;
        }

        public void RebuildStatus(ChartPrice chartPrice)
        {
            if (_tradingCase.MaxPriceOnBuy < chartPrice.ClosePrice)
            {
                _tradingCase.MaxPriceOnBuy = chartPrice.ClosePrice;//Đặt lại giá cao nhất đã đạt được
            }

            if (!_tradingCase.ContinueBuy)
            {
                var limitEma = _limitEmas.Find(chartPrice.TradingDate);
                if (limitEma is null || limitEma.Ema is null)
                {
                    return;
                }

                var highEma = _highEmas.Find(chartPrice.TradingDate);
                if (highEma is null || highEma.Ema is null)
                {
                    return;
                }

                var lowEma = _lowEmas.Find(chartPrice.TradingDate);
                if (lowEma is null || lowEma.Ema is null)
                {
                    return;
                }

                if ((chartPrice.ClosePrice < limitEma.Ema && chartPrice.ClosePrice < highEma.Ema && chartPrice.ClosePrice < lowEma.Ema) || (chartPrice.ClosePrice > _tradingCase.ActionPrice && _tradingCase.NumberChangeDay > 5))
                {
                    _tradingCase.AddNote(0, $"{chartPrice.TradingDate:yy/MM/dd}: Cho phép lệnh mua được hoạt động do đường FastEma đã cắt xuống đường SlowEma.");
                    _tradingCase.ContinueBuy = true;
                }
            }
        }

        public int BuyCondition(ChartPrice chartPrice)
        {
            var limitEma = _limitEmas.Find(chartPrice.TradingDate);
            if (limitEma is null || limitEma.Ema is null)
            {
                return 0;
            }

            var highEma = _highEmas.Find(chartPrice.TradingDate);
            if (highEma is null || highEma.Ema is null)
            {
                return 0;
            }

            var lowEma = _lowEmas.Find(chartPrice.TradingDate);
            if (lowEma is null || lowEma.Ema is null)
            {
                return 0;
            }

            if (chartPrice.ClosePrice < limitEma.Ema || chartPrice.ClosePrice < highEma.Ema || chartPrice.ClosePrice < lowEma.Ema)
            {
                return 0;
            }

            var slowRsi = _slowRsis.Find(chartPrice.TradingDate);
            if (slowRsi is null || slowRsi.Rsi is null)
            {
                return 0;
            }

            var fastRsi = _fastRsis.Find(chartPrice.TradingDate);
            if (fastRsi is null || fastRsi.Rsi is null)
            {
                return 0;
            }

            if (fastRsi.Rsi < slowRsi.Rsi)
            {
                return 0;
            }

            return 100;
        }

        public int SellCondition(ChartPrice chartPrice)
        {
            if (chartPrice.ClosePrice <= _tradingCase.StopLossPrice)
            {
                _tradingCase.AddNote(-1, $"{chartPrice.TradingDate:yy/MM/dd}: Kích hoạt lệnh bán chặn lỗ, giá mua {_tradingCase.ActionPrice:0.0,00} giá kích hoạt {chartPrice.ClosePrice:0.0,00}({chartPrice.ClosePrice.GetPercent(_tradingCase.ActionPrice):0.0,00})");
                _tradingCase.ContinueBuy = false;
                return 100;
            }

            var limitEma = _limitEmas.Find(chartPrice.TradingDate);
            if (limitEma is null || limitEma.Ema is null)
            {
                return 0;
            }

            var highEma = _highEmas.Find(chartPrice.TradingDate);
            if (highEma is null || highEma.Ema is null)
            {
                return 0;
            }

            var lowEma = _lowEmas.Find(chartPrice.TradingDate);
            if (lowEma is null || lowEma.Ema is null)
            {
                return 0;
            }

            if (chartPrice.ClosePrice > limitEma.Ema || chartPrice.ClosePrice > highEma.Ema || chartPrice.ClosePrice > lowEma.Ema)
            {
                return 0;
            }

            var slowRsi = _slowRsis.Find(chartPrice.TradingDate);
            if (slowRsi is null || slowRsi.Rsi is null)
            {
                return 0;
            }

            var fastRsi = _fastRsis.Find(chartPrice.TradingDate);
            if (fastRsi is null || fastRsi.Rsi is null)
            {
                return 0;
            }

            if (fastRsi.Rsi > slowRsi.Rsi)
            {
                return 0;
            }

            return 100;
        }

        public static float CalculateOptimalBuyPrice(List<ChartPrice> chartPrices, float rootPrice)
        {
            var percent = chartPrices.OrderByDescending(q => q.TradingDate).Take(10).Select(q => q.HighestPrice.GetPercent(q.OpenPrice)).Average() / 100;
            var buyPrice = rootPrice - (rootPrice * (percent * 5));
            return (float)Math.Round((decimal)buyPrice, 2);
        }

        public static float CalculateOptimalSellPrice(List<ChartPrice> chartPrices, float rootPrice)
        {
            var percent = chartPrices.OrderByDescending(q => q.TradingDate).Take(10).Select(q => q.OpenPrice.GetPercent(q.LowestPrice)).Average() / 100;
            var buyPrice = rootPrice + (rootPrice * (percent * 5));
            return (float)Math.Round((decimal)buyPrice, 2);
        }
    }
}