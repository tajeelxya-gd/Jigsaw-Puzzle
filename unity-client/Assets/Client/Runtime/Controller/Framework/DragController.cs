using System;
using UniTx.Runtime;
using UniTx.Runtime.Extensions;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Client.Runtime
{
    public abstract class DragController : MonoBehaviour,
        IBeginDragHandler, IDragHandler, IEndDragHandler, ISceneEntity
    {
        public event Action<ISceneEntity> OnBeginDragEvent;
        public event Action<ISceneEntity> OnEndDragEvent;

        public GameObject GameObject => gameObject;
        public Transform Transform => transform;

        private bool _dragStarted;

        public void OnBeginDrag(PointerEventData eventData)
        {
            // Only left click
            if (eventData.button != PointerEventData.InputButton.Left)
                return;

            if (!CanStartDrag(eventData))
                return;

            _dragStarted = true;

            OnDragStarted(eventData);
            OnBeginDragEvent.Broadcast(this);
        }

        public void OnDrag(PointerEventData eventData)
        {
            if (!_dragStarted)
                return;

            OnDragging(eventData);
        }

        public void OnEndDrag(PointerEventData eventData)
        {
            if (!_dragStarted)
                return;

            _dragStarted = false;

            OnDragEnded(eventData);
            OnEndDragEvent.Broadcast(this);
        }

        /// <summary>
        /// Override if you want hit validation
        /// </summary>
        protected virtual bool CanStartDrag(PointerEventData eventData) => true;

        protected abstract void OnDragStarted(PointerEventData eventData);
        protected abstract void OnDragging(PointerEventData eventData);
        protected abstract void OnDragEnded(PointerEventData eventData);
    }
}
