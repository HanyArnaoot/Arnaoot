using Arnaoot.Core;
using Arnaoot.VectorGraphics.Abstractions;
using Avalonia.Controls;
using Avalonia.Layout;
using Avalonia.Media;
using Avalonia.Platform.Storage;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VectorDrawAvoloniaUI.Classes
{
    /// <summary>
    /// Manages UI state updates (text displays, labels)
    /// </summary>
    public class UIStateManager
    {
        private readonly TextBlock _infoText;
        private readonly TextBlock _rotationXText;
        private readonly TextBlock _rotationYText;
        private readonly TextBlock _rotationZText;

        public UIStateManager(
            TextBlock infoText,
            TextBlock rotationXText,
            TextBlock rotationYText,
            TextBlock rotationZText)
        {
            _infoText = infoText ?? throw new ArgumentNullException(nameof(infoText));
            _rotationXText = rotationXText ?? throw new ArgumentNullException(nameof(rotationXText));
            _rotationYText = rotationYText ?? throw new ArgumentNullException(nameof(rotationYText));
            _rotationZText = rotationZText ?? throw new ArgumentNullException(nameof(rotationZText));
        }

        public void UpdateInfoText(IViewSettings viewSettings, long renderTime)
        {
            var shift = viewSettings.ShiftWorld;
            _infoText.Text = $"DrawTime: {renderTime}ms | " +
                           $"Zoom: {viewSettings.ZoomFactorAverage:F2}x | " +
                           $"Shift: X:{(int)shift.X}, Y:{(int)shift.Y}, Z:{(int)shift.Z}";
        }

        public void UpdateMousePosition(Vector3D worldCoords, IViewSettings viewSettings)
        {
            var shift = viewSettings.ShiftWorld;
            _infoText.Text = $"Pan: ({(int)shift.X}, {(int)shift.Y}) | " +
                           $"Zoom: {viewSettings.ZoomFactorAverage:F2}x | " +
                           $"Mouse: ({worldCoords.X:F0}, {worldCoords.Y:F0})";
        }

        public void UpdateRotationDisplay(RotationAngles angles)
        {
            _rotationXText.Text = $"X: {angles.X:F2}°";
            _rotationYText.Text = $"Y: {angles.Y:F2}°";
            _rotationZText.Text = $"Z: {angles.Z:F2}°";
        }
    }

    /// <summary>
    /// Handles file dialogs and message boxes
    /// </summary>
    public class DialogService
    {
        private readonly Window _parentWindow;

        public DialogService(Window parentWindow)
        {
            _parentWindow = parentWindow ?? throw new ArgumentNullException(nameof(parentWindow));
        }

        public async Task<string?> SelectFileToOpenAsync()
        {
            try
            {
                var dialog = new OpenFileDialog
                {
                    Title = "Select SVG File",
                    AllowMultiple = false
                };

                dialog.Filters.Add(new FileDialogFilter
                {
                    Name = "SVG Files",
                    Extensions = { "svg" }
                });
                dialog.Filters.Add(new FileDialogFilter
                {
                    Name = "All Files",
                    Extensions = { "*" }
                });

                var result = await dialog.ShowAsync(_parentWindow);

                if (result != null && result.Length > 0 && File.Exists(result[0]))
                {
                    return result[0];
                }

                return null;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"File selection error: {ex.Message}");
                return null;
            }
        }

        public async Task<string?> SelectFileToSaveAsync()
        {
            try
            {
                var file = await _parentWindow.StorageProvider.SaveFilePickerAsync(
                    new FilePickerSaveOptions
                    {
                        Title = "Save Image As",
                        DefaultExtension = "png",
                        ShowOverwritePrompt = true,
                        FileTypeChoices = new[]
                        {
                            new FilePickerFileType("PNG Files")
                            {
                                Patterns = new[] { "*.png" }
                            },
                            new FilePickerFileType("JPEG Files")
                            {
                                Patterns = new[] { "*.jpg", "*.jpeg" }
                            },
                            new FilePickerFileType("All Files")
                            {
                                Patterns = new[] { "*" }
                            }
                        }
                    });

                return file?.Path.LocalPath;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"File save dialog error: {ex.Message}");
                return null;
            }
        }

        public async Task ShowMessageAsync(string title, string message)
        {
            var dialog = new Window
            {
                Title = title,
                Width = 300,
                Height = 150,
                WindowStartupLocation = WindowStartupLocation.CenterOwner,
                CanResize = false,
            };

            var panel = new StackPanel
            {
                HorizontalAlignment = HorizontalAlignment.Center,
                VerticalAlignment = VerticalAlignment.Center,
                Spacing = 10
            };

            panel.Children.Add(new TextBlock
            {
                Text = message,
                TextWrapping = TextWrapping.Wrap,
                HorizontalAlignment = HorizontalAlignment.Center
            });

            var okButton = new Button { Content = "OK", Width = 80 };
            okButton.Click += (s, e) => dialog.Close();

            panel.Children.Add(okButton);
            dialog.Content = panel;

            await dialog.ShowDialog(_parentWindow);
        }
    }
}
