using KOAStudio.Core.Services;
using KOAStudio.Core.ViewModels;
using System.Windows.Controls;

namespace KOAStudio.Core.Views
{
    /// <summary>
    /// Interaction logic for ChartReqView.xaml
    /// </summary>
    public partial class ChartReqView : UserControl, IUserTool
    {
        public ChartReqView(object ComponentModel)
        {
            InitializeComponent();
            DataContext = ComponentModel;
            if (DataContext is ChartReqViewModel model)
            {
                model.EnableUpdateCodeText = true;
            }
        }

        public void CloseTool()
        {
            if (DataContext is ChartReqViewModel model)
            {
                model.EnableUpdateCodeText = false;
            }
        }
    }
}
