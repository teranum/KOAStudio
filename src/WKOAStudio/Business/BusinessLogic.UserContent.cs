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
        switch (require)
        {
            case "해외선물옵션차트":
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
                        _ = Enum.TryParse(_appRegistry.GetValue(require, "시간타입", string.Empty), out chartRound);
                        model.SelectedChartRound = chartRound;
                    }
                    model.NextEnabled = false;
                    model.EnableUpdateCodeText = true;
                    model.UpdateCodeText();
                    SetUserContent(new CharDataReqView(model));
                }
                break;
            case "해외선물옵션주문":
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
                break;
            default:
                return;
        }
    }

    private async Task OrderExtCallProc(OrderViewModel model, string require)
    {

        bool b매수주문 = require.Equals("매수주문");
        bool b매도주문 = require.Equals("매도주문");
        bool b정정주문 = require.Equals("정정주문");
        bool b취소주문 = require.Equals("취소주문");
        bool b조회 = require.Equals("조 회");

        if (b조회)
        {
            if (_axOpenAPI == null || _axOpenAPI.GetConnectState() == 0)
            {
                OutputLog((int)TAB_LIST_KIND.메시지목록, $"[{model.Title} {require}] : 로그인 후 요청해 주세요");
                return;
            }

            if (model.SelectedTabIndex == 0) // 미체결
            {
                model.MicheItems.Clear();
                var inputs = new Dictionary<string, string>()
                    {
                        { "계좌번호", model.Selected계좌 },
                        { "비밀번호", string.Empty },
                        { "비밀번호입력매체", "00" },
                        { "종목코드", string.Empty },
                        { "통화코드", string.Empty },
                        { "매도수구분", string.Empty },
                    };
                List<string> multies =
                [
                    "종목코드", "종목명", "주문번호", "원주문번호", "매도수구분", "주문수량", "미체결수량", "주문표시가격", "통화코드", "주문시각",
                    ];
                var response = await _axOpenAPI.RequestTrAsync("opw30024", inputs, [], multies);
                if (response.nErrCode != 0)
                {
                    OutputLog((int)TAB_LIST_KIND.메시지목록, $"[{model.Title}] : 미체결 요청오류, [{response.nErrCode}] {response.rsp_msg}");
                    return;
                }

                foreach (var data in response.OutputMultiDatas)
                {
                    string 종목코드 = data[0];
                    string 종목명 = data[1];
                    string 주문번호 = data[2];
                    string 원주문번호 = data[3];
                    bool 매도수구분 = data[4].Equals("1");
                    _ = int.TryParse(data[5], out int 주문수량);
                    _ = int.TryParse(data[6], out int 미체결수량);
                    _ = double.TryParse(data[7], out double 주문가격);
                    string 통화코드 = data[8];
                    string 주문시각 = data[9];
                    model.MicheItems.Add(new MicheItem(
                        종목코드, 종목명, 주문번호, 원주문번호, 매도수구분, 주문수량, 미체결수량, 주문가격, 통화코드, 주문시각
                        ));
                }
            }
            else if (model.SelectedTabIndex == 1) //잔고"
            {
                model.JangoItems.Clear();
                var inputs = new Dictionary<string, string>()
                    {
                        { "계좌번호", model.Selected계좌 },
                        { "비밀번호", string.Empty },
                        { "비밀번호입력매체", "00" },
                        { "통화코드", string.Empty },
                        { "수수료적용여부", "N" },
                    };
                List<string> multies =
                [
                    "종목코드", "종목명", "매도수구분", "수량", "평균단가", "현재가격", "평가손익", "통화코드",
                    ];
                var response = await _axOpenAPI.RequestTrAsync("opw30026", inputs, [], multies);
                if (response.nErrCode != 0)
                {
                    OutputLog((int)TAB_LIST_KIND.메시지목록, $"[{model.Title}] : 잔고 요청오류, [{response.nErrCode}] {response.rsp_msg}");
                    return;
                }

                foreach (var data in response.OutputMultiDatas)
                {
                    string 종목코드 = data[0];
                    string 종목명 = data[1];
                    bool 매도수구분 = data[2].Equals("1");
                    _ = int.TryParse(data[3], out int 수량);
                    _ = double.TryParse(data[4], out double 평균단가);
                    _ = double.TryParse(data[5], out double 현재가격);
                    _ = double.TryParse(data[6], out double 평가손익);
                    평가손익 /= 100;
                    string 통화코드 = data[7];
                    model.JangoItems.Add(new JangoItem(
                        종목코드, 종목명, 매도수구분, 수량, 평균단가, 현재가격, 평가손익, 통화코드
                        ));
                }
            }

            return;
        }

        string sRQName = model.Title + $" {model.매매구분}";
        string sScreenNo = _scrNum_ORDER_CONTENT;
        string sAccNo = model.Selected계좌;
        int nOrderType = model.매매구분 switch //  (1:신규매도, 2:신규매수 3:매도취소, 4:매수취소, 5:매도정정, 6:매수정정)
        {
            OrderType.매수 => 2,
            OrderType.매도 => 1,
            OrderType.정정취소 => model.원주문매도수구분 ? 5 : 6,
            _ => 0,
        };
        string sCode = model.종목코드;
        int nQty = model.주문수량;
        string sPrice = model.매매구분 switch // 거래구분 (1:시장가, 2:지정가, 3:STOP, 4:STOP LIMIT)
        {
            OrderType.매수 => model.주문종류 == OrderKind.시장가 ? "0" : model.주문가격,
            OrderType.매도 => model.주문종류 == OrderKind.시장가 ? "0" : model.주문가격,
            OrderType.정정취소 => model.주문가격,
            _ => model.주문가격,
        };
        string sStop = "0";
        string sHogaGb = model.매매구분 switch // 거래구분 (1:시장가, 2:지정가, 3:STOP, 4:STOP LIMIT)
        {
            OrderType.매수 => model.주문종류 == OrderKind.시장가 ? "1" : "2",
            OrderType.매도 => model.주문종류 == OrderKind.시장가 ? "1" : "2",
            OrderType.정정취소 => "2",
            _ => "2",
        };
        string sOrgOrderNo = model.매매구분 switch // 원주문번호
        {
            OrderType.매수 => string.Empty,
            OrderType.매도 => string.Empty,
            OrderType.정정취소 => model.주문번호,
            _ => string.Empty,
        };

        StringBuilder stringBuilder = new();

        if (model.매매구분 == OrderType.정정취소)
        {
            sRQName = $"{model.Title} 정정";
            stringBuilder.AppendLine($"// {sRQName}");
            stringBuilder.AppendLine($"(int nRet, string sMsg) = await _axOpenAPI.SendOrderAsync(\"{sRQName}\",");
            stringBuilder.AppendLine($"    \"{sScreenNo}\", \"{sAccNo}\", {nOrderType}, \"{sCode}\", {nQty}, \"{sPrice}\", \"{sStop}\", \"{sHogaGb}\", \"{sOrgOrderNo}\");");

            nOrderType -= 2;
            sRQName = $"{model.Title} 취소";
        }
        stringBuilder.AppendLine($"(int nRet, string sMsg) = await _axOpenAPI.SendOrderAsync(\"{sRQName}\",");
        stringBuilder.AppendLine($"    \"{sScreenNo}\", \"{sAccNo}\", {nOrderType}, \"{sCode}\", {nQty}, \"{sPrice}\", \"{sStop}\", \"{sHogaGb}\", \"{sOrgOrderNo}\");");
        stringBuilder.AppendLine($"Output($\"nRet={{nRet}}, sMsg={{sMsg}}\");");

        model.CodeText = stringBuilder.ToString();

        if (b매수주문 || b매도주문 || b정정주문 || b취소주문)
        {
            if (_axOpenAPI == null || _axOpenAPI.GetConnectState() == 0)
            {
                OutputLog((int)TAB_LIST_KIND.메시지목록, $"[{model.Title} {require}] : 로그인 후 요청해 주세요");
                return;
            }

            if (!_axOpenAPI.GetCommonFunc("GetAcnoPswdState", string.Empty).Equals("Y"))
            {
                OutputLog((int)TAB_LIST_KIND.메시지목록, $"[{model.Title} {require}] : 비밀번호 설정 후 요청해 주세요");
                return;
            }

            if (!model.주문확인생략)
            {
                var msgWindow = new MessageOkCancel($"{model.종목코드} {model.주문수량}계약\r\n {require} 하시겠습니까?", $"{require}");
                if (msgWindow.ShowDialog() != true)
                {
                    return;
                }
            }
            // 정정경우 += 2;, 표시에서 마감상태가 취소로 되어있음
            if (b정정주문) nOrderType += 2;

            DateTime CallTime = DateTime.Now;
            Stopwatch stopwatch = Stopwatch.StartNew();
            (int nRet, string sMsg) = await _axOpenAPI.SendOrderAsync(sRQName, sScreenNo, sAccNo, nOrderType, sCode, nQty, sPrice, sStop, sHogaGb, sOrgOrderNo);
            stopwatch.Stop();
            model.CodeText += $"[{CallTime:HH:mm:ss.fff}] : ({stopwatch.Elapsed.TotalMilliseconds})mS, nRet={nRet}, sMsg={sMsg}\r\n";
        }
        return;
    }


    private async Task ChartContentExtProcedure(CharDataReqViewModel model, string require)
    {
        bool b조회 = require.Equals("조 회");
        bool b다음 = require.Equals("다 음");
        string trCode;
        var inputs = new Dictionary<string, string>();
        List<string> multies = ["체결시간", "시가", "고가", "저가", "현재가", "거래량"];
        if (model.Kind == CharDataReqViewModel.KIND.선물)
        {
            trCode = model.SelectedChartRound switch
            {
                ChartRound.틱 => "opc10001",
                ChartRound.분 => "opc10002",
                ChartRound.일 => "opc10003",
                ChartRound.주 => "opc10004",
                ChartRound.월 => "opc10005",
                _ => throw new NotSupportedException(),
            };

            inputs.Add("종목코드", model.Selected종목);
            switch (model.SelectedChartRound)
            {
                case ChartRound.틱:
                case ChartRound.분:
                    inputs.Add("시간단위", model.SelectedChartInterval);
                    break;
                case ChartRound.일:
                case ChartRound.주:
                case ChartRound.월:
                    inputs.Add("조회일자", model.조회일자.ToString("yyyyMMdd"));
                    multies[0] = "일자";
                    multies[5] = "누적거래량";
                    break;
            }
        }
        else
            throw new NotSupportedException();

        StringBuilder sb = new();
        sb.AppendLine($"// {model.Title}");
        sb.AppendLine($"var response = await _axOpenAPI.RequestTrAsync(\"{trCode}\"");
        sb.Append("    , [").AppendJoin(", ", inputs.Select(x => $"(\"{x.Key}\", \"{x.Value}\")")).AppendLine("]");
        sb.AppendLine("    , []");
        sb.Append("    , [").AppendJoin(", ", multies.Select(x => $"\"{x}\"")).Append(']');
        if (b다음)
        {
            sb.AppendLine();
            sb.Append($"    , \"{model.NextText}\"");
        }
        sb.AppendLine(");");
        sb.AppendLine("if (response.nErrCode != 0)");
        sb.AppendLine("{");
        sb.AppendLine($"    Output($\"{model.Title} 요청실패: {{response.rsp_msg}}\");");
        sb.AppendLine("    return;");
        sb.AppendLine("}");
        sb.AppendLine("// 데이터 처리");
        sb.AppendLine($"Output(response.OutputMultiDatas);");

        model.CodeText = sb.ToString();
        if (b조회 || b다음)
        {
            if (_axOpenAPI == null || _axOpenAPI.GetConnectState() == 0)
            {
                OutputLog((int)TAB_LIST_KIND.메시지목록, $"[{model.Title}] : 로그인 후 요청해 주세요");
                return;
            }

            var requestTime = DateTime.Now;
            Stopwatch stopwatch = Stopwatch.StartNew();
            var response = await _axOpenAPI.RequestTrAsync(trCode, inputs, [], multies, b다음 ? model.NextText : string.Empty);
            stopwatch.Stop();
            model.RequestTime = requestTime;
            model.Elapsed_ms = stopwatch.Elapsed.TotalMilliseconds;
            var datas = response.OutputMultiDatas;
            sb.Clear();
            if (b조회)
                sb.AppendJoin(", ", response.InMultiFields).AppendLine();
            foreach (var data in datas)
                sb.AppendJoin(", ", data).AppendLine();
            model.ResultText = sb.ToString();
            model.ReceivedDataCount = datas.Count;
            model.NextText = response.cont_key;
            model.NextEnabled = !string.IsNullOrEmpty(response.cont_key);
        }
        return;
    }

    private void SaveUserContentInfo()
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
