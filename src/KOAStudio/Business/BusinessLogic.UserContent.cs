using KOAStudio.Core.ViewModels;
using KOAStudio.Core.Views;

namespace KOAStudio.Business;

internal sealed partial class BusinessLogic
{
    private ChartReqViewModel? _chartReqViewModel_주식;
    private ChartReqViewModel? _chartReqViewModel_선물;
    void ShowUserContent(string require)
    {
        if (require.Equals("주식차트요청"))
        {
            _chartReqViewModel_주식 ??= new ChartReqViewModel(require)
            {
                Selected종목 = _appRegistry.GetValue(require, "종목코드", "005930"),
            };

            SetUserContent(new ChartReqView(_chartReqViewModel_주식));
        }
        else if (require.Equals("선물차트요청"))
        {
            _chartReqViewModel_선물 ??= new ChartReqViewModel(require)
            {
                Selected종목 = _appRegistry.GetValue(require, "종목코드", string.Empty),
            };

            SetUserContent(new ChartReqView(_chartReqViewModel_선물));
        }
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
