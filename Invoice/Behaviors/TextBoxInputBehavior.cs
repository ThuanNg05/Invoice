using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;

namespace Invoice.Behaviors;

public enum TextBoxInputType
{
    None,
    Numeric,
    Decimal,
    PhoneNumber
}

public static class TextBoxInputBehavior
{
    public static TextBoxInputType GetInputType(DependencyObject obj) => (TextBoxInputType)obj.GetValue(InputTypeProperty);

    public static void SetInputType(DependencyObject obj, TextBoxInputType value) => obj.SetValue(InputTypeProperty, value);

    public static readonly DependencyProperty InputTypeProperty =
        DependencyProperty.RegisterAttached("InputType", typeof(TextBoxInputType), typeof(TextBoxInputBehavior), new PropertyMetadata(TextBoxInputType.None, OnInputTypeChanged));

    private static void OnInputTypeChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is TextBox textBox)
        {
            textBox.BeforeTextChanging -= OnBeforeTextChanging;

            var newValue = (TextBoxInputType)e.NewValue;
            if (newValue != TextBoxInputType.None)
            {
                textBox.BeforeTextChanging += OnBeforeTextChanging;
            }
        }
    }

    private static void OnBeforeTextChanging(TextBox sender, TextBoxBeforeTextChangingEventArgs args)
    {
        var inputType = GetInputType(sender);

        if (inputType == TextBoxInputType.Numeric)
        {
            args.Cancel = args.NewText.Any(c => !char.IsDigit(c));
        }
        else if (inputType == TextBoxInputType.Decimal)
        {
            args.Cancel = args.NewText.Any(c => !char.IsDigit(c) && c != '.');

            if (!args.Cancel && args.NewText.Count(c => c == '.') > 1)
            {
                args.Cancel = true;
            }
        }
        else if (inputType == TextBoxInputType.PhoneNumber)
        {
            args.Cancel = args.NewText.Any(c => !char.IsDigit(c)) || args.NewText.Length > 10;
        }
    }
}
