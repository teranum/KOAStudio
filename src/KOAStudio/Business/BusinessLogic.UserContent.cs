using KOAStudio.Core.ViewModels;
using KOAStudio.Core.Views;
using System.Text;

namespace KOAStudio.Business;

internal sealed partial class BusinessLogic
{
    private ChartReqViewModel? _chartReqViewModel_주식;
    private ChartReqViewModel? _chartReqViewModel_선물;
    void ShowUserContent(string require)
    {
        if (require.Equals("주식차트요청"))
        {
            _chartReqViewModel_주식 ??= new ChartReqViewModel(ChartReqViewModel.KIND.주식 , require)
            {
                ExtProcedure = ChartContentExtProcedure,
                EnableUpdateCodeText = true,
                Selected종목 = _appRegistry.GetValue(require, "종목코드", "005930"),
            };
            _chartReqViewModel_주식.EnableUpdateCodeText = true;
            SetUserContent(new ChartReqView(_chartReqViewModel_주식));

        }
        else if (require.Equals("선물차트요청"))
        {
            _chartReqViewModel_선물 ??= new ChartReqViewModel(ChartReqViewModel.KIND.선물, require)
            {
                ExtProcedure = ChartContentExtProcedure,
                EnableUpdateCodeText = true,
                Selected종목 = _appRegistry.GetValue(require, "종목코드", string.Empty),
            };
            _chartReqViewModel_선물.EnableUpdateCodeText = true;
            SetUserContent(new ChartReqView(_chartReqViewModel_선물));
        }
    }

    string ChartContentExtProcedure(ChartReqViewModel model, string require)
    {
        string result = string.Empty;

        if (require.Equals("조 회"))
        {
            if (_axOpenAPI == null || _axOpenAPI.GetConnectState() == 0)
            {
                OutputLog((int)TAB_LIST_KIND.메시지목록, $"[{model.Title}] : 로그인 후 요청해 주세요");
            }
        }
        else
        {
            StringBuilder stringBuilder = new StringBuilder();
            stringBuilder.AppendLine($"// {model.Title}");
            string trCode = string.Empty;
            if (model.Kind == ChartReqViewModel.KIND.업종)
            {
                trCode = model.SelectedChartRound switch
                {
                    Core.Models.ChartRound.틱 => "opt20004",
                    Core.Models.ChartRound.분 => "opt20005",
                    Core.Models.ChartRound.일 => "opt20006",
                    Core.Models.ChartRound.주 => "opt20007",
                    Core.Models.ChartRound.월 => "opt20008",
                    _ => throw new NotSupportedException()
                };

                stringBuilder.AppendLine($"SetInputValue(\"업종코드\", \"{model.Selected종목}\");");
                switch (model.SelectedChartRound)
                {
                    case Core.Models.ChartRound.틱:
                    case Core.Models.ChartRound.분:
                        stringBuilder.AppendLine($"SetInputValue(\"틱범위\", \"{model.SelectedChartInterval}\");");
                        break;
                    case Core.Models.ChartRound.일:
                    case Core.Models.ChartRound.주:
                    case Core.Models.ChartRound.월:
                        stringBuilder.AppendLine($"SetInputValue(\"기준일자\", \"\");");
                        break;
                }
            } 
            else if (model.Kind == ChartReqViewModel.KIND.주식)
            {
                trCode = model.SelectedChartRound switch
                {
                    Core.Models.ChartRound.틱 => "opt10079",
                    Core.Models.ChartRound.분 => "opt10080",
                    Core.Models.ChartRound.일 => "opt10081",
                    Core.Models.ChartRound.주 => "opt10082",
                    Core.Models.ChartRound.월 => "opt10083",
                    _ => throw new NotSupportedException()
                };

                stringBuilder.AppendLine($"SetInputValue(\"종목코드\", \"{model.Selected종목}\");");
                switch (model.SelectedChartRound)
                {
                    case Core.Models.ChartRound.틱:
                    case Core.Models.ChartRound.분:
                        stringBuilder.AppendLine($"SetInputValue(\"틱범위\", \"{model.SelectedChartInterval}\");");
                        break;
                    case Core.Models.ChartRound.일:
                        stringBuilder.AppendLine($"SetInputValue(\"기준일자\", \"\");");
                        break;
                    case Core.Models.ChartRound.주:
                        stringBuilder.AppendLine($"SetInputValue(\"기준일자\", \"\");");
                        stringBuilder.AppendLine($"SetInputValue(\"끝일자\", \"\");");
                        break;
                    case Core.Models.ChartRound.월:
                        stringBuilder.AppendLine($"SetInputValue(\"기준일자\", \"\");");
                        stringBuilder.AppendLine($"SetInputValue(\"끝일자\", \"\");");
                        break;
                }
                stringBuilder.AppendLine($"SetInputValue(\"수정주가구분\", \"1\");");
            }
            else if (model.Kind == ChartReqViewModel.KIND.선물)
            {
                trCode = model.SelectedChartRound switch
                {
                    Core.Models.ChartRound.틱 => "opt50028",
                    Core.Models.ChartRound.분 => "opt50029",
                    Core.Models.ChartRound.일 => "opt50030",
                    Core.Models.ChartRound.주 => "opt50071",
                    Core.Models.ChartRound.월 => "opt50072",
                    _ => throw new NotSupportedException()
                };

                stringBuilder.AppendLine($"SetInputValue(\"종목코드\", \"{model.Selected종목}\");");
                switch (model.SelectedChartRound)
                {
                    case Core.Models.ChartRound.틱:
                    case Core.Models.ChartRound.분:
                        stringBuilder.AppendLine($"SetInputValue(\"시간단위\", \"{model.SelectedChartInterval}\");");
                        break;
                    case Core.Models.ChartRound.일:
                    case Core.Models.ChartRound.주:
                    case Core.Models.ChartRound.월:
                        stringBuilder.AppendLine($"SetInputValue(\"기준일자\", \"\");");
                        break;
                }
            }
            stringBuilder.AppendLine($"int nRet = CommRqData(\"{model.Title}\", \"{trCode}\", 0, \"{_scrNum_CHART_CONTENT}\");");

            result = stringBuilder.ToString();
        }
        return result;
    }

    void SaveUserContentInfo()
    {
        if (_chartReqViewModel_주식 != null)
        {
            _appRegistry.SetValue(_chartReqViewModel_주식.Title, "종목코드", _chartReqViewModel_주식.Selected종목);
        }
        if (_chartReqViewModel_선물 != null)
        {
            _appRegistry.SetValue(_chartReqViewModel_선물.Title, "종목코드", _chartReqViewModel_선물.Selected종목);
        }
    }
}
