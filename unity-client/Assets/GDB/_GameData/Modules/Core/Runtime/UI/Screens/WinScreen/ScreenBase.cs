using UnityEngine;

public abstract class ScreenBase : MonoBehaviour, IScreen
{

    public virtual void ShowScreen()
    {

    }

    public virtual void ShowScreen<T>(T data)
    {

    }

    public virtual void HideScreen()
    {

    }

    public virtual void Inject()
    {

    }
}

public interface IScreen
{
    public void ShowScreen();
    public void HideScreen();
}
