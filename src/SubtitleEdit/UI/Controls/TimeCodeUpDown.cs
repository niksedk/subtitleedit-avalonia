using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Controls.Templates;
using Avalonia.Input;
using Avalonia.Media;
using System;

namespace Nikse.SubtitleEdit.Controls
{
    public class TimeCodeUpDown : TemplatedControl
    {
        private TextBox? _textBox;
        private ButtonSpinner? _spinner;

        public static readonly StyledProperty<TimeSpan> ValueProperty =
            AvaloniaProperty.Register<TimeCodeUpDown, TimeSpan>(
                nameof(Value),
                defaultValue: TimeSpan.Zero);

        public TimeSpan Value
        {
            get => GetValue(ValueProperty);
            set
            {
                SetValue(ValueProperty, Clamp(value));
                UpdateText();
            }
        }

        public TimeCodeUpDown()
        {
            Template = CreateTemplate();
        }

        private static FuncControlTemplate<TimeCodeUpDown> CreateTemplate()
        {
            return new FuncControlTemplate<TimeCodeUpDown>((control, scope) =>
            {
                var spinner = new ButtonSpinner
                {
                    Name = "PART_Spinner",
                    ButtonSpinnerLocation = Location.Right,
                    ShowButtonSpinner = true
                };

                var textBox = new TextBox
                {
                    Name = "PART_TextBox",
                    IsReadOnly = true,
                    BorderBrush = null,
                    Background = null,
                    Padding = new Thickness(4),
                    VerticalContentAlignment = Avalonia.Layout.VerticalAlignment.Center,
                    HorizontalContentAlignment = Avalonia.Layout.HorizontalAlignment.Left,
                    FontFamily = FontFamily.Parse("Consolas")
                };

                spinner.Content = textBox;

                scope.Register("PART_Spinner", spinner);
                scope.Register("PART_TextBox", textBox);

                return spinner;
            });
        }

        protected override void OnApplyTemplate(TemplateAppliedEventArgs e)
        {
            base.OnApplyTemplate(e);

            _textBox = e.NameScope.Find<TextBox>("PART_TextBox");
            _spinner = e.NameScope.Find<ButtonSpinner>("PART_Spinner");

            if (_spinner != null)
                _spinner.Spin += OnSpin;

            if (_textBox != null)
            {
                _textBox.Text = FormatTime(Value);
                _textBox.KeyDown += OnTextBoxKeyDown;
                _textBox.IsReadOnly = true;
            }
        }

        private void OnSpin(object? sender, SpinEventArgs e)
        {
            ChangeValue(e.Direction == SpinDirection.Increase ? +1 : -1);
        }

        private void OnTextBoxKeyDown(object? sender, KeyEventArgs e)
        {
            if (e.Key == Key.Up)
            {
                ChangeValue(+1);
                e.Handled = true;
            }
            else if (e.Key == Key.Down)
            {
                ChangeValue(-1);
                e.Handled = true;
            }
        }

        private void ChangeValue(int delta)
        {
            if (_textBox == null)
            {
                return;
            }

            var caret = _textBox.CaretIndex;
            TimeSpan newVal = Value;

            if (caret <= 2)
            {
                newVal = newVal.Add(TimeSpan.FromHours(delta));
            }
            else if (caret <= 5)
            {
                newVal = newVal.Add(TimeSpan.FromMinutes(delta));
            }
            else if (caret <= 8)
            {
                newVal = newVal.Add(TimeSpan.FromSeconds(delta));
            }
            else
            {
                newVal = newVal.Add(TimeSpan.FromMilliseconds(delta));
            }

            Value = newVal;
        }

        private void UpdateText()
        {
            if (_textBox != null)
            {
                _textBox.Text = FormatTime(Value);
            }
        }

        private string FormatTime(TimeSpan time) =>
            $"{time.Hours:00}:{time.Minutes:00}:{time.Seconds:00}:{time.Milliseconds:000}";

        private TimeSpan Clamp(TimeSpan time) =>
            time.TotalMilliseconds < 0 ? TimeSpan.Zero : time;
    }
}
