namespace KOAStudio.Core.Services;

// default implementation 
// business logic -> ViewModel, or ViewModel <-> ViewModel
public interface ILogicNotify
{
    /// <summary>
    /// 커스텀 메뉴 설정
    /// </summary>
    /// <param name="headerText">메뉴타이틀</param>
    /// <param name="items">메뉴네임 string lists</param>
    void SetMenuCustomize(string headerText, object items);

    /// <summary>
    /// 애플리케이션 상태바 텍스트 설정
    /// </summary>
    /// <param name="text"></param>
    void SetStatusText(string text, bool changedLoginState = false, bool realServer = false);

    /// <summary>
    /// 결과 텍스트 설정
    /// </summary>
    /// <param name="text">텍스트</param>
    /// <param name="bAdd">추가 Flag</param>
    void SetResultText(string text, bool bAdd = false);

    /// <summary>
    /// 속성창 설정
    /// </summary>
    /// <param name="headerText">요청 텍스트</param>
    /// <param name="items">속성값 리스트</param>
    void SetProperties(string headerText, object items);

    /// <summary>
    /// 속성창 다음 버튼 Enable 설정
    /// </summary>
    /// <param name="bEnable">true: enable, false: disable</param>
    void SetPropertyQueryNextEnable(bool bEnable);

    /// <summary>
    /// 로그 리스트뷰 탭 설정
    /// </summary>
    /// <param name="items"></param>
    void SetTabLists(object items);

    /// <summary>
    /// 로그출력
    /// </summary>
    /// <param name="tabIndex"></param>
    /// <param name="content"></param>
    /// <param name="maxLines"></param>
    /// <param name="focus"></param>
    void OutputLog(int tabIndex, object? content = null, int maxLines = -1, bool focus = false);

    /// <summary>
    /// 아이템 트리뷰 탭 설정
    /// </summary>
    /// <param name="items"></param>
    void SetTabTrees(object items);

    /// <summary>
    /// 트리뷰 목록 데이터 설정
    /// </summary>
    /// <param name="tabIndex"></param>
    /// <param name="items"></param>
    void SetTreeItems(int tabIndex, object items);
}
