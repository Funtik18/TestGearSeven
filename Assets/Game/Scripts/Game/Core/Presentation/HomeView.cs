using TMPro;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

namespace RosiesCornerShop.Core
{
    public sealed class HomeView : MonoBehaviour
    {
        [SerializeField] ShopButton play, levels, collection, settings;
        [SerializeField] TextMeshProUGUI stats;
        public void Initialize(ShopApplication app)
        {
            play.Bind("Играть · Уровень " + (app.NextLevel + 1),() => app.StartLevel(app.NextLevel));
            levels.Bind("Уровни",app.ShowLevels); collection.Bind("Коллекция",app.ShowCollection); settings.Bind("Настройки",app.ShowSettings);
            stats.text = "Монеты: " + app.Progress.coins + "   /   Звёзды: " + app.Progress.stars.Sum();
            ShopMotion.Enter(transform);
        }
    }
}
