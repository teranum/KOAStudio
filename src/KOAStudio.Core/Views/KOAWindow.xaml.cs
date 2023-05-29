using ICSharpCode.AvalonEdit.Editing;
using System;
using System.ComponentModel;
using System.Windows;

namespace KOAStudio.Core.Views
{
    /// <summary>
    /// Interaction logic for KOAWindow.xaml
    /// </summary>
    public partial class KOAWindow : Window
    {
        public KOAWindow()
        {
            InitializeComponent();
        }
    }

    internal class BindableAvalonEditor : ICSharpCode.AvalonEdit.TextEditor, INotifyPropertyChanged
    {
        public BindableAvalonEditor()
        {
            ShowLineNumbers = true;
            var leftMargins = TextArea.LeftMargins;
            foreach (var Margin in leftMargins)
            {
                if (Margin is LineNumberMargin lmargin)
                {
                    lmargin.Width = 30;
                    break;
                }
            }
        }
        /// <summary>
        /// A bindable Text property
        /// </summary>
        public new string Text
        {
            get
            {
                return (string)GetValue(TextProperty);
            }
            set
            {
                SetValue(TextProperty, value);
                RaisePropertyChanged("Text");
            }
        }

        /// <summary>
        /// The bindable text property dependency property
        /// </summary>
        public static readonly DependencyProperty TextProperty =
            DependencyProperty.Register(
                "Text",
                typeof(string),
                typeof(BindableAvalonEditor),
                new FrameworkPropertyMetadata
                {
                    DefaultValue = default(string),
                    BindsTwoWayByDefault = true,
                    PropertyChangedCallback = OnDependencyPropertyChanged
                }
            );

        protected static void OnDependencyPropertyChanged(DependencyObject obj, DependencyPropertyChangedEventArgs args)
        {
            var target = (BindableAvalonEditor)obj;

            if (target.Document is not null)
            {
                var caretOffset = target.CaretOffset;
                var newValue = args.NewValue;

                if (newValue is null)
                {
                    newValue = "";
                }

                string text = (string)newValue;
                target.Document.Text = text;
                target.CaretOffset = Math.Min(caretOffset, text.Length);
            }
        }

        protected override void OnTextChanged(EventArgs e)
        {
            if (this.Document is not null)
            {
                Text = this.Document.Text;
            }

            base.OnTextChanged(e);
        }

        /// <summary>
        /// Raises a property changed event
        /// </summary>
        /// <param name="property">The name of the property that updates</param>
        public void RaisePropertyChanged(string property)
        {
            if (PropertyChanged is not null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(property));
            }
        }

        public event PropertyChangedEventHandler? PropertyChanged;
    }
}
