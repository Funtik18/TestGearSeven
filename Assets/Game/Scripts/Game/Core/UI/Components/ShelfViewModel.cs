using System;
using UnityEngine;

namespace RosiesCornerShop.Core
{
    public sealed class ShelfViewModel
    {
        private readonly ShelfView _view;
        private readonly RectTransform[] _slots;
        private bool _wasBlocked;

        public ShelfView View => _view;

        public ShelfViewModel( ShelfView view, int slotCount, int jarsPerRow, float rowHeight )
        {
            _view = view ?? throw new ArgumentNullException( nameof(view) );
            _slots = new RectTransform[slotCount];
            for ( int i = 0; i < slotCount; i++ )
            {
                var slot = UnityEngine.Object.Instantiate( _view.SlotPrefab, _view.SlotsRoot );
                slot.gameObject.SetActive( true );
                slot.anchorMin = slot.anchorMax = new Vector2( 0, 1 );
                slot.pivot = new Vector2( .5f, .5f );
                slot.sizeDelta = new Vector2( 108f / jarsPerRow - 3, rowHeight - 8 );
                slot.anchoredPosition = new Vector2( ( i % jarsPerRow + .5f ) * 108f / jarsPerRow + 5, -( i / jarsPerRow + .5f ) * rowHeight );
                _slots[ i ] = slot;
            }
        }

        public RectTransform GetSlot( int index ) => _slots[ index ];

        public void Render( ShelfPresentation presentation, float rowHeight, bool animate )
        {
            _view.Rect.sizeDelta = new Vector2( 118, presentation.Height * rowHeight - 3 );
            var destination = new Vector2( 0, presentation.Offset * rowHeight );
            if ( !animate || ( _view.Rect.anchoredPosition - destination ).sqrMagnitude > .1f )
            {
                float delay = _view.Rect.anchoredPosition.y > 300 ? .32f + presentation.Column * .08f : .18f;
                _view.MoveTo( destination, animate, delay );
            }

            _view.Status.text = presentation.Status;
            _view.Barrier.gameObject.SetActive( !presentation.Open );
            _view.Barrier.color = presentation.Frozen ? new Color( .55f, .84f, .96f, .75f ) : new Color( .65f, .43f, .23f, .8f );
            if ( _wasBlocked && presentation.Open ) _view.PlayUnlocked();
            _wasBlocked = !presentation.Open;
            _view.Barrier.transform.SetAsLastSibling();
            _view.Status.transform.SetAsLastSibling();
        }
    }
}
