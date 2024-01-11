using KOAStudio.Core.Models;
using KOAStudio.Core.ViewModels;
using KOAStudio.Core.Views;
using System.Diagnostics;
using System.Text;

namespace KOAStudio.Business;
internal sealed partial class BusinessLogic
{
    private CharDataReqViewModel? _chartDataReqViewModel_업종;
    private CharDataReqViewModel? _chartDataReqViewModel_주식;
    private CharDataReqViewModel? _chartDataReqViewModel_선물;
    private CharDataReqViewModel? _chartDataReqViewModel_옵션;
    private OrderViewModel? _orderViewModel_주식;
    private OrderViewModel? _orderViewModel_선물옵션;

    void ShowUserContent(string require)
    {
        if (require.Equals("업종차트요청"))
        {
            var model = _chartDataReqViewModel_업종;
            if (model == null)
            {
                _chartDataReqViewModel_업종 = model = new CharDataReqViewModel(CharDataReqViewModel.KIND.업종, require)
                {
                    ExtProcedure = ChartContentExtProcedure,
                    Selected종목 = _appRegistry.GetValue(require, "종목코드", "001"),
                    SelectedChartInterval_분 = _appRegistry.GetValue(require, "분주기", "10"),
                    SelectedChartInterval_틱 = _appRegistry.GetValue(require, "틱주기", "100"),
                };
                ChartRound chartRound = ChartRound.분;
                Enum.TryParse(_appRegistry.GetValue(require, "시간타입", string.Empty), out chartRound);
                model.SelectedChartRound = chartRound;
            }
            model.NextEnabled = false;
            model.EnableUpdateCodeText = true;
            model.UpdateCodeText();
            SetUserContent(new CharDataReqView(model));
        }
        else if (require.Equals("주식차트요청"))
        {
            var model = _chartDataReqViewModel_주식;
            if (model == null)
            {
                _chartDataReqViewModel_주식 = model = new CharDataReqViewModel(CharDataReqViewModel.KIND.주식, require)
                {
                    ExtProcedure = ChartContentExtProcedure,
                    Selected종목 = _appRegistry.GetValue(require, "종목코드", "005930"),
                    SelectedChartInterval_분 = _appRegistry.GetValue(require, "분주기", "1"),
                    SelectedChartInterval_틱 = _appRegistry.GetValue(require, "틱주기", "100"),
                };
                ChartRound chartRound = ChartRound.일;
                Enum.TryParse(_appRegistry.GetValue(require, "시간타입", string.Empty), out chartRound);
                model.SelectedChartRound = chartRound;
            }
            model.NextEnabled = false;
            model.EnableUpdateCodeText = true;
            model.UpdateCodeText();
            SetUserContent(new CharDataReqView(model));
        }
        else if (require.Equals("선물차트요청"))
        {
            var model = _chartDataReqViewModel_선물;
            if (model == null)
            {
                _chartDataReqViewModel_선물 = model = new CharDataReqViewModel(CharDataReqViewModel.KIND.선물, require)
                {
                    ExtProcedure = ChartContentExtProcedure,
                    Selected종목 = _appRegistry.GetValue(require, "종목코드", "10100000"),
                    SelectedChartInterval_분 = _appRegistry.GetValue(require, "분주기", "1"),
                    SelectedChartInterval_틱 = _appRegistry.GetValue(require, "틱주기", "300"),
                };
                ChartRound chartRound = ChartRound.틱;
                Enum.TryParse(_appRegistry.GetValue(require, "시간타입", string.Empty), out chartRound);
                model.SelectedChartRound = chartRound;
            }
            model.NextEnabled = false;
            model.EnableUpdateCodeText = true;
            model.UpdateCodeText();
            SetUserContent(new CharDataReqView(model));
        }
        else if (require.Equals("옵션차트요청"))
        {
            var model = _chartDataReqViewModel_옵션;
            if (model == null)
            {
                _chartDataReqViewModel_옵션 = model = new CharDataReqViewModel(CharDataReqViewModel.KIND.옵션, require)
                {
                    ExtProcedure = ChartContentExtProcedure,
                    Selected종목 = _appRegistry.GetValue(require, "종목코드", "201TC340"),
                    SelectedChartInterval_분 = _appRegistry.GetValue(require, "분주기", "1"),
                    SelectedChartInterval_틱 = _appRegistry.GetValue(require, "틱주기", "100"),
                };
                ChartRound chartRound = ChartRound.분;
                Enum.TryParse(_appRegistry.GetValue(require, "시간타입", string.Empty), out chartRound);
                model.SelectedChartRound = chartRound;
            }
            model.NextEnabled = false;
            model.EnableUpdateCodeText = true;
            model.UpdateCodeText();
            SetUserContent(new CharDataReqView(model));
        }
        else if (require.Equals("주식주문요청"))
        {
            var model = _orderViewModel_주식;
            if (model == null)
            {
                _orderViewModel_주식 = model = new OrderViewModel(require, OrderExtCallProc)
                {
                    종목코드 = _appRegistry.GetValue(require, "종목코드", "005930"),
                };
            }
            if ((model.계좌리스트 == null || !model.계좌리스트.Any()) && _axOpenAPI != null && _axOpenAPI.GetConnectState() == 1)
            {
                var accs = _axOpenAPI.GetLoginInfo("ACCNO").Split(';', StringSplitOptions.RemoveEmptyEntries);
                model.계좌리스트 = accs.Where(x => x.EndsWith("11") || x.EndsWith("10"));
                if (model.계좌리스트.Any())
                {
                    model.Selected계좌 = model.계좌리스트.First();
                }
            }
            model.EnableUpdateCodeText = true;
            model.UpdateCodeText();
            SetUserContent(new OrderView(model));
        }
        else if (require.Equals("선물옵션주문요청"))
        {
            var model = _orderViewModel_선물옵션;
            if (model == null)
            {
                _orderViewModel_선물옵션 = model = new OrderViewModel(require, OrderExtCallProc)
                {
                    종목코드 = _appRegistry.GetValue(require, "종목코드", "10100000"),
                };
            }
            if ((model.계좌리스트 == null || !model.계좌리스트.Any()) && _axOpenAPI != null && _axOpenAPI.GetConnectState() == 1)
            {
                var accs = _axOpenAPI.GetLoginInfo("ACCNO").Split(';', StringSplitOptions.RemoveEmptyEntries);
                model.계좌리스트 = accs.Where(x => x.EndsWith("31"));
                if (model.계좌리스트.Any())
                {
                    model.Selected계좌 = model.계좌리스트.First();
                }
            }
            model.EnableUpdateCodeText = true;
            model.UpdateCodeText();
            SetUserContent(new OrderView(model));
        }
    }

    private void OrderExtCallProc(OrderViewModel model, string requre)
    {

        bool b매수주문 = requre.Equals("매수주문");
        bool b매도주문 = requre.Equals("매도주문");
        bool b정정주문 = requre.Equals("정정주문");
        bool b취소주문 = requre.Equals("취소주문");
        bool b조회 = requre.Equals("조 회");

        if (b매수주문 || b매도주문 || b정정주문 || b취소주문 || b조회)
        {
            if (_axOpenAPI == null || _axOpenAPI.GetConnectState() == 0)
            {
                OutputLog((int)TAB_LIST_KIND.메시지목록, $"[{model.Title} {requre}] : 로그인 후 요청해 주세요");
                return;
            }

            if (b조회)
            {
                if (model.Title.Equals("주식주문요청"))
                {
                    if (model.SelectedTabIndex == 0) // 미체결
                    {
                        string sTrCode = "opt10076"; // 체결요청
                        _axOpenAPI.SetInputValue("종목코드", string.Empty);
                        _axOpenAPI.SetInputValue("조회구분", "0");
                        _axOpenAPI.SetInputValue("매도수구분", "0");
                        _axOpenAPI.SetInputValue("계좌번호", model.Selected계좌);
                        _axOpenAPI.SetInputValue("비밀번호", string.Empty);
                        _axOpenAPI.SetInputValue("주문번호", string.Empty);
                        _axOpenAPI.SetInputValue("체결구분", "1");

                        int nRet = _axOpenAPI.CommRqData(model.Title + " 미체결조회", sTrCode, 0, _scrNum_ORDER_CONTENT);
                        if (nRet < 0)
                        {
                            OutputLog((int)TAB_LIST_KIND.메시지목록, $"[{model.Title}] : 미체결 요청오류");
                            return;
                        }
                    }
                    else if (model.SelectedTabIndex == 1) //잔고"
                    {
                        string sTrCode = "opt10085"; // 계좌수익률요청
                        _axOpenAPI.SetInputValue("계좌번호", model.Selected계좌);
                        int nRet = _axOpenAPI.CommRqData(model.Title + " 잔고조회", sTrCode, 0, _scrNum_ORDER_CONTENT);

                        if (nRet < 0)
                        {
                            OutputLog((int)TAB_LIST_KIND.메시지목록, $"[{model.Title}] : 잔고 요청오류");
                            return;
                        }
                    }
                }
                else if (model.Title.Equals("선물옵션주문요청"))
                {
                    if (model.SelectedTabIndex == 0) // 미체결
                    {
                        string sTrCode = "OPT50026"; // 선옵주문체결요청
                        _axOpenAPI.SetInputValue("종목코드", string.Empty);
                        _axOpenAPI.SetInputValue("조회구분", "0");
                        _axOpenAPI.SetInputValue("매매구분", "0");
                        _axOpenAPI.SetInputValue("체결구분", "1");
                        _axOpenAPI.SetInputValue("계좌번호", model.Selected계좌);
                        _axOpenAPI.SetInputValue("주문번호", string.Empty);

                        int nRet = _axOpenAPI.CommRqData(model.Title + " 미체결조회", sTrCode, 0, _scrNum_ORDER_CONTENT);
                        if (nRet < 0)
                        {
                            OutputLog((int)TAB_LIST_KIND.메시지목록, $"[{model.Title}] : 미체결 요청오류");
                            return;
                        }
                    }
                    else if (model.SelectedTabIndex == 1) //잔고"
                    {
                        string sTrCode = "OPT50027"; // 선옵잔고요청
                        _axOpenAPI.SetInputValue("계좌번호", model.Selected계좌);
                        int nRet = _axOpenAPI.CommRqData(model.Title + " 잔고조회", sTrCode, 0, _scrNum_ORDER_CONTENT);

                        if (nRet < 0)
                        {
                            OutputLog((int)TAB_LIST_KIND.메시지목록, $"[{model.Title}] : 잔고 요청오류");
                            return;
                        }
                    }
                }
            }
            else
            {
                if (!model.주문확인생략)
                {
                    var msgWindow = new MessageOkCancel($"{model.종목코드} {model.주문수량}계약\r\n {requre} 하시겠습니까?", $"{requre}");
                    if (msgWindow.ShowDialog() != true)
                    {
                        return;
                    }
                }

                string sRQName = model.Title + " " + ((model.매매구분 != OrderType.정정취소) ? $"{model.매매구분}" : (b정정주문 ? "정정" : "취소"));
                string sScreenNo = _scrNum_ORDER_CONTENT;
                string sAccNo = model.Selected계좌;
                string sCode = model.종목코드;
                int nQty = model.주문수량;
                string sOrgOrderNo = model.매매구분 switch // 원주문번호
                {
                    OrderType.매수 => string.Empty,
                    OrderType.매도 => string.Empty,
                    OrderType.정정취소 => model.주문번호,
                    _ => string.Empty
                };

                if (model.Title.Equals("주식주문요청"))
                {
                    int nOrderType = model.매매구분 switch //  (1:신규매도, 2:신규매수 3:매도취소, 4:매수취소, 5:매도정정, 6:매수정정)
                    {
                        OrderType.매수 => 1,
                        OrderType.매도 => 2,
                        OrderType.정정취소 => model.원주문매도수구분 ? 6 : 5,
                        _ => 0
                    };
                    // 취소경우 -= 2;
                    if (b취소주문) nOrderType -= 2;

                    int.TryParse(model.주문가격, out int nPrice);
                    if ((model.매매구분 == OrderType.매수 || model.매매구분 == OrderType.매도) && model.주문종류 == OrderKind.시장가) nPrice = 0;
                    string sHogaGb = model.주문종류 switch // 거래구분 (1:시장가, 2:지정가)
                    {
                        OrderKind.지정가 => "00",
                        OrderKind.시장가 => "03",
                        _ => "00"
                    };

                    DateTime CallTime = DateTime.Now;
                    Stopwatch stopwatch = Stopwatch.StartNew();
                    int nRet = _axOpenAPI.SendOrder(sRQName, sScreenNo, sAccNo, nOrderType, sCode, nQty, nPrice, sHogaGb, sOrgOrderNo);
                    stopwatch.Stop();
                    model.CodeText += $"[{CallTime:HH:mm:ss.fff}] : ({stopwatch.Elapsed.TotalMilliseconds})mS, nRet={nRet}\r\n";
                }
                else if (model.Title.Equals("선물옵션주문요청"))
                {
                    int lOrdKind = model.매매구분 switch // 주문종류 1:신규매매, 2:정정, 3:취소
                    {
                        OrderType.매수 => 1,
                        OrderType.매도 => 1,
                        OrderType.정정취소 => b취소주문 ? 3 : 2,
                        _ => 0
                    };
                    string sSlbyTp = model.매매구분 switch //  매매구분	1: 매도, 2:매수
                    {
                        OrderType.매수 => "2",
                        OrderType.매도 => "1",
                        OrderType.정정취소 => model.원주문매도수구분 ? "1" : "2",
                        _ => "0"
                    };

                    string sOrdTp = model.주문종류 switch // 거래구분 (1:지정가, 3:시장가)
                    {
                        OrderKind.지정가 => "1",
                        OrderKind.시장가 => "3",
                        _ => "0"
                    };
                    string sPrice = model.매매구분 switch // 거래구분 (1:시장가, 2:지정가, 3:STOP, 4:STOP LIMIT)
                    {
                        OrderType.매수 => model.주문종류 == OrderKind.시장가 ? "0" : model.주문가격,
                        OrderType.매도 => model.주문종류 == OrderKind.시장가 ? "0" : model.주문가격,
                        OrderType.정정취소 => model.주문가격,
                        _ => model.주문가격
                    };

                    DateTime CallTime = DateTime.Now;
                    Stopwatch stopwatch = Stopwatch.StartNew();
                    int nRet = _axOpenAPI.SendOrderFO(sRQName, sScreenNo, sAccNo, sCode, lOrdKind, sSlbyTp, sOrdTp, nQty, sPrice, sOrgOrderNo);
                    stopwatch.Stop();
                    model.CodeText += $"[{CallTime:HH:mm:ss.fff}] : ({stopwatch.Elapsed.TotalMilliseconds})mS, nRet={nRet}\r\n";
                }
            }
        }
        else
        {
            string sRQName = model.Title + $" {model.매매구분}";
            string sScreenNo = _scrNum_ORDER_CONTENT;
            string sAccNo = model.Selected계좌;
            string sCode = model.종목코드;
            int nQty = model.주문수량;
            string sOrgOrderNo = model.매매구분 switch // 원주문번호
            {
                OrderType.매수 => string.Empty,
                OrderType.매도 => string.Empty,
                OrderType.정정취소 => model.주문번호,
                _ => string.Empty
            };

            if (model.Title.Equals("주식주문요청"))
            {
                int nOrderType = model.매매구분 switch //  (1:신규매도, 2:신규매수 3:매도취소, 4:매수취소, 5:매도정정, 6:매수정정)
                {
                    OrderType.매수 => 1,
                    OrderType.매도 => 2,
                    OrderType.정정취소 => model.원주문매도수구분 ? 6 : 5,
                    _ => 0
                };
                // 취소주문 -= 2;

                int.TryParse(model.주문가격, out int nPrice);
                if ((model.매매구분 == OrderType.매수 || model.매매구분 == OrderType.매도) && model.주문종류 == OrderKind.시장가) nPrice = 0;
                string sHogaGb = model.주문종류 switch // 거래구분 (1:시장가, 2:지정가)
                {
                    OrderKind.지정가 => "00",
                    OrderKind.시장가 => "03",
                    _ => "00"
                };

                StringBuilder stringBuilder = new();

                if (model.매매구분 == OrderType.정정취소)
                {
                    sRQName = $"{model.Title} 정정";
                    stringBuilder.AppendLine($"// {sRQName}");
                    stringBuilder.AppendLine($"int nRet = _axOpenAPI.SendOrder(\"{sRQName}\", \"{sScreenNo}\", \"{sAccNo}\", {nOrderType}, \"{sCode}\", {nQty}, {nPrice}, \"{sHogaGb}\", \"{sOrgOrderNo}\");");

                    nOrderType -= 2;
                    sRQName = $"{model.Title} 취소";
                    stringBuilder.AppendLine($"// {sRQName}");
                    stringBuilder.AppendLine($"int nRet = _axOpenAPI.SendOrder(\"{sRQName}\", \"{sScreenNo}\", \"{sAccNo}\", {nOrderType}, \"{sCode}\", {nQty}, {nPrice}, \"{sHogaGb}\", \"{sOrgOrderNo}\");");
                }
                else
                {
                    stringBuilder.AppendLine($"// {sRQName}");
                    stringBuilder.AppendLine($"int nRet = _axOpenAPI.SendOrder(\"{sRQName}\", \"{sScreenNo}\", \"{sAccNo}\", {nOrderType}, \"{sCode}\", {nQty}, {nPrice}, \"{sHogaGb}\", \"{sOrgOrderNo}\");");
                }

                model.CodeText = stringBuilder.ToString();
            }
            else if (model.Title.Equals("선물옵션주문요청"))
            {
                int lOrdKind = model.매매구분 switch //  (1:신규매도, 2:신규매수 3:매도취소, 4:매수취소, 5:매도정정, 6:매수정정)
                {
                    OrderType.매수 => 1,
                    OrderType.매도 => 1,
                    OrderType.정정취소 => 2,
                    _ => 0
                };
                // 취소주문 lOrdKind += 1;
                string sSlbyTp = model.매매구분 switch //  매매구분	1: 매도, 2:매수
                {
                    OrderType.매수 => "2",
                    OrderType.매도 => "1",
                    OrderType.정정취소 => model.원주문매도수구분 ? "1" : "2",
                    _ => "0"
                };

                string sOrdTp = model.주문종류 switch // 거래구분 (1:지정가, 3:시장가)
                {
                    OrderKind.지정가 => "1",
                    OrderKind.시장가 => "3",
                    _ => "0"
                };
                string sPrice = model.매매구분 switch
                {
                    OrderType.매수 => model.주문종류 == OrderKind.시장가 ? "0" : model.주문가격,
                    OrderType.매도 => model.주문종류 == OrderKind.시장가 ? "0" : model.주문가격,
                    OrderType.정정취소 => model.주문가격,
                    _ => model.주문가격
                };

                StringBuilder stringBuilder = new();

                if (model.매매구분 == OrderType.정정취소)
                {
                    sRQName = $"{model.Title} 정정";
                    stringBuilder.AppendLine($"// {sRQName}");
                    stringBuilder.AppendLine($"int nRet = _axOpenAPI.SendOrderFO(\"{sRQName}\", \"{sScreenNo}\", \"{sAccNo}\", \"{sCode}\", {lOrdKind}, \"{sSlbyTp}\", \"{sOrdTp}\", {nQty}, \"{sPrice}\", \"{sOrgOrderNo}\");");

                    lOrdKind += 1;
                    sRQName = $"{model.Title} 취소";
                    stringBuilder.AppendLine($"// {sRQName}");
                    stringBuilder.AppendLine($"int nRet = _axOpenAPI.SendOrderFO(\"{sRQName}\", \"{sScreenNo}\", \"{sAccNo}\", \"{sCode}\", {lOrdKind}, \"{sSlbyTp}\", \"{sOrdTp}\", {nQty}, \"{sPrice}\", \"{sOrgOrderNo}\");");
                }
                else
                {
                    stringBuilder.AppendLine($"// {sRQName}");
                    stringBuilder.AppendLine($"int nRet = _axOpenAPI.SendOrderFO(\"{sRQName}\", \"{sScreenNo}\", \"{sAccNo}\", \"{sCode}\", {lOrdKind}, \"{sSlbyTp}\", \"{sOrdTp}\", {nQty}, \"{sPrice}\", \"{sOrgOrderNo}\");");
                }

                model.CodeText = stringBuilder.ToString();
            }
        }
        return;
    }

    string ChartContentExtProcedure(CharDataReqViewModel model, string require)
    {
        string result = string.Empty;
        bool b조회 = require.Equals("조 회");
        bool b다음 = require.Equals("다 음");
        if (b조회 || b다음)
        {
            if (_axOpenAPI == null || _axOpenAPI.GetConnectState() == 0)
            {
                OutputLog((int)TAB_LIST_KIND.메시지목록, $"[{model.Title}] : 로그인 후 요청해 주세요");
                return result;
            }

            string trCode = string.Empty;
            if (model.Kind == CharDataReqViewModel.KIND.업종)
            {
                trCode = model.SelectedChartRound switch
                {
                    ChartRound.틱 => "opt20004",
                    ChartRound.분 => "opt20005",
                    ChartRound.일 => "opt20006",
                    ChartRound.주 => "opt20007",
                    ChartRound.월 => "opt20008",
                    _ => throw new NotSupportedException()
                };

                _axOpenAPI.SetInputValue("업종코드", model.Selected종목);
                switch (model.SelectedChartRound)
                {
                    case ChartRound.틱:
                    case ChartRound.분:
                        _axOpenAPI.SetInputValue("틱범위", model.SelectedChartInterval);
                        break;
                    case ChartRound.일:
                    case ChartRound.주:
                    case ChartRound.월:
                        if (b다음) _axOpenAPI.SetInputValue("기준일자", model.조회일자.ToString("yyyyMMdd"));
                        break;
                }
            }
            else if (model.Kind == CharDataReqViewModel.KIND.주식)
            {
                trCode = model.SelectedChartRound switch
                {
                    ChartRound.틱 => "opt10079",
                    ChartRound.분 => "opt10080",
                    ChartRound.일 => "opt10081",
                    ChartRound.주 => "opt10082",
                    ChartRound.월 => "opt10083",
                    _ => throw new NotSupportedException()
                };

                _axOpenAPI.SetInputValue("종목코드", model.Selected종목);
                switch (model.SelectedChartRound)
                {
                    case ChartRound.틱:
                    case ChartRound.분:
                        _axOpenAPI.SetInputValue("틱범위", model.SelectedChartInterval);
                        break;
                    case ChartRound.일:
                        _axOpenAPI.SetInputValue("기준일자", model.조회일자.ToString("yyyyMMdd"));
                        break;
                    case ChartRound.주:
                    case ChartRound.월:
                        _axOpenAPI.SetInputValue("기준일자", model.조회일자.ToString("yyyyMMdd"));
                        _axOpenAPI.SetInputValue("끝일자", "");
                        break;
                }
                _axOpenAPI.SetInputValue("수정주가구분", (model.Is수정주가 == true) ? "1" : "0");
            }
            else if (model.Kind == CharDataReqViewModel.KIND.선물)
            {
                trCode = model.SelectedChartRound switch
                {
                    ChartRound.틱 => "opt50028",
                    ChartRound.분 => "opt50029",
                    ChartRound.일 => "opt50030",
                    ChartRound.주 => "opt50071",
                    ChartRound.월 => "opt50072",
                    _ => throw new NotSupportedException()
                };

                _axOpenAPI.SetInputValue("종목코드", model.Selected종목);
                switch (model.SelectedChartRound)
                {
                    case ChartRound.틱:
                    case ChartRound.분:
                        _axOpenAPI.SetInputValue("시간단위", model.SelectedChartInterval);
                        break;
                    case ChartRound.일:
                    case ChartRound.주:
                    case ChartRound.월:
                        _axOpenAPI.SetInputValue("기준일자", model.조회일자.ToString("yyyyMMdd"));
                        break;
                }
            }
            else if (model.Kind == CharDataReqViewModel.KIND.옵션)
            {
                trCode = model.SelectedChartRound switch
                {
                    ChartRound.틱 => "opt50066",
                    ChartRound.분 => "opt50067",
                    ChartRound.일 => "opt50068",
                    _ => "opt50068"
                };

                _axOpenAPI.SetInputValue("종목코드", model.Selected종목);
                switch (model.SelectedChartRound)
                {
                    case ChartRound.틱:
                    case ChartRound.분:
                        _axOpenAPI.SetInputValue("시간단위", model.SelectedChartInterval);
                        break;
                    case ChartRound.일:
                        _axOpenAPI.SetInputValue("기준일자", model.조회일자.ToString("yyyyMMdd"));
                        break;
                }
            }
            else
                new NotSupportedException();

            string sRqName = $"{model.Title}_{model.SelectedChartRound}";

            DateTime CallTime = DateTime.Now;
            Stopwatch stopwatch = Stopwatch.StartNew();
            int nRet = _axOpenAPI.CommRqData(sRqName, trCode, b다음 ? 2 : 0, _scrNum_CHART_CONTENT);
            stopwatch.Stop();

            result = $"[{CallTime:HH:mm:ss.fff}] : ({stopwatch.Elapsed.TotalMilliseconds})mS, nRet={nRet}\r\n";
        }
        else
        {
            StringBuilder stringBuilder = new();
            stringBuilder.AppendLine($"// {model.Title}");
            string trCode = string.Empty;
            if (model.Kind == CharDataReqViewModel.KIND.업종)
            {
                trCode = model.SelectedChartRound switch
                {
                    ChartRound.틱 => "opt20004",
                    ChartRound.분 => "opt20005",
                    ChartRound.일 => "opt20006",
                    ChartRound.주 => "opt20007",
                    ChartRound.월 => "opt20008",
                    _ => throw new NotSupportedException()
                };

                stringBuilder.AppendLine($"_axOpenAPI.SetInputValue(\"업종코드\", \"{model.Selected종목}\");");
                switch (model.SelectedChartRound)
                {
                    case ChartRound.틱:
                    case ChartRound.분:
                        stringBuilder.AppendLine($"_axOpenAPI.SetInputValue(\"틱범위\", \"{model.SelectedChartInterval}\");");
                        break;
                    case ChartRound.일:
                    case ChartRound.주:
                    case ChartRound.월:
                        stringBuilder.AppendLine($"_axOpenAPI.SetInputValue(\"기준일자\", \"{model.조회일자:yyyyMMdd}\");");
                        break;
                }
            }
            else if (model.Kind == CharDataReqViewModel.KIND.주식)
            {
                trCode = model.SelectedChartRound switch
                {
                    ChartRound.틱 => "opt10079",
                    ChartRound.분 => "opt10080",
                    ChartRound.일 => "opt10081",
                    ChartRound.주 => "opt10082",
                    ChartRound.월 => "opt10083",
                    _ => throw new NotSupportedException()
                };

                stringBuilder.AppendLine($"_axOpenAPI.SetInputValue(\"종목코드\", \"{model.Selected종목}\");");
                switch (model.SelectedChartRound)
                {
                    case ChartRound.틱:
                    case ChartRound.분:
                        stringBuilder.AppendLine($"_axOpenAPI.SetInputValue(\"틱범위\", \"{model.SelectedChartInterval}\");");
                        break;
                    case ChartRound.일:
                        stringBuilder.AppendLine($"_axOpenAPI.SetInputValue(\"기준일자\", \"{model.조회일자:yyyyMMdd}\");");
                        break;
                    case ChartRound.주:
                    case ChartRound.월:
                        stringBuilder.AppendLine($"_axOpenAPI.SetInputValue(\"기준일자\", \"{model.조회일자:yyyyMMdd}\");");
                        stringBuilder.AppendLine($"_axOpenAPI.SetInputValue(\"끝일자\", \"\");");
                        break;
                }
                stringBuilder.AppendLine($"_axOpenAPI.SetInputValue(\"수정주가구분\", \"{((model.Is수정주가 == true) ? "1" : "0")}\");");
            }
            else if (model.Kind == CharDataReqViewModel.KIND.선물)
            {
                trCode = model.SelectedChartRound switch
                {
                    ChartRound.틱 => "opt50028",
                    ChartRound.분 => "opt50029",
                    ChartRound.일 => "opt50030",
                    ChartRound.주 => "opt50071",
                    ChartRound.월 => "opt50072",
                    _ => throw new NotSupportedException()
                };

                stringBuilder.AppendLine($"_axOpenAPI.SetInputValue(\"종목코드\", \"{model.Selected종목}\");");
                switch (model.SelectedChartRound)
                {
                    case ChartRound.틱:
                    case ChartRound.분:
                        stringBuilder.AppendLine($"_axOpenAPI.SetInputValue(\"시간단위\", \"{model.SelectedChartInterval}\");");
                        break;
                    case ChartRound.일:
                    case ChartRound.주:
                    case ChartRound.월:
                        stringBuilder.AppendLine($"_axOpenAPI.SetInputValue(\"기준일자\", \"{model.조회일자:yyyyMMdd}\");");
                        break;
                }
            }
            else if (model.Kind == CharDataReqViewModel.KIND.옵션)
            {
                trCode = model.SelectedChartRound switch
                {
                    ChartRound.틱 => "opt50066",
                    ChartRound.분 => "opt50067",
                    ChartRound.일 => "opt50068",
                    _ => "opt50068"
                };

                stringBuilder.AppendLine($"_axOpenAPI.SetInputValue(\"종목코드\", \"{model.Selected종목}\");");
                switch (model.SelectedChartRound)
                {
                    case ChartRound.틱:
                    case ChartRound.분:
                        stringBuilder.AppendLine($"_axOpenAPI.SetInputValue(\"시간단위\", \"{model.SelectedChartInterval}\");");
                        break;
                    case ChartRound.일:
                        stringBuilder.AppendLine($"_axOpenAPI.SetInputValue(\"기준일자\", \"{model.조회일자:yyyyMMdd}\");");
                        break;
                }
            }

            string sRqName = $"{model.Title}_{model.SelectedChartRound}";
            stringBuilder.AppendLine($"int nRet = _axOpenAPI.CommRqData(\"{sRqName}\", \"{trCode}\", 0, \"{_scrNum_CHART_CONTENT}\");");

            result = stringBuilder.ToString();
        }
        return result;
    }

    void SaveUserContentInfo()
    {
        if (_chartDataReqViewModel_업종 != null)
        {
            var model = _chartDataReqViewModel_업종;
            string section = model.Title;
            _appRegistry.SetValue(section, "종목코드", model.Selected종목);
            _appRegistry.SetValue(section, "시간타입", model.SelectedChartRound);
            _appRegistry.SetValue(section, "분주기", model.SelectedChartInterval_분);
            _appRegistry.SetValue(section, "틱주기", model.SelectedChartInterval_틱);
        }
        if (_chartDataReqViewModel_주식 != null)
        {
            var model = _chartDataReqViewModel_주식;
            string section = model.Title;
            _appRegistry.SetValue(section, "종목코드", model.Selected종목);
            _appRegistry.SetValue(section, "시간타입", model.SelectedChartRound);
            _appRegistry.SetValue(section, "분주기", model.SelectedChartInterval_분);
            _appRegistry.SetValue(section, "틱주기", model.SelectedChartInterval_틱);
        }
        if (_chartDataReqViewModel_선물 != null)
        {
            var model = _chartDataReqViewModel_선물;
            string section = model.Title;
            _appRegistry.SetValue(section, "종목코드", model.Selected종목);
            _appRegistry.SetValue(section, "시간타입", model.SelectedChartRound);
            _appRegistry.SetValue(section, "분주기", model.SelectedChartInterval_분);
            _appRegistry.SetValue(section, "틱주기", model.SelectedChartInterval_틱);
        }
        if (_chartDataReqViewModel_옵션 != null)
        {
            var model = _chartDataReqViewModel_옵션;
            string section = model.Title;
            _appRegistry.SetValue(section, "종목코드", model.Selected종목);
            _appRegistry.SetValue(section, "시간타입", model.SelectedChartRound);
            _appRegistry.SetValue(section, "분주기", model.SelectedChartInterval_분);
            _appRegistry.SetValue(section, "틱주기", model.SelectedChartInterval_틱);
        }
        if (_orderViewModel_주식 != null)
        {
            var model = _orderViewModel_주식;
            string section = model.Title;
            _appRegistry.SetValue(section, "종목코드", model.종목코드);
        }
        if (_orderViewModel_선물옵션 != null)
        {
            var model = _orderViewModel_선물옵션;
            string section = model.Title;
            _appRegistry.SetValue(section, "종목코드", model.종목코드);
        }
    }
}
