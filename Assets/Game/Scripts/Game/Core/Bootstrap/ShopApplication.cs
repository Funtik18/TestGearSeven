using DG.Tweening;
using UnityEngine;
using Zenject;

namespace RosiesCornerShop.Core
{
    public sealed class ShopApplication : MonoBehaviour
    {
        [ SerializeField ] private ShopAssets _assets;
        [ SerializeField ] private RectTransform _safeArea;
        [ SerializeField ] private RectTransform _screenRoot;
        [ SerializeField ] private RectTransform _windowRoot;
        [ SerializeField ] private ToastView _toast;
        [ SerializeField ] private ShopAudio _shopAudio;

        private ProgressStore _store;
        private GameProgress _progress;
        private GameSession _session;
        private ShopUIManager _ui;
        private Rect _previousSafeArea;
        private int _previousWidth;
        private int _previousHeight;

        public ShopAssets Assets => _assets;
        public Transform ScreenRoot => _screenRoot;
        public Transform DialogRoot => _windowRoot;
        public ToastView Toast => _toast;
        public ShopAudio Audio => _shopAudio;

        [ Inject ]
        private void Construct( ProgressStore store, GameProgress progress, GameSession session, ShopUIManager ui )
        {
            _store = store;
            _progress = progress;
            _session = session;
            _ui = ui;
            _session.ProgressChanged += Save;
        }

        private void Awake()
        {
            DOTween.Init( false, true, LogBehaviour.ErrorsOnly );
        }

        private void Start()
        {
            Canvas.ForceUpdateCanvases();
            ApplySafeArea();
            _ui.ShowHome();
        }

        private void Update()
        {
            if ( _previousSafeArea != Screen.safeArea || _previousWidth != Screen.width || _previousHeight != Screen.height ) ApplySafeArea();
            _ui.Tick( Time.unscaledDeltaTime );
        }

        private void ApplySafeArea()
        {
            _previousSafeArea = Screen.safeArea;
            _previousWidth = Screen.width;
            _previousHeight = Screen.height;
            if ( Screen.width == 0 || Screen.height == 0 ) return;
            _safeArea.anchorMin = _safeArea.anchorMax = new Vector2( .5f, .5f );
            float scale = GetComponent< Canvas >().scaleFactor;
            if ( scale <= 0 ) scale = 1;
            _safeArea.localScale = Vector3.one * Mathf.Min( _previousSafeArea.width / scale / 430f, _previousSafeArea.height / scale / 900f );
            _safeArea.anchoredPosition = ( _previousSafeArea.center - new Vector2( Screen.width, Screen.height ) * .5f ) / scale;
        }

        private void Save()
        {
            if ( _progress != null ) _store.Save( _progress );
        }

        private void OnApplicationPause( bool paused )
        {
            if ( paused ) Save();
        }

        private void OnApplicationQuit() => Save();

        private void OnDestroy()
        {
            if ( _session != null ) _session.ProgressChanged -= Save;
        }
    }
}
