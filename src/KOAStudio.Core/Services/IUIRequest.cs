using KOAStudio.Core.Models;

namespace KOAStudio.Core.Services;

// ViewModel -> business logic
public interface IUIRequest
{
    /// <summary>
    /// 서비스 데이터 초기설정
    /// </summary>
    void Initialize();

    /// <summary>
    /// OpenApi 연결상태
    /// </summary>
    OpenApiLoginState LoginState { get; }

    /// <summary>
    /// OpenApi 연결/해제 요청
    /// </summary>
    /// <returns></returns>
    int ReqApiLogin(bool bLogin);

    /// <summary>
    /// 실시간 감시 중지
    /// </summary>
    void ReqStopRealTime();

    /// <summary>
    /// TR History 요청 파라미터 설정
    /// </summary>
    /// <param name="tabIndex"></param>
    /// <param name="text"></param>
    void ReqTRHistory(int tabIndex, string text);

    /// <summary>
    /// 목록선택 프러시저
    /// </summary>
    /// <param name="tabIndex"></param>
    /// <param name="selectedItem"></param>
    void ItemSelectedChanged(int tabIndex, IconTextItem selectedItem);

    /// <summary>
    /// 속성창 조회/다음 버튼 실행
    /// </summary>
    /// <param name="reqText"></param>
    /// <param name="parameters"></param>
    /// <param name="bNext"></param>
    void QueryApiAction(string reqText, object parameters, bool bNext);

    /// <summary>
    /// 메뉴 커스텀 요청
    /// </summary>
    /// <param name="text">메뉴네임</param>
    void MenuCustomizeAction(string text);
}
