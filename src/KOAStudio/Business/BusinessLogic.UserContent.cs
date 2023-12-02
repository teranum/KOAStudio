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
                    SelectedChartInterval_틱 = _appRegistry.GetValue(require, "틱주기", "100")
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
                    SelectedChartInterval_틱 = _appRegistry.GetValue(require, "틱주기", "300")
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
    }
}
