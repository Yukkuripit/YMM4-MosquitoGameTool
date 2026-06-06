using System;
using YukkuriMovieMaker.Plugin;

namespace MosquitoGame
{
    public class MosquitoGameTool : IToolPlugin
    {
        public string Name => "蚊叩きゲーム";
        public Type ViewType => typeof(MosquitoGameView);
        public Type ViewModelType => typeof(MosquitoGameViewModel);

        public void Show() { }
    }
}