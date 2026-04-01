namespace oedibud.Services;

public class DataChangeNotifier
{
    public event Action? OnChange;

    public void Notify() => OnChange?.Invoke();
}
