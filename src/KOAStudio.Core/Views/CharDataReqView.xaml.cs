using KOAStudio.Core.Services;
using KOAStudio.Core.ViewModels;
using System.Windows.Controls;

namespace KOAStudio.Core.Views
{
    /// <summary>
    /// Interaction logic for CharDataReqView.xaml
    /// </summary>
    public partial class CharDataReqView : UserControl, IUserTool
    {
        public CharDataReqView(object ControlModel)
        {
            InitializeComponent();
            DataContext = ControlModel;
        }

        public void CloseTool()
        {
            if (DataContext is CharDataReqViewModel model)
            {
                model.EnableUpdateCodeText = false;
                DataContext = null;
            }
        }
    }
}
