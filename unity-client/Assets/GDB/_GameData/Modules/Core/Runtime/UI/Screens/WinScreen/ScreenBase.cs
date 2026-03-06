using UniTx.Runtime.IoC;
using UnityEngine;

public abstract class ScreenBase : MonoBehaviour, IScreen, IInjectable
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

    public abstract void Inject(IResolver resolver);
}

public interface IScreen
{
    public void ShowScreen();
    public void HideScreen();
}
