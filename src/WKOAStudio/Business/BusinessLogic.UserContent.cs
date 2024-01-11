using KOAStudio.Core.Models;
using KOAStudio.Core.ViewModels;
using KOAStudio.Core.Views;
using System.Diagnostics;
using System.Text;

namespace WKOAStudio.Business;

internal sealed partial class BusinessLogic
{
    private CharDataReqViewModel? _chartDataReqViewModel_선물;
    private OrderViewModel? _orderViewModel_선물옵션;
    void ShowUserContent(string require)
    {
        if (require.Equals("해외선물옵션차트요청"))
        {
            var model = _chartDataReqViewModel_선물;
            if (model == null)
            {
                _chartDataReqViewModel_선물 = model = new CharDataReqViewModel(CharDataReqViewModel.KIND.선물, require)
                {
                    ExtProcedure = ChartContentExtProcedure,
                    Selected종목 = _appRegistry.GetValue(require, "종목코드", "NQZ23"),
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
        else if (require.Equals("해외선물옵션주문요청"))
        {
            var model = _orderViewModel_선물옵션;
            if (model == null)
            {
                _orderViewModel_선물옵션 = model = new OrderViewModel(require, OrderExtCallProc)
                {
                    종목코드 = _appRegistry.GetValue(require, "종목코드", "NQZ23"),
                };
            }
            if ((model.계좌리스트 == null || !model.계좌리스트.Any()) && _axOpenAPI != null && _axOpenAPI.GetConnectState() == 1)
            {
                model.계좌리스트 = _axOpenAPI.GetLoginInfo("ACCNO").Split(';', StringSplitOptions.RemoveEmptyEntries);
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

            if (!_axOpenAPI.GetCommonFunc("GetAcnoPswdState", string.Empty).Equals("Y"))
            {
                OutputLog((int)TAB_LIST_KIND.메시지목록, $"[{model.Title} {requre}] : 비밀번호 설정 후 요청해 주세요");
                return;
            }

            if (b조회)
            {
                if (model.SelectedTabIndex == 0) // 미체결
                {
                    string sTrCode = "opw30024";
                    _axOpenAPI.SetInputValue("계좌번호", model.Selected계좌);
                    _axOpenAPI.SetInputValue("비밀번호", string.Empty);
                    _axOpenAPI.SetInputValue("비밀번호입력매체", "00");
                    _axOpenAPI.SetInputValue("종목코드", string.Empty);
                    _axOpenAPI.SetInputValue("통화코드", string.Empty);
                    _axOpenAPI.SetInputValue("매도수구분", string.Empty);

                    int nRet = _axOpenAPI.CommRqData(model.Title + " 미체결조회", sTrCode, "", _scrNum_ORDER_CONTENT);
                    if (nRet < 0)
                    {
                        OutputLog((int)TAB_LIST_KIND.메시지목록, $"[{model.Title}] : 미체결 요청오류");
                        return;
                    }
                }
                else if (model.SelectedTabIndex == 1) //잔고"
                {
                    string sTrCode = "opw30026";
                    _axOpenAPI.SetInputValue("계좌번호", model.Selected계좌);
                    _axOpenAPI.SetInputValue("비밀번호", string.Empty);
                    _axOpenAPI.SetInputValue("비밀번호입력매체", "00");
                    _axOpenAPI.SetInputValue("통화코드", string.Empty);
                    _axOpenAPI.SetInputValue("수수료적용여부", "N");
                    int nRet = _axOpenAPI.CommRqData(model.Title + " 잔고조회", sTrCode, "", _scrNum_ORDER_CONTENT);

                    if (nRet < 0)
                    {
                        OutputLog((int)TAB_LIST_KIND.메시지목록, $"[{model.Title}] : 잔고 요청오류");
                        return;
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
                // LONG SendOrder( BSTR sRQName, BSTR sScreenNo, BSTR sAccNo, LONG nOrderType, BSTR sCode, LONG nQty, BSTR sPrice, BSTR sStop, BSTR sHogaGb, BSTR sOrgOrderNo) 
                string sRQName = model.Title + " " + ((model.매매구분 != OrderType.정정취소) ? $"{model.매매구분}" : (b정정주문 ? "정정" : "취소"));
                string sScreenNo = _scrNum_ORDER_CONTENT;
                string sAccNo = model.Selected계좌;
                int nOrderType = model.매매구분 switch //  (1:신규매도, 2:신규매수 3:매도취소, 4:매수취소, 5:매도정정, 6:매수정정)
                {
                    OrderType.매수 => 2,
                    OrderType.매도 => 1,
                    OrderType.정정취소 => model.원주문매도수구분 ? 5 : 6,
                    _ => 0
                };
                // 취소경우 -= 2;
                if (b취소주문) nOrderType -= 2;
                string sCode = model.종목코드;
                int nQty = model.주문수량;
                string sPrice = model.매매구분 switch // 거래구분 (1:시장가, 2:지정가, 3:STOP, 4:STOP LIMIT)
                {
                    OrderType.매수 => model.주문종류 == OrderKind.시장가 ? "0" : model.주문가격,
                    OrderType.매도 => model.주문종류 == OrderKind.시장가 ? "0" : model.주문가격,
                    OrderType.정정취소 => model.주문가격,
                    _ => model.주문가격
                };
                string sStop = "0";
                string sHogaGb = model.매매구분 switch // 거래구분 (1:시장가, 2:지정가, 3:STOP, 4:STOP LIMIT)
                {
                    OrderType.매수 => model.주문종류 == OrderKind.시장가 ? "1" : "2",
                    OrderType.매도 => model.주문종류 == OrderKind.시장가 ? "1" : "2",
                    OrderType.정정취소 => "2",
                    _ => "2"
                };
                string sOrgOrderNo = model.매매구분 switch // 원주문번호
                {
                    OrderType.매수 => string.Empty,
                    OrderType.매도 => string.Empty,
                    OrderType.정정취소 => model.주문번호,
                    _ => string.Empty
                };

                DateTime CallTime = DateTime.Now;
                Stopwatch stopwatch = Stopwatch.StartNew();
                int nRet = _axOpenAPI.SendOrder(sRQName, sScreenNo, sAccNo, nOrderType, sCode, nQty, sPrice, sStop, sHogaGb, sOrgOrderNo);
                stopwatch.Stop();
                model.CodeText += $"[{CallTime:HH:mm:ss.fff}] : ({stopwatch.Elapsed.TotalMilliseconds})mS, nRet={nRet}\r\n";

            }
        }
        else
        {
            // LONG SendOrder( BSTR sRQName, BSTR sScreenNo, BSTR sAccNo, LONG nOrderType, BSTR sCode, LONG nQty, BSTR sPrice, BSTR sStop, BSTR sHogaGb, BSTR sOrgOrderNo) 
            string sRQName = model.Title + $" {model.매매구분}";
            string sScreenNo = _scrNum_ORDER_CONTENT;
            string sAccNo = model.Selected계좌;
            int nOrderType = model.매매구분 switch //  (1:신규매도, 2:신규매수 3:매도취소, 4:매수취소, 5:매도정정, 6:매수정정)
            {
                OrderType.매수 => 2,
                OrderType.매도 => 1,
                OrderType.정정취소 => model.원주문매도수구분 ? 5 : 6,
                _ => 0
            };
            string sCode = model.종목코드;
            int nQty = model.주문수량;
            string sPrice = model.매매구분 switch // 거래구분 (1:시장가, 2:지정가, 3:STOP, 4:STOP LIMIT)
            {
                OrderType.매수 => model.주문종류 == OrderKind.시장가 ? "0" : model.주문가격,
                OrderType.매도 => model.주문종류 == OrderKind.시장가 ? "0" : model.주문가격,
                OrderType.정정취소 => model.주문가격,
                _ => model.주문가격
            };
            string sStop = "0";
            string sHogaGb = model.매매구분 switch // 거래구분 (1:시장가, 2:지정가, 3:STOP, 4:STOP LIMIT)
            {
                OrderType.매수 => model.주문종류 == OrderKind.시장가 ? "1" : "2",
                OrderType.매도 => model.주문종류 == OrderKind.시장가 ? "1" : "2",
                OrderType.정정취소 => "2",
                _ => "2"
            };
            string sOrgOrderNo = model.매매구분 switch // 원주문번호
            {
                OrderType.매수 => string.Empty,
                OrderType.매도 => string.Empty,
                OrderType.정정취소 => model.주문번호,
                _ => string.Empty
            };

            StringBuilder stringBuilder = new();

            if (model.매매구분 == OrderType.정정취소)
            {
                sRQName = $"{model.Title} 정정";
                stringBuilder.AppendLine($"// {sRQName}");
                stringBuilder.AppendLine($"int nRet = _axOpenAPI.SendOrder(\"{sRQName}\", \"{sScreenNo}\", \"{sAccNo}\", {nOrderType}, \"{sCode}\", {nQty}, \"{sPrice}\", \"{sStop}\", \"{sHogaGb}\", \"{sOrgOrderNo}\");");

                nOrderType -= 2;
                sRQName = $"{model.Title} 정정";
                stringBuilder.AppendLine($"// {sRQName}");
                stringBuilder.AppendLine($"int nRet = _axOpenAPI.SendOrder(\"{sRQName}\", \"{sScreenNo}\", \"{sAccNo}\", {nOrderType}, \"{sCode}\", {nQty}, \"{sPrice}\", \"{sStop}\", \"{sHogaGb}\", \"{sOrgOrderNo}\");");
            }
            else
            {
                stringBuilder.AppendLine($"// {model.Title} {model.매매구분}");
                stringBuilder.AppendLine($"int nRet = _axOpenAPI.SendOrder(\"{sRQName}\", \"{sScreenNo}\", \"{sAccNo}\", {nOrderType}, \"{sCode}\", {nQty}, \"{sPrice}\", \"{sStop}\", \"{sHogaGb}\", \"{sOrgOrderNo}\");");
            }

            model.CodeText = stringBuilder.ToString();
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
            if (model.Kind == CharDataReqViewModel.KIND.선물)
            {
                trCode = model.SelectedChartRound switch
                {
                    ChartRound.틱 => "opc10001",
                    ChartRound.분 => "opc10002",
                    ChartRound.일 => "opc10003",
                    ChartRound.주 => "opc10004",
                    ChartRound.월 => "opc10005",
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
                        _axOpenAPI.SetInputValue("조회일자", model.조회일자.ToString("yyyyMMdd"));
                        break;
                }
            }
            else
                new NotSupportedException();

            string sRqName = $"{model.Title}_{model.SelectedChartRound}";

            DateTime CallTime = DateTime.Now;
            Stopwatch stopwatch = Stopwatch.StartNew();
            int nRet = _axOpenAPI.CommRqData(sRqName, trCode, b다음 ? model.NextText : "", _scrNum_CHART_CONTENT);
            stopwatch.Stop();

            result = $"[{CallTime:HH:mm:ss.fff}] : ({stopwatch.Elapsed.TotalMilliseconds})mS, nRet={nRet}\r\n";
        }
        else
        {
            StringBuilder stringBuilder = new();
            stringBuilder.AppendLine($"// {model.Title}");
            string trCode = string.Empty;
            if (model.Kind == CharDataReqViewModel.KIND.선물)
            {
                trCode = model.SelectedChartRound switch
                {
                    ChartRound.틱 => "opc10001",
                    ChartRound.분 => "opc10002",
                    ChartRound.일 => "opc10003",
                    ChartRound.주 => "opc10004",
                    ChartRound.월 => "opc10005",
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
                        stringBuilder.AppendLine($"_axOpenAPI.SetInputValue(\"조회일자\", \"{model.조회일자:yyyyMMdd}\");");
                        break;
                }
            }

            string sRqName = $"{model.Title}_{model.SelectedChartRound}";
            stringBuilder.AppendLine($"int nRet = _axOpenAPI.CommRqData(\"{sRqName}\", \"{trCode}\", \"\", \"{_scrNum_CHART_CONTENT}\");");

            result = stringBuilder.ToString();
        }
        return result;
    }

    void SaveUserContentInfo()
    {
        if (_chartDataReqViewModel_선물 != null)
        {
            var model = _chartDataReqViewModel_선물;
            string section = model.Title;
            _appRegistry.SetValue(section, "종목코드", model.Selected종목);
            _appRegistry.SetValue(section, "시간타입", model.SelectedChartRound);
            _appRegistry.SetValue(section, "분주기", model.SelectedChartInterval_분);
            _appRegistry.SetValue(section, "틱주기", model.SelectedChartInterval_틱);
        }
        if (_orderViewModel_선물옵션 != null)
        {
            var model = _orderViewModel_선물옵션;
            string section = model.Title;
            _appRegistry.SetValue(section, "종목코드", model.종목코드);
        }
    }
}
