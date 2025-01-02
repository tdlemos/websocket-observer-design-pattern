namespace Observer.Lib;

public interface IObserver
{
    Task Update(string message);
}
