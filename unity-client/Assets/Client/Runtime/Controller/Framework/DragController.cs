using System;
using UniTx.Runtime;
using UniTx.Runtime.Extensions;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Client.Runtime
{
    public abstract class DragController : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler, ISceneEntity
    {
        public event Action<ISceneEntity> OnBeginDragEvent;
        public event Action<ISceneEntity> OnEndDragEvent;

        public GameObject GameObject => gameObject;
        public Transform Transform => transform;

        public void OnBeginDrag(PointerEventData eventData)
        {
            OnDragStarted(eventData);
            OnBeginDragEvent.Broadcast(this);
        }

        public void OnDrag(PointerEventData eventData)
        {
            OnDragging(eventData);
        }

        public void OnEndDrag(PointerEventData eventData)
        {
            OnDragEnded(eventData);
            OnEndDragEvent.Broadcast(this);
        }

        protected abstract void OnDragStarted(PointerEventData eventData);
        protected abstract void OnDragging(PointerEventData eventData);
        protected abstract void OnDragEnded(PointerEventData eventData);
    }
}
