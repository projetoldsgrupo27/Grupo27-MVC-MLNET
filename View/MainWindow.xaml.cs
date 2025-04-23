using System;
using System.Windows;
using MovieBookRecommendation.Controllers; // Referência ao Controller
using MovieBookRecommendation.Models;     // Se precisar de Movie, Book, etc.

namespace MovieBookRecommendation
{
    public partial class MainWindow : Window
    {
        private MainController _controller;

        public MainWindow()
        {
            InitializeComponent();
            // Instancia o Controller passando "this" para que ele possa manipular a UI
            _controller = new MainController(this);

            // Registra o evento Loaded da janela
            this.Loaded += MainWindow_Loaded;
        }

        // ======================================
        // ============ EVENTOS DA UI ===========
        // ======================================

        // Evento de carregamento inicial da janela
        private async void MainWindow_Loaded(object sender, RoutedEventArgs e)
        {
            await _controller.OnWindowLoaded();
        }

        // Botão de busca (SearchButton)
        private async void SearchButton_Click(object sender, RoutedEventArgs e)
        {
            await _controller.OnSearchButtonClicked(SearchTextBox.Text);
        }

        // Botão de avaliação de filmes (StarButton)
        private async void StarButton_Click(object sender, RoutedEventArgs e)
        {
            await _controller.OnStarButtonClicked(sender, e);
        }

        // Botão de avaliação de livros (BookStarButton)
        private async void BookStarButton_Click(object sender, RoutedEventArgs e)
        {
            await _controller.OnBookStarButtonClicked(sender, e);
        }

        // Botão de Login/Logout
        private void LoginButton_Click(object sender, RoutedEventArgs e)
        {
            _controller.OnLoginButtonClicked();
        }

        // Botão para enviar preferências (Email + PDF)
        private void SendPreferencesButton_Click(object sender, RoutedEventArgs e)
        {
            _controller.OnSendPreferencesButtonClicked();
        }

        // Botão para abrir o ChatBot
        private void ChatBotButton_Click(object sender, RoutedEventArgs e)
        {
            _controller.OnChatBotButtonClicked();
        }
    }
}
