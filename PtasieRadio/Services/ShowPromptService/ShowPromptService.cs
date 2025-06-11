using Windows.UI.Core;
using Windows.ApplicationModel.Core;
public class ShowPromptService : IShowPromptService
{
    public ShowPromptService()
    {

    }
    public async Task ShowMessageAsync(string content, string title)
    {
        await CoreApplication.MainView.CoreWindow.Dispatcher.RunAsync(CoreDispatcherPriority.Normal, async () =>
        {
            try
            {
                var dialog = new ContentDialog
                {
                    Title = title,
                    Content = content,
                    CloseButtonText = "OK",
                    XamlRoot = Window.Current.Content.XamlRoot
                };
                await dialog.ShowAsync();
            }
            catch (FileNotFoundException ex)
            {
                System.Diagnostics.Debug.WriteLine($"{ex}");
            }
            
        });
    }
}