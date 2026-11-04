using System;
using System.Linq;
using SoosvetGames.VVM;

namespace RosiesCornerShop.Core
{
    public sealed class HomeViewModel : ViewModel< HomeView >
    {
        private readonly GameProgress _progress;
        private readonly ShopUIManager _ui;

        public HomeViewModel( GameProgress progress, ShopUIManager ui )
        {
            _progress = progress ?? throw new ArgumentNullException( nameof(progress) );
            _ui = ui ?? throw new ArgumentNullException( nameof(ui) );
        }

        protected override void SubscribeView()
        {
            base.SubscribeView();
            ModelView.OnPlayClicked += PlayClickedHandler;
            ModelView.OnLevelsClicked += LevelsClickedHandler;
            ModelView.OnCollectionClicked += CollectionClickedHandler;
            ModelView.OnSettingsClicked += SettingsClickedHandler;

            ModelView.PlayButton.Bind( "", ModelView.OnPlayClick );
            ModelView.LevelsButton.Bind( "Уровни", ModelView.OnLevelsClick );
            ModelView.CollectionButton.Bind( "Коллекция", ModelView.OnCollectionClick );
            ModelView.SettingsButton.Bind( "Настройки", ModelView.OnSettingsClick );
        }

        protected override void UnSubscribeView()
        {
            base.UnSubscribeView();
            ModelView.OnPlayClicked -= PlayClickedHandler;
            ModelView.OnLevelsClicked -= LevelsClickedHandler;
            ModelView.OnCollectionClicked -= CollectionClickedHandler;
            ModelView.OnSettingsClicked -= SettingsClickedHandler;
            ModelView.PlayButton.Unbind();
            ModelView.LevelsButton.Unbind();
            ModelView.CollectionButton.Unbind();
            ModelView.SettingsButton.Unbind();
        }

        protected override void OnViewCreated()
        {
            ModelView.PlayButton.SetLabel( "Играть · Уровень " + ( _ui.NextLevel + 1 ) );
            ModelView.Statistics.text = "Монеты: " + _progress.coins + "   /   Звёзды: " + _progress.stars.Sum();
        }

        private void PlayClickedHandler() => _ui.StartLevel( _ui.NextLevel );
        private void LevelsClickedHandler() => _ui.ShowLevels();
        private void CollectionClickedHandler() => _ui.ShowCollection();
        private void SettingsClickedHandler() => _ui.ShowSettings();
    }
}
