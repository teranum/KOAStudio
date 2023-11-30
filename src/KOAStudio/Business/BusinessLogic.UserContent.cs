using KOAStudio.Core.Models;
using KOAStudio.Core.ViewModels;
using KOAStudio.Core.Views;
using System.Diagnostics;
using System.Text;

namespace KOAStudio.Business;
internal sealed partial class BusinessLogic
{
    private ChartReqViewModel? _chartReqViewModel_업종;
    private ChartReqViewModel? _chartReqViewModel_주식;
    private ChartReqViewModel? _chartReqViewModel_선물;
    private ChartReqViewModel? _chartReqViewModel_옵션;
    void ShowUserContent(string require)
    {
        if (require.Equals("업종차트요청"))
        {
            _chartReqViewModel_업종 ??= new ChartReqViewModel(ChartReqViewModel.KIND.업종, require)
            {
                ExtProcedure = ChartContentExtProcedure,
                Selected종목 = _appRegistry.GetValue(require, "종목코드", "001"),
            };
            _chartReqViewModel_업종.NextEnabled = false;
            _chartReqViewModel_업종.EnableUpdateCodeText = true;
            _chartReqViewModel_업종.UpdateCodeText();
            SetUserContent(new ChartReqView(_chartReqViewModel_업종));

        }
        else if (require.Equals("주식차트요청"))
        {
            _chartReqViewModel_주식 ??= new ChartReqViewModel(ChartReqViewModel.KIND.주식, require)
            {
                ExtProcedure = ChartContentExtProcedure,
                Is수정주가 = true,
                Selected종목 = _appRegistry.GetValue(require, "종목코드", "005930"),
            };
            _chartReqViewModel_주식.NextEnabled = false;
            _chartReqViewModel_주식.EnableUpdateCodeText = true;
            _chartReqViewModel_주식.UpdateCodeText();
            SetUserContent(new ChartReqView(_chartReqViewModel_주식));

        }
        else if (require.Equals("선물차트요청"))
        {
            _chartReqViewModel_선물 ??= new ChartReqViewModel(ChartReqViewModel.KIND.선물, require)
            {
                ExtProcedure = ChartContentExtProcedure,
                Selected종목 = _appRegistry.GetValue(require, "종목코드", "10100000"),
            };
            _chartReqViewModel_선물.NextEnabled = false;
            _chartReqViewModel_선물.EnableUpdateCodeText = true;
            _chartReqViewModel_선물.UpdateCodeText();
            SetUserContent(new ChartReqView(_chartReqViewModel_선물));
        }
        else if (require.Equals("옵션차트요청"))
        {
            _chartReqViewModel_옵션 ??= new ChartReqViewModel(ChartReqViewModel.KIND.옵션, require)
            {
                ExtProcedure = ChartContentExtProcedure,
                Selected종목 = _appRegistry.GetValue(require, "종목코드", "201TC340"),
            };
            _chartReqViewModel_옵션.NextEnabled = false;
            _chartReqViewModel_옵션.EnableUpdateCodeText = true;
            _chartReqViewModel_옵션.UpdateCodeText();
            SetUserContent(new ChartReqView(_chartReqViewModel_옵션));
        }
    }

    string ChartContentExtProcedure(ChartReqViewModel model, string require)
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
            if (model.Kind == ChartReqViewModel.KIND.업종)
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
            else if (model.Kind == ChartReqViewModel.KIND.주식)
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
            else if (model.Kind == ChartReqViewModel.KIND.선물)
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
            else if (model.Kind == ChartReqViewModel.KIND.옵션)
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

            result = $"[{CallTime:HH:mm:ss.fff}] : ({stopwatch.Elapsed.TotalMilliseconds * 1000})uS, nRet={nRet}\r\n";
        }
        else
        {
            StringBuilder stringBuilder = new();
            stringBuilder.AppendLine($"// {model.Title}");
            string trCode = string.Empty;
            if (model.Kind == ChartReqViewModel.KIND.업종)
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
            else if (model.Kind == ChartReqViewModel.KIND.주식)
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
            else if (model.Kind == ChartReqViewModel.KIND.선물)
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
            else if (model.Kind == ChartReqViewModel.KIND.옵션)
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
        if (_chartReqViewModel_업종 != null)
        {
            _appRegistry.SetValue(_chartReqViewModel_업종.Title, "종목코드", _chartReqViewModel_업종.Selected종목);
        }
        if (_chartReqViewModel_주식 != null)
        {
            _appRegistry.SetValue(_chartReqViewModel_주식.Title, "종목코드", _chartReqViewModel_주식.Selected종목);
        }
        if (_chartReqViewModel_선물 != null)
        {
            _appRegistry.SetValue(_chartReqViewModel_선물.Title, "종목코드", _chartReqViewModel_선물.Selected종목);
        }
        if (_chartReqViewModel_옵션 != null)
        {
            _appRegistry.SetValue(_chartReqViewModel_옵션.Title, "종목코드", _chartReqViewModel_옵션.Selected종목);
        }
    }
}
