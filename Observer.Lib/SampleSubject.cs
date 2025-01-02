namespace Observer.Lib;

public class SampleSubject : ISubject
{
    private static SampleSubject? instance = null;
    private List<IObserver> _observers = [];
    
    private SampleSubject() { }

    public static SampleSubject Instance
    {
        get
        {
            // Check if the Instance already exists
            if (instance == null)
                instance = new SampleSubject();

            return instance;
        }
    }

    public void Attach(IObserver observer)
    {
        _observers.Add(observer);
    }
    public void Detach(IObserver observer)
    {
        if (observer is not null)
            _observers.Remove(observer);
    }

    public void Notify(string message)
    {
        foreach (var observer in _observers)
        {
            observer.Update(message);
        }
    }
}