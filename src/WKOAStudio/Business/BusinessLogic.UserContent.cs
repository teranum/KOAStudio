using KOAStudio.Core.Models;
using KOAStudio.Core.ViewModels;
using KOAStudio.Core.Views;
using System.Diagnostics;
using System.Text;

namespace WKOAStudio.Business;

internal sealed partial class BusinessLogic
{
    private CharDataReqViewModel? _chartDataReqViewModel_선물;
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
                    SelectedChartInterval_틱 = _appRegistry.GetValue(require, "틱주기", "100")
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
                _orderViewModel_선물옵션 = model = new OrderViewModel(require, [], OrderExtCallProc)
                {
                };
            }
            model.EnableUpdateCodeText = true;
            model.UpdateCodeText();
            SetUserContent(new OrderView(model));
        }
    }

    private string OrderExtCallProc(OrderViewModel model, string action)
    {
        string result = string.Empty;
        return result;
    }

    OrderViewModel? _orderViewModel_선물옵션;

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
    }
}
