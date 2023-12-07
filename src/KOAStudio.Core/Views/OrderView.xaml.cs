using KOAStudio.Core.Models;
using KOAStudio.Core.Services;
using KOAStudio.Core.ViewModels;
using System.Windows.Controls;
using System.Windows.Input;

namespace KOAStudio.Core.Views
{
    /// <summary>
    /// OrderView.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class OrderView : UserControl, IUserTool
    {
        public OrderView(object ControlModel)
        {
            InitializeComponent();
            DataContext = ControlModel;
        }

        public void CloseTool()
        {
            if (DataContext is OrderViewModel model)
            {
                model.EnableUpdateCodeText = false;
                DataContext = null;
            }
        }

        private void 잔고Row_DoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (DataContext is OrderViewModel model)
            {
                var item = model.SelectedJangoItem;
                if (item == null) return;
                model.EnableUpdateCodeText = false;
                model.종목코드 = item.종목코드;
                model.매매구분 = item.매도수구분 ? OrderType.매수 : OrderType.매도;
                model.주문수량 = item.보유수량;
                model.주문가격 = item.현재가.ToString();
                model.EnableUpdateCodeText = true;
                model.UpdateCodeText();
            }
        }

        private void 미체결Row_DoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (DataContext is OrderViewModel model)
            {
                var item = model.SelectedMicheItem;
                if (item == null) return;
                model.EnableUpdateCodeText = false;
                model.종목코드 = item.종목코드;
                model.매매구분 = OrderType.정정취소;
                model.주문번호 = item.주문번호;
                model.원주문매도수구분 = item.매도수구분;
                model.주문수량 = item.주문수량;
                model.주문가격 = item.주문가격.ToString();
                model.EnableUpdateCodeText = true;
                model.UpdateCodeText();
            }
        }
    }
}
