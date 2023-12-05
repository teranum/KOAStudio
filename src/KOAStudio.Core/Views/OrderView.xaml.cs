using KOAStudio.Core.Services;
using KOAStudio.Core.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

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
            if (DataContext is OrderViewModel model)
            {
                //model.EnableUpdateCodeText = true;
            }
        }

        public void CloseTool()
        {
            if (DataContext is OrderViewModel model)
            {
                //model.EnableUpdateCodeText = false;
                DataContext = null;
            }
        }
    }
}
