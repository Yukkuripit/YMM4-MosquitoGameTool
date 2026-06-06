using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace MosquitoGame
{
    public partial class MosquitoGameView : UserControl
    {
        public MosquitoGameView()
        {
            InitializeComponent();
        }

        // XAML の Canvas の Loaded イベント用ハンドラ（必須）
        private void GameCanvas_Loaded(object sender, RoutedEventArgs e)
        {
            if (DataContext is MosquitoGameViewModel vm && GameCanvas != null)
            {
                vm.SetCanvasSize(GameCanvas.ActualWidth, GameCanvas.ActualHeight);
            }
        }

        // XAML の Canvas の MouseLeftButtonDown イベント用ハンドラ（蚊の外側クリック用、空でOK）
        private void GameCanvas_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            // 蚊のボタンがコマンドで処理するため、ここでは何もしない
        }
    }
}